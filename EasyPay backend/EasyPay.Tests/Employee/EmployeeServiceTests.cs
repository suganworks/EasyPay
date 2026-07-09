using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Employee;
using EasyPay.Core.Entities;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using EasyPay.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasyPay.Tests.Employee;

[TestFixture]
public class EmployeeServiceNUnitTests
{
    private Mock<IEmployeeRepository>    _employeeRepoMock;
    private Mock<IUserRepository>        _userRepoMock;
    private Mock<IDepartmentRepository>  _deptRepoMock;
    private Mock<IDesignationRepository> _desigRepoMock;
    private Mock<IAuditService>          _auditMock;
    private Mock<ICurrentUserService>    _currentUserMock;
    private Mock<ILogger<EmployeeService>> _loggerMock;
    private EmployeeService              _sut;

    [SetUp]
    public void SetUp()
    {
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        _userRepoMock     = new Mock<IUserRepository>();
        _deptRepoMock     = new Mock<IDepartmentRepository>();
        _desigRepoMock    = new Mock<IDesignationRepository>();
        _auditMock        = new Mock<IAuditService>();
        _currentUserMock  = new Mock<ICurrentUserService>();
        _loggerMock       = new Mock<ILogger<EmployeeService>>();

        _currentUserMock.Setup(c => c.UserId).Returns(1);

        _sut = new EmployeeService(
            _employeeRepoMock.Object, _userRepoMock.Object,
            _deptRepoMock.Object, _desigRepoMock.Object,
            _auditMock.Object, _currentUserMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task CreateAsync_WhenEmailAlreadyExists_ThrowsConflictException()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync("dup@test.com")).ReturnsAsync(true);

        Func<Task> act = () => _sut.CreateAsync(BuildCreateDto("dup@test.com"));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Test]
    public async Task CreateAsync_WhenDepartmentNotFound_ThrowsNotFoundException()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _deptRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Department?)null);

        Func<Task> act = () => _sut.CreateAsync(BuildCreateDto("new@test.com", deptId: 99));

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Department*");
    }

    [Test]
    public async Task CreateAsync_WhenDesignationNotFound_ThrowsNotFoundException()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _deptRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Department { DepartmentId = 1, DepartmentName = "IT" });
        _desigRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Designation?)null);

        Func<Task> act = () => _sut.CreateAsync(BuildCreateDto("new@test.com", deptId: 1, desigId: 99));

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Designation*");
    }

    [Test]
    public async Task CreateAsync_WithValidData_CreatesEmployeeAndReturnsDto()
    {
        var dept  = new Department  { DepartmentId  = 1, DepartmentName  = "IT"       };
        var desig = new Designation { DesignationId = 1, DesignationName = "Engineer", DepartmentId = 1 };
        var role  = new Role        { RoleId        = 4, RoleName        = "Employee" };

        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _deptRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dept);
        _desigRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(desig);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.UserId = 10; return u; });
        _employeeRepoMock.Setup(r => r.GenerateEmployeeCodeAsync()).ReturnsAsync("EMP00001");
        _employeeRepoMock.Setup(r => r.AddAsync(It.IsAny<Core.Entities.Employee>()))
            .ReturnsAsync((Core.Entities.Employee e) =>
            {
                e.EmployeeId = 1; e.Department = dept;
                e.Designation = desig;
                e.User = new User { Email = "emp@test.com", Role = role };
                return e;
            });

        var fullEmployee = new Core.Entities.Employee
        {
            EmployeeId = 1, EmployeeCode = "EMP00001",
            FirstName = "John", LastName = "Doe",
            DepartmentId = 1, Department = dept,
            DesignationId = 1, Designation = desig,
            UserId = 10, User = new User { Email = "emp@test.com", Role = role },
            JoiningDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DateOfBirth = new DateOnly(1990, 1, 1),
            Gender = "Male", EmploymentType = "FullTime",
            EmploymentStatus = "Active", IsActive = true, CreatedAt = DateTime.UtcNow
        };

        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(fullEmployee);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, It.IsAny<object>(), true, null))
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(BuildCreateDto("emp@test.com"));

        result.Should().NotBeNull();
        result.EmployeeCode.Should().Be("EMP00001");
        result.FullName.Should().Be("John Doe");
        result.DepartmentName.Should().Be("IT");
    }

    [Test]
    public async Task GetByIdAsync_WhenEmployeeNotFound_ThrowsNotFoundException()
    {
        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(999))
            .ReturnsAsync((Core.Entities.Employee?)null);

        Func<Task> act = () => _sut.GetByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Employee*999*");
    }

    [Test]
    public async Task GetByIdAsync_WithValidId_ReturnsEmployeeDto()
    {
        var dept  = new Department  { DepartmentId  = 1, DepartmentName  = "HR"       };
        var desig = new Designation { DesignationId = 2, DesignationName = "HR Exec", DepartmentId = 1 };
        var role  = new Role        { RoleId        = 4, RoleName        = "Employee" };

        var employee = new Core.Entities.Employee
        {
            EmployeeId = 5, EmployeeCode = "EMP00005",
            FirstName = "Jane", LastName = "Smith",
            DepartmentId = 1, Department = dept,
            DesignationId = 2, Designation = desig,
            UserId = 5, User = new User { Email = "jane@test.com", Role = role },
            JoiningDate = new DateOnly(2023, 6, 1),
            DateOfBirth = new DateOnly(1995, 3, 15),
            Gender = "Female", EmploymentType = "FullTime",
            EmploymentStatus = "Active", IsActive = true, CreatedAt = DateTime.UtcNow
        };

        _employeeRepoMock.Setup(r => r.GetWithDetailsAsync(5)).ReturnsAsync(employee);

        var result = await _sut.GetByIdAsync(5);

        result.EmployeeId.Should().Be(5);
        result.FullName.Should().Be("Jane Smith");
        result.DepartmentName.Should().Be("HR");
    }

    [Test]
    public async Task DeactivateAsync_WhenEmployeeNotFound_ThrowsNotFoundException()
    {
        _employeeRepoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Core.Entities.Employee?)null);

        Func<Task> act = () => _sut.DeactivateAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task DeactivateAsync_WithValidId_SetsInactive()
    {
        var employee = new Core.Entities.Employee
        {
            EmployeeId = 1, IsActive = true, EmploymentStatus = "Active"
        };

        _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(employee);
        _employeeRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Core.Entities.Employee>()))
            .Returns(Task.CompletedTask);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, null, true, null)).Returns(Task.CompletedTask);

        await _sut.DeactivateAsync(1);

        employee.IsActive.Should().BeFalse();
        employee.EmploymentStatus.Should().Be("Terminated");
        _employeeRepoMock.Verify(r => r.UpdateAsync(employee), Times.Once);
    }

    [Test]
    public async Task GetPagedAsync_ReturnsPagedResponse()
    {
        var dept  = new Department  { DepartmentId = 1, DepartmentName = "IT" };
        var desig = new Designation { DesignationId = 1, DesignationName = "Dev" };
        var role  = new Role { RoleId = 4, RoleName = "Employee" };

        var employees = Enumerable.Range(1, 3).Select(i => new Core.Entities.Employee
        {
            EmployeeId = i, EmployeeCode = $"EMP0000{i}",
            FirstName = $"User{i}", LastName = "Test",
            DepartmentId = 1, Department = dept,
            DesignationId = 1, Designation = desig,
            UserId = i, User = new User { Email = $"user{i}@test.com", Role = role },
            JoiningDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DateOfBirth = new DateOnly(1990, 1, 1),
            Gender = "Male", EmploymentType = "FullTime",
            EmploymentStatus = "Active", IsActive = true, CreatedAt = DateTime.UtcNow
        }).ToList();

        _employeeRepoMock.Setup(r => r.GetPagedAsync(It.IsAny<PaginationParams>(), null))
            .ReturnsAsync((employees, 3));

        var result = await _sut.GetPagedAsync(new PaginationParams { PageNumber = 1, PageSize = 10 });

        result.TotalCount.Should().Be(3);
        result.Data.Should().HaveCount(3);
    }

    private static CreateEmployeeDto BuildCreateDto(
        string email = "test@company.com", int deptId = 1, int desigId = 1) => new()
    {
        FirstName = "John", LastName = "Doe",
        DateOfBirth = new DateOnly(1990, 6, 15),
        Gender = "Male", DepartmentId = deptId, DesignationId = desigId,
        JoiningDate = DateOnly.FromDateTime(DateTime.UtcNow),
        EmploymentType = "FullTime", WorkEmail = email, Password = "TestPass@123"
    };
}
