using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ProMa.Hubs;
using ProMa.Models;
using ProMa.Controllers;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;

namespace ProMa
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc().AddSessionStateTempDataProvider().AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
			services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
			services.AddDistributedMemoryCache();
			services.AddHttpContextAccessor();
			services.AddSession();
			services.AddSignalR();

			services.AddDirectoryBrowser();
        }

		private StaticFileOptions GetStaticFileConfiguration()
		{
			var provider = new FileExtensionContentTypeProvider();
			provider.Mappings[".unityweb"] = "application/octect-stream";
			return new StaticFileOptions { ContentTypeProvider = provider };
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles(GetStaticFileConfiguration());

			app.UseStaticHttpContext();
			app.UseSession();

			app.UseWebSockets();

			app.UseSignalR(routes =>
			{
				routes.MapHub<LongPollHub>("/longpollHub");
			});

			app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

				routes.MapRoute(
					name: "Services",
					template: "Services/{controller=Home}/{action=Index}/{id?}");
			});

			ClearDemoAccount();
        }

		void ClearDemoAccount()
		{
			ProMaUser demoUser = ProMaUserHandler.GetUserByUserName("DemoAccount");

			if (demoUser != null)
			{
				ProMaUserHandler.PermanentlyDeleteUser(demoUser);
			}

			ProMaUser demoAccount =
				new DataController().RegisterProMaUser(new DataController.RegisterProMaUserRequestObject() { userName = "DemoAccount", md5Password = ProMaUser.ComputeMD5Hash("DemoAccount") });

			demoAccount.IsDemo = true;

			ProMaUserHandler.UpdateUser(demoAccount);
		}
    }
}
