using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace WalletApi.IntegrationTests;

public class AuthorizationTests : IntegrationTestBase
{
    public AuthorizationTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Protected_endpoints_return_401_without_a_token()
    {
        var anonymous = Factory.CreateClient();

        var createWallet = await anonymous.PostAsJsonAsync("/api/wallets", new
        {
            documentId = RandomDocumentId(),
            ownerName = "No Token",
            initialBalance = 0m
        });
        var getWallet = await anonymous.GetAsync("/api/wallets/1");
        var transfer = await PostTransferAsync(anonymous, 1, 2, 1m);

        createWallet.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        getWallet.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        transfer.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Movement_history_is_public()
    {
        var authenticated = await CreateAuthenticatedClientAsync();
        var wallet = await CreateWalletAsync(authenticated, 10m);

        var anonymous = Factory.CreateClient();
        var response = await anonymous.GetAsync($"/api/wallets/{wallet.Id}/movements");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_with_wrong_credentials_returns_a_generic_401()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { username = "admin", password = "wrong-password" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("INVALID_CREDENTIALS");
        body.Should().NotContain("admin", because: "the error must not reveal which part was wrong");
    }
}
