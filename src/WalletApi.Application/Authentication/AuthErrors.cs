using WalletApi.Domain.Shared;

namespace WalletApi.Application.Authentication;

public sealed class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("INVALID_CREDENTIALS", "Invalid username or password.")
    {
    }
}
