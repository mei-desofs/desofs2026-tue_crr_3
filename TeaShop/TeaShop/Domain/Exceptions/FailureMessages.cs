namespace TeaShop.Domain.Exceptions;

public static class FailureMessages
{
    public static class User
    {
        public const string PasswordHashRequired = "Password hash is required.";
        public const string PasswordHashEmpty = "Password hash cannot be empty.";
        public const string NotFound = "User not found.";
        public const string AddressNotFound = "No address on file.";
        public const string EmailAlreadyExists = "A user with this email already exists.";

        public static string InvalidStaffRole(string role) => $"'{role}' is not a valid staff role.";
    }
}
