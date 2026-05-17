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
    public static class Category
    {
        public const string NameRequired = "Category name is required.";
        public const string NameTooLong = "Category name cannot exceed 100 characters.";
        public const string DescriptionTooLong = "Category description cannot exceed 500 characters.";
    }

    public static class Order
    {
        public const string UserIdRequired = "User id is required.";
        public const string ItemsRequired = "Order must have at least one item.";
        public const string NotFound = "Order not found.";
        public const string CannotCancelOtherUser = "Cannot cancel another user's order.";
        public const string OnlyPendingCanBeCancelled = "Only pending orders can be cancelled.";
    }

    public static class OrderItem
    {
        public const string TeaIdRequired = "Tea id is required.";
        public const string QuantityMustBePositive = "Quantity must be greater than zero.";
        public const string UnitPriceMustBePositive = "Unit price must be greater than zero.";
        public const string TeaNotFound = "Tea not found.";
        public const string InsufficientStock = "Insufficient stock for";
    }
}

