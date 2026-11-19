namespace SqlTrainer.Core.Models;

/// <summary>Maps each <see cref="Topic"/> to its key in <c>aufgaben.txt</c> and its display name (see Draft Importformat.md).</summary>
public static class TopicNames
{
    private static readonly (Topic Topic, string Key, string DisplayName)[] _entries =
    [
        (Topic.Select, "SELECT", "SELECT"),
        (Topic.Aggregate, "AGGREGAT", "Aggregatfunktionen"),
        (Topic.Join, "JOIN", "JOIN"),
        (Topic.Grouping, "GRUPPIERUNG", "GROUP BY"),
        (Topic.Subselect, "SUBSELECT", "Subselects"),
    ];

    /// <summary>Key as written in <c>aufgaben.txt</c>, e.g. "AGGREGAT".</summary>
    public static string GetKey(Topic topic) => Find(topic).Key;

    public static string GetDisplayName(Topic topic) => Find(topic).DisplayName;

    /// <summary>Parses a key from <c>aufgaben.txt</c> or a URL, ignoring case and surrounding whitespace.</summary>
    public static bool TryParse(string? key, out Topic topic)
    {
        foreach (var entry in _entries)
        {
            if (string.Equals(entry.Key, key?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                topic = entry.Topic;
                return true;
            }
        }

        topic = default;
        return false;
    }

    private static (Topic Topic, string Key, string DisplayName) Find(Topic topic)
    {
        foreach (var entry in _entries)
        {
            if (entry.Topic == topic)
            {
                return entry;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(topic), topic, "Unknown topic.");
    }
}
