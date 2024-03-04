using CandidateDashboardApi.Interfaces;
using CandidateDashboardApi.Services;
using Domain.Entities;
using Domain.Entities.CandidateEntities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Presistance;
using Serilog;
using Serilog.Events;
using System.Text;
using WebApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var containerName = "profilephoto";
var template = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
string azureBlobStorageConnectionString = builder.Configuration.GetConnectionString("AzureBlobStorage");

if (string.IsNullOrWhiteSpace(azureBlobStorageConnectionString))
{
    throw new InvalidOperationException("Azure Blob Storage connection string is not configured properly.");
}

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CandidateDashboardContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<CandidateDashboardContext>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularCandidateDashboardFront",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(hostingContext.Configuration)
        .Enrich.FromLogContext());


builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<EmployerService>();
builder.Services.AddScoped<IBlobService, BlobService>();
builder.Services.AddScoped<IOpenAIService, OpenAIService>(provider =>
{
    return new OpenAIService(builder.Configuration["OpenAI:ApiKey"]);
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AngularCandidateDashboardFront");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();



using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new List<string> { nameof(Candidate), nameof(Employer) })
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

app.Run();
