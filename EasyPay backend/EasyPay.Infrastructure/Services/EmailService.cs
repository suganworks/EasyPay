using EasyPay.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace EasyPay.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger        = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetLink)
    {
        var subject = "EasyPay — Password Reset Request";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; padding: 20px; background-color: #f9f9f9;'>
                <h2 style='color: #2c3e50; text-align: center; border-bottom: 2px solid #3498db; padding-bottom: 10px;'>Password Reset Request</h2>
                <p style='color: #333; font-size: 16px; line-height: 1.5;'>Hello,</p>
                <p style='color: #333; font-size: 16px; line-height: 1.5;'>We received a request to reset your EasyPay account password. Click the button below to proceed. This link is valid for 30 minutes.</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{resetLink}' style='background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px; display: inline-block;'>Reset Password</a>
                </div>
                <p style='color: #555; font-size: 14px; line-height: 1.5;'>If the button doesn't work, you can copy and paste this link into your browser:</p>
                <p style='color: #3498db; font-size: 12px; word-break: break-all;'>{resetLink}</p>
                <p style='color: #555; font-size: 14px; line-height: 1.5; margin-top: 20px;'>If you did not request a password reset, please ignore this email or contact support if you have concerns.</p>
                <br/>
                <p style='color: #7f8c8d; font-size: 14px; border-top: 1px solid #e0e0e0; padding-top: 10px;'>Best Regards,<br/><strong>The EasyPay Team</strong></p>
            </div>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPayslipEmailAsync(string toEmail, string employeeName,
        string payPeriod, decimal netSalary)
    {
        var subject = $"EasyPay — Payslip for {payPeriod}";
        var body = $@"
            <h2>Salary Credited</h2>
            <p>Dear {employeeName},</p>
            <p>Your salary for <strong>{payPeriod}</strong> has been processed.</p>
            <p>Net Salary: <strong>₹{netSalary:N2}</strong></p>
            <p>Please login to the EasyPay portal to view your detailed payslip.</p>
            <br/><p>— EasyPay Payroll Team</p>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendLeaveStatusEmailAsync(string toEmail, string employeeName,
        string leaveType, string status, string? reason = null)
    {
        var subject = $"EasyPay — Leave Request {status}";
        var body = $@"
            <h2>Leave Request {status}</h2>
            <p>Dear {employeeName},</p>
            <p>Your <strong>{leaveType}</strong> request has been <strong>{status.ToLower()}</strong>.</p>
            {(reason != null ? $"<p>Reason: {reason}</p>" : "")}
            <p>Login to EasyPay portal for details.</p>
            <br/><p>— EasyPay HR Team</p>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string employeeName, string tempPassword)
    {
        var subject = "Welcome to EasyPay";
        var body = $@"
            <h2>Welcome to EasyPay, {employeeName}!</h2>
            <p>Your account has been created.</p>
            <p>Email: <strong>{toEmail}</strong></p>
            <p>Temporary Password: <strong>{tempPassword}</strong></p>
            <p>Please login and change your password immediately.</p>
            <br/><p>— EasyPay Team</p>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings");
        var host         = smtpSettings["Host"];
        var portStr      = smtpSettings["Port"];
        var username     = smtpSettings["Username"];
        var password     = smtpSettings["Password"];
        var fromEmail    = smtpSettings["FromEmail"];
        var fromName     = smtpSettings["FromName"] ?? "EasyPay";
        var enableSsl    = bool.TryParse(smtpSettings["EnableSsl"], out var ssl) && ssl;

        // If SMTP is not configured, log and skip (dev mode)
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username))
        {
            _logger.LogWarning("SMTP not configured. Email to {To} with subject '{Subject}' was not sent.",
                toEmail, subject);
            _logger.LogDebug("Email body: {Body}", htmlBody);
            return;
        }

        try
        {
            using var client = new SmtpClient(host)
            {
                Port        = int.TryParse(portStr, out var p) ? p : 587,
                Credentials = new NetworkCredential(username, password),
                EnableSsl   = enableSsl
            };

            var message = new MailMessage
            {
                From       = new MailAddress(fromEmail ?? username, fromName),
                Subject    = subject,
                Body       = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {Email} — Subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            // Don't throw — email failure should not crash the main flow
        }
    }
}
