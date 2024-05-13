using AuthServer.Shared.Configuration;
using AuthServer.Shared.Extensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOptions"));

var tokeOptions = builder.Configuration.GetSection("TokenOptions").Get<CustomTokenOption>();
builder.Services.AddCustomTokenAuth(tokeOptions);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "Client.API v2", Version = "v2" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v2/swagger.json", "Client.API v2"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();