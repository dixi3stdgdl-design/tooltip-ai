using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TooltipAI.Core.Models;

namespace TooltipAI.Service.Services;

public class NamedPipeService : IDisposable
{
    private const string PipeName = "TooltipAI_Pipe";
    private readonly List<NamedPipeServerStream> _clients = new();
    private readonly object _lock = new();
    private readonly ILogger<NamedPipeService> _logger;
    private CancellationTokenSource? _cts;
    private Task? _acceptTask;

    public NamedPipeService(ILogger<NamedPipeService> logger)
    {
        _logger = logger;
    }

    public void Start()
    {
        if (_cts is not null)
            throw new InvalidOperationException("Named pipe service is already running.");

        _cts = new CancellationTokenSource();
        _acceptTask = AcceptClientsLoopAsync(_cts.Token);
    }

    private async Task AcceptClientsLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            NamedPipeServerStream? server = null;
            try
            {
                server = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                Console.WriteLine($"[PIPE] Waiting for client on '{PipeName}'...");
                await server.WaitForConnectionAsync(ct);
                Console.WriteLine("[PIPE] Client connected!");

                lock (_lock)
                {
                    _clients.Add(server);
                    server = null;
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Named pipe accept loop failed");
                try
                {
                    await Task.Delay(1000, ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
            }
            finally
            {
                server?.Dispose();
            }
        }
    }

    public async Task SendTooltipDataAsync(TooltipData data, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(data);
        var jsonBytes = Encoding.UTF8.GetBytes(json);
        var lengthPrefix = BitConverter.GetBytes(jsonBytes.Length);
        var message = new byte[4 + jsonBytes.Length];
        Buffer.BlockCopy(lengthPrefix, 0, message, 0, 4);
        Buffer.BlockCopy(jsonBytes, 0, message, 4, jsonBytes.Length);

        List<NamedPipeServerStream> snapshot;
        lock (_lock)
        {
            _clients.RemoveAll(c => !c.IsConnected);
            snapshot = _clients.ToList();
        }

        foreach (var client in snapshot)
        {
            try
            {
                await client.WriteAsync(message, ct);
                await client.FlushAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Removing a disconnected named pipe client");
                lock (_lock)
                {
                    _clients.Remove(client);
                }
                client.Dispose();
            }
        }
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        var cts = _cts;
        if (cts is null)
            return;

        _cts = null;
        cts.Cancel();

        var acceptTask = _acceptTask;
        _acceptTask = null;
        try
        {
            if (acceptTask is not null)
                await acceptTask.WaitAsync(ct);
        }
        finally
        {
            lock (_lock)
            {
                foreach (var client in _clients)
                    client.Dispose();
                _clients.Clear();
            }

            cts.Dispose();
        }
    }

    public void Dispose()
    {
        try
        {
            StopAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop named pipe service cleanly");
        }
    }
}
