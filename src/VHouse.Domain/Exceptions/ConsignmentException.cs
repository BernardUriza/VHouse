namespace VHouse.Domain.Exceptions;

public class ConsignmentException : DomainException
{
    public ConsignmentException(string message) : base(message)
    {
    }

    public ConsignmentException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class ConsignmentNotFoundException : ConsignmentException
{
    public ConsignmentNotFoundException(int id)
        : base($"Consignment with ID {id} not found")
    {
    }
}

public class ConsignmentItemNotFoundException : ConsignmentException
{
    public ConsignmentItemNotFoundException(int id)
        : base($"Consignment item with ID {id} not found")
    {
    }
}

public class InvalidConsignmentOperationException : ConsignmentException
{
    public InvalidConsignmentOperationException(string message) : base(message)
    {
    }
}
