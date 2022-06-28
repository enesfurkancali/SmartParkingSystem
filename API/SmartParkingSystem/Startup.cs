using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SmartParkingSystemAPI.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SmartParkingSystemAPI.Subscription;
using SmartParkingSystem.Models;
using SmartParkingSystemAPI.Subscription.Middleware;
using SmartParkingSystemAPI.Hubs;

namespace SmartParkingSystem
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
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).
             AddJwtBearer(opt =>
             {
                 opt.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateAudience = true,
                     ValidateIssuer = true,
                     ValidateLifetime = true,
                     ValidateIssuerSigningKey = true,
                     ValidIssuer = Configuration["Token:Issuer"],
                     ValidAudience = Configuration["Token:Audience"],
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Token:SecurityKey"])),
                     ClockSkew = TimeSpan.Zero

                 };

             });
            //services.AddCors(options => options.AddDefaultPolicy(policy =>
            // policy.AllowCredentials().AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(x => true)));
            

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                builder.WithOrigins("http://localhost:27535")
                       .AllowAnyMethod()
                       .AllowAnyHeader());
            });
            services.AddSignalR();
            services.AddControllers();
            services.AddDbContext<SmartParkingSystemContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default")));
            services.AddDbContext<SecurityContext>(options => options.UseInMemoryDatabase(databaseName: "SecurityDB"));
            services.AddSingleton<DatabaseSubscription<Entry>>();
            services.AddSingleton<DatabaseSubscription<Config>>();
            services.AddSingleton<DatabaseSubscription<User>>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartParkingSystem", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartParkingSystem v1"));
            }

            //Middlewares for db subscription
            app.UseDatabaseSubscription<DatabaseSubscription<Config>>("Config");
            app.UseDatabaseSubscription<DatabaseSubscription<User>>("User");
            app.UseDatabaseSubscription<DatabaseSubscription<Entry>>("Entry");

            app.UseAuthentication();
            app.UseRouting();

            app.UseCors();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ParksisHub>("/parksishub");
            });
        }
    }
}
