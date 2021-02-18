namespace KRFGateway.App.Handler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;

    using KRFCommon.JSON;

    using KRFGateway.Domain.Model;

    public class ServerConfigurationHandler : IServerConfigurationHandler
    {
        private readonly IDictionary<string, ServerConfiguration> ServerConfigurations;

        public ServerConfigurationHandler( string path )
        {
            try
            {
                var configPath = Path.IsPathRooted( path ) ? path : $"{Environment.CurrentDirectory}\\{path}";
                //Read all json configurations from path
                var configurations = new Dictionary<string, ServerConfiguration>();
                var configFilePaths = Directory.GetFiles( configPath, "*.json" );

                foreach ( var filePath in configFilePaths )
                {
                    var file = File.ReadAllText( filePath );
                    var data = JsonSerializer.Deserialize<ServerConfiguration>( file, KRFJsonSerializerOptions.GetJsonSerializerOptions() );
                    configurations.Add( data.ServerName.ToLowerInvariant(), data );
                }

                this.ServerConfigurations = configurations;
            }
            catch ( Exception ex )
            {
                throw new Exception( $"Could not load configuration files - {ex.Message}" );
            }
        }

        public ServerConfiguration GetServerConfiguration( string serverName )
        {
            ServerConfiguration serverConfiguration;

            if ( !this.ServerConfigurations.TryGetValue( serverName.ToLowerInvariant(), out serverConfiguration ) )
            {
                throw new Exception( "Requested server is not configured on system" );
            }

            return serverConfiguration;
        }
    }
}
