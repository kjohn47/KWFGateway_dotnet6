namespace KWFGateway.Extensions
{
    using KWFGateway.Authorization;
    using KWFGateway.Common;
    using KWFGateway.Gateway;

    using Microsoft.Extensions.DependencyInjection;

    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public static class RegisterGatewayServices
    {
        public static void AddGatewayServices(this IServiceCollection services, IConfiguration configuration)
        {
            var globalCertConf = configuration.GetValue<ApiCertificateConfiguration>("GlobalCertificateConfiguration");
            //Read files and parse json to configs
            var apiConfigs = GetGatewayApiConfigurationFromFiles(configuration);
            
            //Add clients
            foreach (var api in apiConfigs)
            {
                foreach (var port in api.Ports)
                {
                    services.AddHttpClient($"{api.ServiceName}#{port}", client =>
                    {
                        client.BaseAddress = Uri.TryCreate($"{api.ApiBaseUrl}:{port}", UriKind.Absolute, out var uri) ? uri : throw new InvalidOperationException();
                        if (api.Timeout.HasValue)
                        {
                            client.Timeout = TimeSpan.FromMilliseconds(api.Timeout.Value);
                        }
                    })
                    .ConfigureHttpHandler(api.CertificateConfiguration ?? globalCertConf);
                }
            }

            var authorizationConfiguration = configuration.GetSection(nameof(AuthorizationConfig)).Get<AuthorizationConfig>()
                                             ?? throw new ArgumentNullException(nameof(AuthorizationConfig), "Authorization configuration not found");

            if (!authorizationConfiguration.RedirectTokenFromRequest)
            {
                foreach (var port in authorizationConfiguration.Ports)
                {
                    services.AddHttpClient($"{AuthorizationHandler.AuthorizationApiKey}#{port}", client =>
                    {
                        client.BaseAddress = Uri.TryCreate($"{authorizationConfiguration.BaseUrl}:{port}", UriKind.Absolute, out var uri) ? uri : throw new InvalidOperationException();

                        if (authorizationConfiguration.Timeout.HasValue)
                        {
                            client.Timeout = TimeSpan.FromMilliseconds(authorizationConfiguration.Timeout.Value);
                        }
                    })
                    .ConfigureHttpHandler(authorizationConfiguration.CertificateConfiguration ?? globalCertConf);
                }
            }

            if (apiConfigs.Count == 0 && authorizationConfiguration.RedirectTokenFromRequest)
            {
                services.AddHttpClient();
            }

            services.AddSingleton(apiConfigs);
            services.AddSingleton<IGatewayConfigurationHandler, GatewayConfigurationHandler>();
            services.AddSingleton(authorizationConfiguration);
            services.AddSingleton<IAuthorizationHandler, AuthorizationHandler>();
            services.AddSingleton<IKwfHttpClientHandler, KwfHttpClientHandler>();
        }

        private static IReadOnlyList<GatewayClientConfiguration> GetGatewayApiConfigurationFromFiles(IConfiguration configuration)
        {
            var apiConfigPath = configuration.GetValue<string?>("ApiConfigurationPath");
            var apiConfigUseCurrentDomainPath = configuration.GetValue<bool>("ApiConfigurationUseCurrentDomainPath");
            var jsonConfigurationOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            jsonConfigurationOptions.Converters.Add(new JsonStringEnumConverter());

            if (string.IsNullOrEmpty(apiConfigPath))
            {
                apiConfigPath = "Configurations";
            }

            var configurationFilesPath = Path.IsPathRooted(apiConfigPath) && !apiConfigUseCurrentDomainPath
                                         ? Path.GetFullPath(apiConfigPath)
                                         : apiConfigUseCurrentDomainPath
                                           ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, apiConfigPath)
                                           : Path.GetFullPath(apiConfigPath, Environment.CurrentDirectory);

            var apiConfigs = new List<GatewayClientConfiguration>();

            if (!Directory.Exists(configurationFilesPath))
            {
                throw new DirectoryNotFoundException($"{configurationFilesPath} could not be found");
            }
            //foreach file {
            var configFilePaths = Directory.GetFiles(configurationFilesPath, "*.json");
            if (configFilePaths != null && configFilePaths.Length > 0)
            {
                foreach (var filePath in configFilePaths)
                {
                    var fileData = File.ReadAllText(filePath);
                    var nextFile = JsonSerializer.Deserialize<GatewayClientConfiguration>(fileData, jsonConfigurationOptions);
                    
                    if (string.IsNullOrEmpty(nextFile?.ServiceName))
                    {
                        throw new FileLoadException("Invalid file or missing ServiceName", filePath);
                    }

                    if (string.IsNullOrEmpty(nextFile.ApiBaseUrl))
                    {
                        throw new FileLoadException("Missing ApiBaseUrl", filePath);
                    }

                    if (nextFile.Ports.Count == 0)
                    {
                        throw new FileLoadException("Missing Ports settings", filePath);
                    }

                    if (apiConfigs.Any(x => x.ServiceName.Equals(nextFile.ServiceName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        throw new DuplicateWaitObjectException(nextFile.ServiceName, "Service already has configuration defined");
                    }

                    apiConfigs.Add(nextFile);
                }
            }

            return apiConfigs;
        }

        private static void ConfigureHttpHandler(this IHttpClientBuilder builder, ApiCertificateConfiguration? certificateConfiguration)
        {
            if (certificateConfiguration == null)
            {
                return;
            }

            builder.ConfigureHttpMessageHandlerBuilder(handler =>
            {
                if (handler.PrimaryHandler is HttpClientHandler messageHandler)
                {
                    switch (certificateConfiguration.CertificateSource)
                    {
                        case GatewaySSLCertificateEnum.ForceDisableRequirement:
                            {
                                messageHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
                                break;
                            }
                        case GatewaySSLCertificateEnum.FromStore:
                            {
                                messageHandler.ClientCertificates.Add(new X509Certificate2(certificateConfiguration.CertificateValue, certificateConfiguration.CertificatePassword));
                                messageHandler.SslProtocols = certificateConfiguration.SslProtocols;
                                break;
                            }
                        case GatewaySSLCertificateEnum.FromPKCSFile:
                            {
                                messageHandler.ClientCertificates.Add(X509Certificate2.CreateFromCertFile(certificateConfiguration.CertificateValue));
                                messageHandler.SslProtocols = certificateConfiguration.SslProtocols;
                                break;
                            }
                        case GatewaySSLCertificateEnum.FromEncryptedPemFile:
                            {
                                messageHandler.ClientCertificates.Add(X509Certificate2.CreateFromEncryptedPemFile(certificateConfiguration.CertificateValue, certificateConfiguration.CertificatePassword, certificateConfiguration.CertificatePrivateValue));
                                messageHandler.SslProtocols = certificateConfiguration.SslProtocols;
                                break;
                            }
                        case GatewaySSLCertificateEnum.FromPemFile:
                            {
                                messageHandler.ClientCertificates.Add(X509Certificate2.CreateFromPemFile(certificateConfiguration.CertificateValue, certificateConfiguration.CertificatePrivateValue));
                                messageHandler.SslProtocols = certificateConfiguration.SslProtocols;
                                break;
                            }
                        case GatewaySSLCertificateEnum.FromPemValueBase64:
                            {
                                if (!string.IsNullOrEmpty(certificateConfiguration.CertificatePrivateValue))
                                {
                                    messageHandler.ClientCertificates.Add(X509Certificate2.CreateFromPem(certificateConfiguration.CertificateValue.GetStringFromBase64(), certificateConfiguration.CertificatePrivateValue.GetStringFromBase64()));
                                }
                                else
                                {
                                    messageHandler.ClientCertificates.Add(X509Certificate2.CreateFromPem(certificateConfiguration.CertificateValue.GetStringFromBase64()));
                                }

                                messageHandler.SslProtocols = certificateConfiguration.SslProtocols;
                                break;
                            }
                        case GatewaySSLCertificateEnum.FromEncryptedPemValueBase64:
                            {
                                messageHandler.ClientCertificates.Add(X509Certificate2.CreateFromEncryptedPem(certificateConfiguration.CertificateValue.GetStringFromBase64(), certificateConfiguration.CertificatePrivateValue.GetStringFromBase64(), certificateConfiguration.CertificatePassword));
                                messageHandler.SslProtocols = certificateConfiguration.SslProtocols;
                                break;
                            }
                    }
                }
            });
        }

        private static string GetStringFromBase64(this string? encodedString)
        {
            if (string.IsNullOrEmpty(encodedString))
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(Convert.FromBase64String(encodedString));
        }
    }
}
