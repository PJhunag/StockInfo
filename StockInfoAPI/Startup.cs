using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace StockInfoAPI {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            services.AddControllers ();
            /*            services.AddCors (options => {
                           options.AddPolicy ("CorsPolicy",
                               builder => builder.AllowAnyOrigin ()
                               .AllowAnyMethod ()
                               .AllowAnyHeader ()
                               .AllowCredentials ());
                       }); */
            services.AddOptions ();
            //services.AddSwaggerGen();
            ///Authentication configuration went here.
            services.AddSingleton<IConfiguration> (Configuration);
            services.AddMvc ();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen (c => { c.SwaggerDoc ("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "CqingForumAPI", Version = "v1" }); });

            //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"; //引用System.Reflection

            //var xmlPath = Path.Combine (AppContext.BaseDirectory, xmlFile); //引用System.IO;

            //.IncludeXmlComments (xmlPath);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            }

            app.UseHttpsRedirection ();

            app.UseRouting ();

            app.UseAuthorization ();

            app.UseEndpoints (endpoints => {
                endpoints.MapControllers ();
            });

            //swagger
            app.UseSwagger ();
            app.UseSwaggerUI (c => { c.SwaggerEndpoint ("/swagger/v1/swagger.json", "CqingForumAPI V1"); });
        }
    }
}