using FluentValidation;
using WalletApi.Application.Abstractions;

namespace WalletApi.Application.Authentication.Login;

public sealed class LoginHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IValidator<LoginCommand> _validator;
    private readonly ICredentialVerifier _credentialVerifier;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginHandler(
        IValidator<LoginCommand> validator,
        ICredentialVerifier credentialVerifier,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _validator = validator;
        _credentialVerifier = credentialVerifier;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        if (!_credentialVerifier.Verify(command.Username, command.Password))
        {
            throw new InvalidCredentialsException();
        }

        var (accessToken, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(command.Username);

        return new LoginResponse(accessToken, expiresAtUtc);
    }
}
