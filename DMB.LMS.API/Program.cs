using System.Text;
using Dmb.Lms.Api.Filters;
using Dmb.Lms.Data.Context;
using Dmb.Lms.Data.Mapper;
using Dmb.Lms.Data.Repository.Implementation;
using Dmb.Lms.Data.Repository.Implementation.Auth;
using Dmb.Lms.Data.Repository.Interface;
using Dmb.Lms.Data.Repository.Interface.Auth;
using Dmb.Lms.Model.Abstractions;
using Dmb.Lms.Service.Implementation;
using Dmb.Lms.Service.Implementation.Auth;
using Dmb.Lms.Service.Implementation.Email;
using Dmb.Lms.Service.Interface;
using Dmb.Lms.Service.Interface.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

#region Database
builder.Services.AddDbContext<LmsContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("LmsDb")));
#endregion

#region JWT
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "dmblms";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "dmblms";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();
#endregion

#region CORS
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? ["http://localhost:3000"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("LmsWeb", policy =>
        policy.SetIsOriginAllowed(origin =>
            {
                if (corsOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase)) return true;
                return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                    && (uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase)
                        || uri.Host.Equals("dmbwebsolutions.com", StringComparison.OrdinalIgnoreCase)
                        || uri.Host.Equals("www.dmbwebsolutions.com", StringComparison.OrdinalIgnoreCase));
            })
            .AllowAnyHeader()
            .AllowAnyMethod());
});
#endregion

#region AutoMapper + cache
builder.Services.AddAutoMapper(typeof(LmsMappingProfile));
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
#endregion

#region Repositories
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
builder.Services.AddScoped<ILmsRepository, LmsRepository>();
#endregion

#region Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<ILmsService, LmsService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IActivationEmailSender>(sp => sp.GetRequiredService<EmailService>());
builder.Services.AddScoped<IPasswordResetEmailSender>(sp => sp.GetRequiredService<EmailService>());
builder.Services.AddScoped<LocationContextFilter>();
#endregion

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DMB LMS API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("LmsWeb");
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var jti = context.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
        if (!string.IsNullOrWhiteSpace(jti))
        {
            var authService = context.RequestServices.GetRequiredService<IAuthService>();
            if (await authService.IsJtiRevokedAsync(jti, context.RequestAborted))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }
    }

    await next();
});

app.MapControllers();
app.MapGet("/", () => Results.Ok(new { status = "ok" }));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.Run();
