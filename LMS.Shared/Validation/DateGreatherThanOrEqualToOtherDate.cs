using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.Validation;

public class DateGreatherThanOrEqualToOtherDate : ValidationAttribute
{
    private readonly string _comparisonProperty;

    public DateGreatherThanOrEqualToOtherDate(string comparisonProperty)
    {
        _comparisonProperty = comparisonProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var currentValue = value as DateTime?;

        var property = validationContext.ObjectType.GetProperty(_comparisonProperty) ??
            throw new ArgumentException($"Property \"{_comparisonProperty}\" kunde inte hittas.");

        var comparisonValue = property.GetValue(validationContext.ObjectInstance) as DateTime?;

        if (currentValue.HasValue && comparisonValue.HasValue && currentValue < comparisonValue)
        {
            return new ValidationResult(ErrorMessage ??
                $"{validationContext.MemberName} får inte vara tidigare än {_comparisonProperty}.");
        }

        return ValidationResult.Success;
    }
}
