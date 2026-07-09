using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Payroll;
using EasyPay.Core.Entities;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace EasyPay.Infrastructure.Services;

// ─── PayrollPolicyService ─────────────────────────────────────────────────────
public class PayrollPolicyService : IPayrollPolicyService
{
    private readonly IPayrollPolicyRepository _repo;
    private readonly IAuditService _auditService;

    public PayrollPolicyService(IPayrollPolicyRepository repo, IAuditService auditService)
    {
        _repo         = repo;
        _auditService = auditService;
    }

    public async Task<PayrollPolicyResponseDto> CreateAsync(CreatePayrollPolicyDto dto)
    {
        var policy = new PayrollPolicy
        {
            PolicyName       = dto.PolicyName,
            EffectiveFrom    = dto.EffectiveFrom,
            EffectiveTo      = dto.EffectiveTo,
            PayFrequency     = dto.PayFrequency,
            OvertimeRate     = dto.OvertimeRate,
            PfEmployeeRate   = dto.PfEmployeeRate,
            PfEmployerRate   = dto.PfEmployerRate,
            EsiEmployeeRate  = dto.EsiEmployeeRate,
            EsiEmployerRate  = dto.EsiEmployerRate,
            ProfessionalTax  = dto.ProfessionalTax,
            GratuityRate     = dto.GratuityRate,
            WorkingDaysMonth = dto.WorkingDaysMonth,
            WorkingHoursDay  = dto.WorkingHoursDay,
            Description      = dto.Description,
            IsActive         = true
        };

        await _repo.AddAsync(policy);
        await _auditService.LogAsync("Create", "PayrollPolicy", policy.PolicyId.ToString(), newValues: dto);
        return MapToResponse(policy);
    }

    public async Task<PayrollPolicyResponseDto> UpdateAsync(int policyId, CreatePayrollPolicyDto dto)
    {
        var policy = await _repo.GetByIdAsync(policyId)
            ?? throw new NotFoundException("PayrollPolicy", policyId);

        policy.PolicyName       = dto.PolicyName;
        policy.EffectiveFrom    = dto.EffectiveFrom;
        policy.EffectiveTo      = dto.EffectiveTo;
        policy.PayFrequency     = dto.PayFrequency;
        policy.OvertimeRate     = dto.OvertimeRate;
        policy.PfEmployeeRate   = dto.PfEmployeeRate;
        policy.PfEmployerRate   = dto.PfEmployerRate;
        policy.EsiEmployeeRate  = dto.EsiEmployeeRate;
        policy.EsiEmployerRate  = dto.EsiEmployerRate;
        policy.ProfessionalTax  = dto.ProfessionalTax;
        policy.GratuityRate     = dto.GratuityRate;
        policy.WorkingDaysMonth = dto.WorkingDaysMonth;
        policy.WorkingHoursDay  = dto.WorkingHoursDay;
        policy.Description      = dto.Description;

        await _repo.UpdateAsync(policy);
        await _auditService.LogAsync("Update", "PayrollPolicy", policyId.ToString(), newValues: dto);
        return MapToResponse(policy);
    }

    public async Task<PayrollPolicyResponseDto> GetByIdAsync(int policyId)
    {
        var policy = await _repo.GetByIdAsync(policyId)
            ?? throw new NotFoundException("PayrollPolicy", policyId);
        return MapToResponse(policy);
    }

    public async Task<IEnumerable<PayrollPolicyResponseDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return items.Select(MapToResponse);
    }

    public async Task<PayrollPolicyResponseDto?> GetActiveAsync()
    {
        var policy = await _repo.GetActivePolicyAsync();
        return policy == null ? null : MapToResponse(policy);
    }

    public async Task DeactivateAsync(int policyId)
    {
        var policy = await _repo.GetByIdAsync(policyId)
            ?? throw new NotFoundException("PayrollPolicy", policyId);
        policy.IsActive    = false;
        policy.EffectiveTo = DateOnly.FromDateTime(DateTime.UtcNow);
        await _repo.UpdateAsync(policy);
        await _auditService.LogAsync("Deactivate", "PayrollPolicy", policyId.ToString());
    }

    private static PayrollPolicyResponseDto MapToResponse(PayrollPolicy p) => new()
    {
        PolicyId        = p.PolicyId,
        PolicyName      = p.PolicyName,
        EffectiveFrom   = p.EffectiveFrom,
        EffectiveTo     = p.EffectiveTo,
        PayFrequency    = p.PayFrequency,
        OvertimeRate    = p.OvertimeRate,
        PfEmployeeRate  = p.PfEmployeeRate,
        PfEmployerRate  = p.PfEmployerRate,
        EsiEmployeeRate = p.EsiEmployeeRate,
        EsiEmployerRate = p.EsiEmployerRate,
        ProfessionalTax = p.ProfessionalTax,
        GratuityRate    = p.GratuityRate,
        WorkingDaysMonth= p.WorkingDaysMonth,
        WorkingHoursDay = p.WorkingHoursDay,
        IsActive        = p.IsActive,
        Description     = p.Description
    };
}

// ─── SalaryStructureService ───────────────────────────────────────────────────
public class SalaryStructureService : ISalaryStructureService
{
    private readonly ISalaryStructureRepository _repo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IPayrollPolicyRepository _policyRepo;
    private readonly IAuditService _auditService;

    public SalaryStructureService(
        ISalaryStructureRepository repo,
        IEmployeeRepository employeeRepo,
        IPayrollPolicyRepository policyRepo,
        IAuditService auditService)
    {
        _repo         = repo;
        _employeeRepo = employeeRepo;
        _policyRepo   = policyRepo;
        _auditService = auditService;
    }

    public async Task<SalaryStructureResponseDto> CreateAsync(CreateSalaryStructureDto dto)
    {
        var employee = await _employeeRepo.GetWithDetailsAsync(dto.EmployeeId)
            ?? throw new NotFoundException("Employee", dto.EmployeeId);

        var policy = await _policyRepo.GetByIdAsync(dto.PolicyId)
            ?? throw new NotFoundException("PayrollPolicy", dto.PolicyId);

        // Deactivate any existing active structure
        await _repo.DeactivatePreviousAsync(dto.EmployeeId);

        var structure = new SalaryStructure
        {
            EmployeeId           = dto.EmployeeId,
            PolicyId             = dto.PolicyId,
            EffectiveFrom        = dto.EffectiveFrom,
            EffectiveTo          = dto.EffectiveTo,
            BasicSalary          = dto.BasicSalary,
            HRA                  = dto.HRA,
            ConveyanceAllowance  = dto.ConveyanceAllowance,
            MedicalAllowance     = dto.MedicalAllowance,
            SpecialAllowance     = dto.SpecialAllowance,
            LTA                  = dto.LTA,
            OtherAllowances      = dto.OtherAllowances,
            IsActive             = true
        };

        await _repo.AddAsync(structure);
        await _auditService.LogAsync("Create", "SalaryStructure",
            structure.SalaryStructureId.ToString(), newValues: dto);

        return MapToResponse(structure, employee, policy);
    }

    public async Task<SalaryStructureResponseDto> GetCurrentForEmployeeAsync(int employeeId)
    {
        var structure = await _repo.GetCurrentForEmployeeAsync(employeeId)
            ?? throw new NotFoundException($"No active salary structure found for employee {employeeId}.");
        return MapToResponse(structure, structure.Employee, structure.Policy);
    }

    public async Task<IEnumerable<SalaryStructureResponseDto>> GetHistoryForEmployeeAsync(int employeeId)
    {
        _ = await _employeeRepo.GetByIdAsync(employeeId)
            ?? throw new NotFoundException("Employee", employeeId);

        var history = await _repo.GetHistoryForEmployeeAsync(employeeId);
        return history.Select(s => MapToResponse(s, s.Employee, s.Policy));
    }

    private static SalaryStructureResponseDto MapToResponse(
        SalaryStructure s, EasyPay.Core.Entities.Employee? emp, PayrollPolicy? pol) => new()
    {
        SalaryStructureId   = s.SalaryStructureId,
        EmployeeId          = s.EmployeeId,
        EmployeeName        = emp?.FullName ?? string.Empty,
        EmployeeCode        = emp?.EmployeeCode ?? string.Empty,
        PolicyId            = s.PolicyId,
        PolicyName          = pol?.PolicyName ?? string.Empty,
        EffectiveFrom       = s.EffectiveFrom,
        EffectiveTo         = s.EffectiveTo,
        BasicSalary         = s.BasicSalary,
        HRA                 = s.HRA,
        ConveyanceAllowance = s.ConveyanceAllowance,
        MedicalAllowance    = s.MedicalAllowance,
        SpecialAllowance    = s.SpecialAllowance,
        LTA                 = s.LTA,
        OtherAllowances     = s.OtherAllowances,
        GrossSalary         = s.GrossSalary,
        IsActive            = s.IsActive
    };
}

// ─── PayrollService ───────────────────────────────────────────────────────────
public class PayrollService : IPayrollService
{
    private readonly IPayrollRepository _payrollRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly ISalaryStructureRepository _salaryRepo;
    private readonly IPayrollPolicyRepository _policyRepo;
    private readonly ITimesheetRepository _timesheetRepo;
    private readonly ILeaveRequestRepository _leaveRepo;
    private readonly IEmployeeBenefitRepository _empBenefitRepo;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepo;
    private readonly IEmailService _emailService;
    private readonly ILogger<PayrollService> _logger;

    public PayrollService(
        IPayrollRepository payrollRepo,
        IEmployeeRepository employeeRepo,
        ISalaryStructureRepository salaryRepo,
        IPayrollPolicyRepository policyRepo,
        ITimesheetRepository timesheetRepo,
        ILeaveRequestRepository leaveRepo,
        IEmployeeBenefitRepository empBenefitRepo,
        IAuditService auditService,
        ICurrentUserService currentUser,
        INotificationService notificationService,
        IUserRepository userRepo,
        IEmailService emailService,
        ILogger<PayrollService> logger)
    {
        _payrollRepo         = payrollRepo;
        _employeeRepo        = employeeRepo;
        _salaryRepo          = salaryRepo;
        _policyRepo          = policyRepo;
        _timesheetRepo       = timesheetRepo;
        _leaveRepo           = leaveRepo;
        _empBenefitRepo      = empBenefitRepo;
        _auditService        = auditService;
        _currentUser         = currentUser;
        _notificationService = notificationService;
        _userRepo            = userRepo;
        _emailService        = emailService;
        _logger              = logger;
    }

    public async Task<PayrollResponseDto> ProcessAsync(ProcessPayrollDto dto)
    {
        var employee = await _employeeRepo.GetWithDetailsAsync(dto.EmployeeId)
            ?? throw new NotFoundException("Employee", dto.EmployeeId);

        if (!employee.IsActive || employee.EmploymentStatus != "Active")
            throw new BusinessRuleException($"Employee {employee.EmployeeCode} is not active.");

        if (await _payrollRepo.PayrollExistsForPeriodAsync(
            dto.EmployeeId, dto.PayPeriodStart, dto.PayPeriodEnd))
            throw new ConflictException("Payroll already exists for this employee and pay period.");

        var salary = await _salaryRepo.GetCurrentForEmployeeAsync(dto.EmployeeId)
            ?? throw new BusinessRuleException($"No active salary structure for employee {employee.EmployeeCode}.");

        var policy = await _policyRepo.GetByIdAsync(salary.PolicyId)
            ?? throw new NotFoundException("PayrollPolicy", salary.PolicyId);

        // ─── Calculate attendance ──────────────────────────────────────────────
        var totalOvertimeHours = await _timesheetRepo.GetTotalOvertimeAsync(
            dto.EmployeeId, dto.PayPeriodStart, dto.PayPeriodEnd);

        var leaveDays = await _leaveRepo.GetUsedLeaveDaysAsync(
            dto.EmployeeId,
            leaveTypeId: 0, // 0 = all types; we sum below
            year: dto.PayPeriodStart.Year);

        // Paid leave days in period
        var approvedLeaves = await _leaveRepo.GetByEmployeeAndYearAsync(
            dto.EmployeeId, dto.PayPeriodStart.Year);
        var leaveDaysInPeriod = approvedLeaves
            .Where(l => l.Status == "Approved" &&
                        l.FromDate >= dto.PayPeriodStart &&
                        l.ToDate   <= dto.PayPeriodEnd)
            .Sum(l => l.TotalDays);

        var workingDays  = policy.WorkingDaysMonth;
        var presentDays  = workingDays - leaveDaysInPeriod;
        presentDays      = Math.Max(presentDays, 0);

        // ─── Per-day salary for LOP calculation ───────────────────────────────
        var perDaySalary  = salary.BasicSalary / workingDays;

        // ─── Earnings ─────────────────────────────────────────────────────────
        var basicEarned    = perDaySalary * presentDays;
        var hraEarned      = salary.HRA / workingDays * presentDays;
        var convEarned     = salary.ConveyanceAllowance / workingDays * presentDays;
        var medEarned      = salary.MedicalAllowance / workingDays * presentDays;
        var specialEarned  = salary.SpecialAllowance / workingDays * presentDays;
        var otherEarned    = salary.OtherAllowances / workingDays * presentDays;

        var hourlyRate     = salary.BasicSalary / (workingDays * (decimal)policy.WorkingHoursDay);
        var overtimePay    = hourlyRate * policy.OvertimeRate * totalOvertimeHours;

        // ─── Include Employee Benefits in Gross ───────────────────────────────
        var employeeBenefits = await _empBenefitRepo.GetActiveForEmployeeAsync(dto.EmployeeId);
        var benefitsTotal    = employeeBenefits.Sum(eb =>
        {
            var amount = eb.OverrideAmount ?? eb.Benefit?.Amount ?? 0;
            return eb.Benefit?.IsPercentage == true
                ? Math.Round(basicEarned * amount / 100, 2)
                : amount;
        });

        var grossEarnings  = basicEarned + hraEarned + convEarned + medEarned
                             + specialEarned + otherEarned + overtimePay
                             + benefitsTotal + dto.BonusAmount;

        // ─── Deductions ───────────────────────────────────────────────────────
        var pfEmployee     = Math.Round(basicEarned * policy.PfEmployeeRate / 100, 2);
        var pfEmployer     = Math.Round(basicEarned * policy.PfEmployerRate  / 100, 2);
        var esiEmployee    = grossEarnings <= 21000
            ? Math.Round(grossEarnings * policy.EsiEmployeeRate / 100, 2) : 0;
        var esiEmployer    = grossEarnings <= 21000
            ? Math.Round(grossEarnings * policy.EsiEmployerRate  / 100, 2) : 0;
        var profTax        = policy.ProfessionalTax;
        var incomeTax      = Math.Round(grossEarnings * employee.TaxWithholding / 100, 2);
        var totalDeductions= pfEmployee + esiEmployee + profTax + incomeTax + dto.OtherDeductions;
        var netSalary      = grossEarnings - totalDeductions;

        var payroll = new Payroll
        {
            EmployeeId        = dto.EmployeeId,
            PolicyId          = policy.PolicyId,
            SalaryStructureId = salary.SalaryStructureId,
            PayPeriodStart    = dto.PayPeriodStart,
            PayPeriodEnd      = dto.PayPeriodEnd,
            WorkingDays       = workingDays,
            PresentDays       = presentDays,
            LeaveDays         = leaveDaysInPeriod,
            OvertimeHours     = totalOvertimeHours,
            BasicSalary       = Math.Round(basicEarned,   2),
            HRA               = Math.Round(hraEarned,     2),
            ConveyanceAllow   = Math.Round(convEarned,    2),
            MedicalAllow      = Math.Round(medEarned,     2),
            SpecialAllow      = Math.Round(specialEarned, 2),
            OtherAllowances   = Math.Round(otherEarned,   2),
            OvertimePay       = Math.Round(overtimePay,   2),
            BonusAmount       = Math.Round(dto.BonusAmount, 2),
            GrossEarnings     = Math.Round(grossEarnings, 2),
            PfEmployee        = pfEmployee,
            PfEmployer        = pfEmployer,
            EsiEmployee       = esiEmployee,
            EsiEmployer       = esiEmployer,
            ProfessionalTax   = profTax,
            IncomeTax         = incomeTax,
            OtherDeductions   = Math.Round(dto.OtherDeductions, 2),
            TotalDeductions   = Math.Round(totalDeductions, 2),
            NetSalary         = Math.Round(netSalary, 2),
            Status            = "Pending",
            ProcessedById     = _currentUser.UserId,
            ProcessedAt       = DateTime.UtcNow,
            Remarks           = dto.Remarks,
            CreatedBy         = _currentUser.UserId,
            UpdatedBy         = _currentUser.UserId
        };

        await _payrollRepo.AddAsync(payroll);

        // Send notification to employee
        await _notificationService.SendAsync(
            employee.UserId,
            "Payroll Processed",
            $"Your payroll for {dto.PayPeriodStart:MMM yyyy} has been processed. Net Salary: ₹{netSalary:N2}",
            "Info", "Payroll", payroll.PayrollId);

        await _auditService.LogAsync("ProcessPayroll", "Payroll", payroll.PayrollId.ToString(),
            newValues: new { payroll.EmployeeId, payroll.NetSalary, payroll.Status });

        _logger.LogInformation("Payroll processed for {Code}: Net={Net}",
            employee.EmployeeCode, netSalary);

        return await GetByIdAsync(payroll.PayrollId);
    }

    public async Task<IEnumerable<PayrollResponseDto>> BulkProcessAsync(BulkProcessPayrollDto dto)
    {
        IEnumerable<int> employeeIds;

        if (dto.EmployeeIds?.Any() == true)
        {
            employeeIds = dto.EmployeeIds;
        }
        else
        {
            var allActive = await _employeeRepo.FindAsync(
                e => e.IsActive && e.EmploymentStatus == "Active");
            employeeIds = allActive.Select(e => e.EmployeeId);
        }

        var results = new List<PayrollResponseDto>();
        foreach (var empId in employeeIds)
        {
            try
            {
                var singleDto = new ProcessPayrollDto
                {
                    EmployeeId    = empId,
                    PayPeriodStart = dto.PayPeriodStart,
                    PayPeriodEnd   = dto.PayPeriodEnd,
                    Remarks        = dto.Remarks
                };
                var result = await ProcessAsync(singleDto);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process payroll for EmployeeId {Id}", empId);
            }
        }

        return results;
    }

    public async Task<PayrollResponseDto> ApproveAsync(int payrollId)
    {
        var payroll = await _payrollRepo.GetWithDetailsAsync(payrollId)
            ?? throw new NotFoundException("Payroll", payrollId);

        if (payroll.Status != "Pending")
            throw new BusinessRuleException($"Payroll cannot be approved in '{payroll.Status}' status.");

        payroll.Status       = "Approved";
        payroll.ApprovedById = _currentUser.UserId;
        payroll.ApprovedAt   = DateTime.UtcNow;
        payroll.UpdatedBy    = _currentUser.UserId;

        await _payrollRepo.UpdateAsync(payroll);

        await _notificationService.SendAsync(
            payroll.Employee.UserId,
            "Payroll Approved",
            $"Your payroll for {payroll.PayPeriodStart:MMM yyyy} has been approved.",
            "Success", "Payroll", payrollId);

        await _auditService.LogAsync("ApprovePayroll", "Payroll", payrollId.ToString());
        return MapToResponse(payroll);
    }

    public async Task<PayrollResponseDto> RejectAsync(int payrollId, string remarks)
    {
        var payroll = await _payrollRepo.GetWithDetailsAsync(payrollId)
            ?? throw new NotFoundException("Payroll", payrollId);

        if (payroll.Status != "Pending")
            throw new BusinessRuleException($"Payroll cannot be rejected in '{payroll.Status}' status.");

        payroll.Status    = "Cancelled";
        payroll.Remarks   = remarks;
        payroll.UpdatedBy = _currentUser.UserId;

        await _payrollRepo.UpdateAsync(payroll);
        await _auditService.LogAsync("RejectPayroll", "Payroll", payrollId.ToString(),
            newValues: new { Remarks = remarks });

        return MapToResponse(payroll);
    }

    public async Task<PayrollResponseDto> GetByIdAsync(int payrollId)
    {
        var payroll = await _payrollRepo.GetWithDetailsAsync(payrollId)
            ?? throw new NotFoundException("Payroll", payrollId);
        return MapToResponse(payroll);
    }

    public async Task<PagedResponse<PayrollResponseDto>> GetPagedAsync(
        PaginationParams pagination, int? employeeId = null, string? status = null,
        int? year = null, int? month = null)
    {
        var (items, total) = await _payrollRepo.GetPagedAsync(
            pagination, employeeId, status, year, month);
        return new PagedResponse<PayrollResponseDto>
        {
            Data       = items.Select(MapToResponse),
            TotalCount = total,
            PageNumber = pagination.PageNumber,
            PageSize   = pagination.PageSize
        };
    }

    public async Task<PayrollSummaryDto> GetSummaryAsync(DateOnly periodStart, DateOnly periodEnd)
    {
        var payrolls = await _payrollRepo.FindAsync(p =>
            p.PayPeriodStart == periodStart && p.PayPeriodEnd == periodEnd);

        var list = payrolls.ToList();
        return new PayrollSummaryDto
        {
            TotalEmployees    = list.Count,
            TotalGrossEarnings= list.Sum(p => p.GrossEarnings),
            TotalDeductions   = list.Sum(p => p.TotalDeductions),
            TotalNetSalary    = list.Sum(p => p.NetSalary),
            PayPeriod         = $"{periodStart:MMM dd} - {periodEnd:MMM dd, yyyy}",
            Status            = list.Any() ? list.First().Status : "N/A"
        };
    }

    public async Task<PayrollResponseDto> MarkAsPaidAsync(int payrollId, DateOnly paymentDate)
    {
        var payroll = await _payrollRepo.GetWithDetailsAsync(payrollId)
            ?? throw new NotFoundException("Payroll", payrollId);

        if (payroll.Status != "Approved")
            throw new BusinessRuleException($"Only approved payrolls can be marked as Paid. Current status: '{payroll.Status}'.");

        payroll.Status      = "Paid";
        payroll.PaymentDate = paymentDate;
        payroll.UpdatedBy   = _currentUser.UserId;

        await _payrollRepo.UpdateAsync(payroll);

        await _notificationService.SendAsync(
            payroll.Employee.UserId,
            "Salary Credited",
            $"Your salary for {payroll.PayPeriodStart:MMM yyyy} of ₹{payroll.NetSalary:N2} has been credited on {paymentDate:dd MMM yyyy}.",
            "Success", "Payroll", payrollId);

        try
        {
            var user = await _userRepo.GetByIdAsync(payroll.Employee.UserId);
            if (user != null)
            {
                var payPeriod = $"{payroll.PayPeriodStart:MMM yyyy}";
                await _emailService.SendPayslipEmailAsync(user.Email, payroll.Employee.FirstName, payPeriod, payroll.NetSalary);
                _logger.LogInformation("Payslip email sent to {Email} for period {Period}", user.Email, payPeriod);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payslip email for PayrollId {Id}", payrollId);
        }

        await _auditService.LogAsync("MarkAsPaid", "Payroll", payrollId.ToString(),
            newValues: new { payroll.Status, PaymentDate = paymentDate });

        return MapToResponse(payroll);
    }

    public async Task<IEnumerable<PayrollResponseDto>> GetByStatusAsync(string status)
    {
        var items = await _payrollRepo.GetByStatusAsync(status);
        return items.Select(MapToResponse);
    }

    private static PayrollResponseDto MapToResponse(Payroll p) => new()
    {
        PayrollId        = p.PayrollId,
        EmployeeId       = p.EmployeeId,
        EmployeeName     = p.Employee?.FullName ?? string.Empty,
        EmployeeCode     = p.Employee?.EmployeeCode ?? string.Empty,
        DepartmentName   = p.Employee?.Department?.DepartmentName ?? string.Empty,
        PayPeriodStart   = p.PayPeriodStart,
        PayPeriodEnd     = p.PayPeriodEnd,
        PaymentDate      = p.PaymentDate,
        WorkingDays      = p.WorkingDays,
        PresentDays      = p.PresentDays,
        LeaveDays        = p.LeaveDays,
        OvertimeHours    = p.OvertimeHours,
        BasicSalary      = p.BasicSalary,
        HRA              = p.HRA,
        ConveyanceAllow  = p.ConveyanceAllow,
        MedicalAllow     = p.MedicalAllow,
        SpecialAllow     = p.SpecialAllow,
        OtherAllowances  = p.OtherAllowances,
        OvertimePay      = p.OvertimePay,
        BonusAmount      = p.BonusAmount,
        GrossEarnings    = p.GrossEarnings,
        PfEmployee       = p.PfEmployee,
        PfEmployer       = p.PfEmployer,
        EsiEmployee      = p.EsiEmployee,
        EsiEmployer      = p.EsiEmployer,
        ProfessionalTax  = p.ProfessionalTax,
        IncomeTax        = p.IncomeTax,
        OtherDeductions  = p.OtherDeductions,
        TotalDeductions  = p.TotalDeductions,
        NetSalary        = p.NetSalary,
        Status           = p.Status,
        ProcessedAt      = p.ProcessedAt,
        ApprovedAt       = p.ApprovedAt,
        Remarks          = p.Remarks
    };
}
