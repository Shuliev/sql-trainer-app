namespace SqlTrainer.Core;

/// <summary>An import package was rejected. <see cref="Exception.Message"/> is shown to the user.</summary>
public sealed class InvalidDatabasePackageException : Exception
{
    public InvalidDatabasePackageException()
    {
    }

    public InvalidDatabasePackageException(string message)
        : base(message)
    {
    }

    public InvalidDatabasePackageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
