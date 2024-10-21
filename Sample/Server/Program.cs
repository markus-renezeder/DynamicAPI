using DynamicAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Server.Models;
using Server.Services;
using Shared.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<IPeopleService, PeopleService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();

builder.Services.AddScoped<IAuthorizationHandler, AuthorizationHandler>();

builder.Services.AddAuthorization(options =>
{
    var policyUser = new AuthorizationPolicyBuilder().AddRequirements(new AuthorizationRequirement("user")).Build();
    options.AddPolicy("user", policyUser);

    var policyAdmin = new AuthorizationPolicyBuilder().AddRequirements(new AuthorizationRequirement("admin")).Build();
    options.AddPolicy("admin", policyAdmin);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDynamicAPIExceptionHandler();

app.AddDynamicController<IPeopleService>();
app.AddDynamicController<ICompanyService>();

app.Run();
