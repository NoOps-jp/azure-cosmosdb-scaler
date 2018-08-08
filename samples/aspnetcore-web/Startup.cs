using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Documents.Client;

using NoOpsJp.CosmosDbScaler;
using NoOpsJp.CosmosDbScaler.Strategies;

using todo;

namespace quickstartcore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddUserSecrets<Startup>()
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // Add Connection Policy
            var connectionPolicy = new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp
            };
            connectionPolicy.PreferredLocations.Add("Japan East");

            services.AddStreamlinedDocumentClient(options =>
                    {
                        options.AccountEndpoint = Configuration.GetValue<string>("CosmosDB:AccountEndpoint");
                        options.AccountKey = Configuration.GetValue<string>("CosmosDB:AccountKeys");
                        options.DatabaseId = Configuration.GetValue<string>("CosmosDB:Database");
                    })
                    .SetConnectionPolicy(connectionPolicy)
                    .SetRequestProcessors(new ScaleController<SimpleScaleStrategy>());

            services.Configure<DocumentDBOptions>(Configuration.GetSection("CosmosDB"));

            services.AddSingleton<IDocumentDBRepository<todo.Models.Item>, DocumentDBRepository<todo.Models.Item>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Item}/{action=Index}/{id?}");
            });
        }
    }
}
