namespace DiscountApp.Domain.Entities;

public class DiscountCode
{
    public int DiscountCodeId { get; set; }
    public string Code { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public DateTime? UsedAt { get; set; }
}