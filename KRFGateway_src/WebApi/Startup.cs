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
            this._requestContext = configuration.GetSection( KRFApiSettings.RequestContext_Key ).Get<RequestContext>();
            this._enableLogs = configuration.GetValue( KRFApiSettings.LogsOnPrd_Key, false );
            this._sessionServer = configuration.GetSection( AppConstants.SessionServerKey ).Get<SessionServer>();
            this.HostingEnvironment = env;
        }

        private readonly AppConfiguration _apiSettings;
        private readonly RequestContext _requestContext;
        private readonly bool _enableLogs;
        private readonly SessionServer _sessionServer;

        public IWebHostEnvironment HostingEnvironment { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices( IServiceCollection services )
        {
            //Add logger config
            services.AddLogging( l =>
            {
                var config = this.Configuration.GetSection( KRFApiSettings.Logging_Key );
                l.ClearProviders();
                l.AddConfiguration( config );
                l.AddConsole();
                l.AddDebug();
                l.AddEventLog();
                l.AddEventSourceLogger();
                l.AddKRFLogToFileLogger( this.Configuration.GetSection( KRFApiSettings.KRFLoggerConfig_Key ) );
            } );

            services.InjectUserContext( this._apiSettings.TokenIdentifier, this._apiSettings.TokenKey );

            services.AddKRFController();

            services.SwaggerInit( this._apiSettings.ApiName, this._apiSettings.TokenIdentifier );

            services.AddSingleton<IServerConfigurationHandler>( new ServerConfigurationHandler( this.Configuration.GetValue<string>( AppConstants.ConfigurationPathKey, "Configurations" ) ) );
            services.AddSingleton( _sessionServer );
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

            app.KRFLogAndExceptionHandlerConfigure(
            loggerFactory,
            ( this._enableLogs || isDev ),
            this._apiSettings.ApiName,
            this._apiSettings.TokenIdentifier,
            this._requestContext.EnableRequestRead );

            app.UseHttpsRedirection();

            app.UseRouting();

            app.AuthConfigure();

            app.UseEndpoints( endpoints =>
            {
                endpoints.MapControllers();
            } );

            app.SwaggerConfigure( this._apiSettings.ApiName );
        }
    }
}