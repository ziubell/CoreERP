using CoreERP.Domain.ValueObjects;
using FluentAssertions;

namespace CoreERP.Domain.Tests.ValueObjects;

public class CodiceFiscaleTests
{
    [Theory]
    [InlineData("RSSMRA85M01H501Z")]
    [InlineData("BNCGNN90A41F205X")]
    public void Should_Accept_Valid_CodiceFiscale(string value)
    {
        var cf = new CodiceFiscale(value);
        cf.Valore.Should().Be(value.ToUpperInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("INVALID")]
    [InlineData("12345678901")]
    [InlineData("RSSMRA85M01H501")] // troppo corto
    public void Should_Reject_Invalid_CodiceFiscale(string value)
    {
        var act = () => new CodiceFiscale(value);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_Normalize_To_Uppercase()
    {
        var cf = new CodiceFiscale("rssmra85m01h501z");
        cf.Valore.Should().Be("RSSMRA85M01H501Z");
    }
}
