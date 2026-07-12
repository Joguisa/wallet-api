using WalletApi.Application.Abstractions;
using WalletApi.Domain.Wallets;

namespace WalletApi.Application.Wallets.GetById;

public sealed class GetWalletHandler : IRequestHandler<GetWalletQuery, WalletResponse>
{
    private readonly IWalletRepository _walletRepository;

    public GetWalletHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<WalletResponse> HandleAsync(
        GetWalletQuery query,
        CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByIdAsync(query.WalletId, cancellationToken)
            ?? throw new WalletNotFoundException(query.WalletId);

        return WalletResponse.FromDomain(wallet);
    }
}
