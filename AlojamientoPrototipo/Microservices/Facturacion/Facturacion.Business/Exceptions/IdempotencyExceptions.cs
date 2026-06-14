namespace Facturacion.Business.Exceptions;

public class DuplicateOperationInProgressException : Exception
{
    public DuplicateOperationInProgressException(string operationName)
        : base($"La operacion '{operationName}' ya se encuentra en proceso con la misma IdempotencyKey.") { }
}

public class IdempotencyKeyReuseException : Exception
{
    public IdempotencyKeyReuseException(string operationName)
        : base($"La IdempotencyKey enviada ya fue usada para otra carga distinta en la operacion '{operationName}'.") { }
}
