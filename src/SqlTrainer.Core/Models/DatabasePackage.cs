namespace SqlTrainer.Core.Models;

/// <summary>
/// Contents of a validated import package: the required files (<see cref="DatabasePackageFormat.RequiredFiles"/>),
/// keyed by their file name.
/// </summary>
public sealed record DatabasePackage(IReadOnlyDictionary<string, byte[]> Files);
