using Xunit;
using FluentAssertions;
using TooltipAI.Core.Translate;

namespace TooltipAI.Tests.Translate;

public class LanguageDetectorTests
{
    private readonly LanguageDetector _detector;

    public LanguageDetectorTests()
    {
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<LanguageDetector>();
        _detector = new LanguageDetector(logger);
    }

    [Fact]
    public void ShouldDetectEnglish()
    {
        var result = _detector.DetectLanguage("The quick brown fox jumps over the lazy dog");

        result.Code.Should().Be("en");
        result.Name.Should().Be("English");
    }

    [Fact]
    public void ShouldDetectSpanish()
    {
        var result = _detector.DetectLanguage("¿Cómo estás hoy? Me gustaría hablar en español contigo.");

        result.Code.Should().Be("es");
        result.Name.Should().Be("Spanish");
    }

    [Fact]
    public void ShouldDetectFrench()
    {
        var result = _detector.DetectLanguage("Le renard brun rapide saute par-dessus le chien paresseux");

        result.Code.Should().Be("fr");
        result.Name.Should().Be("French");
    }

    [Fact]
    public void ShouldDetectGerman()
    {
        var result = _detector.DetectLanguage("Der schnelle braune Fuchs springt über den faulen Hund");

        result.Code.Should().Be("de");
        result.Name.Should().Be("German");
    }

    [Fact]
    public void ShouldDetectPortuguese()
    {
        var result = _detector.DetectLanguage("A raposa marrom rápida pula sobre o cão preguiçoso");

        result.Code.Should().Be("pt");
        result.Name.Should().Be("Portuguese");
    }

    [Fact]
    public void ShouldDetectItalian()
    {
        var result = _detector.DetectLanguage("La volpe marrone rapida salta sopra il cane pigro");

        result.Code.Should().Be("it");
        result.Name.Should().Be("Italian");
    }

    [Fact]
    public void ShouldReturnSupportedLanguages()
    {
        var languages = _detector.GetSupportedLanguages();

        languages.Should().NotBeEmpty();
        languages.Should().Contain(l => l.Code == "en");
        languages.Should().Contain(l => l.Code == "es");
        languages.Should().Contain(l => l.Code == "fr");
    }

    [Fact]
    public void ShouldReturnConfidence()
    {
        var result = _detector.DetectLanguage("This is a clear English sentence with common words.");

        result.Confidence.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ShouldHandleEmptyText()
    {
        var result = _detector.DetectLanguage("");

        result.Code.Should().Be("unknown");
    }

    [Fact]
    public void ShouldHandleShortText()
    {
        var result = _detector.DetectLanguage("Hi");

        result.Should().NotBeNull();
        result.Code.Should().NotBeNullOrEmpty();
    }
}
