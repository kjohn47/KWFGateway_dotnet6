Add .json configurations on this folder to be loaded by gateway for redirect and validate routes

Template:
{
  "ServerName": "",
  "ServerUrl": "",
  "InternalTokenKey": "",
  "InternalTokenIdentifier": "",
  "CertificatePath" : "",
  "CertificateKey" : "",
  "ForceDisableSSL": false,
  "RouteConfiguration": [
    {
      "Route": "",
      "Method": "",
      "NeedAuthorization": false,
      "NeedRequestBody": false
    }
  ]
}