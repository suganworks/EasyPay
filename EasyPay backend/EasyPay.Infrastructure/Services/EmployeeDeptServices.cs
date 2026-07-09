using EasyPay.Core.Constants;
using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Department;
using EasyPay.Core.DTOs.Designation;
using EasyPay.Core.DTOs.Employee;
using EasyPay.Core.Entities;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace EasyPay.Infrastructure.Services;

// ─── EmployeeService ──────────────────────────────────────────────────────────
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IUserRepository _userRepo;
    private readonly IDepartmentRepository _deptRepo;
    private readonly IDesignationRepository _desigRepo;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IEmployeeRepository employeeRepo,
        IUserRepository userRepo,
        IDepartmentRepository deptRepo,
        IDesignationRepository desigRepo,
        IAuditService auditService,
        ICurrentUserService currentUser,
        IEmailService emailService,
        ILogger<EmployeeService> logger)
    {
        _employeeRepo  = employeeRepo;
        _userRepo      = userRepo;
        _deptRepo      = deptRepo;
        _desigRepo     = desigRepo;
        _auditService  = auditService;
        _currentUser   = currentUser;
        _emailService  = emailService;
        _logger        = logger;
    }

    public async Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto)
    {
        // Validate email uniqueness
        if (await _userRepo.EmailExistsAsync(dto.WorkEmail.ToLower()))
            throw new ConflictException(AppConstants.ErrorMessages.DuplicateEmail);

        // Validate department & designation exist
        _ = await _deptRepo.GetByIdAsync(dto.DepartmentId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Department, dto.DepartmentId);

        _ = await _desigRepo.GetByIdAsync(dto.DesignationId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Designation, dto.DesignationId);

        // Create user account (role = Employee)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 11);
        var user = new User
        {
            Username         = dto.WorkEmail.Split('@')[0].ToLower(),
            Email            = dto.WorkEmail.ToLower(),
            PasswordHash     = passwordHash,
            RoleId           = 4, // Employee
            IsActive         = true,
            IsEmailVerified  = true
        };
        await _userRepo.AddAsync(user);

        // Generate employee code
        var code = await _employeeRepo.GenerateEmployeeCodeAsync();

        var employee = new Employee
        {
            EmployeeCode     = code,
            UserId           = user.UserId,
            FirstName        = dto.FirstName,
            LastName         = dto.LastName,
            DateOfBirth      = dto.DateOfBirth,
            Gender           = dto.Gender,
            NationalId       = dto.NationalId,
            Phone            = dto.Phone,
            PersonalEmail    = dto.PersonalEmail,
            Address          = dto.Address,
            City             = dto.City,
            State            = dto.State,
            PostalCode       = dto.PostalCode,
            Country          = dto.Country,
            DepartmentId     = dto.DepartmentId,
            DesignationId    = dto.DesignationId,
            ManagerId        = dto.ManagerId,
            JoiningDate      = dto.JoiningDate,
            EmploymentType   = dto.EmploymentType,
            EmploymentStatus = AppConstants.WorkflowStatus.Active,
            BankName         = dto.BankName,
            BankAccountNo    = dto.BankAccountNo,
            BankIFSC         = dto.BankIFSC,
            PanNumber        = dto.PanNumber,
            IsActive         = true,
            CreatedBy        = _currentUser.UserId,
            UpdatedBy        = _currentUser.UserId
        };

        await _employeeRepo.AddAsync(employee);

        await _auditService.LogAsync("Create", AppConstants.EntityNames.Employee, employee.EmployeeId.ToString(),
            newValues: new { employee.EmployeeCode, employee.FirstName, employee.LastName });

        _logger.LogInformation("Employee created: {Code} - {Name}", employee.EmployeeCode, employee.FullName);

        try
        {
            await _emailService.SendWelcomeEmailAsync(user.Email, employee.FirstName, dto.Password);
            _logger.LogInformation("Welcome email sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
        }

        return await GetByIdAsync(employee.EmployeeId);
    }

    public async Task<EmployeeResponseDto> UpdateAsync(int employeeId, UpdateEmployeeDto dto)
    {
        var employee = await _employeeRepo.GetWithDetailsAsync(employeeId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Employee, employeeId);

        var oldValues = MapToResponse(employee);

        // Validate department & designation
        _ = await _deptRepo.GetByIdAsync(dto.DepartmentId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Department, dto.DepartmentId);
        _ = await _desigRepo.GetByIdAsync(dto.DesignationId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Designation, dto.DesignationId);

        employee.FirstName        = dto.FirstName;
        employee.LastName         = dto.LastName;
        employee.Phone            = dto.Phone;
        employee.AlternatePhone   = dto.AlternatePhone;
        employee.PersonalEmail    = dto.PersonalEmail;
        employee.Address          = dto.Address;
        employee.City             = dto.City;
        employee.State            = dto.State;
        employee.PostalCode       = dto.PostalCode;
        employee.Country          = dto.Country;
        employee.DepartmentId     = dto.DepartmentId;
        employee.DesignationId    = dto.DesignationId;
        employee.ManagerId        = dto.ManagerId;
        employee.EmploymentType   = dto.EmploymentType;
        employee.EmploymentStatus = dto.EmploymentStatus;
        employee.BankName         = dto.BankName;
        employee.BankAccountNo    = dto.BankAccountNo;
        employee.BankIFSC         = dto.BankIFSC;
        employee.PanNumber        = dto.PanNumber;
        employee.PfNumber         = dto.PfNumber;
        employee.EsiNumber        = dto.EsiNumber;
        employee.TaxWithholding   = dto.TaxWithholding;
        employee.ConfirmationDate = dto.ConfirmationDate;
        employee.TerminationDate  = dto.TerminationDate;
        employee.UpdatedBy        = _currentUser.UserId;

        await _employeeRepo.UpdateAsync(employee);

        await _auditService.LogAsync("Update", AppConstants.EntityNames.Employee, employeeId.ToString(),
            oldValues: oldValues, newValues: dto);

        return await GetByIdAsync(employeeId);
    }

    public async Task<EmployeeResponseDto> GetByIdAsync(int employeeId)
    {
        var employee = await _employeeRepo.GetWithDetailsAsync(employeeId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Employee, employeeId);
        return MapToResponse(employee);
    }

    public async Task<EmployeeResponseDto> GetByCodeAsync(string code)
    {
        var employee = await _employeeRepo.GetByEmployeeCodeAsync(code)
            ?? throw new NotFoundException(AppConstants.EntityNames.Employee, code);
        return MapToResponse(employee);
    }

    public async Task<PagedResponse<EmployeeListDto>> GetPagedAsync(
        PaginationParams pagination, int? departmentId = null, int? designationId = null)
    {
        var (items, total) = await _employeeRepo.GetPagedAsync(pagination, departmentId, designationId);
        return new PagedResponse<EmployeeListDto>
        {
            Data       = items.Select(MapToListDto),
            TotalCount = total,
            PageNumber = pagination.PageNumber,
            PageSize   = pagination.PageSize
        };
    }

    public async Task<IEnumerable<EmployeeListDto>> GetByDepartmentAsync(int departmentId)
    {
        var employees = await _employeeRepo.GetByDepartmentAsync(departmentId);
        return employees.Select(MapToListDto);
    }

    public async Task<IEnumerable<EmployeeListDto>> GetByManagerAsync(int managerId)
    {
        var employees = await _employeeRepo.GetByManagerAsync(managerId);
        return employees.Select(MapToListDto);
    }

    public async Task DeactivateAsync(int employeeId)
    {
        var employee = await _employeeRepo.GetByIdAsync(employeeId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Employee, employeeId);

        employee.IsActive         = false;
        employee.EmploymentStatus = AppConstants.WorkflowStatus.Terminated;
        employee.TerminationDate  = DateOnly.FromDateTime(DateTime.UtcNow);
        employee.UpdatedBy        = _currentUser.UserId;

        await _employeeRepo.UpdateAsync(employee);
        await _auditService.LogAsync("Deactivate", AppConstants.EntityNames.Employee, employeeId.ToString());
    }

    public async Task ReactivateAsync(int employeeId)
    {
        var employee = await _employeeRepo.GetByIdAsync(employeeId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Employee, employeeId);

        employee.IsActive         = true;
        employee.EmploymentStatus = AppConstants.WorkflowStatus.Active;
        employee.TerminationDate  = null;
        employee.UpdatedBy        = _currentUser.UserId;

        await _employeeRepo.UpdateAsync(employee);
        await _auditService.LogAsync("Reactivate", AppConstants.EntityNames.Employee, employeeId.ToString());
    }

    // ─── Mappers ──────────────────────────────────────────────────────────────
    private static EmployeeResponseDto MapToResponse(Employee e) => new()
    {
        EmployeeId       = e.EmployeeId,
        EmployeeCode     = e.EmployeeCode,
        FirstName        = e.FirstName,
        LastName         = e.LastName,
        FullName         = e.FullName,
        DateOfBirth      = e.DateOfBirth,
        Gender           = e.Gender,
        Phone            = e.Phone,
        PersonalEmail    = e.PersonalEmail,
        Address          = e.Address,
        City             = e.City,
        State            = e.State,
        Country          = e.Country,
        DepartmentId     = e.DepartmentId,
        DepartmentName   = e.Department?.DepartmentName ?? string.Empty,
        DesignationId    = e.DesignationId,
        DesignationName  = e.Designation?.DesignationName ?? string.Empty,
        ManagerId        = e.ManagerId,
        ManagerName      = e.Manager?.FullName,
        JoiningDate      = e.JoiningDate,
        ConfirmationDate = e.ConfirmationDate,
        TerminationDate  = e.TerminationDate,
        EmploymentType   = e.EmploymentType,
        EmploymentStatus = e.EmploymentStatus,
        BankName         = e.BankName,
        PanNumber        = e.PanNumber,
        PfNumber         = e.PfNumber,
        WorkEmail        = e.User?.Email ?? string.Empty,
        PhotoUrl         = e.PhotoUrl,
        IsActive         = e.IsActive,
        CreatedAt        = e.CreatedAt
    };

    private static EmployeeListDto MapToListDto(Employee e) => new()
    {
        EmployeeId       = e.EmployeeId,
        EmployeeCode     = e.EmployeeCode,
        FullName         = e.FullName,
        DepartmentName   = e.Department?.DepartmentName ?? string.Empty,
        DesignationName  = e.Designation?.DesignationName ?? string.Empty,
        EmploymentType   = e.EmploymentType,
        EmploymentStatus = e.EmploymentStatus,
        WorkEmail        = e.User?.Email ?? string.Empty,
        Phone            = e.Phone,
        JoiningDate      = e.JoiningDate,
        IsActive         = e.IsActive
    };
}

// ─── DepartmentService ────────────────────────────────────────────────────────
public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repo;
    private readonly IAuditService _auditService;

    public DepartmentService(IDepartmentRepository repo, IAuditService auditService)
    {
        _repo         = repo;
        _auditService = auditService;
    }

    public async Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto)
    {
        if (await _repo.DepartmentCodeExistsAsync(dto.DepartmentCode.ToUpper()))
            throw new ConflictException($"Department code '{dto.DepartmentCode}' already exists.");

        var dept = new Department
        {
            DepartmentCode = dto.DepartmentCode.ToUpper(),
            DepartmentName = dto.DepartmentName,
            Description    = dto.Description,
            ManagerUserId  = dto.ManagerUserId,
            IsActive       = true
        };

        await _repo.AddAsync(dept);
        await _auditService.LogAsync("Create", AppConstants.EntityNames.Department, dept.DepartmentId.ToString(), newValues: dto);
        return MapToResponse(dept);
    }

    public async Task<DepartmentResponseDto> UpdateAsync(int departmentId, UpdateDepartmentDto dto)
    {
        var dept = await _repo.GetByIdAsync(departmentId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Department, departmentId);

        dept.DepartmentName = dto.DepartmentName;
        dept.Description    = dto.Description;
        dept.ManagerUserId  = dto.ManagerUserId;
        dept.IsActive       = dto.IsActive;

        await _repo.UpdateAsync(dept);
        await _auditService.LogAsync("Update", AppConstants.EntityNames.Department, departmentId.ToString(), newValues: dto);
        return MapToResponse(dept);
    }

    public async Task<DepartmentResponseDto> GetByIdAsync(int departmentId)
    {
        var dept = await _repo.GetWithEmployeesAsync(departmentId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Department, departmentId);
        return MapToResponse(dept);
    }

    public async Task<IEnumerable<DepartmentResponseDto>> GetAllAsync()
    {
        var depts = await _repo.GetAllAsync();
        return depts.Select(MapToResponse);
    }

    public async Task<IEnumerable<DepartmentResponseDto>> GetActiveAsync()
    {
        var depts = await _repo.GetActiveAsync();
        return depts.Select(MapToResponse);
    }

    public async Task DeleteAsync(int departmentId)
    {
        var dept = await _repo.GetWithEmployeesAsync(departmentId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Department, departmentId);

        if (dept.Employees.Any(e => e.IsActive))
            throw new BusinessRuleException("Cannot delete a department with active employees.");

        dept.IsActive = false;
        await _repo.UpdateAsync(dept);
        await _auditService.LogAsync("Delete", AppConstants.EntityNames.Department, departmentId.ToString());
    }

    private static DepartmentResponseDto MapToResponse(Department d) => new()
    {
        DepartmentId   = d.DepartmentId,
        DepartmentCode = d.DepartmentCode,
        DepartmentName = d.DepartmentName,
        Description    = d.Description,
        ManagerUserId  = d.ManagerUserId,
        EmployeeCount  = d.Employees.Count(e => e.IsActive),
        IsActive       = d.IsActive,
        CreatedAt      = d.CreatedAt
    };
}

// ─── DesignationService ───────────────────────────────────────────────────────
public class DesignationService : IDesignationService
{
    private readonly IDesignationRepository _repo;
    private readonly IDepartmentRepository _deptRepo;
    private readonly IAuditService _auditService;

    public DesignationService(
        IDesignationRepository repo,
        IDepartmentRepository deptRepo,
        IAuditService auditService)
    {
        _repo         = repo;
        _deptRepo     = deptRepo;
        _auditService = auditService;
    }

    public async Task<DesignationResponseDto> CreateAsync(CreateDesignationDto dto)
    {
        if (await _repo.DesignationCodeExistsAsync(dto.DesignationCode.ToUpper()))
            throw new ConflictException($"Designation code '{dto.DesignationCode}' already exists.");

        _ = await _deptRepo.GetByIdAsync(dto.DepartmentId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Department, dto.DepartmentId);

        var desig = new Designation
        {
            DesignationCode = dto.DesignationCode.ToUpper(),
            DesignationName = dto.DesignationName,
            DepartmentId    = dto.DepartmentId,
            GradeLevel      = dto.GradeLevel,
            IsActive        = true
        };

        await _repo.AddAsync(desig);
        await _auditService.LogAsync("Create", AppConstants.EntityNames.Designation, desig.DesignationId.ToString(), newValues: dto);
        return await GetByIdAsync(desig.DesignationId);
    }

    public async Task<DesignationResponseDto> UpdateAsync(int designationId, UpdateDesignationDto dto)
    {
        var desig = await _repo.GetByIdAsync(designationId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Designation, designationId);

        _ = await _deptRepo.GetByIdAsync(dto.DepartmentId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Department, dto.DepartmentId);

        desig.DesignationName = dto.DesignationName;
        desig.DepartmentId    = dto.DepartmentId;
        desig.GradeLevel      = dto.GradeLevel;
        desig.IsActive        = dto.IsActive;

        await _repo.UpdateAsync(desig);
        await _auditService.LogAsync("Update", AppConstants.EntityNames.Designation, designationId.ToString(), newValues: dto);
        return await GetByIdAsync(designationId);
    }

    public async Task<DesignationResponseDto> GetByIdAsync(int designationId)
    {
        var items = await _repo.GetActiveAsync();
        var desig = items.FirstOrDefault(d => d.DesignationId == designationId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Designation, designationId);
        return MapToResponse(desig);
    }

    public async Task<IEnumerable<DesignationResponseDto>> GetAllAsync()
    {
        var items = await _repo.GetActiveAsync();
        return items.Select(MapToResponse);
    }

    public async Task<IEnumerable<DesignationResponseDto>> GetByDepartmentAsync(int departmentId)
    {
        var items = await _repo.GetByDepartmentAsync(departmentId);
        return items.Select(MapToResponse);
    }

    public async Task DeleteAsync(int designationId)
    {
        var desig = await _repo.GetByIdAsync(designationId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Designation, designationId);

        desig.IsActive = false;
        await _repo.UpdateAsync(desig);
        await _auditService.LogAsync("Delete", AppConstants.EntityNames.Designation, designationId.ToString());
    }

    private static DesignationResponseDto MapToResponse(Designation d) => new()
    {
        DesignationId   = d.DesignationId,
        DesignationCode = d.DesignationCode,
        DesignationName = d.DesignationName,
        DepartmentId    = d.DepartmentId,
        DepartmentName  = d.Department?.DepartmentName ?? string.Empty,
        GradeLevel      = d.GradeLevel,
        IsActive        = d.IsActive,
        CreatedAt       = d.CreatedAt
    };
}
