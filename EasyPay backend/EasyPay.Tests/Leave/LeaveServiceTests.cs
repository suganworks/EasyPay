using EasyPay.Core.DTOs.Leave;
using EasyPay.Core.Entities;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using EasyPay.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasyPay.Tests.Leave;

[TestFixture]
public class LeaveServiceNUnitTests
{
    private Mock<ILeaveRequestRepository> _leaveRepoMock;
    private Mock<ILeaveTypeRepository>    _leaveTypeRepoMock;
    private Mock<IEmployeeRepository>     _employeeRepoMock;
    private Mock<IAuditService>           _auditMock;
    private Mock<INotificationService>    _notificationMock;
    private Mock<ICurrentUserService>     _currentUserMock;
    private Mock<ILogger<LeaveService>>   _loggerMock;
    private LeaveService                  _sut;

    [SetUp]
    public void SetUp()
    {
        _leaveRepoMock     = new Mock<ILeaveRequestRepository>();
        _leaveTypeRepoMock = new Mock<ILeaveTypeRepository>();
        _employeeRepoMock  = new Mock<IEmployeeRepository>();
        _auditMock         = new Mock<IAuditService>();
        _notificationMock  = new Mock<INotificationService>();
        _currentUserMock   = new Mock<ICurrentUserService>();
        _loggerMock        = new Mock<ILogger<LeaveService>>();

        _currentUserMock.Setup(c => c.UserId).Returns(1);

        _sut = new LeaveService(
            _leaveRepoMock.Object, _leaveTypeRepoMock.Object,
            _employeeRepoMock.Object, _auditMock.Object,
            _notificationMock.Object, _currentUserMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task SubmitAsync_WhenEmployeeNotFound_ThrowsNotFoundException()
    {
        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(99))
            .ReturnsAsync((Core.Entities.Employee?)null);

        Func<Task> act = () => _sut.SubmitAsync(99, BuildCreateDto());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Employee*99*");
    }

    [Test]
    public async Task SubmitAsync_WhenLeaveTypeNotFound_ThrowsNotFoundException()
    {
        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(BuildEmployee());
        _leaveTypeRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((LeaveType?)null);

        Func<Task> act = () => _sut.SubmitAsync(1, BuildCreateDto(leaveTypeId: 99));

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*LeaveType*99*");
    }

    [Test]
    public async Task SubmitAsync_WhenToDateBeforeFromDate_ThrowsValidationException()
    {
        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(BuildEmployee());
        _leaveTypeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BuildLeaveType());

        Func<Task> act = () => _sut.SubmitAsync(1, BuildCreateDto(
            fromDate: new DateOnly(2024, 6, 10),
            toDate:   new DateOnly(2024, 6, 5)));

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*End date must be on or after start date*");
    }

    [Test]
    public async Task SubmitAsync_WhenOverlappingLeaveExists_ThrowsBusinessRuleException()
    {
        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(BuildEmployee());
        _leaveTypeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BuildLeaveType());
        _leaveRepoMock.Setup(r => r.HasOverlappingLeaveAsync(
            1, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), null)).ReturnsAsync(true);

        Func<Task> act = () => _sut.SubmitAsync(1, BuildCreateDto());

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*already have*");
    }

    [Test]
    public async Task SubmitAsync_WhenInsufficientBalance_ThrowsBusinessRuleException()
    {
        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(BuildEmployee());
        _leaveTypeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BuildLeaveType(maxDays: 5));
        _leaveRepoMock.Setup(r => r.HasOverlappingLeaveAsync(
            It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), null)).ReturnsAsync(false);
        _leaveRepoMock.Setup(r => r.GetUsedLeaveDaysAsync(1, 1, It.IsAny<int>())).ReturnsAsync(4);

        Func<Task> act = () => _sut.SubmitAsync(1, BuildCreateDto(
            fromDate: new DateOnly(2024, 6, 10),
            toDate:   new DateOnly(2024, 6, 12)));

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*Insufficient*");
    }

    [Test]
    public async Task SubmitAsync_WithValidData_CreatesLeaveRequest()
    {
        var employee  = BuildEmployee();
        var leaveType = BuildLeaveType(maxDays: 12);

        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(employee);
        _leaveTypeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(leaveType);
        _leaveRepoMock.Setup(r => r.HasOverlappingLeaveAsync(
            It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), null)).ReturnsAsync(false);
        _leaveRepoMock.Setup(r => r.GetUsedLeaveDaysAsync(1, 1, It.IsAny<int>())).ReturnsAsync(2);
        _notificationMock.Setup(n => n.SendAsync(It.IsAny<int>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .Returns(Task.CompletedTask);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, It.IsAny<object>(), true, null)).Returns(Task.CompletedTask);

        LeaveRequest? saved = null;
        _leaveRepoMock.Setup(r => r.AddAsync(It.IsAny<LeaveRequest>()))
            .ReturnsAsync((LeaveRequest l) =>
            {
                l.LeaveRequestId = 1; l.Employee = employee; l.LeaveType = leaveType;
                saved = l; return l;
            });
        _leaveRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<LeaveRequest, bool>>>()))
            .ReturnsAsync(() => saved != null ? new List<LeaveRequest> { saved } : new List<LeaveRequest>());

        var result = await _sut.SubmitAsync(1, BuildCreateDto(
            fromDate: new DateOnly(2024, 6, 10),
            toDate:   new DateOnly(2024, 6, 11)));

        result.Should().NotBeNull();
        result.TotalDays.Should().Be(2);
        result.Status.Should().Be("Pending");
        _leaveRepoMock.Verify(r => r.AddAsync(It.IsAny<LeaveRequest>()), Times.Once);
    }

    [Test]
    public async Task ApproveOrRejectAsync_WhenLeaveNotPending_ThrowsBusinessRuleException()
    {
        var leave = BuildLeaveRequest(status: "Approved");
        _leaveRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<LeaveRequest, bool>>>()))
            .ReturnsAsync(leave);

        Func<Task> act = () => _sut.ApproveOrRejectAsync(1, 2, new ApproveLeaveDto { Action = "Approved" });

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*already*");
    }

    [Test]
    public async Task ApproveOrRejectAsync_WithInvalidAction_ThrowsValidationException()
    {
        var leave = BuildLeaveRequest(status: "Pending");
        _leaveRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<LeaveRequest, bool>>>()))
            .ReturnsAsync(leave);

        Func<Task> act = () => _sut.ApproveOrRejectAsync(1, 2, new ApproveLeaveDto { Action = "Maybe" });

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ApproveOrRejectAsync_WhenRejectWithoutReason_ThrowsValidationException()
    {
        var leave = BuildLeaveRequest(status: "Pending");
        _leaveRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<LeaveRequest, bool>>>()))
            .ReturnsAsync(leave);

        Func<Task> act = () => _sut.ApproveOrRejectAsync(1, 2, new ApproveLeaveDto { Action = "Rejected", RejectionReason = null });

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*Rejection reason is required*");
    }

    [Test]
    public async Task ApproveOrRejectAsync_WithValidApproval_SetsStatusAndNotifies()
    {
        var leave    = BuildLeaveRequest(status: "Pending");
        var employee = BuildEmployee();

        _leaveRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<LeaveRequest, bool>>>()))
            .ReturnsAsync(leave);
        _leaveRepoMock.Setup(r => r.UpdateAsync(It.IsAny<LeaveRequest>())).Returns(Task.CompletedTask);
        _leaveRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<LeaveRequest, bool>>>()))
            .ReturnsAsync(new List<LeaveRequest> { leave });
        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(leave.EmployeeId)).ReturnsAsync(employee);
        _notificationMock.Setup(n => n.SendAsync(It.IsAny<int>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .Returns(Task.CompletedTask);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, It.IsAny<object>(), true, null)).Returns(Task.CompletedTask);

        var result = await _sut.ApproveOrRejectAsync(1, 2, new ApproveLeaveDto { Action = "Approved" });

        result.Status.Should().Be("Approved");
        _notificationMock.Verify(n => n.SendAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Once);
    }

    [Test]
    public async Task CancelAsync_WhenLeaveNotFound_ThrowsNotFoundException()
    {
        _leaveRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<LeaveRequest, bool>>>()))
            .ReturnsAsync((LeaveRequest?)null);

        Func<Task> act = () => _sut.CancelAsync(99, 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task CancelAsync_WhenApprovedLeaveStarted_ThrowsBusinessRuleException()
    {
        var leave = BuildLeaveRequest(status: "Approved",
            fromDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));
        _leaveRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<LeaveRequest, bool>>>()))
            .ReturnsAsync(leave);

        Func<Task> act = () => _sut.CancelAsync(1, 1);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*already started*");
    }

    [Test]
    public async Task GetBalancesAsync_ReturnsCorrectRemainingDays()
    {
        var leaveTypes = new List<LeaveType>
        {
            BuildLeaveType(id: 1, name: "Casual Leave",    code: "CL", maxDays: 12),
            BuildLeaveType(id: 2, name: "Privilege Leave", code: "PL", maxDays: 15)
        };

        _leaveTypeRepoMock.Setup(r => r.GetActiveAsync()).ReturnsAsync(leaveTypes);
        _leaveRepoMock.Setup(r => r.GetUsedLeaveDaysAsync(1, 1, 2024)).ReturnsAsync(3);
        _leaveRepoMock.Setup(r => r.GetUsedLeaveDaysAsync(1, 2, 2024)).ReturnsAsync(7);

        var result = (await _sut.GetBalancesAsync(1, 2024)).ToList();

        result.Should().HaveCount(2);
        result.First(b => b.LeaveCode == "CL").RemainingDays.Should().Be(9);
        result.First(b => b.LeaveCode == "PL").RemainingDays.Should().Be(8);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static Core.Entities.Employee BuildEmployee(int id = 1)
    {
        var dept  = new Department  { DepartmentId = 1, DepartmentName = "IT" };
        var desig = new Designation { DesignationId = 1, DesignationName = "Dev" };
        var role  = new Role { RoleId = 4, RoleName = "Employee" };
        return new Core.Entities.Employee
        {
            EmployeeId = id, EmployeeCode = $"EMP0000{id}",
            FirstName = "John", LastName = "Doe",
            IsActive = true, DepartmentId = 1, Department = dept,
            DesignationId = 1, Designation = desig,
            UserId = id + 10, User = new User { UserId = id + 10, Email = $"emp{id}@t.com", Role = role },
            DateOfBirth = new DateOnly(1990, 1, 1),
            JoiningDate = new DateOnly(2020, 1, 1), Gender = "Male"
        };
    }

    private static LeaveType BuildLeaveType(int id = 1, string name = "Casual Leave",
        string code = "CL", int maxDays = 12) => new()
    {
        LeaveTypeId = id, LeaveTypeName = name, LeaveCode = code,
        MaxDaysPerYear = maxDays, IsCarryForward = false, IsPaid = true, IsActive = true
    };

    private static CreateLeaveRequestDto BuildCreateDto(int leaveTypeId = 1,
        DateOnly? fromDate = null, DateOnly? toDate = null) => new()
    {
        LeaveTypeId = leaveTypeId,
        FromDate = fromDate ?? new DateOnly(2024, 6, 10),
        ToDate   = toDate   ?? new DateOnly(2024, 6, 12),
        Reason   = "Personal work"
    };

    private static LeaveRequest BuildLeaveRequest(string status = "Pending",
        DateOnly? fromDate = null) => new()
    {
        LeaveRequestId = 1, EmployeeId = 1, LeaveTypeId = 1,
        FromDate = fromDate ?? new DateOnly(2024, 6, 10),
        ToDate = new DateOnly(2024, 6, 12),
        TotalDays = 3, Status = status,
        LeaveType = new LeaveType
        {
            LeaveTypeId = 1, LeaveTypeName = "Casual Leave", LeaveCode = "CL"
        }
    };
}
