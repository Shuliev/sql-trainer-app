using SqlTrainer.Core;
using SqlTrainer.Core.Models;

namespace SqlTrainer.Import.Tests;

public class ExerciseFileParserTests
{
    private readonly ExerciseFileParser _parser = new();

    [Fact]
    public void ParseReadsAllFieldsOfEveryRecord()
    {
        var content = "\uFEFF" + Record("1", "SELECT", "Gib alle Filme aus.", "Verwende *.", "SELECT * FROM film;")
                      + Record("6", "aggregat", "Zähle.", "COUNT(*)", "SELECT COUNT(*) FROM film;");

        var exercises = _parser.Parse(content);

        Assert.Equal(
            [
                new ExerciseDefinition(1, Topic.Select, "Gib alle Filme aus.", "Verwende *.", "SELECT * FROM film;"),
                new ExerciseDefinition(6, Topic.Aggregate, "Zähle.", "COUNT(*)", "SELECT COUNT(*) FROM film;"),
            ],
            exercises);
    }

    [Fact]
    public void ParseKeepsMultiLineFieldsAndAcceptsCrLf()
    {
        var content = Record("1", "JOIN", "Text", "Tipp", "SELECT *\nFROM a\nJOIN b ON a.id = b.id;").Replace("\n", "\r\n", StringComparison.Ordinal);

        var exercise = Assert.Single(_parser.Parse(content));

        Assert.Equal("SELECT *\nFROM a\nJOIN b ON a.id = b.id;", exercise.SampleSolution);
    }

    [Fact]
    public void ParseAllowsEmptyHint() =>
        Assert.Equal(string.Empty, Assert.Single(_parser.Parse(Record("1", "SELECT", "Text", "", "SELECT 1;"))).Hint);

    [Theory]
    [InlineData("##\n#\n1\n#\nSELECT\n#\nText\n#\nSELECT 1;\n#\n##\n", "5 Felder")]
    [InlineData("##\n#\nx\n#\nSELECT\n#\nText\n#\nTipp\n#\nSELECT 1;\n#\n##\n", "Aufgabennummer")]
    [InlineData("##\n#\n1\n#\nUNION\n#\nText\n#\nTipp\n#\nSELECT 1;\n#\n##\n", "Themenbereich")]
    [InlineData("##\n#\n1\n#\nSELECT\n#\n\n#\nTipp\n#\nSELECT 1;\n#\n##\n", "nicht leer")]
    [InlineData("##\n#\n1\n#\nSELECT\n#\nText\n#\nTipp\n#\nSELECT 1;\n##\n", "abgeschlossen")]
    [InlineData("##\n#\n1\n#\nSELECT\n#\nText\n#\nTipp\n#\nSELECT 1;\n#\n", "nicht mit \"##\"")]
    [InlineData("1###TRENN###SELECT###TRENN###Text###TRENN###Tipp###TRENN###SELECT 1;\n", "außerhalb")]
    [InlineData("", "keine Aufgaben")]
    public void ParseInvalidContentThrowsWithReason(string content, string expected)
    {
        var ex = Assert.Throws<InvalidDatabasePackageException>(() => _parser.Parse(content));

        Assert.Contains(expected, ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseDuplicateNumberThrows()
    {
        var content = Record("1", "SELECT", "A", "B", "C") + Record("1", "JOIN", "A", "B", "C");

        var ex = Assert.Throws<InvalidDatabasePackageException>(() => _parser.Parse(content));

        Assert.Contains("mehrfach", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseReportsAllErrorsTogether()
    {
        var content = Record("x", "SELECT", "A", "B", "C") + Record("2", "UNION", "A", "B", "C");

        var ex = Assert.Throws<InvalidDatabasePackageException>(() => _parser.Parse(content));

        Assert.Contains("Aufgabennummer", ex.Message, StringComparison.Ordinal);
        Assert.Contains("Themenbereich", ex.Message, StringComparison.Ordinal);
    }

    /// <summary>Every bundled sample database must parse: 25 exercises, five per topic.</summary>
    [Fact]
    public void ParseBundledSampleDatabases()
    {
        var files = SampleDatabases.Files("aufgaben.txt");
        Assert.NotEmpty(files);

        Assert.All(files, file =>
        {
            var exercises = _parser.Parse(File.ReadAllText(file));

            Assert.Equal(25, exercises.Count);
            Assert.All(exercises.GroupBy(e => e.Topic), group => Assert.Equal(5, group.Count()));
        });
    }

    private static string Record(string number, string topic, string text, string hint, string solution) =>
        $"##\n#\n{number}\n#\n{topic}\n#\n{text}\n#\n{hint}\n#\n{solution}\n#\n##\n";
}
