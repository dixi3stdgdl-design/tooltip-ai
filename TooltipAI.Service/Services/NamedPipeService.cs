using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using TooltipAI.Core.Models;

namespace TooltipAI.Service.Services;

public class NamedPipeService : IDisposable
{
    private const string PipeName = "TooltipAI_Pipe";
    private readonly List<NamedPipeServerStream> _clients = new();
    private readonly object _lock = new();
    private CancellationTokenSource? _cts;

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _ = AcceptClientsLoopAsync(_cts.Token);
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
                    4, // Allow up to 4 simultaneous clients
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
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PIPE] Error: {ex.Message}");
                await Task.Delay(1000, ct);
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

        Console.WriteLine($"[PIPE] Sending to {snapshot.Count} clients: {data.Element?.Name} | {data.Element?.ControlType}");

        foreach (var client in snapshot)
        {
            try
            {
                await client.WriteAsync(message, ct);
                await client.FlushAsync(ct);
            }
            catch
            {
                lock (_lock)
                {
                    _clients.Remove(client);
                }
            }
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        lock (_lock)
        {
            foreach (var client in _clients)
                client.Dispose();
            _clients.Clear();
        }
        _cts?.Dispose();
    }
}
