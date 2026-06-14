namespace GraphQLGateway.Models;

public sealed record LoginInput(
    string Email,
    string Password);
