using System.Net;
using System.Net.Sockets;
using System.Text;
using DiscountApp.Service.Interfaces;
using DiscountApp.Service.Models;
using static DiscountApp.Service.Helpers.TcpServiceHelper;

namespace DiscountApp.Service.Services;

public class TcpBackgroundService : BackgroundService
{
    private readonly TcpListener _listener;
    private readonly IDiscountCodeService _service;
    private readonly ILogger<TcpBackgroundService> _logger;
    private readonly CancellationTokenSource _serverCts = new();

    public TcpBackgroundService(IDiscountCodeService service, ILogger<TcpBackgroundService> logger, IConfiguration configuration)
    {
        _service = service;
        _logger = logger;
        var port = configuration.GetValue("TcpServer:Port", 8080);
        _listener = new TcpListener(IPAddress.Any, port);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _serverCts.Token);
        var cancellationToken = combinedCts.Token;

        await RunAsync(cancellationToken);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        _listener.Start();
        _logger.LogInformation("TCP Server started on port {Port}", ((IPEndPoint)_listener.LocalEndpoint).Port);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                _ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("TCP Server stopping due to cancellation request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TCP Server error occurred");
        }
        finally
        {
            _listener.Stop();
            _logger.LogInformation("TCP Server stopped");
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var clientEndPoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        _logger.LogInformation("TCP Client connected: {ClientEndPoint}", clientEndPoint);

        try
        {
            using (client)
            using (var stream = client.GetStream())
            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                var requestType = await ReadByteAsync(stream, cancellationToken);
                if (requestType == -1) break;

                byte[] response = requestType switch
                {
                    0x01 => await GenerateCodesAsync(stream, cancellationToken),
                    0x02 => await UseCodeAsync(stream, cancellationToken),
                    _ => ReturnUnknownRequest(requestType)
                };

                await stream.WriteAsync(response, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling TCP client {ClientEndPoint}", clientEndPoint);
        }

        _logger.LogInformation("TCP Client disconnected: {ClientEndPoint}", clientEndPoint);
    }

    public async Task<byte[]> GenerateCodesAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var generateRequest = await ReadGenerateRequestAsync(stream, cancellationToken);

        if (generateRequest != null)
        {
            var generateResponse = await _service.GenerateCodes(generateRequest, cancellationToken);
            return CreateGenerateResponse(generateResponse);
        }

        return CreateGenerateResponse(new GenerateResponse { Result = false });
    }

    public async Task<byte[]> UseCodeAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var useCodeRequest = await ReadUseCodeRequestAsync(stream, cancellationToken);
        if (useCodeRequest != null)
        {
            var useCodeResponse = await _service.UseCode(useCodeRequest, cancellationToken);
            return CreateUseCodeResponse(useCodeResponse);
        }

        return CreateUseCodeResponse(new UseCodeResponse { Result = ResponseCodes.ServerError });
    }

    public byte[] ReturnUnknownRequest(int requestType)
    {
        _logger.LogWarning("Unknown request type: {RequestType}", requestType);
        return [0xFF]; // Error response
    }

    public override void Dispose()
    {
        _serverCts.Cancel();
        _listener?.Stop();
        base.Dispose();
        _logger.LogInformation("TcpBackgroundService disposed");
    }
}