using EasyPay.Core.DTOs.Department;
using EasyPay.Core.DTOs.Designation;
using EasyPay.Core.Entities;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using EasyPay.Infrastructure.Services;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace EasyPay.Tests.Employee;

[TestFixture]
public class DepartmentServiceTests
{
    private Mock<IDepartmentRepository> _repoMock;
    private Mock<IAuditService>         _auditMock;
    private DepartmentService           _sut;

    [SetUp]
    public void SetUp()
    {
        _repoMock  = new Mock<IDepartmentRepository>();
        _auditMock = new Mock<IAuditService>();
        _sut       = new DepartmentService(_repoMock.Object, _auditMock.Object);
    }

    [Test]
    public async Task CreateAsync_WhenCodeAlreadyExists_ThrowsConflictException()
    {
        _repoMock.Setup(r => r.DepartmentCodeExistsAsync("IT")).ReturnsAsync(true);

        Func<Task> act = () => _sut.CreateAsync(new CreateDepartmentDto
        {
            DepartmentCode = "IT",
            DepartmentName = "Information Technology"
        });

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*IT*");
    }

    [Test]
    public async Task CreateAsync_WithValidData_CreatesDepartment()
    {
        _repoMock.Setup(r => r.DepartmentCodeExistsAsync("HR")).ReturnsAsync(false);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, It.IsAny<object>(), true, null)).Returns(Task.CompletedTask);

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Department>()))
            .ReturnsAsync((Department d) => { d.DepartmentId = 1; return d; });

        var result = await _sut.CreateAsync(new CreateDepartmentDto
        {
            DepartmentCode = "HR",
            DepartmentName = "Human Resources",
            Description    = "HR Dept"
        });

        result.Should().NotBeNull();
        result.DepartmentCode.Should().Be("HR");
        result.DepartmentName.Should().Be("Human Resources");
        result.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task GetByIdAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetWithEmployeesAsync(99)).ReturnsAsync((Department?)null);

        Func<Task> act = () => _sut.GetByIdAsync(99);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Department*99*");
    }

    [Test]
    public async Task GetByIdAsync_WithValidId_ReturnsDepartmentDto()
    {
        var dept = new Department
        {
            DepartmentId   = 1,
            DepartmentCode = "IT",
            DepartmentName = "Information Technology",
            IsActive       = true,
            CreatedAt      = DateTime.UtcNow,
            Employees      = new List<Core.Entities.Employee>()
        };
        _repoMock.Setup(r => r.GetWithEmployeesAsync(1)).ReturnsAsync(dept);

        var result = await _sut.GetByIdAsync(1);

        result.DepartmentId.Should().Be(1);
        result.DepartmentCode.Should().Be("IT");
        result.EmployeeCount.Should().Be(0);
    }

    [Test]
    public async Task DeleteAsync_WhenHasActiveEmployees_ThrowsBusinessRuleException()
    {
        var dept = new Department
        {
            DepartmentId = 1,
            DepartmentName = "IT",
            IsActive = true,
            Employees = new List<Core.Entities.Employee>
            {
                new() { EmployeeId = 1, IsActive = true, EmploymentStatus = "Active" }
            }
        };
        _repoMock.Setup(r => r.GetWithEmployeesAsync(1)).ReturnsAsync(dept);

        Func<Task> act = () => _sut.DeleteAsync(1);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*active employees*");
    }

    [Test]
    public async Task UpdateAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Department?)null);

        Func<Task> act = () => _sut.UpdateAsync(99, new UpdateDepartmentDto
        {
            DepartmentName = "Test", IsActive = true
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllDepartments()
    {
        var depts = new List<Department>
        {
            new() { DepartmentId = 1, DepartmentCode = "IT", DepartmentName = "IT", IsActive = true, CreatedAt = DateTime.UtcNow, Employees = new List<Core.Entities.Employee>() },
            new() { DepartmentId = 2, DepartmentCode = "HR", DepartmentName = "HR", IsActive = true, CreatedAt = DateTime.UtcNow, Employees = new List<Core.Entities.Employee>() }
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(depts);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }
}

[TestFixture]
public class DesignationServiceTests
{
    private Mock<IDesignationRepository> _repoMock;
    private Mock<IDepartmentRepository>  _deptRepoMock;
    private Mock<IAuditService>          _auditMock;
    private DesignationService           _sut;

    [SetUp]
    public void SetUp()
    {
        _repoMock     = new Mock<IDesignationRepository>();
        _deptRepoMock = new Mock<IDepartmentRepository>();
        _auditMock    = new Mock<IAuditService>();
        _sut = new DesignationService(_repoMock.Object, _deptRepoMock.Object, _auditMock.Object);
    }

    [Test]
    public async Task CreateAsync_WhenCodeExists_ThrowsConflictException()
    {
        _repoMock.Setup(r => r.DesignationCodeExistsAsync("SWE")).ReturnsAsync(true);

        Func<Task> act = () => _sut.CreateAsync(new CreateDesignationDto
        {
            DesignationCode = "SWE",
            DesignationName = "Software Engineer",
            DepartmentId    = 1
        });

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*SWE*");
    }

    [Test]
    public async Task CreateAsync_WhenDepartmentNotFound_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.DesignationCodeExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _deptRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Department?)null);

        Func<Task> act = () => _sut.CreateAsync(new CreateDesignationDto
        {
            DesignationCode = "SWE",
            DesignationName = "Software Engineer",
            DepartmentId    = 99
        });

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Department*");
    }

    [Test]
    public async Task CreateAsync_WithValidData_CreatesDesignation()
    {
        var dept = new Department { DepartmentId = 1, DepartmentName = "IT" };
        _repoMock.Setup(r => r.DesignationCodeExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _deptRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dept);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, It.IsAny<object>(), true, null)).Returns(Task.CompletedTask);

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Designation>()))
            .ReturnsAsync((Designation d) => { d.DesignationId = 1; return d; });

        var activeDesigs = new List<Designation>
        {
            new() { DesignationId = 1, DesignationCode = "SWE", DesignationName = "Software Engineer",
                    DepartmentId = 1, Department = dept, IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        _repoMock.Setup(r => r.GetActiveAsync()).ReturnsAsync(activeDesigs);

        var result = await _sut.CreateAsync(new CreateDesignationDto
        {
            DesignationCode = "SWE",
            DesignationName = "Software Engineer",
            DepartmentId    = 1,
            GradeLevel      = "L3"
        });

        result.Should().NotBeNull();
        result.DesignationCode.Should().Be("SWE");
        result.DepartmentName.Should().Be("IT");
    }

    [Test]
    public async Task GetByDepartmentAsync_ReturnsDesignationsForDept()
    {
        var desigs = new List<Designation>
        {
            new() { DesignationId = 1, DesignationCode = "SWE", DesignationName = "SW Engineer",
                    DepartmentId = 1, IsActive = true },
            new() { DesignationId = 2, DesignationCode = "SSE", DesignationName = "Senior SW Eng",
                    DepartmentId = 1, IsActive = true }
        };
        _repoMock.Setup(r => r.GetByDepartmentAsync(1)).ReturnsAsync(desigs);

        var result = await _sut.GetByDepartmentAsync(1);

        result.Should().HaveCount(2);
    }
}
