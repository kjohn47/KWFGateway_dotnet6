namespace KRFGateway.WebApi
{
    using System.Net;

    using KRFCommon.Api;
    using KRFCommon.Constants;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Server.Kestrel.Https;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;

    public class Program
    {
        public static void Main( string[] args )
        {
            CreateHostBuilder( args ).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder( string[] args ) =>
            Host.CreateDefaultBuilder( args )
                .ConfigureWebHostDefaults( webBuilder =>
                 {
                     webBuilder.UseKestrel( ( c, o ) =>
                     {
                         var kestrelSettings = c.Configuration.GetSection( KRFApiSettings.KestrelConfiguration_Key ).Get<KestrelConfiguration>();
                         int httpPort = kestrelSettings.HttpPort == 0 ? 5051 : kestrelSettings.HttpPort;
                         int httpsPort = kestrelSettings.HttpsPort == 0 ? 15051 : kestrelSettings.HttpsPort;

                         if ( c.HostingEnvironment.IsDevelopment() )
                         {
                             o.ListenLocalhost( httpsPort, l => l.UseHttps( h =>
                             {
                                 h.AllowAnyClientCertificate();
                                 h.ClientCertificateMode = ClientCertificateMode.NoCertificate;
                             } ) );
                             o.ListenLocalhost( httpPort );
                         }
                         else
                         {
                             o.Listen( IPAddress.Any, httpsPort, l => l.UseHttps( h =>
                             {
                                 h.AllowAnyClientCertificate();
                                 h.ClientCertificateMode = ClientCertificateMode.NoCertificate;
                             } ) );
                             o.Listen( IPAddress.Any, httpPort );
                         }
                     } )
                     .UseStartup<Startup>();
                 } );
    }
}
