using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.Activity
{
    using LMS.Shared.Validation;
    using System.ComponentModel.DataAnnotations;

    public abstract class ActivityBaseDto
    {
        [Required(ErrorMessage = "Aktivitetsnamn måste anges.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Aktivitetsnamn måste vara mellan 3 och 100 tecken.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Beskrivning måste anges.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Beskrivning måste vara mellan 10 och 500 tecken.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Starttidpunkt måste anges.")]
        [DateLessThanOrEqualToOtherDate(nameof(EndTime), ErrorMessage = "Starttidpunkt får inte vara senare än sluttidpunkt.")]
        [DataType(DataType.Date)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Sluttidpunkt måste anges.")]
        [DateGreatherThanOrEqualToOtherDate(nameof(StartTime), ErrorMessage = "Sluttidpunkt får inte vara tidigare än starttidpunkt.")]
        [DataType(DataType.Date)]
        public DateTime EndTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Aktivitetstyp måste anges.")]
        [Range(1, int.MaxValue, ErrorMessage = "Aktivitets-id måste vara ett positivt tal.")]
        public int ActivityTypeId { get; set; }
    }
}
