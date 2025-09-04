using System.ComponentModel.DataAnnotations;

namespace Shared.Contracts.Models;

public class ValidHttpMethodAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string method && HttpMethods.IsValid(method))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"'{value}' is not a valid HTTP method. Valid methods are: {string.Join(", ", HttpMethods.AllMethods)}");
    }
}

public class ValidPathAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string path)
        {
            return new ValidationResult("Path must be a string");
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            return new ValidationResult("Path cannot be empty");
        }

        if (!path.StartsWith('/'))
        {
            return new ValidationResult("Path must start with '/'");
        }

        return ValidationResult.Success;
    }
}

public class ValidJsonAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null or "")
        {
            return ValidationResult.Success; // Allow null/empty
        }

        if (value is not string json)
        {
            return new ValidationResult("Value must be a string");
        }

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return ValidationResult.Success;
        }
        catch (System.Text.Json.JsonException ex)
        {
            return new ValidationResult($"Invalid JSON: {ex.Message}");
        }
    }
}
