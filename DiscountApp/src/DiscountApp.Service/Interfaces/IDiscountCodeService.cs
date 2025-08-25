using DiscountApp.Service.Models;

namespace DiscountApp.Service.Interfaces;

public interface IDiscountCodeService
{
    Task<GenerateResponse> GenerateCodes(GenerateRequest request, CancellationToken cancellationToken);
    Task<UseCodeResponse> UseCode(UseCodeRequest request, CancellationToken cancellationToken);
}