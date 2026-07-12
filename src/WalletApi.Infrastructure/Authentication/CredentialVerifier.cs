using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using WalletApi.Application.Abstractions;

namespace WalletApi.Infrastructure.Authentication;

internal sealed class CredentialVerifier : ICredentialVerifier
{
    private readonly DemoUserOptions _options;

    public CredentialVerifier(IOptions<DemoUserOptions> options)
    {
        _options = options.Value;
    }

    public bool Verify(string username, string password)
    {
        var usernameMatches = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(username),
            Encoding.UTF8.GetBytes(_options.Username));

        var passwordMatches = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(password),
            Encoding.UTF8.GetBytes(_options.Password));

        return usernameMatches && passwordMatches;
    }
}
