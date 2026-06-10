namespace ACC.Identity.Application.UseCases.AuthenticateUser;

public sealed record AuthenticateUserCommand(string Email, string Password);
