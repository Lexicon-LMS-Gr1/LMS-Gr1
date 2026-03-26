using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.Activity
{
    using System.ComponentModel.DataAnnotations;

    public abstract class ActivityBaseDto
    {
        [Required(ErrorMessage = "Aktivitetsnamn saknas")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Aktivitetsnamn måste vara mellan 3 och 100 tecken")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Beskrivning saknas")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Beskrivning måste vara mellan 10 och 500 tecken")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Startdatum saknas")]
        [DataType(DataType.Date)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Slutdatum saknas")]
        [DataType(DataType.Date)]
        public DateTime EndTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Aktivitetstyp saknas")]
        [Range(1, int.MaxValue, ErrorMessage = "Aktivitets-id måste vara positivt")]
        public int ActivityTypeId { get; set; }
    }
}
