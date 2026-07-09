namespace EasyPay.Core.Entities;

public class Employee : BaseEntity
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? PassportNumber { get; set; }
    public string? Phone { get; set; }
    public string? AlternatePhone { get; set; }
    public string? PersonalEmail { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public int DepartmentId { get; set; }
    public int DesignationId { get; set; }
    public int? ManagerId { get; set; }
    public DateOnly JoiningDate { get; set; }
    public DateOnly? ConfirmationDate { get; set; }
    public DateOnly? TerminationDate { get; set; }
    public string EmploymentType { get; set; } = "FullTime";
    public string EmploymentStatus { get; set; } = "Active";
    public string? BankName { get; set; }
    public string? BankAccountNo { get; set; }
    public string? BankIFSC { get; set; }
    public string? PanNumber { get; set; }
    public string? PfNumber { get; set; }
    public string? EsiNumber { get; set; }
    public decimal TaxWithholding { get; set; } = 0;
    public string? PhotoUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public User User { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public Designation Designation { get; set; } = null!;
    public Employee? Manager { get; set; }
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();
    public ICollection<SalaryStructure> SalaryStructures { get; set; } = new List<SalaryStructure>();
    public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<Timesheet> Timesheets { get; set; } = new List<Timesheet>();
    public ICollection<EmployeeBenefit> EmployeeBenefits { get; set; } = new List<EmployeeBenefit>();

    public string FullName => $"{FirstName} {LastName}";
}
