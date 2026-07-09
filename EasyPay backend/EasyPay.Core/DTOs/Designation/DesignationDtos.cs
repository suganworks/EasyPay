using System.ComponentModel.DataAnnotations;

namespace EasyPay.Core.DTOs.Designation;

public class CreateDesignationDto
{
    [Required, StringLength(20)]  public string DesignationCode { get; set; } = string.Empty;
    [Required, StringLength(100)] public string DesignationName { get; set; } = string.Empty;
    [Required] public int DepartmentId { get; set; }
    [StringLength(20)] public string? GradeLevel { get; set; }
}

public class UpdateDesignationDto
{
    [Required, StringLength(100)] public string DesignationName { get; set; } = string.Empty;
    [Required] public int DepartmentId { get; set; }
    [StringLength(20)] public string? GradeLevel { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DesignationResponseDto
{
    public int DesignationId { get; set; }
    public string DesignationCode { get; set; } = string.Empty;
    public string DesignationName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? GradeLevel { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
