using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.Validation;

public class DateLessThanOrEqualToOtherDate : ValidationAttribute
{
    private readonly string _comparisonProperty;

    public DateLessThanOrEqualToOtherDate(string comparisonProperty)
    {
        _comparisonProperty = comparisonProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var currentValue = value as DateTime?;

        var property = validationContext.ObjectType.GetProperty(_comparisonProperty) ??
            throw new ArgumentException($"Property '{_comparisonProperty}' not found.");

        var comparisonValue = property.GetValue(validationContext.ObjectInstance) as DateTime?;

        if (currentValue.HasValue && comparisonValue.HasValue && currentValue > comparisonValue)
        {
            return new ValidationResult(ErrorMessage ??
                $"{validationContext.MemberName} must be less than or equal to {_comparisonProperty}.");
        }

        return ValidationResult.Success;
    }
}
