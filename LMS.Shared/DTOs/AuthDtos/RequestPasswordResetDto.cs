using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.AuthDtos
{
    public class RequestPasswordResetDto
    {
        public string Email { get; set; } = string.Empty;
    }
}
