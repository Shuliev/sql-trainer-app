using SqlTrainer.Core.Models;

namespace SqlTrainer.Core.Tests;

public class DatabasePackageFormatTests
{
    [Theory]
    [InlineData("Name_der_Datenbank.zip", "Name_der_Datenbank")]
    [InlineData("  Fuhrpark .zip", "Fuhrpark")]
    [InlineData("..zip", "")]
    public void GetDatabaseName_StripsExtensionAndTrims(string fileName, string expected) =>
        Assert.Equal(expected, DatabasePackageFormat.GetDatabaseName(fileName));

    [Fact]
    public void GetDatabaseName_ReplacesInvalidCharacters()
    {
        var name = DatabasePackageFormat.GetDatabaseName("a\0b.zip");

        Assert.Equal("a_b", name);
    }
}
