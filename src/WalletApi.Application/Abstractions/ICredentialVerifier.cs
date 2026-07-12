namespace WalletApi.Application.Abstractions;

public interface ICredentialVerifier
{
    bool Verify(string username, string password);
}
