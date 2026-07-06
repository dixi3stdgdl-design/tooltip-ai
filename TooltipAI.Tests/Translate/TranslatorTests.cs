using Xunit;
using FluentAssertions;
using TooltipAI.Core.Translate;

namespace TooltipAI.Tests.Translate;

public class TranslatorTests
{
    private readonly Translator _translator;

    public TranslatorTests()
    {
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<Translator>();
        _translator = new Translator(logger);
    }

    [Fact]
    public async Task ShouldTranslateEnglishToSpanish()
    {
        var result = await _translator.TranslateAsync("hello", "en", "es");

        result.TranslatedText.Should().NotBeNullOrEmpty();
        result.SourceLanguage.Should().Be("en");
        result.TargetLanguage.Should().Be("es");
    }

    [Fact]
    public async Task ShouldTranslateSpanishToEnglish()
    {
        var result = await _translator.TranslateAsync("hola", "es", "en");

        result.TranslatedText.Should().NotBeNullOrEmpty();
        result.SourceLanguage.Should().Be("es");
        result.TargetLanguage.Should().Be("en");
    }

    [Fact]
    public async Task ShouldTranslateEnglishToFrench()
    {
        var result = await _translator.TranslateAsync("hello", "en", "fr");

        result.TranslatedText.Should().NotBeNullOrEmpty();
        result.SourceLanguage.Should().Be("en");
        result.TargetLanguage.Should().Be("fr");
    }

    [Fact]
    public async Task ShouldTranslateEnglishToGerman()
    {
        var result = await _translator.TranslateAsync("hello", "en", "de");

        result.TranslatedText.Should().NotBeNullOrEmpty();
        result.SourceLanguage.Should().Be("en");
        result.TargetLanguage.Should().Be("de");
    }

    [Fact]
    public async Task ShouldReturnAlternatives()
    {
        var result = await _translator.TranslateAsync("This is a complex sentence that requires alternatives", "en", "es");

        result.Alternatives.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldReturnProviderInfo()
    {
        var result = await _translator.TranslateAsync("hello", "en", "es");

        result.Provider.Should().NotBe(TranslationProvider.Error);
    }

    [Fact]
    public async Task ShouldHandleEmptyText()
    {
        var result = await _translator.TranslateAsync("", "en", "es");

        result.TranslatedText.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShouldHaveLatency()
    {
        var result = await _translator.TranslateAsync("This is a longer text that will take more time to process", "en", "es");

        result.LatencyMs.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task ShouldHandleContext()
    {
        var context = new TranslationContext
        {
            Domain = "business",
            Formality = "formal"
        };

        var result = await _translator.TranslateAsync("meeting", "en", "es", context);

        result.TranslatedText.Should().NotBeNullOrEmpty();
    }
}
