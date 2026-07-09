using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Benefit;
using EasyPay.Core.DTOs.Department;
using EasyPay.Core.DTOs.Designation;
using EasyPay.Core.DTOs.Employee;
using EasyPay.Core.DTOs.Leave;
using EasyPay.Core.DTOs.Payroll;
using EasyPay.Core.DTOs.Timesheet;

namespace EasyPay.Core.Interfaces.Services;

public interface IEmployeeService
{
    Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto);
    Task<EmployeeResponseDto> UpdateAsync(int employeeId, UpdateEmployeeDto dto);
    Task<EmployeeResponseDto> GetByIdAsync(int employeeId);
    Task<EmployeeResponseDto> GetByCodeAsync(string code);
    Task<PagedResponse<EmployeeListDto>> GetPagedAsync(PaginationParams pagination, int? departmentId = null, int? designationId = null);
    Task<IEnumerable<EmployeeListDto>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<EmployeeListDto>> GetByManagerAsync(int managerId);
    Task DeactivateAsync(int employeeId);
    Task ReactivateAsync(int employeeId);
}

public interface IDepartmentService
{
    Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto);
    Task<DepartmentResponseDto> UpdateAsync(int departmentId, UpdateDepartmentDto dto);
    Task<DepartmentResponseDto> GetByIdAsync(int departmentId);
    Task<IEnumerable<DepartmentResponseDto>> GetAllAsync();
    Task<IEnumerable<DepartmentResponseDto>> GetActiveAsync();
    Task DeleteAsync(int departmentId);
}

public interface IDesignationService
{
    Task<DesignationResponseDto> CreateAsync(CreateDesignationDto dto);
    Task<DesignationResponseDto> UpdateAsync(int designationId, UpdateDesignationDto dto);
    Task<DesignationResponseDto> GetByIdAsync(int designationId);
    Task<IEnumerable<DesignationResponseDto>> GetAllAsync();
    Task<IEnumerable<DesignationResponseDto>> GetByDepartmentAsync(int departmentId);
    Task DeleteAsync(int designationId);
}

public interface IPayrollPolicyService
{
    Task<PayrollPolicyResponseDto> CreateAsync(CreatePayrollPolicyDto dto);
    Task<PayrollPolicyResponseDto> UpdateAsync(int policyId, CreatePayrollPolicyDto dto);
    Task<PayrollPolicyResponseDto> GetByIdAsync(int policyId);
    Task<IEnumerable<PayrollPolicyResponseDto>> GetAllAsync();
    Task<PayrollPolicyResponseDto?> GetActiveAsync();
    Task DeactivateAsync(int policyId);
}

public interface ISalaryStructureService
{
    Task<SalaryStructureResponseDto> CreateAsync(CreateSalaryStructureDto dto);
    Task<SalaryStructureResponseDto> GetCurrentForEmployeeAsync(int employeeId);
    Task<IEnumerable<SalaryStructureResponseDto>> GetHistoryForEmployeeAsync(int employeeId);
}

public interface IPayrollService
{
    Task<PayrollResponseDto> ProcessAsync(ProcessPayrollDto dto);
    Task<IEnumerable<PayrollResponseDto>> BulkProcessAsync(BulkProcessPayrollDto dto);
    Task<PayrollResponseDto> ApproveAsync(int payrollId);
    Task<PayrollResponseDto> RejectAsync(int payrollId, string remarks);
    Task<PayrollResponseDto> MarkAsPaidAsync(int payrollId, DateOnly paymentDate);
    Task<PayrollResponseDto> GetByIdAsync(int payrollId);
    Task<PagedResponse<PayrollResponseDto>> GetPagedAsync(PaginationParams pagination,
        int? employeeId = null, string? status = null, int? year = null, int? month = null);
    Task<PayrollSummaryDto> GetSummaryAsync(DateOnly periodStart, DateOnly periodEnd);
    Task<IEnumerable<PayrollResponseDto>> GetByStatusAsync(string status);
}

public interface ILeaveService
{
    Task<LeaveRequestResponseDto> SubmitAsync(int employeeId, CreateLeaveRequestDto dto);
    Task<LeaveRequestResponseDto> ApproveOrRejectAsync(int leaveRequestId, int approverId, ApproveLeaveDto dto);
    Task<LeaveRequestResponseDto> CancelAsync(int leaveRequestId, int employeeId);
    Task<LeaveRequestResponseDto> GetByIdAsync(int leaveRequestId);
    Task<PagedResponse<LeaveRequestResponseDto>> GetPagedAsync(PaginationParams pagination,
        int? employeeId = null, string? status = null);
    Task<IEnumerable<LeaveRequestResponseDto>> GetPendingForManagerAsync(int managerId);
    Task<IEnumerable<LeaveBalanceDto>> GetBalancesAsync(int employeeId, int year);
    Task<LeaveCarryForwardDto> ProcessCarryForwardAsync(int employeeId, int fromYear);
    Task<IEnumerable<LeaveCarryForwardDto>> BulkCarryForwardAsync(int fromYear);
}

public interface ITimesheetService
{
    Task<TimesheetResponseDto> CreateAsync(int employeeId, CreateTimesheetDto dto);
    Task<TimesheetResponseDto> UpdateAsync(int timesheetId, int employeeId, UpdateTimesheetDto dto);
    Task<TimesheetResponseDto> ApproveOrRejectAsync(int timesheetId, int approverId, ApproveTimesheetDto dto);
    Task<int> BulkApproveAsync(List<int> timesheetIds, int approverId);
    Task<TimesheetResponseDto> GetByIdAsync(int timesheetId);
    Task<PagedResponse<TimesheetResponseDto>> GetPagedAsync(PaginationParams pagination,
        int? employeeId = null, DateOnly? fromDate = null, DateOnly? toDate = null);
    Task<TimesheetMonthlySummaryDto> GetMonthlySummaryAsync(int employeeId, int year, int month);
}

public interface IBenefitService
{
    Task<BenefitResponseDto> CreateAsync(CreateBenefitDto dto);
    Task<BenefitResponseDto> UpdateAsync(int benefitId, CreateBenefitDto dto);
    Task<BenefitResponseDto> GetByIdAsync(int benefitId);
    Task<IEnumerable<BenefitResponseDto>> GetAllAsync();
    Task<EmployeeBenefitResponseDto> AssignToEmployeeAsync(int employeeId, AssignBenefitDto dto);
    Task<IEnumerable<EmployeeBenefitResponseDto>> GetForEmployeeAsync(int employeeId);
    Task RemoveFromEmployeeAsync(int employeeBenefitId);
}

// Extension for IPayrollService - MarkAsPaid
// Added to IPayrollService interface below
