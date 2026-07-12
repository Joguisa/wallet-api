namespace WalletApi.Application.Authentication.Login;

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc);
