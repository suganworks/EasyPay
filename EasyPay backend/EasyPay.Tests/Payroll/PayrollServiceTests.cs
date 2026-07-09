using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Payroll;
using EasyPay.Core.Entities;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using EasyPay.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasyPay.Tests.Payroll;

[TestFixture]
public class PayrollServiceNUnitTests
{
    private Mock<IPayrollRepository>         _payrollRepoMock;
    private Mock<IEmployeeRepository>        _employeeRepoMock;
    private Mock<ISalaryStructureRepository> _salaryRepoMock;
    private Mock<IPayrollPolicyRepository>   _policyRepoMock;
    private Mock<ITimesheetRepository>       _timesheetRepoMock;
    private Mock<ILeaveRequestRepository>    _leaveRepoMock;
    private Mock<IEmployeeBenefitRepository> _empBenefitRepoMock;
    private Mock<IAuditService>              _auditMock;
    private Mock<ICurrentUserService>        _currentUserMock;
    private Mock<INotificationService>       _notificationMock;
    private Mock<IUserRepository>            _userRepoMock;
    private Mock<IEmailService>              _emailMock;
    private Mock<ILogger<PayrollService>>    _loggerMock;
    private PayrollService                   _sut;

    [SetUp]
    public void SetUp()
    {
        _payrollRepoMock    = new Mock<IPayrollRepository>();
        _employeeRepoMock   = new Mock<IEmployeeRepository>();
        _salaryRepoMock     = new Mock<ISalaryStructureRepository>();
        _policyRepoMock     = new Mock<IPayrollPolicyRepository>();
        _timesheetRepoMock  = new Mock<ITimesheetRepository>();
        _leaveRepoMock      = new Mock<ILeaveRequestRepository>();
        _empBenefitRepoMock = new Mock<IEmployeeBenefitRepository>();
        _auditMock          = new Mock<IAuditService>();
        _currentUserMock    = new Mock<ICurrentUserService>();
        _notificationMock   = new Mock<INotificationService>();
        _userRepoMock       = new Mock<IUserRepository>();
        _emailMock          = new Mock<IEmailService>();
        _loggerMock         = new Mock<ILogger<PayrollService>>();

        _currentUserMock.Setup(c => c.UserId).Returns(1);

        // Default — no benefits assigned (so gross = salary only)
        _empBenefitRepoMock
            .Setup(r => r.GetActiveForEmployeeAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<EasyPay.Core.Entities.EmployeeBenefit>());

        var repos = new PayrollRepositories(
            _payrollRepoMock.Object, _employeeRepoMock.Object,
            _salaryRepoMock.Object,  _policyRepoMock.Object,
            _timesheetRepoMock.Object, _leaveRepoMock.Object,
            _empBenefitRepoMock.Object
        );

        var support = new PayrollSupportServices(
            _auditMock.Object, _currentUserMock.Object,
            _notificationMock.Object, _userRepoMock.Object,
            _emailMock.Object, _loggerMock.Object
        );

        _sut = new PayrollService(repos, support);
    }

    [Test]
    public async Task ProcessAsync_WhenEmployeeNotFound_ThrowsNotFoundException()
    {
        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(999))
            .ReturnsAsync((Core.Entities.Employee?)null);

        Func<Task> act = () => _sut.ProcessAsync(BuildProcessDto(employeeId: 999));

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Employee*999*");
    }

    [Test]
    public async Task ProcessAsync_WhenEmployeeInactive_ThrowsBusinessRuleException()
    {
        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(1))
            .ReturnsAsync(BuildEmployee(isActive: false, status: "Terminated"));

        Func<Task> act = () => _sut.ProcessAsync(BuildProcessDto());

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*not active*");
    }

    [Test]
    public async Task ProcessAsync_WhenPayrollAlreadyExists_ThrowsConflictException()
    {
        var period = (new DateOnly(2024, 6, 1), new DateOnly(2024, 6, 30));
        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(BuildEmployee());
        _payrollRepoMock.Setup(r => r.PayrollExistsForPeriodAsync(1, period.Item1, period.Item2))
            .ReturnsAsync(true);

        Func<Task> act = () => _sut.ProcessAsync(BuildProcessDto(periodStart: period.Item1, periodEnd: period.Item2));

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*Payroll already exists*");
    }

    [Test]
    public async Task ProcessAsync_WhenNoSalaryStructure_ThrowsBusinessRuleException()
    {
        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(BuildEmployee());
        _payrollRepoMock.Setup(r => r.PayrollExistsForPeriodAsync(
            It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(false);
        _salaryRepoMock.Setup(r => r.GetCurrentForEmployeeAsync(1))
            .ReturnsAsync((SalaryStructure?)null);

        Func<Task> act = () => _sut.ProcessAsync(BuildProcessDto());

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*salary structure*");
    }

    [Test]
    public async Task ProcessAsync_WithFullPresentMonth_CalculatesCorrectNetSalary()
    {
        var period   = (new DateOnly(2024, 6, 1), new DateOnly(2024, 6, 30));
        var employee = BuildEmployee();
        var policy   = BuildPolicy();
        var salary   = BuildSalary(policyId: policy.PolicyId);

        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(employee);
        _payrollRepoMock.Setup(r => r.PayrollExistsForPeriodAsync(
            It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(false);
        _salaryRepoMock.Setup(r => r.GetCurrentForEmployeeAsync(1)).ReturnsAsync(salary);
        _policyRepoMock.Setup(r => r.GetByIdAsync(salary.PolicyId)).ReturnsAsync(policy);
        _timesheetRepoMock.Setup(r => r.GetTotalOvertimeAsync(
            It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(0m);
        _leaveRepoMock.Setup(r => r.GetUsedLeaveDaysAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(0);
        _leaveRepoMock.Setup(r => r.GetByEmployeeAndYearAsync(
            It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<LeaveRequest>());

        Core.Entities.Payroll? savedPayroll = null;
        _payrollRepoMock.Setup(r => r.AddAsync(It.IsAny<Core.Entities.Payroll>()))
            .ReturnsAsync((Core.Entities.Payroll p) =>
            {
                p.PayrollId = 1; p.Employee = employee;
                p.Policy = policy; p.SalaryStructure = salary;
                savedPayroll = p; return p;
            });
        _payrollRepoMock.Setup(r => r.GetWithDetailsAsync(1))
            .ReturnsAsync(() => savedPayroll!);
        _notificationMock.Setup(n => n.SendAsync(It.IsAny<int>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .Returns(Task.CompletedTask);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, It.IsAny<object>(), true, null))
            .Returns(Task.CompletedTask);

        var result = await _sut.ProcessAsync(BuildProcessDto(periodStart: period.Item1, periodEnd: period.Item2));

        result.Should().NotBeNull();
        result.BasicSalary.Should().Be(50000m);
        result.GrossEarnings.Should().BeGreaterThan(0);
        result.NetSalary.Should().BeLessThan(result.GrossEarnings);
        result.Status.Should().Be("Pending");
    }

    [Test]
    public async Task ApproveAsync_WhenPayrollNotFound_ThrowsNotFoundException()
    {
        _payrollRepoMock.Setup(r => r.GetWithDetailsAsync(999))
            .ReturnsAsync((Core.Entities.Payroll?)null);

        Func<Task> act = () => _sut.ApproveAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ApproveAsync_WhenNotPending_ThrowsBusinessRuleException()
    {
        _payrollRepoMock.Setup(r => r.GetWithDetailsAsync(1))
            .ReturnsAsync(BuildPayroll(status: "Approved"));

        Func<Task> act = () => _sut.ApproveAsync(1);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*cannot be approved*");
    }

    [Test]
    public async Task ApproveAsync_WhenPending_SetsApprovedStatus()
    {
        var payroll = BuildPayroll(status: "Pending");
        _payrollRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(payroll);
        _payrollRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Core.Entities.Payroll>()))
            .Returns(Task.CompletedTask);
        _notificationMock.Setup(n => n.SendAsync(It.IsAny<int>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .Returns(Task.CompletedTask);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, null, true, null)).Returns(Task.CompletedTask);

        var result = await _sut.ApproveAsync(1);

        result.Status.Should().Be("Approved");
        payroll.ApprovedById.Should().Be(1);
        _payrollRepoMock.Verify(r => r.UpdateAsync(payroll), Times.Once);
    }

    [Test]
    public async Task RejectAsync_WhenPending_SetsCancelledStatus()
    {
        var payroll = BuildPayroll(status: "Pending");
        _payrollRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(payroll);
        _payrollRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Core.Entities.Payroll>()))
            .Returns(Task.CompletedTask);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, It.IsAny<object>(), true, null))
            .Returns(Task.CompletedTask);

        var result = await _sut.RejectAsync(1, "Error in calculation");

        result.Status.Should().Be("Cancelled");
        result.Remarks.Should().Be("Error in calculation");
    }

    [Test]
    public async Task GetSummaryAsync_ReturnsCorrectTotals()
    {
        var start = new DateOnly(2024, 6, 1);
        var end   = new DateOnly(2024, 6, 30);
        var payrolls = new List<Core.Entities.Payroll>
        {
            new() { EmployeeId=1, PayPeriodStart=start, PayPeriodEnd=end,
                    GrossEarnings=70000m, TotalDeductions=10000m, NetSalary=60000m, Status="Approved" },
            new() { EmployeeId=2, PayPeriodStart=start, PayPeriodEnd=end,
                    GrossEarnings=50000m, TotalDeductions=8000m,  NetSalary=42000m, Status="Approved" },
        };

        _payrollRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Core.Entities.Payroll, bool>>>()))
            .ReturnsAsync(payrolls);

        var result = await _sut.GetSummaryAsync(start, end);

        result.TotalEmployees.Should().Be(2);
        result.TotalGrossEarnings.Should().Be(120000m);
        result.TotalNetSalary.Should().Be(102000m);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static ProcessPayrollDto BuildProcessDto(int employeeId = 1,
        DateOnly? periodStart = null, DateOnly? periodEnd = null) => new()
    {
        EmployeeId = employeeId,
        PayPeriodStart = periodStart ?? new DateOnly(2024, 6, 1),
        PayPeriodEnd   = periodEnd   ?? new DateOnly(2024, 6, 30)
    };

    private static Core.Entities.Employee BuildEmployee(bool isActive = true, string status = "Active")
    {
        var dept  = new Department  { DepartmentId = 1, DepartmentName = "IT" };
        var desig = new Designation { DesignationId = 1, DesignationName = "Dev" };
        var role  = new Role { RoleId = 4, RoleName = "Employee" };
        return new Core.Entities.Employee
        {
            EmployeeId = 1, EmployeeCode = "EMP00001",
            FirstName = "John", LastName = "Doe",
            IsActive = isActive, EmploymentStatus = status,
            DepartmentId = 1, Department = dept,
            DesignationId = 1, Designation = desig,
            UserId = 10, User = new User { UserId = 10, Email = "j@t.com", Role = role },
            DateOfBirth = new DateOnly(1990, 1, 1),
            JoiningDate = new DateOnly(2020, 1, 1),
            Gender = "Male", TaxWithholding = 10m
        };
    }

    private static PayrollPolicy BuildPolicy() => new()
    {
        PolicyId = 1, PolicyName = "Standard 2024",
        OvertimeRate = 1.5m, PfEmployeeRate = 12m, PfEmployerRate = 12m,
        EsiEmployeeRate = 0.75m, EsiEmployerRate = 3.25m,
        ProfessionalTax = 200m, GratuityRate = 4.81m,
        WorkingDaysMonth = 26, WorkingHoursDay = 8m, IsActive = true
    };

    private static SalaryStructure BuildSalary(int policyId = 1) => new()
    {
        SalaryStructureId = 1, EmployeeId = 1, PolicyId = policyId,
        EffectiveFrom = new DateOnly(2024, 1, 1),
        BasicSalary = 50000m, HRA = 20000m, ConveyanceAllowance = 1600m,
        MedicalAllowance = 1250m, SpecialAllowance = 5000m,
        LTA = 2000m, OtherAllowances = 0m, IsActive = true
    };

    private static Core.Entities.Payroll BuildPayroll(string status = "Pending")
    {
        var employee = new Core.Entities.Employee
        {
            EmployeeId = 1, FirstName = "John", LastName = "Doe",
            Department = new Department { DepartmentName = "IT" },
            User = new User { UserId = 10 }
        };
        return new Core.Entities.Payroll
        {
            PayrollId = 1, EmployeeId = 1, Employee = employee,
            GrossEarnings = 70000m, TotalDeductions = 10000m,
            NetSalary = 60000m, Status = status,
            PayPeriodStart = new DateOnly(2024, 6, 1),
            PayPeriodEnd   = new DateOnly(2024, 6, 30)
        };
    }
}
