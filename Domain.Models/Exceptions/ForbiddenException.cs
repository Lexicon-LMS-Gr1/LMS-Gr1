using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions;

public class ForbiddenException : DomainException
{
	public ForbiddenException(string message, string title = "Forbidden") : base(message, title, 403)
	{
	}
}
