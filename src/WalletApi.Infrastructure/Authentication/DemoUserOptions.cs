namespace WalletApi.Infrastructure.Authentication;

public sealed class DemoUserOptions
{
    public const string SectionName = "DemoUser";

    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
