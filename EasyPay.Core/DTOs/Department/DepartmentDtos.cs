using System.ComponentModel.DataAnnotations;

namespace EasyPay.Core.DTOs.Department;

public class CreateDepartmentDto
{
    [Required, StringLength(20)]  public string DepartmentCode { get; set; } = string.Empty;
    [Required, StringLength(100)] public string DepartmentName { get; set; } = string.Empty;
    [StringLength(500)]           public string? Description { get; set; }
    public int? ManagerUserId { get; set; }
}

public class UpdateDepartmentDto
{
    [Required, StringLength(100)] public string DepartmentName { get; set; } = string.Empty;
    [StringLength(500)]           public string? Description { get; set; }
    public int? ManagerUserId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DepartmentResponseDto
{
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ManagerUserId { get; set; }
    public string? ManagerName { get; set; }
    public int EmployeeCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
