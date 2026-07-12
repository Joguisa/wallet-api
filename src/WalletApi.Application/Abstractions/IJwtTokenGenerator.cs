namespace WalletApi.Application.Abstractions;

public interface IJwtTokenGenerator
{
    (string AccessToken, DateTime ExpiresAtUtc) GenerateToken(string username);
}
