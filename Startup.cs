using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using ShoppingCartRestAPI.Data;

namespace ShoppingCartRestAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shopping Cart API", Version = "v1" });

                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("https://accounts.google.com/o/oauth2/auth"), // Google OAuth authorization URL
                            TokenUrl = new Uri("https://accounts.google.com/o/oauth2/token"), // Google OAuth token URL
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "OpenID" },
                                { "profile", "Profile" }
                            }
                        }
                    }
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                        },
                        new[] { "openid", "profile" }
                    }
                });

                // Add support for uploading files
                c.OperationFilter<FileUploadFilter>();
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", builder =>
                {
                    builder.WithOrigins("https://localhost:7085")
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            var googleClientId = Configuration["Authentication:Google:ClientId"];
            var googleClientSecret = Configuration["Authentication:Google:ClientSecret"];

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddGoogle(options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
                options.CallbackPath = "/signin-google";

            });

            //Rate limiting to protect the API against abuse
            services.AddMemoryCache();
            //services.Configure<IpRateLimitOptions>(options =>
            //{
            //    options.GeneralRules = new List<RateLimitRule>
            //    {
            //        new RateLimitRule
            //        {
            //            Endpoint = "*",
            //            Limit = 100, // Limit per client
            //            Period = "1m" // 1 minute
            //        }
            //    };
            //});

            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)  
                .WriteTo.Console()  
                .WriteTo.File("shoppingCartApi.log", rollingInterval: RollingInterval.Day)  
                .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.AddSerilog(); // Add Serilog as the logger
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApplicationDbContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Shopping Cart API v1");
                    options.OAuthClientId(Configuration["Authentication:Google:ClientId"]);
                    options.OAuthClientSecret(Configuration["Authentication:Google:ClientSecret"]);
                    options.OAuthAppName("Shopping Cart");
                });
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowSpecificOrigin");
            app.UseAuthentication();
            app.UseAuthorization();
            //app.UseIpRateLimiting();
           // context.Database.Migrate();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
