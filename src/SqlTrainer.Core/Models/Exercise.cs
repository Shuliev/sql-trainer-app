namespace SqlTrainer.Core.Models;

/// <summary>An exercise ("Aufgabe") that belongs to one practice database.</summary>
public sealed record Exercise(
    Guid Id,
    Guid DatabaseId,
    int Number,
    Topic Topic,
    string Text,
    string Hint,
    string SampleSolution);
