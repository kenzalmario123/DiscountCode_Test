using System.Net.Sockets;
using System.Text;
using DiscountApp.Service.Models;

namespace DiscountApp.Service.Helpers;

public static class TcpServiceHelper
{
    public static async Task<int> ReadByteAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[1];
        int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, 1), cancellationToken);
        return bytesRead == 1 ? buffer[0] : -1;
    }

    public static async Task<GenerateRequest?> ReadGenerateRequestAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[3]; // Count (2 bytes) + Length (1 byte)
        int totalRead = 0;

        while (totalRead < 3)
        {
            int bytesRead = await stream.ReadAsync(buffer.AsMemory(totalRead, 3 - totalRead), cancellationToken);
            if (bytesRead == 0) return null;
            totalRead += bytesRead;
        }

        return new GenerateRequest
        {
            Count = BitConverter.ToUInt16(buffer, 0),
            Length = buffer[2]
        };
    }

    public static async Task<UseCodeRequest?> ReadUseCodeRequestAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[8]; // Fixed 8-byte code
        int totalRead = 0;

        while (totalRead < 8)
        {
            int bytesRead = await stream.ReadAsync(buffer.AsMemory(totalRead, 8 - totalRead), cancellationToken);
            if (bytesRead == 0) return null;
            totalRead += bytesRead;
        }

        return new UseCodeRequest
        {
            Code = Encoding.UTF8.GetString(buffer).Trim()
        };
    }

    public static byte[] CreateGenerateResponse(GenerateResponse response)
    {
        if (!response.Result || response.Codes == null)
        {
            return [0x01, 0x00]; // Result = false
        }

        using var ms = new MemoryStream();
        ms.WriteByte(0x01); // Response type
        ms.WriteByte(0x01); // Result = true

        foreach (var code in response.Codes)
        {
            var codeBytes = Encoding.UTF8.GetBytes(code.PadRight(8)[..8]);
            ms.Write(codeBytes, 0, 8);
        }

        return ms.ToArray();
    }

    public static byte[] CreateUseCodeResponse(UseCodeResponse response)
    {
        return [0x02, response.Result];
    }
}