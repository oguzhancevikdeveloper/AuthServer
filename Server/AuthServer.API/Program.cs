using AuthServer.Core.Configuration;
using AuthServer.Core.Models;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using AuthServer.Data;
using AuthServer.Data.Repositories;
using AuthServer.Service.Services;
using AuthServer.Shared.Configuration;
using AuthServer.Shared.Services;
using AuthServer.Shared.Extensions;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();

builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOptions"));
builder.Services.Configure<List<Client>>(builder.Configuration.GetSection("Clients"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("FrostlineGamesAuthServerConnection"),
        sqlServerOptionsAction =>
        {
            sqlServerOptionsAction.MigrationsAssembly("AuthServer.Data");
        });
});

builder.Services.AddIdentity<UserApp, IdentityRole>(opt =>
{
    opt.User.RequireUniqueEmail = true;
    opt.Password.RequireNonAlphanumeric = false;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();


builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,opt =>
{
    var tokeOptions = builder.Configuration.GetSection("TokenOptions").Get<CustomTokenOption>();
    opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        ValidIssuer = tokeOptions.Issuer,
        ValidAudience = tokeOptions.Audience[0],
        IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokeOptions.SecurityKey),
        ValidateIssuerSigningKey = true,
        ValidateAudience = true,
        ValidateIssuer =true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericService<,>),typeof(GenericService<,>));
builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();


builder.Services.AddControllers().AddFluentValidation(options =>
{
    options.RegisterValidatorsFromAssemblyContaining<Program>();
});

builder.Services.UseCustomValidationResponse();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UdemyAuthServer.API", Version = "v1" });
});
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "UdemyAuthServer.API v1"));
}
app.UseCustomException();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();

app.Run();
