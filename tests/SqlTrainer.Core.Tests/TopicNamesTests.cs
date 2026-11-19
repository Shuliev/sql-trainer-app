using SqlTrainer.Core.Models;

namespace SqlTrainer.Core.Tests;

public class TopicNamesTests
{
    [Theory]
    [InlineData("SELECT", Topic.Select)]
    [InlineData("aggregat", Topic.Aggregate)]
    [InlineData(" Join ", Topic.Join)]
    [InlineData("GRUPPIERUNG", Topic.Grouping)]
    [InlineData("subselect", Topic.Subselect)]
    public void TryParse_AcceptsKeysIgnoringCase(string key, Topic expected)
    {
        Assert.True(TopicNames.TryParse(key, out var topic));
        Assert.Equal(expected, topic);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("Aggregate")]
    public void TryParse_RejectsUnknownKeys(string? key) => Assert.False(TopicNames.TryParse(key, out _));

    [Fact]
    public void EveryTopic_HasKeyAndDisplayName() =>
        Assert.All(Enum.GetValues<Topic>(), topic =>
        {
            Assert.True(TopicNames.TryParse(TopicNames.GetKey(topic), out var parsed));
            Assert.Equal(topic, parsed);
            Assert.NotEmpty(TopicNames.GetDisplayName(topic));
        });
}
