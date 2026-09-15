using ECommerce.ApiGateway.Logging;

namespace ECommerce.ContractTests;

public sealed class GatewayLogRedactionTests
{
    [Fact]
    public void DownstreamUrlLoggingHidesAccessToken()
    {
        string message = SensitiveQueryRedactor.Redact(
            "Scheme 'http' of the downstream 'http://notification-api:8080/hubs/notifications" +
            "?id=vLxJTSMfVQ9ohRHXpxJCBg&access_token=eyJhbGciOiJSUzI1NiJ9.payload.signature'.");

        Assert.DoesNotContain("eyJhbGciOiJSUzI1NiJ9", message, StringComparison.Ordinal);
        Assert.Contains($"access_token={SensitiveQueryRedactor.Placeholder}", message, StringComparison.Ordinal);
        Assert.Contains("id=vLxJTSMfVQ9ohRHXpxJCBg", message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("id_token")]
    [InlineData("refresh_token")]
    [InlineData("client_secret")]
    [InlineData("code")]
    public void OtherCredentialParametersAreHidden(string parameterName)
    {
        string message = SensitiveQueryRedactor.Redact(
            $"https://gateway.local/callback?{parameterName}=super-secret-value&state=abc");

        Assert.DoesNotContain("super-secret-value", message, StringComparison.Ordinal);
        Assert.Contains("state=abc", message, StringComparison.Ordinal);
    }

    [Fact]
    public void StructuredLogValuesAreRedacted()
    {
        object? redacted = SensitiveQueryRedactor.RedactValue(
            "http://notification-api:8080/hubs/notifications?access_token=secret-token");

        Assert.Equal(
            $"http://notification-api:8080/hubs/notifications?access_token={SensitiveQueryRedactor.Placeholder}",
            redacted);
    }

    [Fact]
    public void NonStringValuesArePreserved()
    {
        Assert.Equal(200, SensitiveQueryRedactor.RedactValue(200));
        Assert.Null(SensitiveQueryRedactor.RedactValue(null));
    }
}
