using System.Net.Sockets;
using System.Text;

namespace DiscountApp.Service;

public class DiscountApplication
{
    public async Task RunAsync(string[] args)
    {
        Console.WriteLine("Discount Code Service Tester");
        Console.WriteLine("=============================");

        var host = "localhost";
        var port = 8080;

        if (args.Length >= 1) host = args[0];
        if (args.Length >= 2) port = int.Parse(args[1]);

        try
        {
            // using var client = new TcpClient();
            // await client.ConnectAsync(host, port);

            Console.WriteLine($"Connected to {host}:{port}");
            Console.WriteLine("Available commands:");
            Console.WriteLine("  generate <count> <length> - Generate discount codes");
            Console.WriteLine("  use <code>               - Use a discount code");
            Console.WriteLine("  exit                     - Exit the tester");
            Console.WriteLine();

            while (true)
            {
                using var client = new TcpClient();
                await client.ConnectAsync(host, port);
                
                Console.Write("> ");
                var input = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(input))
                    continue;

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var command = parts[0].ToLower();

                try
                {
                    switch (command)
                    {
                        case "generate":
                            if (parts.Length != 3)
                            {
                                Console.WriteLine("Usage: generate <count> <length>");
                                continue;
                            }

                            var count = ushort.Parse(parts[1]);
                            var length = byte.Parse(parts[2]);

                            await GenerateCodes(client, count, length);
                            break;

                        case "use":
                            if (parts.Length != 2)
                            {
                                Console.WriteLine("Usage: use <code>");
                                continue;
                            }

                            var code = parts[1];
                            await UseCode(client, code);
                            break;

                        default:
                            Console.WriteLine("Unknown command. Available: generate, use, exit");
                            break;
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid number format");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection error: {ex.Message}");
        }
    }

    static async Task GenerateCodes(TcpClient client, ushort count, byte length)
    {
        using var stream = client.GetStream();

        // Create generate request: [0x01] + [count (2 bytes)] + [length (1 byte)]
        var request = new byte[4];
        request[0] = 0x01; // Generate request
        Buffer.BlockCopy(BitConverter.GetBytes(count), 0, request, 1, 2);
        request[3] = length;

        // Send request
        await stream.WriteAsync(request, 0, request.Length);
        Console.WriteLine($"Sent generate request: Count={count}, Length={length}");

        // Read response
        var responseType = await ReadByteAsync(stream);
        if (responseType != 0x01)
        {
            Console.WriteLine($"Unexpected response type: 0x{responseType:X2}");
            return;
        }

        var result = await ReadByteAsync(stream);
        if (result == 0x00)
        {
            Console.WriteLine("❌ Generate request failed");
            return;
        }

        // Read generated codes
        var codes = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var codeBytes = new byte[8];
            await stream.ReadAsync(codeBytes, 0, 8);
            var code = Encoding.UTF8.GetString(codeBytes).Trim();
            codes.Add(code);
        }

        Console.WriteLine("✅ Generate request successful");
        Console.WriteLine($"Generated {codes.Count} codes:");
        foreach (var code in codes)
        {
            Console.WriteLine($"  {code}");
        }
    }

    static async Task UseCode(TcpClient client, string code)
    {
        using var stream = client.GetStream();

        // Pad code to 8 characters
        var paddedCode = code.PadRight(8).Substring(0, 8);
        var codeBytes = Encoding.UTF8.GetBytes(paddedCode);

        // Create use code request: [0x02] + [code (8 bytes)]
        var request = new byte[9];
        request[0] = 0x02; // Use code request
        Buffer.BlockCopy(codeBytes, 0, request, 1, 8);

        // Send request
        await stream.WriteAsync(request, 0, request.Length);
        Console.WriteLine($"Sent use code request: Code='{paddedCode}'");

        // Read response
        var responseType = await ReadByteAsync(stream);
        if (responseType != 0x02)
        {
            Console.WriteLine($"Unexpected response type: 0x{responseType:X2}");
            return;
        }

        var resultCode = await ReadByteAsync(stream);

        switch (resultCode)
        {
            case 0x00:
                Console.WriteLine("✅ Code used successfully");
                break;
            case 0x01:
                Console.WriteLine("❌ Invalid code");
                break;
            case 0x02:
                Console.WriteLine("❌ Code already used");
                break;
            case 0x03:
                Console.WriteLine("❌ Server error");
                break;
            default:
                Console.WriteLine($"❌ Unknown result code: 0x{resultCode:X2}");
                break;
        }
    }

    static async Task<byte> ReadByteAsync(NetworkStream stream)
    {
        var buffer = new byte[1];
        await stream.ReadAsync(buffer, 0, 1);
        return buffer[0];
    }
}