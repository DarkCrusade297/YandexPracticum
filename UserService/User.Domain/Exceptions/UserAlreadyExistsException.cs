namespace User.Domain.Exceptions;

public class UserAlreadyExistsException(string message) : DomainException(message);
