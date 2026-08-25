namespace ECommerce.Identity.Application;

public static class IdentityErrorCodes
{
    public const string UserAlreadyExists = "USER_ALREADY_EXISTS";
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string ExternalRoleSynchronizationFailed = "EXTERNAL_ROLE_SYNCHRONIZATION_FAILED";
    public const string ExternalEmailNotVerified = "EXTERNAL_EMAIL_NOT_VERIFIED";
    public const string ExternalAccountLinkRequired = "EXTERNAL_ACCOUNT_LINK_REQUIRED";
    public const string UserDisabled = "USER_DISABLED";
}
