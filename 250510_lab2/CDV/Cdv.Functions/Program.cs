using System.Reflection;
using Cdv.Domain.DbContext;
using Cdv.Functions;
using Cdv.Functions.Profiles;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

string? cs = Environment.GetEnvironmentVariable("Db", EnvironmentVariableTarget.Process);

builder.Services.AddDbContext<PeopleDbContext>(options => { options.UseSqlServer(cs); });

builder.Build().Run();