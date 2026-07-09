using System.ComponentModel.DataAnnotations;

namespace EasyPay.Core.DTOs.Benefit;

public class CreateBenefitDto
{
    [Required, StringLength(100)] public string BenefitName { get; set; } = string.Empty;
    [Required, StringLength(20)]  public string BenefitCode { get; set; } = string.Empty;
    [Required, StringLength(50)]  public string BenefitType { get; set; } = string.Empty;
    [Range(0, double.MaxValue)]   public decimal Amount { get; set; }
    public bool IsPercentage { get; set; }
    [StringLength(500)] public string? Description { get; set; }
}

public class AssignBenefitDto
{
    [Required] public int BenefitId { get; set; }
    [Required] public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    [Range(0, double.MaxValue)] public decimal? OverrideAmount { get; set; }
}

public class BenefitResponseDto
{
    public int BenefitId { get; set; }
    public string BenefitName { get; set; } = string.Empty;
    public string BenefitCode { get; set; } = string.Empty;
    public string BenefitType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class EmployeeBenefitResponseDto
{
    public int EmployeeBenefitId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int BenefitId { get; set; }
    public string BenefitName { get; set; } = string.Empty;
    public string BenefitType { get; set; } = string.Empty;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public decimal? OverrideAmount { get; set; }
    public decimal EffectiveAmount { get; set; }
    public bool IsActive { get; set; }
}
