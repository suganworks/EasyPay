using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;

namespace EasyPay.Infrastructure.Services;

public record SharedSupportServices(
    IAuditService AuditService,
    ICurrentUserService CurrentUser,
    IEmailService EmailService,
    INotificationService NotificationService,
    IUserRepository UserRepo
);
