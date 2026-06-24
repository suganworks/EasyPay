namespace EasyPay.Core.Entities;

public class Designation : BaseEntity
{
    public int DesignationId { get; set; }
    public string DesignationCode { get; set; } = string.Empty;
    public string DesignationName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string? GradeLevel { get; set; }
    public bool IsActive { get; set; } = true;

    public Department Department { get; set; } = null!;
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
