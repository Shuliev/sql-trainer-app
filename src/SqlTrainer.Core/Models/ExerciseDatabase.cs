namespace SqlTrainer.Core.Models;

/// <summary>A practice database ("Übungs-DB") that learners run their queries against.</summary>
public sealed record ExerciseDatabase(Guid Id, string Name, string Description);
