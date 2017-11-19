using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProMa
{
	public class InvalidLogInException : Exception
	{
		public InvalidLogInException()
		{
			throw new UnauthorizedAccessException("Invalid login credentials.");
		}

		public InvalidLogInException(string message)
			: base(message)
		{
			throw new UnauthorizedAccessException(message);
		}

		public InvalidLogInException(string message, Exception inner)
			: base(message, inner)
		{
			throw new UnauthorizedAccessException(message, inner);
		}
	}
}