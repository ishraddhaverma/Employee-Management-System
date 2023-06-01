using EmployeeManagement.Models;
using EmployeeManagement.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Google;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace EmployeeManagement
{
    public class Startup
    {
        private readonly IConfiguration _config;
        public Startup(IConfiguration config)
        {
            _config = config;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AppDbContext>(options => options.UseSqlServer(_config.GetConnectionString("EmployeeDBConnection")));  //connection string
            services.AddMvc(options => options.EnableEndpointRouting = false);
            services.AddMvc(options => {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = "296984161760-mh68q599b50v29hrs3vtsejqqc5gi971.apps.googleusercontent.com";
                options.ClientSecret = "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";
            });

            services.ConfigureApplicationCookie(options => {

                options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });

           

            services.AddAuthorization(options =>
            {
            options.AddPolicy("DeleteRolePolicy",
                policy => policy.RequireClaim("Delete Role")
                .RequireClaim("Create Role"));


                options.AddPolicy("AdminRolePolicy", 
                    policy => policy.RequireRole("Admin"));

            });



            /*
            services.AddAuthorization(options =>
            {
                options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Edit Role","true"));
            });
            */

            services.AddAuthorization(options =>
            {
                options.AddPolicy("EditRolePolicy", policy =>
                    policy.RequireAssertion(context => AuthorizeAccess(context)));
            });

             bool AuthorizeAccess(AuthorizationHandlerContext context)
            {
                return context.User.IsInRole("Admin") &&
                        context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                        context.User.IsInRole("Super Admin");
            }


            services.AddControllersWithViews();
            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>(); //dependancy injection

            services.AddSingleton<IAuthorizationHandler,CanEditOnlyOtherAdminRolesAndClaimsHandler>();

            ////services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>();


            //  services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>();

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);


            })
            .AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders()
             .AddTokenProvider<CustomEmailConfirmationTokenProvider
            <ApplicationUser>>("CustomEmailConfirmation");

            services.Configure<DataProtectionTokenProviderOptions>(o =>
               o.TokenLifespan = TimeSpan.FromHours(5));
            services.Configure<CustomEmailConfirmationTokenProviderOptions>(o =>
            o.TokenLifespan = TimeSpan.FromDays(3));

            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
            services.AddSingleton<DataProtectionPurposeStrings>();


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
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }
            
            app.UseStaticFiles();
            app.UseAuthentication();

               app.UseMvc(routes =>
              {                      //convenctional routing   
                  routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
              });

            



        }
    }
}
