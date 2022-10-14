namespace KWFGateway.Common
{
    public enum GatewaySSLCertificateEnum
    {
        NotSet,
        FromPKCSFile,
        FromPemFile,
        FromEncryptedPemFile,
        FromPemValueBase64,
        FromEncryptedPemValueBase64,
        ForceDisableRequirement,
        FromStore
    }
}
