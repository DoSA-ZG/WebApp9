using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RPPP_WebApp.Models;
using RPPP_WebApp.ModelsValidation;

namespace RPPP_WebApp
{
    public static class StartupExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            var appSection = builder.Configuration.GetSection("AppSettings");
            builder.Services.Configure<AppSettings>(appSection);
            builder.Services.AddDbContext<RPPP09Context>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RPPP09")));
            builder.Services.AddControllersWithViews();
            builder.Services
                .AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters()
                .AddValidatorsFromAssemblyContaining<SuradnikValidator>()
                .AddValidatorsFromAssemblyContaining<TransakcijaValidator>()
                .AddValidatorsFromAssemblyContaining<VrstaTransakcijeValidator>()
                .AddValidatorsFromAssemblyContaining<ValutaValidator>()
                .AddValidatorsFromAssemblyContaining<RacunValidator>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(Constants.ApiVersion, new OpenApiInfo
                {
                    Title = "RPPP09 Web API",
                    Version = Constants.ApiVersion
                });
            });



            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            #region Needed for nginx and Kestrel (do not remove or change this region)
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                 ForwardedHeaders.XForwardedProto
            });
            string pathBase = app.Configuration["PathBase"];
            if (!string.IsNullOrWhiteSpace(pathBase))
            {
                app.UsePathBase(pathBase);
            }
            #endregion

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles()
               .UseRouting()
               .UseEndpoints(endpoints =>
               {
                   endpoints.MapDefaultControllerRoute();
               });

            return app;
        }
    }
}