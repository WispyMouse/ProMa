// Taken from https://www.strathweb.com/2016/12/accessing-httpcontext-outside-of-framework-components-in-asp-net-core/ to help port from 4.5 to dotnetcore

using Microsoft.AspNetCore.Http;

namespace ProMa
{
	public static class ContextTools
	{
		private static IHttpContextAccessor _contextAccessor;

		public static Microsoft.AspNetCore.Http.HttpContext Current => _contextAccessor.HttpContext;

		internal static void Configure(IHttpContextAccessor contextAccessor)
		{
			_contextAccessor = contextAccessor;
		}
	}
}