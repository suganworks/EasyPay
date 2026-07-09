using EasyPay.Core.DTOs.Benefit;
using EasyPay.Core.Entities;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using EasyPay.Infrastructure.Services;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace EasyPay.Tests.Leave;

// ─── BenefitService Tests ─────────────────────────────────────────────────────
[TestFixture]
public class BenefitServiceTests
{
    private Mock<IBenefitRepository>         _benefitRepoMock;
    private Mock<IEmployeeBenefitRepository> _empBenefitRepoMock;
    private Mock<IEmployeeRepository>        _employeeRepoMock;
    private Mock<IAuditService>              _auditMock;
    private BenefitService                   _sut;

    [SetUp]
    public void SetUp()
    {
        _benefitRepoMock    = new Mock<IBenefitRepository>();
        _empBenefitRepoMock = new Mock<IEmployeeBenefitRepository>();
        _employeeRepoMock   = new Mock<IEmployeeRepository>();
        _auditMock          = new Mock<IAuditService>();

        _sut = new BenefitService(
            _benefitRepoMock.Object,
            _empBenefitRepoMock.Object,
            _employeeRepoMock.Object,
            _auditMock.Object);
    }

    [Test]
    public async Task CreateAsync_WhenCodeExists_ThrowsConflictException()
    {
        _benefitRepoMock.Setup(r => r.BenefitCodeExistsAsync("HI001")).ReturnsAsync(true);

        Func<Task> act = () => _sut.CreateAsync(new CreateBenefitDto
        {
            BenefitName = "Health Insurance",
            BenefitCode = "HI001",
            BenefitType = "Health",
            Amount      = 5000
        });

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*HI001*");
    }

    [Test]
    public async Task CreateAsync_WithValidData_CreatesBenefit()
    {
        _benefitRepoMock.Setup(r => r.BenefitCodeExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, It.IsAny<object>(), true, null)).Returns(Task.CompletedTask);

        _benefitRepoMock.Setup(r => r.AddAsync(It.IsAny<Benefit>()))
            .ReturnsAsync((Benefit b) => { b.BenefitId = 1; return b; });

        var result = await _sut.CreateAsync(new CreateBenefitDto
        {
            BenefitName = "Health Insurance",
            BenefitCode = "HI001",
            BenefitType = "Health",
            Amount      = 5000,
            IsPercentage= false
        });

        result.Should().NotBeNull();
        result.BenefitName.Should().Be("Health Insurance");
        result.BenefitCode.Should().Be("HI001");
        result.Amount.Should().Be(5000);
        _benefitRepoMock.Verify(r => r.AddAsync(It.IsAny<Benefit>()), Times.Once);
    }

    [Test]
    public async Task GetByIdAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _benefitRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Benefit?)null);

        Func<Task> act = () => _sut.GetByIdAsync(99);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Benefit*99*");
    }

    [Test]
    public async Task AssignToEmployeeAsync_WhenEmployeeNotFound_ThrowsNotFoundException()
    {
        _employeeRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Core.Entities.Employee?)null);

        Func<Task> act = () => _sut.AssignToEmployeeAsync(99, new AssignBenefitDto
        {
            BenefitId     = 1,
            EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Employee*99*");
    }

    [Test]
    public async Task AssignToEmployeeAsync_WhenBenefitNotFound_ThrowsNotFoundException()
    {
        var employee = BuildEmployee();
        _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(employee);
        _benefitRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Benefit?)null);

        Func<Task> act = () => _sut.AssignToEmployeeAsync(1, new AssignBenefitDto
        {
            BenefitId     = 99,
            EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Benefit*99*");
    }

    [Test]
    public async Task AssignToEmployeeAsync_WithValidData_AssignsBenefit()
    {
        var employee = BuildEmployee();
        var benefit  = new Benefit
        {
            BenefitId   = 1,
            BenefitName = "Health Insurance",
            BenefitCode = "HI001",
            BenefitType = "Health",
            Amount      = 5000,
            IsActive    = true
        };

        _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(employee);
        _benefitRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(benefit);
        _empBenefitRepoMock.Setup(r => r.DeactivatePreviousAsync(1, 1)).Returns(Task.CompletedTask);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, It.IsAny<object>(), true, null)).Returns(Task.CompletedTask);

        _empBenefitRepoMock.Setup(r => r.AddAsync(It.IsAny<EmployeeBenefit>()))
            .ReturnsAsync((EmployeeBenefit eb) =>
            {
                eb.EmployeeBenefitId = 1;
                eb.Employee = employee;
                eb.Benefit  = benefit;
                return eb;
            });

        var result = await _sut.AssignToEmployeeAsync(1, new AssignBenefitDto
        {
            BenefitId     = 1,
            EffectiveFrom = new DateOnly(2024, 1, 1)
        });

        result.Should().NotBeNull();
        result.BenefitName.Should().Be("Health Insurance");
        result.EffectiveAmount.Should().Be(5000);
        _empBenefitRepoMock.Verify(r => r.AddAsync(It.IsAny<EmployeeBenefit>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _benefitRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Benefit?)null);

        Func<Task> act = () => _sut.UpdateAsync(99, new CreateBenefitDto
        {
            BenefitName = "Test", BenefitCode = "T001", BenefitType = "Health", Amount = 100
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllBenefits()
    {
        var benefits = new List<Benefit>
        {
            new() { BenefitId = 1, BenefitName = "Health Insurance", BenefitCode = "HI001", BenefitType = "Health", Amount = 5000, IsActive = true },
            new() { BenefitId = 2, BenefitName = "Transport",        BenefitCode = "TR001", BenefitType = "Transport", Amount = 2000, IsActive = true }
        };
        _benefitRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(benefits);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    private static Core.Entities.Employee BuildEmployee() => new()
    {
        EmployeeId = 1, EmployeeCode = "EMP00001",
        FirstName = "John", LastName = "Doe",
        UserId = 10, User = new User { UserId = 10, Email = "j@t.com",
            Role = new Role { RoleId = 4, RoleName = "Employee" } },
        DateOfBirth = new DateOnly(1990, 1, 1),
        JoiningDate = new DateOnly(2020, 1, 1), Gender = "Male"
    };
}

// ─── NotificationService Tests ────────────────────────────────────────────────
[TestFixture]
public class NotificationServiceTests
{
    private Mock<INotificationRepository> _notifRepoMock;
    private NotificationService           _sut;

    [SetUp]
    public void SetUp()
    {
        _notifRepoMock = new Mock<INotificationRepository>();
        _sut           = new NotificationService(_notifRepoMock.Object);
    }

    [Test]
    public async Task SendAsync_CreatesNotification()
    {
        _notifRepoMock.Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .ReturnsAsync((Notification n) => { n.NotificationId = 1; return n; });

        await _sut.SendAsync(1, "Test Title", "Test Message", "Info", "Leave", 1);

        _notifRepoMock.Verify(r => r.AddAsync(It.Is<Notification>(n =>
            n.UserId  == 1 &&
            n.Title   == "Test Title" &&
            n.Message == "Test Message" &&
            n.NotificationType == "Info"
        )), Times.Once);
    }

    [Test]
    public async Task SendBulkAsync_CreatesNotificationsForAllUsers()
    {
        var userIds = new List<int> { 1, 2, 3 };
        _notifRepoMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Notification>>()))
            .Returns(Task.CompletedTask);

        await _sut.SendBulkAsync(userIds, "Payday", "Salary credited", "Success");

        _notifRepoMock.Verify(r => r.AddRangeAsync(It.Is<IEnumerable<Notification>>(n =>
            n.Count() == 3)), Times.Once);
    }

    [Test]
    public async Task GetUnreadCountAsync_ReturnsCount()
    {
        _notifRepoMock.Setup(r => r.GetUnreadCountAsync(1)).ReturnsAsync(5);

        var count = await _sut.GetUnreadCountAsync(1);

        count.Should().Be(5);
    }

    [Test]
    public async Task MarkAllAsReadAsync_CallsRepository()
    {
        _notifRepoMock.Setup(r => r.MarkAllAsReadAsync(1)).Returns(Task.CompletedTask);

        await _sut.MarkAllAsReadAsync(1);

        _notifRepoMock.Verify(r => r.MarkAllAsReadAsync(1), Times.Once);
    }

    [Test]
    public async Task MarkAsReadAsync_CallsRepository()
    {
        _notifRepoMock.Setup(r => r.MarkAsReadAsync(1)).Returns(Task.CompletedTask);

        await _sut.MarkAsReadAsync(1, userId: 1);

        _notifRepoMock.Verify(r => r.MarkAsReadAsync(1), Times.Once);
    }
}
