using SqlTrainer.Core;
using SqlTrainer.Core.Abstractions;
using SqlTrainer.Core.Models;

namespace SqlTrainer.Import;

/// <summary>
/// Reads <c>aufgaben.txt</c>: each record lies between two lines <c>##</c>, its five fields
/// (Nr, Themenbereich, Aufgabe, Hilfestellung, Musterlösung) are separated by lines <c>#</c>.
/// Fields may span several lines. All errors are collected and reported together.
/// </summary>
public sealed class ExerciseFileParser : IExerciseFileParser
{
    private const string _recordSeparator = "##";
    private const string _fieldSeparator = "#";
    private const int _fieldCount = 5;

    public IReadOnlyList<ExerciseDefinition> Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var lines = content.TrimStart('\uFEFF').ReplaceLineEndings("\n").Split('\n');
        var exercises = new List<ExerciseDefinition>();
        var errors = new List<string>();

        List<string>? fields = null;
        List<string>? currentField = null;
        var recordStartLine = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            if (line == _recordSeparator)
            {
                if (fields is null)
                {
                    fields = [];
                    currentField = null;
                    recordStartLine = i + 1;
                }
                else
                {
                    if (currentField is not null && currentField.Any(l => l.Trim().Length > 0))
                    {
                        errors.Add($"Zeile {recordStartLine}: Das letzte Feld muss mit einer Zeile \"#\" abgeschlossen werden.");
                    }
                    else
                    {
                        AddExercise(fields, recordStartLine, exercises, errors);
                    }
                    fields = null;
                }
            }
            else if (fields is null)
            {
                if (line.Length > 0)
                {
                    errors.Add($"Zeile {i + 1}: Text außerhalb eines Aufgabenblocks (\"##\").");
                }
            }
            else if (line == _fieldSeparator)
            {
                if (currentField is not null)
                {
                    fields.Add(string.Join('\n', currentField).Trim());
                }
                currentField = [];
            }
            else if (currentField is not null)
            {
                currentField.Add(lines[i]);
            }
            else if (line.Length > 0)
            {
                errors.Add($"Zeile {i + 1}: Ein Feld muss mit einer Zeile \"#\" beginnen.");
            }
        }

        if (fields is not null)
        {
            errors.Add($"Zeile {recordStartLine}: Der Aufgabenblock wird nicht mit \"##\" abgeschlossen.");
        }

        foreach (var duplicate in exercises.GroupBy(e => e.Number).Where(g => g.Count() > 1))
        {
            errors.Add($"Die Aufgabennummer {duplicate.Key} kommt mehrfach vor.");
        }

        if (errors.Count == 0 && exercises.Count == 0)
        {
            errors.Add("Die Datei aufgaben.txt enthält keine Aufgaben.");
        }

        if (errors.Count > 0)
        {
            throw new InvalidDatabasePackageException(
                "Die Datei aufgaben.txt ist fehlerhaft:" + Environment.NewLine + string.Join(Environment.NewLine, errors));
        }

        return exercises;
    }

    private static void AddExercise(List<string> fields, int line, List<ExerciseDefinition> exercises, List<string> errors)
    {
        if (fields.Count != _fieldCount)
        {
            errors.Add($"Zeile {line}: Eine Aufgabe braucht {_fieldCount} Felder, gefunden wurden {fields.Count}.");
            return;
        }

        var valid = true;
        if (!int.TryParse(fields[0], out var number) || number < 1)
        {
            errors.Add($"Zeile {line}: \"{fields[0]}\" ist keine gültige Aufgabennummer.");
            valid = false;
        }
        if (!TopicNames.TryParse(fields[1], out var topic))
        {
            errors.Add($"Zeile {line}: Unbekannter Themenbereich \"{fields[1]}\".");
            valid = false;
        }
        if (fields[2].Length == 0 || fields[4].Length == 0)
        {
            errors.Add($"Zeile {line}: Aufgabe und Musterlösung dürfen nicht leer sein.");
            valid = false;
        }

        if (valid)
        {
            exercises.Add(new ExerciseDefinition(number, topic, fields[2], fields[3], fields[4]));
        }
    }
}
