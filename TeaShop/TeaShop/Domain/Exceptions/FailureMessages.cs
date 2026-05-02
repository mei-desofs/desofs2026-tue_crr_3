namespace TeaShop.Domain.Exceptions;

public static class FailureMessages
{
    public static class Category
    {
        public const string NameRequired = "Category name is required.";
        public const string NameTooLong = "Category name cannot exceed 100 characters.";
        public const string DescriptionTooLong = "Category description cannot exceed 500 characters.";
    }
}
