using FluentAssertions;
using WalletApi.Domain.Shared;

namespace WalletApi.UnitTests.Domain;

public class DocumentIdTests
{
    [Fact]
    public void From_accepts_ten_numeric_digits()
    {
        var documentId = DocumentId.From("0941262000");

        documentId.Value.Should().Be("0941262000");
    }

    [Fact]
    public void From_trims_surrounding_whitespace()
    {
        var documentId = DocumentId.From(" 0941262000 ");

        documentId.Value.Should().Be("0941262000");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void From_rejects_empty_values(string? value)
    {
        var act = () => DocumentId.From(value!);

        act.Should().Throw<InvalidDocumentIdException>()
            .Which.ErrorCode.Should().Be("INVALID_DOCUMENT_ID");
    }

    [Theory]
    [InlineData("094126200")]
    [InlineData("09412620001")]
    public void From_rejects_wrong_length(string value)
    {
        var act = () => DocumentId.From(value);

        act.Should().Throw<InvalidDocumentIdException>();
    }

    [Theory]
    [InlineData("094126200a")]
    [InlineData("094126200-")]
    public void From_rejects_non_numeric_characters(string value)
    {
        var act = () => DocumentId.From(value);

        act.Should().Throw<InvalidDocumentIdException>();
    }

    [Fact]
    public void DocumentIds_with_the_same_value_are_equal()
    {
        DocumentId.From("0941262000").Should().Be(DocumentId.From("0941262000"));
    }
}
