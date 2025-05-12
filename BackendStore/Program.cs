
using System.Reflection;
using System.Text;
using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;
using RepoLayer.Helper;
using RepositoryLayer.Context;
using RepositoryLayer.Helper;
using RepositoryLayer.Interface;
using RepositoryLayer.Service;
using StackExchange.Redis;

namespace BackendStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            try
            {
                logger.Info("Starting application...");

                var builder = WebApplication.CreateBuilder(args);
                var redisConfig = builder.Configuration.GetSection("Redis:ConnectionString").Value;

                builder.Logging.ClearProviders();
                builder.Host.UseNLog();

                // Add services to the container.
                builder.Services.AddDbContext<UserContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("StartConnection")));
                builder.Services.AddScoped<IUserBL, UserImplBL>();
                builder.Services.AddScoped<IUserRL, UserImplRL>();
                builder.Services.AddScoped<IBookBL, BookImplBL>();
                builder.Services.AddScoped<IBookRL, BookImplRL>();
                builder.Services.AddScoped<IAddressBL, AddressImplBL>();
                builder.Services.AddScoped<IAddressRL, AddressImplRL>();
                builder.Services.AddScoped<IWishListBL, WishListImplBL>();
                builder.Services.AddScoped<IWishListRL, WishListImplRL>();
                builder.Services.AddScoped<ICartBL, CartImplBL>();
                builder.Services.AddScoped<ICartRL, CartImplRL>();
                builder.Services.AddScoped<IOrderBL, OrderImplBL>();
                builder.Services.AddScoped<IOrderRL, OrderImplRL>();
                builder.Services.AddSingleton<PasswordHashService>();
                builder.Services.AddSingleton<AuthService>();
                builder.Services.AddSingleton<EmailService>();
                builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfig));

                builder.Services.AddControllers();

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", policy =>
                    {
                        policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    });
                });

                builder.Services.AddEndpointsApiExplorer();

                // JWT Authentication
                var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:Key"]);
                builder.Services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Book Store", Version = "v1" });

                    var securityScheme = new OpenApiSecurityScheme
                    {
                        Name = "JWT Authentication",
                        Description = "Enter JWT token **_only_**",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Reference = new OpenApiReference
                        {
                            Id = JwtBearerDefaults.AuthenticationScheme,
                            Type = ReferenceType.SecurityScheme
                        }
                    };
                    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });

                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                });

                builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

                var app = builder.Build();
                app.UseCors("AllowAll");
                app.MapReverseProxy();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();
                app.Run();
            }
            catch (Exception ex)
            {
                // Catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                return;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

    }
}
