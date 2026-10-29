using SqlTrainer.Core.Models;

namespace SqlTrainer.Core.Abstractions;

/// <summary>Decides whether a learner's result matches the sample solution's result.</summary>
public interface IResultComparer
{
    CheckResult Compare(ResultSet expected, ResultSet actual);
}
