namespace GraphQLGateway.Models;

public sealed record OperationResult(
    bool Success,
    string Message);
