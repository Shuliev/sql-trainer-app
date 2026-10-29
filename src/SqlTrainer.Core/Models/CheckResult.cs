namespace SqlTrainer.Core.Models;

/// <summary>Verdict of comparing a learner's result with the sample solution's result.</summary>
/// <param name="IsCorrect">True if the results count as equal.</param>
/// <param name="Reason">Short reason shown to the learner when wrong; must not reveal the solution.</param>
public sealed record CheckResult(bool IsCorrect, string? Reason = null);
