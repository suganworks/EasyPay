using EasyPay.API.Extensions;
using EasyPay.API.Middleware;
using EasyPay.Infrastructure;
using EasyPay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ─── Serilog ───────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// ─── Services ──────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

// Infrastructure (DbContext + Repos + Services)
builder.Services.AddInfrastructureServices(builder.Configuration);

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",        p => p.RequireRole("Admin"));
    options.AddPolicy("HROrAdmin",        p => p.RequireRole("Admin", "HRManager"));
    options.AddPolicy("PayrollAccess",    p => p.RequireRole("Admin", "HRManager", "PayrollProcessor"));
    options.AddPolicy("ManagerAccess",    p => p.RequireRole("Admin", "HRManager", "Manager"));
    options.AddPolicy("AllAuthenticated", p => p.RequireAuthenticatedUser());
});

// API Versioning
builder.Services.AddApiVersioningConfig();

// ─── Swagger (manual setup to avoid versioning conflicts) ──────────────────
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "EasyPay Payroll Management System API",
        Version     = "v1",
        Description = "Production-ready REST API for EasyPay — Hexaware Technologies."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.ApiKey,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter: Bearer {your-JWT-token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Tell Swagger to resolve versioning conflicts
    options.DocInclusionPredicate((_, _) => true);
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:3000";
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ─── App Pipeline ──────────────────────────────────────────────────────────
var app = builder.Build();

// ─── Apply Migrations & Seed Database ───────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EasyPayDbContext>();
    try
    {
        Log.Information("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        Log.Information("Migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Error applying migrations, continuing...");
    }
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();

// Always show Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EasyPay API v1");
    c.RoutePrefix = string.Empty; // Opens Swagger at root URL https://localhost:7245
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

try
{
    Log.Information("Starting EasyPay API...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "EasyPay API failed to start.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
