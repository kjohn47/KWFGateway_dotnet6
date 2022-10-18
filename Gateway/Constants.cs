namespace KWFGateway.Gateway
{
    using Microsoft.Extensions.Primitives;

    public static class Constants
    {
        public const char UrlSplitChar = '/';
        public const string AnyMethod = "ANY";
        public const string AnyRouteValue = "*";
        public const string AnyRoutePattern = "**";
        public const string UrlEncodedFormData = "application/x-www-form-urlencoded";
        public const string IconMediaType = "image/x-icon";
        public static readonly KeyValuePair<string, StringValues> CspHeader = new("Content-Security-Policy", "default-src *; img-src * 'self' data: https: http:; script-src 'self' 'unsafe-inline' 'unsafe-eval' *;style-src 'self' 'unsafe-inline' * ");
    }
}
