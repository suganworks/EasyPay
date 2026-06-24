namespace EasyPay.Core.Interfaces.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetLink);
    Task SendPayslipEmailAsync(string toEmail, string employeeName, string payPeriod, decimal netSalary);
    Task SendLeaveStatusEmailAsync(string toEmail, string employeeName, string leaveType, string status, string? reason = null);
    Task SendWelcomeEmailAsync(string toEmail, string employeeName, string tempPassword);
}
