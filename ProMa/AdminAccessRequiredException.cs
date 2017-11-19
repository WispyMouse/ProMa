using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProMa
{
	public class AdminAccessRequiredException : Exception
	{
		public AdminAccessRequiredException()
		{
			throw new UnauthorizedAccessException("User attempted to utilize Admin-only functionality, without being logged in to an account with Admin privileges.");
		}

		public AdminAccessRequiredException(string message)
			: base(message)
		{
			throw new UnauthorizedAccessException(message);
		}

		public AdminAccessRequiredException(string message, Exception inner)
			: base(message, inner)
		{
			throw new UnauthorizedAccessException(message, inner);
		}
	}
}