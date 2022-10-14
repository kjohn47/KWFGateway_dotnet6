namespace KWFGateway.Gateway
{
    public class GatewayRouteConfiguration
    {
        private readonly object _lockObject = new();

        private string[]? _pattern = null;

        public string Pattern { get; set; } = string.Empty;

        public string Method { get; set; } = Constants.AnyMethod;

        public bool? RequireAuthorization { get; set; }

        public bool IsMatch(string[] urlParts, string method)
        {
            if (!Method.Equals(Constants.AnyMethod) && !method.Equals(Method))
            {
                return false;
            }

            if (_pattern is null)
            {
                if (string.IsNullOrEmpty(Pattern))
                {
                    return false;
                }

                lock (_lockObject)
                {
                    _pattern ??= Pattern.ToLowerInvariant().Split(Constants.UrlSplitChar, StringSplitOptions.RemoveEmptyEntries);
                }
            }

            if (_pattern.Length == 0)
            {
                return false;
            }

            if (_pattern.Length == 1 && _pattern[0].Equals(Constants.AnyRoutePattern))
            {
                return true;
            }

            if (urlParts.Length == 1)
            {
                return false;
            }

            var patternMaxLength = urlParts.Length - 1;
            if (_pattern.Length == patternMaxLength ||
                ((_pattern.Length < patternMaxLength || _pattern.Length == urlParts.Length) && _pattern.Last().Equals(Constants.AnyRoutePattern)))
            {
                var isMatch = false;

                for (int i = 0; i < _pattern.Length; i++)
                {
                    var patternPart = _pattern[i];
                    if (i == (_pattern.Length - 1) && patternPart.Equals(Constants.AnyRoutePattern))
                    {
                        return true;
                    }

                    if (patternPart.Equals(Constants.AnyRouteValue))
                    {
                        isMatch = true;
                        continue;
                    }

                    var urlPart = urlParts[i + 1].ToLowerInvariant();
                    if (patternPart.Equals(urlPart))
                    {
                        isMatch = true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return isMatch;
            }

            return false;
        }
    }
}
