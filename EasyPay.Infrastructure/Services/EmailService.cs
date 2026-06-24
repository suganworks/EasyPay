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
            <h2>Password Reset</h2>
            <p>You requested a password reset for your EasyPay account.</p>
            <p>Click the link below to reset your password (valid for 24 hours):</p>
            <p><a href='{resetLink}'>Reset Password</a></p>
            <p>Or use this token: <strong>{resetToken}</strong></p>
            <p>If you did not request this, please ignore this email.</p>
            <br/><p>— EasyPay Team</p>";

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

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings");
        var host         = smtpSettings["Host"];
        var portStr      = smtpSettings["Port"];
        var username     = smtpSettings["Username"];
        var password     = smtpSettings["Password"];
        var fromEmail    = smtpSettings["FromEmail"];
        var fromName     = smtpSettings["FromName"] ?? "EasyPay";
        var enableSsl    = bool.TryParse(smtpSettings["EnableSsl"], out var ssl) && ssl;

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
        }
    }
}
