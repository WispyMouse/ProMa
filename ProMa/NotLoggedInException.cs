using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProMa
{
	public class NotLoggedInException : Exception
	{
		public NotLoggedInException()
		{
			throw new UnauthorizedAccessException("User is not logged in. This may happen on the initial log in check, or in the case of attempting to access a non-public api without an active login session.");
		}

		public NotLoggedInException(string message)
			: base(message)
		{
			throw new UnauthorizedAccessException(message);
		}

		public NotLoggedInException(string message, Exception inner)
			: base(message, inner)
		{
			throw new UnauthorizedAccessException(message, inner);
		}
	}
}