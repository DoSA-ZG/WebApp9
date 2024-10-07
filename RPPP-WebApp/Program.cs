using NLog.Web;
using NLog;
using RPPP_WebApp;
using Microsoft.OpenApi.Models;
using RPPP_WebApp.Models;
using RPPP_WebApp.ModelsValidation;
using FluentValidation;
using FluentValidation.AspNetCore;

//NOTE: Add dependencies/services in StartupExtensions.cs and keep this file as-is

var logger = LogManager.Setup().GetCurrentClassLogger();
var builder = WebApplication.CreateBuilder(args);
logger.Debug("init main");

try
{
    builder.Host.UseNLog(new NLogAspNetCoreOptions() { RemoveLoggerFactoryFilter = false });
    var app = builder.ConfigureServices().ConfigurePipeline();

    #region Configure middleware pipeline.
    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0#middleware-order-1

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "docs";
        c.DocumentTitle = "RPPP Web Api";
        c.SwaggerEndpoint($"../swagger/{Constants.ApiVersion}/swagger.json", "RPPP WebAPI");
    });


    app.UseStaticFiles();

    app.UseRouting();

    app.MapControllers();
    #endregion

    app.Run();
    app.Run();

}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}

public partial class Program { }