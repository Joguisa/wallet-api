using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WalletApi.Application.Movements.GetHistory;
using WalletApi.Application.Transfers;

namespace WalletApi.IntegrationTests;

public class TransferEndpointsTests : IntegrationTestBase
{
    public TransferEndpointsTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Transfer_debits_credits_and_writes_both_ledger_movements()
    {
        var client = await CreateAuthenticatedClientAsync();
        var source = await CreateWalletAsync(client, 100m);
        var destination = await CreateWalletAsync(client, 20m);

        var response = await PostTransferAsync(client, source.Id, destination.Id, 30.50m);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var transfer = await response.Content.ReadFromJsonAsync<TransferResponse>();

        (await GetBalanceAsync(client, source.Id)).Should().Be(69.50m);
        (await GetBalanceAsync(client, destination.Id)).Should().Be(50.50m);

        var sourceHistory = await client.GetFromJsonAsync<MovementHistoryResponse>(
            $"/api/wallets/{source.Id}/movements");
        var destinationHistory = await client.GetFromJsonAsync<MovementHistoryResponse>(
            $"/api/wallets/{destination.Id}/movements");

        sourceHistory!.Items.Should().ContainSingle(m =>
            m.Type == "Debit" && m.Amount == 30.50m && m.TransferId == transfer!.TransferId);
        destinationHistory!.Items.Should().ContainSingle(m =>
            m.Type == "Credit" && m.Amount == 30.50m && m.TransferId == transfer!.TransferId);
    }

    [Fact]
    public async Task Insufficient_funds_returns_422_and_leaves_no_trace_in_the_database()
    {
        var client = await CreateAuthenticatedClientAsync();
        var source = await CreateWalletAsync(client, 50m);
        var destination = await CreateWalletAsync(client, 0m);

        var response = await PostTransferAsync(client, source.Id, destination.Id, 50.01m);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadAsStringAsync()).Should().Contain("INSUFFICIENT_FUNDS");

        (await GetBalanceAsync(client, source.Id)).Should().Be(50m);
        (await GetBalanceAsync(client, destination.Id)).Should().Be(0m);

        var history = await client.GetFromJsonAsync<MovementHistoryResponse>(
            $"/api/wallets/{source.Id}/movements");
        history!.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Repeating_a_request_with_the_same_idempotency_key_does_not_execute_twice()
    {
        var client = await CreateAuthenticatedClientAsync();
        var source = await CreateWalletAsync(client, 100m);
        var destination = await CreateWalletAsync(client, 0m);
        var key = Guid.NewGuid();

        var first = await PostTransferAsync(client, source.Id, destination.Id, 40m, key);
        var replay = await PostTransferAsync(client, source.Id, destination.Id, 40m, key);

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        replay.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstTransfer = await first.Content.ReadFromJsonAsync<TransferResponse>();
        var replayedTransfer = await replay.Content.ReadFromJsonAsync<TransferResponse>();
        replayedTransfer!.TransferId.Should().Be(firstTransfer!.TransferId);

        (await GetBalanceAsync(client, source.Id)).Should().Be(60m, "the debit must happen exactly once");
        (await GetBalanceAsync(client, destination.Id)).Should().Be(40m);
    }

    [Fact]
    public async Task Transfer_to_a_missing_wallet_returns_404()
    {
        var client = await CreateAuthenticatedClientAsync();
        var source = await CreateWalletAsync(client, 10m);

        var response = await PostTransferAsync(client, source.Id, 999999, 1m);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.Content.ReadAsStringAsync()).Should().Contain("WALLET_NOT_FOUND");
    }

    [Fact]
    public async Task Parallel_transfers_never_overdraw_and_money_is_conserved()
    {
        var client = await CreateAuthenticatedClientAsync();
        var source = await CreateWalletAsync(client, 100m);
        var destination = await CreateWalletAsync(client, 0m);

        var responses = await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(_ => PostTransferAsync(client, source.Id, destination.Id, 10m)));

        var succeeded = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var conflicted = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);
        (succeeded + conflicted).Should().Be(10, "every request must end in success or an honest 409");
        succeeded.Should().BeGreaterThan(0);

        var sourceBalance = await GetBalanceAsync(client, source.Id);
        var destinationBalance = await GetBalanceAsync(client, destination.Id);

        sourceBalance.Should().Be(100m - succeeded * 10m);
        destinationBalance.Should().Be(succeeded * 10m);
        (sourceBalance + destinationBalance).Should().Be(100m, "money must be conserved");
        sourceBalance.Should().BeGreaterThanOrEqualTo(0m, "the balance can never go negative");
    }
}
