using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Models;

namespace TooltipAI.Platform.Mac.Services;

/// <summary>
/// macOS IPC using Unix Domain Sockets (replaces Named Pipes from Windows).
/// Sends TooltipData from Service to UI process.
/// </summary>
public sealed class MacNamedPipeService : IDisposable
{
    private readonly string _socketPath;
    private readonly ILogger<MacNamedPipeService> _logger;
    private UnixDomainSocketServer? _server;
    private bool _disposed;

    public MacNamedPipeService(ILogger<MacNamedPipeService> logger)
    {
        _logger = logger;
        _socketPath = Path.Combine(Path.GetTempPath(), "tooltipai.sock");
    }

    public void Start()
    {
        if (File.Exists(_socketPath))
            File.Delete(_socketPath);

        _server = new UnixDomainSocketServer(_socketPath);
        _logger.LogInformation("Unix socket server started at {Path}", _socketPath);
    }

    public async Task SendTooltipDataAsync(TooltipData data)
    {
        if (_server == null) return;

        try
        {
            var json = JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _server.SendAsync(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send tooltip data");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _server?.Dispose();
            if (File.Exists(_socketPath))
                File.Delete(_socketPath);
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    private sealed class UnixDomainSocketServer : IDisposable
    {
        private readonly Socket _socket;
        private readonly string _socketPath;

        public UnixDomainSocketServer(string socketPath)
        {
            _socketPath = socketPath;
            _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unix);

            var endpoint = new UnixDomainSocketEndPoint(socketPath);
            _socket.Bind(endpoint);
            _socket.Listen(1);
        }

        public async Task SendAsync(byte[] data)
        {
            var client = await _socket.AcceptAsync();
            try
            {
                await client.SendAsync(data, SocketFlags.None);
            }
            finally
            {
                client.Close();
            }
        }

        public void Dispose()
        {
            _socket.Close();
            _socket.Dispose();
        }
    }
}
