namespace SqlTrainer.Core.Models;

/// <summary>One exercise as read from <c>aufgaben.txt</c> of a database package.</summary>
public sealed record ExerciseDefinition(int Number, Topic Topic, string Text, string Hint, string SampleSolution);
