// Taken from https://www.strathweb.com/2016/12/accessing-httpcontext-outside-of-framework-components-in-asp-net-core/ to help port from 4.5 to dotnetcore

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Web;

public static class StaticHttpContextExtensions
{
	public static void AddHttpContextAccessor(this IServiceCollection services)
	{
		services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
	}

	public static IApplicationBuilder UseStaticHttpContext(this IApplicationBuilder app)
	{
		var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
		ProMa.ContextTools.Configure(httpContextAccessor);
		return app;
	}
}