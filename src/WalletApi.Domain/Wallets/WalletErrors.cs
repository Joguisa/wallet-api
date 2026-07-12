using WalletApi.Domain.Shared;

namespace WalletApi.Domain.Wallets;

public sealed class InvalidOwnerNameException : DomainException
{
    public InvalidOwnerNameException(string message)
        : base("INVALID_OWNER_NAME", message)
    {
    }
}

public sealed class InsufficientFundsException : DomainException
{
    public InsufficientFundsException(int walletId, Money balance, Money attempted)
        : base("INSUFFICIENT_FUNDS",
            $"Wallet {walletId} has balance {balance}, debit of {attempted} was rejected.")
    {
    }
}

public sealed class WalletNotFoundException : DomainException
{
    public WalletNotFoundException(int walletId)
        : base("WALLET_NOT_FOUND", $"Wallet {walletId} was not found.")
    {
    }
}

public sealed class DuplicateDocumentIdException : DomainException
{
    public DuplicateDocumentIdException(string documentId)
        : base("DUPLICATE_DOCUMENT_ID", $"A wallet for document id '{documentId}' already exists.")
    {
    }
}
