using System;
using System.ComponentModel.DataAnnotations;
using IotaWebApp.Models; // Add this directive

public class RequiredIfContentTypeIsAttribute : ValidationAttribute
{
    private readonly string _expectedContentType;

    public RequiredIfContentTypeIsAttribute(string expectedContentType)
    {
        _expectedContentType = expectedContentType;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var instance = (WebsiteContent)validationContext.ObjectInstance;
        if (instance.ContentType == _expectedContentType && string.IsNullOrWhiteSpace((string)value))
        {
            return new ValidationResult($"{validationContext.DisplayName} is required when ContentType is '{_expectedContentType}'.");
        }
        return ValidationResult.Success;
    }
}
