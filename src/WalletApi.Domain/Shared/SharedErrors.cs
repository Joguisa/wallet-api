namespace WalletApi.Domain.Shared;

public sealed class InvalidAmountException : DomainException
{
    public InvalidAmountException(string message)
        : base("INVALID_AMOUNT", message)
    {
    }
}

public sealed class InvalidDocumentIdException : DomainException
{
    public InvalidDocumentIdException(string message)
        : base("INVALID_DOCUMENT_ID", message)
    {
    }
}
