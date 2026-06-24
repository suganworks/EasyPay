using EasyPay.Core.DTOs.Timesheet;
using EasyPay.Core.Entities;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using EasyPay.Infrastructure.Services;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace EasyPay.Tests.Payroll;

[TestFixture]
public class TimesheetServiceTests
{
    private Mock<ITimesheetRepository> _timesheetRepoMock;
    private Mock<IEmployeeRepository>  _employeeRepoMock;
    private Mock<IAuditService>        _auditMock;
    private Mock<ICurrentUserService>  _currentUserMock;
    private TimesheetService           _sut;

    [SetUp]
    public void SetUp()
    {
        _timesheetRepoMock = new Mock<ITimesheetRepository>();
        _employeeRepoMock  = new Mock<IEmployeeRepository>();
        _auditMock         = new Mock<IAuditService>();
        _currentUserMock   = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(c => c.UserId).Returns(1);

        _sut = new TimesheetService(
            _timesheetRepoMock.Object,
            _employeeRepoMock.Object,
            _auditMock.Object,
            _currentUserMock.Object);
    }

    [Test]
    public async Task CreateAsync_WhenEmployeeNotFound_ThrowsNotFoundException()
    {
        _employeeRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((EasyPay.Core.Entities.Employee?)null);

        Func<Task> act = () => _sut.CreateAsync(99, BuildCreateDto());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Employee*99*");
    }

    [Test]
    public async Task CreateAsync_WhenTimesheetAlreadyExists_ThrowsConflictException()
    {
        var employee = BuildEmployee();
        _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(employee);
        _timesheetRepoMock.Setup(r => r.GetForDateAsync(1, It.IsAny<DateOnly>()))
            .ReturnsAsync(new Timesheet { TimesheetId = 1 });

        Func<Task> act = () => _sut.CreateAsync(1, BuildCreateDto());

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Test]
    public async Task CreateAsync_WithFutureDate_ThrowsBusinessRuleException()
    {
        var employee = BuildEmployee();
        _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(employee);
        _timesheetRepoMock.Setup(r => r.GetForDateAsync(1, It.IsAny<DateOnly>()))
            .ReturnsAsync((Timesheet?)null);

        var dto = BuildCreateDto(workDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        Func<Task> act = () => _sut.CreateAsync(1, dto);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*future*");
    }

    [Test]
    public async Task CreateAsync_WithValidData_CreatesTimesheet()
    {
        var employee = BuildEmployee();
        _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(employee);
        _timesheetRepoMock.Setup(r => r.GetForDateAsync(1, It.IsAny<DateOnly>()))
            .ReturnsAsync((Timesheet?)null);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, It.IsAny<object>(), true, null)).Returns(Task.CompletedTask);

        Timesheet? saved = null;
        _timesheetRepoMock.Setup(r => r.AddAsync(It.IsAny<Timesheet>()))
            .ReturnsAsync((Timesheet t) =>
            {
                t.TimesheetId = 1; t.Employee = employee;
                saved = t; return t;
            });
        _timesheetRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Timesheet, bool>>>()))
            .ReturnsAsync(() => saved);

        var result = await _sut.CreateAsync(1, BuildCreateDto());

        result.Should().NotBeNull();
        result.Status.Should().Be("Pending");
        result.HoursWorked.Should().Be(8);
        _timesheetRepoMock.Verify(r => r.AddAsync(It.IsAny<Timesheet>()), Times.Once);
    }

    [Test]
    public async Task ApproveOrRejectAsync_WhenNotPending_ThrowsBusinessRuleException()
    {
        var timesheet = new Timesheet { TimesheetId = 1, Status = "Approved" };
        _timesheetRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(timesheet);

        Func<Task> act = () => _sut.ApproveOrRejectAsync(1, 2, new ApproveTimesheetDto { Action = "Approved" });

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*already*");
    }

    [Test]
    public async Task ApproveOrRejectAsync_WhenPending_SetsStatus()
    {
        var timesheet = new Timesheet
        {
            TimesheetId = 1, Status = "Pending",
            Employee = BuildEmployee()
        };
        _timesheetRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(timesheet);
        _timesheetRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Timesheet>())).Returns(Task.CompletedTask);
        _timesheetRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Timesheet, bool>>>()))
            .ReturnsAsync(timesheet);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, null, true, null)).Returns(Task.CompletedTask);

        var result = await _sut.ApproveOrRejectAsync(1, 2, new ApproveTimesheetDto { Action = "Approved" });

        result.Status.Should().Be("Approved");
        _timesheetRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Timesheet>()), Times.Once);
    }

    [Test]
    public async Task BulkApproveAsync_ApprovesOnlyPendingTimesheets()
    {
        var ts1 = new Timesheet { TimesheetId = 1, Status = "Pending" };
        var ts2 = new Timesheet { TimesheetId = 2, Status = "Approved" }; // already approved
        var ts3 = new Timesheet { TimesheetId = 3, Status = "Pending" };

        _timesheetRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ts1);
        _timesheetRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(ts2);
        _timesheetRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(ts3);
        _timesheetRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Timesheet>())).Returns(Task.CompletedTask);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, It.IsAny<object>(), true, null)).Returns(Task.CompletedTask);

        var count = await _sut.BulkApproveAsync(new List<int> { 1, 2, 3 }, approverId: 0);

        count.Should().Be(2); // only ts1 and ts3 approved
        _timesheetRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Timesheet>()), Times.Exactly(2));
    }

    [Test]
    public async Task UpdateAsync_WhenApproved_ThrowsBusinessRuleException()
    {
        var timesheet = new Timesheet { TimesheetId = 1, EmployeeId = 1, Status = "Approved" };
        _timesheetRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Timesheet, bool>>>()))
            .ReturnsAsync(timesheet);

        Func<Task> act = () => _sut.UpdateAsync(1, 1, new UpdateTimesheetDto { HoursWorked = 8 });

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*approved*");
    }

    private static EasyPay.Core.Entities.Employee BuildEmployee(int id = 1) => new()
    {
        EmployeeId = id, EmployeeCode = $"EMP0000{id}",
        FirstName = "John", LastName = "Doe",
        UserId = id + 10, User = new User { UserId = id + 10, Email = "j@t.com",
            Role = new Role { RoleId = 4, RoleName = "Employee" } },
        DateOfBirth = new DateOnly(1990, 1, 1),
        JoiningDate = new DateOnly(2020, 1, 1), Gender = "Male"
    };

    private static CreateTimesheetDto BuildCreateDto(DateOnly? workDate = null) => new()
    {
        WorkDate      = workDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
        CheckIn       = new TimeOnly(9, 0),
        CheckOut      = new TimeOnly(17, 0),
        HoursWorked   = 8,
        OvertimeHours = 0
    };
}
