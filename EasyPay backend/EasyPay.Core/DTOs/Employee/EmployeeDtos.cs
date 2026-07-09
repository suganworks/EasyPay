using System.ComponentModel.DataAnnotations;

namespace EasyPay.Core.DTOs.Employee;

// ─── Employee DTOs ────────────────────────────────────────────────────────────

public class CreateEmployeeDto
{
    [Required, StringLength(100)] public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(100)] public string LastName  { get; set; } = string.Empty;
    [Required] public DateOnly DateOfBirth { get; set; }
    [Required, StringLength(20)]  public string Gender { get; set; } = string.Empty;
    [StringLength(50)]            public string? NationalId { get; set; }
    [Phone, StringLength(20)]     public string? Phone { get; set; }
    [EmailAddress, StringLength(255)] public string? PersonalEmail { get; set; }
    [StringLength(500)]           public string? Address { get; set; }
    [StringLength(100)]           public string? City { get; set; }
    [StringLength(100)]           public string? State { get; set; }
    [StringLength(20)]            public string? PostalCode { get; set; }
    [StringLength(100)]           public string? Country { get; set; }
    [Required] public int DepartmentId { get; set; }
    [Required] public int DesignationId { get; set; }
    public int? ManagerId { get; set; }
    [Required] public DateOnly JoiningDate { get; set; }
    [Required, StringLength(50)] public string EmploymentType { get; set; } = "FullTime";
    [StringLength(100)] public string? BankName { get; set; }
    [StringLength(50)]  public string? BankAccountNo { get; set; }
    [StringLength(20)]  public string? BankIFSC { get; set; }
    [StringLength(20)]  public string? PanNumber { get; set; }

    // User account details
    [Required, EmailAddress, StringLength(255)] public string WorkEmail { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 8)] public string Password { get; set; } = string.Empty;
}

public class UpdateEmployeeDto
{
    [Required, StringLength(100)] public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(100)] public string LastName  { get; set; } = string.Empty;
    [Phone, StringLength(20)]     public string? Phone { get; set; }
    [StringLength(20)]            public string? AlternatePhone { get; set; }
    [EmailAddress, StringLength(255)] public string? PersonalEmail { get; set; }
    [StringLength(500)]           public string? Address { get; set; }
    [StringLength(100)]           public string? City { get; set; }
    [StringLength(100)]           public string? State { get; set; }
    [StringLength(20)]            public string? PostalCode { get; set; }
    [StringLength(100)]           public string? Country { get; set; }
    [Required] public int DepartmentId { get; set; }
    [Required] public int DesignationId { get; set; }
    public int? ManagerId { get; set; }
    [Required, StringLength(50)] public string EmploymentType { get; set; } = "FullTime";
    [Required, StringLength(50)] public string EmploymentStatus { get; set; } = "Active";
    [StringLength(100)] public string? BankName { get; set; }
    [StringLength(50)]  public string? BankAccountNo { get; set; }
    [StringLength(20)]  public string? BankIFSC { get; set; }
    [StringLength(20)]  public string? PanNumber { get; set; }
    [StringLength(50)]  public string? PfNumber { get; set; }
    [StringLength(50)]  public string? EsiNumber { get; set; }
    public decimal TaxWithholding { get; set; }
    public DateOnly? ConfirmationDate { get; set; }
    public DateOnly? TerminationDate { get; set; }
}

public class EmployeeResponseDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? PersonalEmail { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int DesignationId { get; set; }
    public string DesignationName { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public DateOnly JoiningDate { get; set; }
    public DateOnly? ConfirmationDate { get; set; }
    public DateOnly? TerminationDate { get; set; }
    public string EmploymentType { get; set; } = string.Empty;
    public string EmploymentStatus { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? PanNumber { get; set; }
    public string? PfNumber { get; set; }
    public string WorkEmail { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EmployeeListDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string DesignationName { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string EmploymentStatus { get; set; } = string.Empty;
    public string WorkEmail { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateOnly JoiningDate { get; set; }
    public bool IsActive { get; set; }
}
