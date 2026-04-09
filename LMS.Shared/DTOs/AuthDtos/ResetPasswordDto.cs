using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.AuthDtos
{
    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

}
