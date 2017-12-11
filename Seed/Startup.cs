using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Seed.Host;
using Seed.Host.Exceptions;

namespace Seed {
    public class Startup {

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {

            services.AddMvc();

            services.AddOptions()
                .Configure<AppSettingOptions>(this.Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandling(this.Configuration.GetSection("exceptions").Get<ExceptionOptions>())
                .UseMvc()
                .UseRewriter(this.BuildRewriteOptions())
                .UseStaticFiles(this.BuildStaticFileOptions());
        }

        /// <summary>
        /// Builds the rewrite middleware options for the application.
        /// </summary>
        /// <returns>The rewrite options to use for the application</returns>
        private RewriteOptions BuildRewriteOptions() {
            var options = new RewriteOptions()
              //this maps all extensionless urls to the index.html file
              .AddRewrite(@"^(?:|[^.]|(?:\.(?=.*/)))+$", "index.html", true);

            return options;
        }

        /// <summary>
        /// Builds the options for the static file middleware
        /// </summary>
        /// <returns>The file options to use for the static file middleware</returns>
        private StaticFileOptions BuildStaticFileOptions() {
            return new StaticFileOptions {
                OnPrepareResponse = fileResponseContext => {
                    if (fileResponseContext.File.Name == "index.html") {
                        //no client caching permitted for the index file - we always need to check for a new one
                        var response = fileResponseContext.Context.Response;
                        response.Headers[HeaderNames.CacheControl] = "no-cache";
                    }
                }
            };
        }

    }
}
