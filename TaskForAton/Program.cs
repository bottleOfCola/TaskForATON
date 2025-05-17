using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography;
using System.Text.Json;
using System.Web.Mvc;
using TaskForATON.DbStuff;
using TaskForATON.ViewModels;

byte[] secretkey = new byte[64];
RandomNumberGenerator.Fill(secretkey);
AuthData.Key = new SymmetricSecurityKey(secretkey);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<UserDbContext>(
    optionsAction: (DbContextOptionsBuilder x) => x.UseInMemoryDatabase("UserServiceTest").ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning)),
    ServiceLifetime.Scoped
    );

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "MyAuthServer",
            ValidateAudience = true,
            ValidAudience = "MyAuthClient",
            ValidateLifetime = true,
            IssuerSigningKey = AuthData.Key,
            ValidateIssuerSigningKey = true,
        };
    });
builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Descr",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var userDbContext = scope.ServiceProvider.GetService<UserDbContext>()!;
    if (!userDbContext.Users.Any(x => x.Admin))
    {
        userDbContext.Users.Add(new()
        {
            Guid = Guid.NewGuid(),
            Login = "admin",
            Password = "MEGA_ADMIN_OF_THIS_SITE",
            Name = "admin",
            Gender = 3,
            Birthday = null,
            Admin = true,
            CreatedOn = DateTime.Now,
            CreatedBy = "admin",
            ModifiedOn = DateTime.Now,
            ModifiedBy = "admin",
            RevokedOn = null,
            RevokedBy = null
        });
        userDbContext.SaveChanges();
    }
}

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();
app.UseDeveloperExceptionPage();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;