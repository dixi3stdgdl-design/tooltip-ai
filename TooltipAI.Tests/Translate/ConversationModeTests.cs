using Xunit;
using FluentAssertions;
using TooltipAI.Core.Translate;

namespace TooltipAI.Tests.Translate;

public class ConversationModeTests
{
    private readonly ConversationMode _conversation;

    public ConversationModeTests()
    {
        var translatorLogger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<Translator>();
        var translator = new Translator(translatorLogger);

        var detectorLogger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<LanguageDetector>();
        var detector = new LanguageDetector(detectorLogger);

        var conversationLogger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ConversationMode>();
        _conversation = new ConversationMode(translator, detector, conversationLogger);
    }

    [Fact]
    public async Task ShouldAnswerQuestion()
    {
        var result = await _conversation.AskAsync("Que significa?", "Hello world");

        result.Answer.Should().NotBeNullOrEmpty();
        result.Language.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShouldAnswerInSpanish()
    {
        var result = await _conversation.AskAsync("Cómo se traduce esto?", "Buenos días, ¿cómo estás hoy?");

        result.Answer.Should().NotBeNullOrEmpty();
        result.Language.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShouldMaintainHistory()
    {
        await _conversation.AskAsync("Question 1", "Context 1");
        await _conversation.AskAsync("Question 2", "Context 2");

        var history = _conversation.GetHistory();

        history.Should().HaveCount(4); // 2 user + 2 assistant
    }

    [Fact]
    public async Task ShouldClearHistory()
    {
        await _conversation.AskAsync("Test", "Context");
        _conversation.ClearHistory();

        var history = _conversation.GetHistory();

        history.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReturnProviderInfo()
    {
        var result = await _conversation.AskAsync("Test question", "Test context");

        result.Provider.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShouldHaveLatency()
    {
        var result = await _conversation.AskAsync("Test", "Test");

        result.LatencyMs.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldHandleEmptyQuestion()
    {
        var result = await _conversation.AskAsync("", "Context");

        result.Answer.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShouldHandleEmptyContext()
    {
        var result = await _conversation.AskAsync("Question", "");

        result.Answer.Should().NotBeNullOrEmpty();
    }
}
