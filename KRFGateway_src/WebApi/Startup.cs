namespace KRFGateway.WebApi
{
    using KRFCommon.Api;
    using KRFCommon.Constants;
    using KRFCommon.Context;
    using KRFCommon.Controller;
    using KRFCommon.Logger;
    using KRFCommon.Middleware;
    using KRFCommon.Swagger;

    using KRFGateway.App.Constants;
    using KRFGateway.App.Handler;
    using KRFGateway.Domain.Model;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class Startup
    {
        public Startup( IConfiguration configuration, IWebHostEnvironment env )
        {
            this.Configuration = configuration;
            this._apiSettings = configuration.GetSection( KRFApiSettings.AppConfiguration_Key ).Get<AppConfiguration>();
            this.HostingEnvironment = env;
        }

        private readonly AppConfiguration _apiSettings;

        public IWebHostEnvironment HostingEnvironment { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices( IServiceCollection services )
        {
            //Add logger config
            services.AddLogging( l =>
            {
                l.AddKRFLogger( this.Configuration );
            } );

            services.AddUserBearerContext( this._apiSettings );

            services.AddKRFController();

            services.SwaggerInit( this._apiSettings );

            services.AddSingleton<IServerConfigurationHandler>( new ServerConfigurationHandler( this.Configuration.GetValue<string>( AppConstants.ConfigurationPathKey, "Configurations" ) ) );

            services.AddSingleton( new GatewaySettings
            {
                AppConfiguration = this._apiSettings,
                SessionServerEnabled = this.Configuration.GetValue<bool>( AppConstants.SessionServerKey, false ),
                SessionServerSettings = this.Configuration.GetSection( AppConstants.SessionServerKey ).Get<SessionServer>()
            } );

            services.AddScoped<IRouteHandler, RouteHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, ILoggerFactory loggerFactory )
        {
            //server config settings
            var isDev = this.HostingEnvironment.IsDevelopment();

            if ( isDev )
            {
                app.UseDeveloperExceptionPage();
            }

            app.ApiConfigure( this._apiSettings, loggerFactory, isDev );

            app.SwaggerConfigure( this._apiSettings.ApiName );
        }
    }
}