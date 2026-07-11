namespace WalletApi.Domain.Transfers;

public interface ITransferRepository
{
    Task<Transfer?> GetByIdempotencyKeyAsync(Guid idempotencyKey, CancellationToken cancellationToken = default);
    void Add(Transfer transfer);
}
