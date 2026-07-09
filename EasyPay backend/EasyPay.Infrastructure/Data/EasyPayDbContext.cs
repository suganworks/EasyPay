using EasyPay.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EasyPay.Infrastructure.Data;

public class EasyPayDbContext : DbContext
{
    public EasyPayDbContext(DbContextOptions<EasyPayDbContext> options) : base(options) { }

    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Designation> Designations { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<PayrollPolicy> PayrollPolicies { get; set; }
    public DbSet<SalaryStructure> SalaryStructures { get; set; }
    public DbSet<Payroll> Payrolls { get; set; }
    public DbSet<Benefit> Benefits { get; set; }
    public DbSet<EmployeeBenefit> EmployeeBenefits { get; set; }
    public DbSet<LeaveType> LeaveTypes { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<Timesheet> Timesheets { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Role ──────────────────────────────────────────────────────
        modelBuilder.Entity<Role>(e =>
        {
            e.HasKey(x => x.RoleId);
            e.Property(x => x.RoleName).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.RoleName).IsUnique();
        });

        // ── User ──────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.UserId);
            e.Property(x => x.Username).IsRequired().HasMaxLength(100);
            e.Property(x => x.Email).IsRequired().HasMaxLength(255);
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(500);
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.Username).IsUnique();
            e.HasOne(x => x.Role)
             .WithMany(r => r.Users)
             .HasForeignKey(x => x.RoleId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Department ────────────────────────────────────────────────
        modelBuilder.Entity<Department>(e =>
        {
            e.HasKey(x => x.DepartmentId);
            e.Property(x => x.DepartmentCode).IsRequired().HasMaxLength(20);
            e.Property(x => x.DepartmentName).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.DepartmentCode).IsUnique();
        });

        // ── Designation ───────────────────────────────────────────────
        modelBuilder.Entity<Designation>(e =>
        {
            e.HasKey(x => x.DesignationId);
            e.Property(x => x.DesignationCode).IsRequired().HasMaxLength(20);
            e.Property(x => x.DesignationName).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.DesignationCode).IsUnique();
            e.HasOne(x => x.Department)
             .WithMany(d => d.Designations)
             .HasForeignKey(x => x.DepartmentId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Employee ──────────────────────────────────────────────────
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasKey(x => x.EmployeeId);
            e.Property(x => x.EmployeeCode).IsRequired().HasMaxLength(20);
            e.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            e.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            e.Property(x => x.TaxWithholding).HasColumnType("decimal(5,2)");
            e.HasIndex(x => x.EmployeeCode).IsUnique();
            e.Ignore(x => x.FullName);

            e.HasOne(x => x.User)
             .WithOne(u => u.Employee)
             .HasForeignKey<Employee>(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Department)
             .WithMany(d => d.Employees)
             .HasForeignKey(x => x.DepartmentId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Designation)
             .WithMany(d => d.Employees)
             .HasForeignKey(x => x.DesignationId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Manager)
             .WithMany(m => m.DirectReports)
             .HasForeignKey(x => x.ManagerId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── PayrollPolicy ─────────────────────────────────────────────
        modelBuilder.Entity<PayrollPolicy>(e =>
        {
            e.HasKey(x => x.PolicyId);
            e.Property(x => x.PolicyName).IsRequired().HasMaxLength(100);
            e.Property(x => x.OvertimeRate).HasColumnType("decimal(5,2)");
            e.Property(x => x.PfEmployeeRate).HasColumnType("decimal(5,2)");
            e.Property(x => x.PfEmployerRate).HasColumnType("decimal(5,2)");
            e.Property(x => x.EsiEmployeeRate).HasColumnType("decimal(5,2)");
            e.Property(x => x.EsiEmployerRate).HasColumnType("decimal(5,2)");
            e.Property(x => x.ProfessionalTax).HasColumnType("decimal(10,2)");
            e.Property(x => x.GratuityRate).HasColumnType("decimal(5,2)");
            e.Property(x => x.WorkingHoursDay).HasColumnType("decimal(4,2)");
        });

        // ── SalaryStructure ───────────────────────────────────────────
        modelBuilder.Entity<SalaryStructure>(e =>
        {
            e.HasKey(x => x.SalaryStructureId);
            e.Property(x => x.BasicSalary).HasColumnType("decimal(18,2)");
            e.Property(x => x.HRA).HasColumnType("decimal(18,2)");
            e.Property(x => x.ConveyanceAllowance).HasColumnType("decimal(18,2)");
            e.Property(x => x.MedicalAllowance).HasColumnType("decimal(18,2)");
            e.Property(x => x.SpecialAllowance).HasColumnType("decimal(18,2)");
            e.Property(x => x.LTA).HasColumnType("decimal(18,2)");
            e.Property(x => x.OtherAllowances).HasColumnType("decimal(18,2)");
            e.Ignore(x => x.GrossSalary);

            e.HasOne(x => x.Employee)
             .WithMany(emp => emp.SalaryStructures)
             .HasForeignKey(x => x.EmployeeId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Policy)
             .WithMany(p => p.SalaryStructures)
             .HasForeignKey(x => x.PolicyId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Payroll ───────────────────────────────────────────────────
        modelBuilder.Entity<Payroll>(e =>
        {
            e.HasKey(x => x.PayrollId);
            e.Property(x => x.BasicSalary).HasColumnType("decimal(18,2)");
            e.Property(x => x.HRA).HasColumnType("decimal(18,2)");
            e.Property(x => x.ConveyanceAllow).HasColumnType("decimal(18,2)");
            e.Property(x => x.MedicalAllow).HasColumnType("decimal(18,2)");
            e.Property(x => x.SpecialAllow).HasColumnType("decimal(18,2)");
            e.Property(x => x.OtherAllowances).HasColumnType("decimal(18,2)");
            e.Property(x => x.OvertimePay).HasColumnType("decimal(18,2)");
            e.Property(x => x.BonusAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.GrossEarnings).HasColumnType("decimal(18,2)");
            e.Property(x => x.PfEmployee).HasColumnType("decimal(18,2)");
            e.Property(x => x.PfEmployer).HasColumnType("decimal(18,2)");
            e.Property(x => x.EsiEmployee).HasColumnType("decimal(18,2)");
            e.Property(x => x.EsiEmployer).HasColumnType("decimal(18,2)");
            e.Property(x => x.ProfessionalTax).HasColumnType("decimal(18,2)");
            e.Property(x => x.IncomeTax).HasColumnType("decimal(18,2)");
            e.Property(x => x.OtherDeductions).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalDeductions).HasColumnType("decimal(18,2)");
            e.Property(x => x.NetSalary).HasColumnType("decimal(18,2)");
            e.Property(x => x.OvertimeHours).HasColumnType("decimal(8,2)");

            e.HasOne(x => x.Employee)
             .WithMany(emp => emp.Payrolls)
             .HasForeignKey(x => x.EmployeeId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Policy)
             .WithMany(p => p.Payrolls)
             .HasForeignKey(x => x.PolicyId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.SalaryStructure)
             .WithMany(s => s.Payrolls)
             .HasForeignKey(x => x.SalaryStructureId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Benefit ───────────────────────────────────────────────────
        modelBuilder.Entity<Benefit>(e =>
        {
            e.HasKey(x => x.BenefitId);
            e.Property(x => x.BenefitCode).IsRequired().HasMaxLength(20);
            e.HasIndex(x => x.BenefitCode).IsUnique();
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        });

        // ── EmployeeBenefit ───────────────────────────────────────────
        modelBuilder.Entity<EmployeeBenefit>(e =>
        {
            e.HasKey(x => x.EmployeeBenefitId);
            e.Property(x => x.OverrideAmount).HasColumnType("decimal(18,2)");

            e.HasOne(x => x.Employee)
             .WithMany(emp => emp.EmployeeBenefits)
             .HasForeignKey(x => x.EmployeeId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Benefit)
             .WithMany(b => b.EmployeeBenefits)
             .HasForeignKey(x => x.BenefitId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── LeaveType ─────────────────────────────────────────────────
        modelBuilder.Entity<LeaveType>(e =>
        {
            e.HasKey(x => x.LeaveTypeId);
            e.Property(x => x.LeaveTypeName).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.LeaveTypeName).IsUnique();
            e.Property(x => x.LeaveCode).IsRequired().HasMaxLength(10);
            e.HasIndex(x => x.LeaveCode).IsUnique();
        });

        // ── LeaveRequest ──────────────────────────────────────────────
        modelBuilder.Entity<LeaveRequest>(e =>
        {
            e.HasKey(x => x.LeaveRequestId);

            e.HasOne(x => x.Employee)
             .WithMany(emp => emp.LeaveRequests)
             .HasForeignKey(x => x.EmployeeId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.LeaveType)
             .WithMany(lt => lt.LeaveRequests)
             .HasForeignKey(x => x.LeaveTypeId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ApprovedBy)
             .WithMany()
             .HasForeignKey(x => x.ApprovedById)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── Timesheet ─────────────────────────────────────────────────
        modelBuilder.Entity<Timesheet>(e =>
        {
            e.HasKey(x => x.TimesheetId);
            e.Property(x => x.HoursWorked).HasColumnType("decimal(5,2)");
            e.Property(x => x.OvertimeHours).HasColumnType("decimal(5,2)");
            e.HasIndex(x => new { x.EmployeeId, x.WorkDate }).IsUnique();

            e.HasOne(x => x.Employee)
             .WithMany(emp => emp.Timesheets)
             .HasForeignKey(x => x.EmployeeId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ApprovedBy)
             .WithMany()
             .HasForeignKey(x => x.ApprovedById)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── Notification ──────────────────────────────────────────────
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.NotificationId);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Message).IsRequired().HasMaxLength(1000);

            e.HasOne(x => x.User)
             .WithMany(u => u.Notifications)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── AuditLog ──────────────────────────────────────────────────
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.AuditLogId);
            e.Property(x => x.Action).IsRequired().HasMaxLength(100);
            e.Property(x => x.OldValues).HasColumnType("nvarchar(max)");
            e.Property(x => x.NewValues).HasColumnType("nvarchar(max)");
        });

        // ── Seed Data ──────────────────────────────────────────────────
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Admin",            Description = "System Administrator",         IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Role { RoleId = 2, RoleName = "HRManager",        Description = "HR Manager",                   IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Role { RoleId = 3, RoleName = "PayrollProcessor",  Description = "Payroll Processor",            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Role { RoleId = 4, RoleName = "Employee",          Description = "Regular Employee",             IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Role { RoleId = 5, RoleName = "Manager",           Description = "Team Manager / Supervisor",    IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<LeaveType>().HasData(
            new LeaveType { LeaveTypeId = 1, LeaveTypeName = "Casual Leave",     LeaveCode = "CL",  MaxDaysPerYear = 12, IsCarryForward = false, MaxCarryForward = 0,  IsPaid = true,  IsActive = true },
            new LeaveType { LeaveTypeId = 2, LeaveTypeName = "Sick Leave",       LeaveCode = "SL",  MaxDaysPerYear = 12, IsCarryForward = false, MaxCarryForward = 0,  IsPaid = true,  IsActive = true },
            new LeaveType { LeaveTypeId = 3, LeaveTypeName = "Privilege Leave",  LeaveCode = "PL",  MaxDaysPerYear = 15, IsCarryForward = true,  MaxCarryForward = 15, IsPaid = true,  IsActive = true },
            new LeaveType { LeaveTypeId = 4, LeaveTypeName = "Loss of Pay",      LeaveCode = "LOP", MaxDaysPerYear = 0,  IsCarryForward = false, MaxCarryForward = 0,  IsPaid = false, IsActive = true },
            new LeaveType { LeaveTypeId = 5, LeaveTypeName = "Compensatory Off", LeaveCode = "CO",  MaxDaysPerYear = 0,  IsCarryForward = true,  MaxCarryForward = 5,  IsPaid = true,  IsActive = true }
        );
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
