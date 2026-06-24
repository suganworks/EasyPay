namespace EasyPay.Core.Entities;

public class Benefit
{
    public int BenefitId { get; set; }
    public string BenefitName { get; set; } = string.Empty;
    public string BenefitCode { get; set; } = string.Empty;
    public string BenefitType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; } = false;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<EmployeeBenefit> EmployeeBenefits { get; set; } = new List<EmployeeBenefit>();
}

public class EmployeeBenefit
{
    public int EmployeeBenefitId { get; set; }
    public int EmployeeId { get; set; }
    public int BenefitId { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public decimal? OverrideAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }

    public Employee Employee { get; set; } = null!;
    public Benefit Benefit { get; set; } = null!;
}
