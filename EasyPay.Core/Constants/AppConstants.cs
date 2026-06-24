namespace EasyPay.Core.Constants;

public static class AppConstants
{
    public static class Roles
    {
        public const string Admin            = "Admin";
        public const string HRManager        = "HRManager";
        public const string PayrollProcessor  = "PayrollProcessor";
        public const string Employee          = "Employee";
        public const string Manager           = "Manager";
        public const string AdminOrHR         = "Admin,HRManager";
        public const string AdminHROrPayroll   = "Admin,HRManager,PayrollProcessor";
        public const string AdminHROrManager   = "Admin,HRManager,Manager";
    }

    public static class Jwt
    {
        public const string TokenType         = "Bearer";
        public const int AccessTokenMinutes   = 60;
        public const int RefreshTokenDays     = 7;
    }

    public static class ErrorMessages
    {
        public const string Unauthorized          = "You are not authorized to perform this action.";
        public const string NotFound              = "The requested resource was not found.";
        public const string ValidationFailed      = "One or more validation errors occurred.";
        public const string InternalServerError   = "An unexpected error occurred. Please try again.";
        public const string InvalidCredentials    = "Invalid email or password.";
        public const string AccountLocked         = "Your account has been locked. Please try again later.";
        public const string AccountInactive       = "Your account is inactive. Please contact HR.";
        public const string TokenExpired          = "Your session has expired. Please log in again.";
        public const string DuplicateEmail        = "A user with this email already exists.";
        public const string DuplicateUsername     = "A user with this username already exists.";
        public const string InvalidRefreshToken   = "Invalid or expired refresh token.";
        public const string PasswordResetInvalid  = "Invalid or expired password reset token.";
    }

    public static class SuccessMessages
    {
        public const string Created     = "Resource created successfully.";
        public const string Updated     = "Resource updated successfully.";
        public const string Deleted     = "Resource deleted successfully.";
        public const string LoggedIn    = "Login successful.";
        public const string LoggedOut   = "Logout successful.";
    }

    public static class Pagination
    {
        public const int DefaultPage     = 1;
        public const int DefaultPageSize = 10;
        public const int MaxPageSize     = 100;
    }

    public static class Security
    {
        public const int MaxFailedAttempts  = 5;
        public const int LockoutMinutes     = 30;
        public const int PasswordResetHours = 24;
    }
}
