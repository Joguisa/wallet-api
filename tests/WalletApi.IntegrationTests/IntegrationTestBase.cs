using System.Net.Http.Headers;
using System.Net.Http.Json;
using WalletApi.Application.Authentication.Login;
using WalletApi.Application.Wallets;

namespace WalletApi.IntegrationTests;

[Collection("integration")]
public abstract class IntegrationTestBase
{
    protected IntegrationTestWebAppFactory Factory { get; }

    protected IntegrationTestBase(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
    }

    protected async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = Factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { username = "admin", password = "Admin_Dev123!" });

        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        return client;
    }

    protected static string RandomDocumentId() =>
        Random.Shared.NextInt64(1_000_000_000, 9_999_999_999).ToString();

    protected static async Task<WalletResponse> CreateWalletAsync(HttpClient client, decimal initialBalance)
    {
        var response = await client.PostAsJsonAsync("/api/wallets", new
        {
            documentId = RandomDocumentId(),
            ownerName = "Integration Test",
            initialBalance
        });

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<WalletResponse>())!;
    }

    protected static async Task<HttpResponseMessage> PostTransferAsync(
        HttpClient client,
        int sourceWalletId,
        int destinationWalletId,
        decimal amount,
        Guid? idempotencyKey = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/transfers")
        {
            Content = JsonContent.Create(new { sourceWalletId, destinationWalletId, amount })
        };
        request.Headers.Add("Idempotency-Key", (idempotencyKey ?? Guid.NewGuid()).ToString());

        return await client.SendAsync(request);
    }

    protected static async Task<decimal> GetBalanceAsync(HttpClient client, int walletId)
    {
        var wallet = await client.GetFromJsonAsync<WalletResponse>($"/api/wallets/{walletId}");

        return wallet!.Balance;
    }
}
