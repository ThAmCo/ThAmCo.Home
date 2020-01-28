using Home.App.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace Home.App
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddTransient<AuthService>();
			services.AddHttpClient();
			services.AddControllersWithViews();

			string azureStorageUri = Environment.GetEnvironmentVariable("COOKIE_STORAGE_URI");

			// Create the new Storage URI
			Uri storageUri = new Uri(azureStorageUri);

			//Create the blob client object.
			CloudBlobClient blobClient = new CloudBlobClient(storageUri);

			//Get a reference to a container to use for the sample code, and create it if it does not exist.
			CloudBlobContainer container = blobClient.GetContainerReference("keys");

			services.AddDataProtection()
				.PersistKeysToAzureBlobStorage(container, "cookies")
				.SetApplicationName("ThAmCo");

			services.AddAuthentication("Cookies")
				.AddCookie("Cookies", options =>
				{
					options.Cookie.Name = ".ThAmCo.SharedCookie";
					options.Cookie.Path = "/";
				});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
