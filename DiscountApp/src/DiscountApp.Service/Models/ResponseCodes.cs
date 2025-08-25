namespace DiscountApp.Service.Models;

public static class ResponseCodes
{
    public const byte Success = 0x00;
    public const byte InvalidCode = 0x01;
    public const byte AlreadyUsed = 0x02;
    public const byte ServerError = 0x03;
}