using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using TooltipAI.Core.Models;

namespace TooltipAI.UI.Services;

public class NamedPipeClient : IDisposable
{
    private const string PipeName = "TooltipAI_Pipe";
    private NamedPipeClientStream? _pipe;
    private CancellationTokenSource? _cts;
    private Task? _readTask;

    public event Action<TooltipData>? DataReceived;
    public event Action? Disconnected;
    public bool IsConnected => _pipe?.IsConnected == true;

    public async Task ConnectAsync(CancellationToken ct)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                _pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.In);
                await _pipe.ConnectAsync(5000, _cts.Token);
                _readTask = ReadLoopAsync(_cts.Token);
                return;
            }
            catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UI] Pipe connection attempt failed: {ex}");
                _pipe?.Dispose();
                _pipe = null;
                await Task.Delay(1000, _cts.Token);
            }
        }
    }

    private async Task ReadLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && _pipe?.IsConnected == true)
            {
                var lengthBuffer = new byte[4];
                var bytesRead = await _pipe.ReadAsync(lengthBuffer, 0, 4, ct);

                if (bytesRead == 0)
                {
                    Disconnected?.Invoke();
                    break;
                }

                if (bytesRead < 4)
                {
                    var remaining = 4 - bytesRead;
                    while (remaining > 0 && !ct.IsCancellationRequested)
                    {
                        var n = await _pipe.ReadAsync(lengthBuffer, 4 - remaining, remaining, ct);
                        if (n == 0) { Disconnected?.Invoke(); return; }
                        remaining -= n;
                    }
                }

                var messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                if (messageLength <= 0 || messageLength > 1024 * 1024)
                {
                    Disconnected?.Invoke();
                    break;
                }

                var messageBuffer = new byte[messageLength];
                var totalRead = 0;
                while (totalRead < messageLength && !ct.IsCancellationRequested)
                {
                    var n = await _pipe.ReadAsync(messageBuffer, totalRead, messageLength - totalRead, ct);
                    if (n == 0) { Disconnected?.Invoke(); return; }
                    totalRead += n;
                }

                var json = Encoding.UTF8.GetString(messageBuffer, 0, messageLength);
                var data = JsonSerializer.Deserialize<TooltipData>(json);
                if (data is not null)
                    DataReceived?.Invoke(data);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UI] Pipe read loop failed: {ex}");
            Disconnected?.Invoke();
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        try
        {
            if (_readTask is not null && !_readTask.Wait(TimeSpan.FromSeconds(2)))
                Debug.WriteLine("[UI] Pipe read loop did not stop within two seconds.");
        }
        catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is OperationCanceledException))
        {
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UI] Failed while stopping pipe read loop: {ex}");
        }
        _pipe?.Dispose();
        _cts?.Dispose();
    }
}
