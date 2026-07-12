using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WalletApi.Application.Wallets;

namespace WalletApi.IntegrationTests;

public class WalletEndpointsTests : IntegrationTestBase
{
    public WalletEndpointsTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Create_wallet_returns_201_with_location_and_persists_it()
    {
        var client = await CreateAuthenticatedClientAsync();
        var documentId = RandomDocumentId();

        var response = await client.PostAsJsonAsync("/api/wallets", new
        {
            documentId,
            ownerName = "Jonatan Guillen",
            initialBalance = 100.00m
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<WalletResponse>();
        response.Headers.Location!.ToString().Should().Contain($"/api/wallets/{created!.Id}");

        var fetched = await client.GetFromJsonAsync<WalletResponse>($"/api/wallets/{created.Id}");
        fetched!.DocumentId.Should().Be(documentId);
        fetched.Balance.Should().Be(100.00m);
    }

    [Fact]
    public async Task Creating_a_wallet_with_a_duplicate_document_id_returns_409()
    {
        var client = await CreateAuthenticatedClientAsync();
        var documentId = RandomDocumentId();
        var request = new { documentId, ownerName = "First Owner", initialBalance = 0m };

        (await client.PostAsJsonAsync("/api/wallets", request))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicate = await client.PostAsJsonAsync("/api/wallets", request);

        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await duplicate.Content.ReadAsStringAsync()).Should().Contain("DUPLICATE_DOCUMENT_ID");
    }

    [Fact]
    public async Task Getting_a_missing_wallet_returns_404_problem_details()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/wallets/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.Content.ReadAsStringAsync()).Should().Contain("WALLET_NOT_FOUND");
    }

    [Fact]
    public async Task Invalid_payload_returns_400_with_field_errors()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/wallets", new
        {
            documentId = "123",
            ownerName = "",
            initialBalance = -5m
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("VALIDATION_ERROR").And.Contain("DocumentId").And.Contain("OwnerName");
    }
}
