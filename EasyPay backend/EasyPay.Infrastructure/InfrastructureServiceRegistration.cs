using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using EasyPay.Infrastructure.Data;
using EasyPay.Infrastructure.Repositories;
using EasyPay.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyPay.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<EasyPayDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(EasyPayDbContext).Assembly.FullName)));

        // HTTP Context
        services.AddHttpContextAccessor();

        // Repositories
        services.AddScoped<IUserRepository,              UserRepository>();
        services.AddScoped<IAuditLogRepository,          AuditLogRepository>();
        services.AddScoped<INotificationRepository,      NotificationRepository>();
        services.AddScoped<IEmployeeRepository,          EmployeeRepository>();
        services.AddScoped<IDepartmentRepository,        DepartmentRepository>();
        services.AddScoped<IDesignationRepository,       DesignationRepository>();
        services.AddScoped<IPayrollPolicyRepository,     PayrollPolicyRepository>();
        services.AddScoped<ISalaryStructureRepository,   SalaryStructureRepository>();
        services.AddScoped<IPayrollRepository,           PayrollRepository>();
        services.AddScoped<ILeaveRequestRepository,      LeaveRequestRepository>();
        services.AddScoped<ILeaveTypeRepository,         LeaveTypeRepository>();
        services.AddScoped<ITimesheetRepository,         TimesheetRepository>();
        services.AddScoped<IBenefitRepository,           BenefitRepository>();
        services.AddScoped<IEmployeeBenefitRepository,   EmployeeBenefitRepository>();

        // Services
        services.AddScoped<IJwtService,              JwtService>();
        services.AddScoped<IAuthService,             AuthService>();
        services.AddScoped<IAuditService,            AuditService>();
        services.AddScoped<INotificationService,     NotificationService>();
        services.AddScoped<ICurrentUserService,      CurrentUserService>();
        services.AddScoped<IEmailService,            EmailService>();
        services.AddScoped<IEmployeeService,         EmployeeService>();
        services.AddScoped<IDepartmentService,       DepartmentService>();
        services.AddScoped<IDesignationService,      DesignationService>();
        services.AddScoped<IPayrollPolicyService,    PayrollPolicyService>();
        services.AddScoped<ISalaryStructureService,  SalaryStructureService>();
        services.AddScoped<IPayrollService,          PayrollService>();
        services.AddScoped<ILeaveService,            LeaveService>();
        services.AddScoped<ITimesheetService,        TimesheetService>();
        services.AddScoped<IBenefitService,          BenefitService>();
        services.AddScoped<IUserService,             UserService>();

        return services;
    }
}
