using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication;
using AuthServer.Entities;
using AuthServer.Repository;



namespace AuthServer
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
            services.AddDbContext<Repository.AuthContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("Postgres")));
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.SetIsOriginAllowedToAllowWildcardSubdomains();
                });
            });
            services.AddDefaultIdentity<User>()
                .AddEntityFrameworkStores<AuthContext>();
            services.AddIdentityServer()
                .AddApiAuthorization<User, AuthContext>();
            services.AddAuthentication()
                .AddIdentityServerJwt();
            services.AddControllers();
            services.AddLogging();
            var settingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(settingsSection);
            services.AddRouting();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseRouting();

            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
