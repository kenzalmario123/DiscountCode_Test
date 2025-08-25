using DiscountApp.Domain.Entities;
using DiscountApp.Persistence.Interfaces;
using DiscountApp.Service.Interfaces;
using DiscountApp.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscountApp.Service.Services;

public class DiscountCodeService(ILogger<DiscountCodeService> logger, IDiscountAppDbContext dbContext) : IDiscountCodeService
{
    private readonly Random _random = new();
    private readonly ILogger<DiscountCodeService> _logger = logger;
    private readonly IDiscountAppDbContext _dbContext = dbContext;
    private readonly HashSet<string> _codeHashList = [];

    // Initial load of codes into hashlist
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var codes = await _dbContext.DiscountCodes.Select(dc => dc.Code).ToListAsync(cancellationToken);

        foreach (var code in codes)
            _codeHashList.Add(code);

        _logger.LogInformation("Loaded {Count} discount codes into hashlist", _codeHashList.Count);
    }

    public async Task<GenerateResponse> GenerateCodes(GenerateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate request according to specs
            if (request.Count < 1 || request.Count > 2000)
            {
                _logger.LogWarning("Invalid count: {Count}. Must be between 1-2000", request.Count);
                return new GenerateResponse { Result = false };
            }

            if (request.Length < 7 || request.Length > 8)
            {
                _logger.LogWarning("Invalid length: {Length}. Must be 7 or 8", request.Length);
                return new GenerateResponse { Result = false };
            }

            var codes = GenerateCodes(request.Count, request.Length);

            await _dbContext.DiscountCodes.AddRangeAsync(codes, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var codesList = codes.ToList();
            _logger.LogInformation("Generated {Count} discount codes of length {Length}", codesList.Count, request.Length);
            return new GenerateResponse { Result = codesList.Count == request.Count, Codes = [.. codesList.Select(c => c.Code)] };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating discount codes");
            return new GenerateResponse { Result = false };
        }
    }

    public async Task<UseCodeResponse> UseCode(UseCodeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate code length
            if (string.IsNullOrEmpty(request.Code) || (request.Code.Length != 7 && request.Code.Length != 8))
            {
                _logger.LogWarning("Invalid code length: {Code}", request.Code);
                return new UseCodeResponse { Result = ResponseCodes.InvalidCode };
            }

            var discountCode = await _dbContext.DiscountCodes.Where(dc => dc.Code == request.Code).FirstOrDefaultAsync(cancellationToken);

            if (discountCode == null)
            {
                _logger.LogWarning("Invalid code: {Code}", request.Code);
                return new UseCodeResponse { Result = ResponseCodes.InvalidCode };
            }

            if (discountCode.IsUsed)
            {
                _logger.LogWarning("Code already used: {Code}", request.Code);
                return new UseCodeResponse { Result = ResponseCodes.AlreadyUsed };
            }

            discountCode.IsUsed = true;
            discountCode.UsedAt = DateTime.Now;

            _dbContext.DiscountCodes.Update(discountCode);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Discount code {Code} was successfully used", request.Code);
            return new UseCodeResponse { Result = ResponseCodes.Success };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error using discount code: {Code}", request.Code);
            return new UseCodeResponse { Result = ResponseCodes.ServerError };
        }
    }

    // Simple code generation with uniqueness check
    public IEnumerable<DiscountCode> GenerateCodes(int count, int length)
    {
        var codes = new List<string>();
        int attempts = 0;
        while (codes.Count < count && attempts < count * 10) // attempts limit to avoid infinite loop
        {
            var code = GenerateRandomCode(length);
            if (!_codeHashList.Contains(code) && !codes.Contains(code))
            {
                codes.Add(code);
                _codeHashList.Add(code);
            }
            attempts++;
        }

        return codes.Select(c => new DiscountCode { Code = c, IsUsed = false });
    }

    private string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Base32 without ambiguous characters
        var code = new char[length];

        for (int i = 0; i < length; i++)
        {
            code[i] = chars[_random.Next(chars.Length)];
        }

        return new string(code);
    }
}