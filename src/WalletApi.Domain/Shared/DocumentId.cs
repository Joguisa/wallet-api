namespace WalletApi.Domain.Shared;

public sealed record DocumentId
{
    public const int Length = 10;

    public string Value { get; }

    private DocumentId(string value) => Value = value;

    public static DocumentId From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDocumentIdException("Document id cannot be empty.");
        }

        var trimmed = value.Trim();

        if (trimmed.Length != Length)
        {
            throw new InvalidDocumentIdException(
                $"Document id must be exactly {Length} digits, got '{trimmed}'.");
        }

        if (!trimmed.All(char.IsAsciiDigit))
        {
            throw new InvalidDocumentIdException(
                $"Document id must contain only digits, got '{trimmed}'.");
        }

        return new DocumentId(trimmed);
    }

    public override string ToString() => Value;
}
