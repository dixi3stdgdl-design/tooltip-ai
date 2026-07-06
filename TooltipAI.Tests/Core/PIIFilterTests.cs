using FluentAssertions;
using Xunit;
using TooltipAI.Core.Services;

namespace TooltipAI.Tests.Core;

public class PIIFilterTests
{
    private readonly PIIFilter _filter = PIIFilter.Instance;

    [Fact]
    public void ShouldDetectEmailAddresses()
    {
        var input = "Contact me at john.doe@example.com for more info";

        var result = _filter.Filter(input);

        result.ContainsPII.Should().BeTrue();
        result.DetectedTypes.Should().Contain("Email");
        result.FilteredText.Should().NotContain("john.doe@example.com");
        result.FilteredText.Should().Contain("[EMAIL]");
    }

    [Fact]
    public void ShouldDetectPhoneNumbers()
    {
        var input = "Call me at (555) 123-4567 or 555-987-6543";

        var result = _filter.Filter(input);

        result.ContainsPII.Should().BeTrue();
        result.DetectedTypes.Should().Contain("Phone");
    }

    [Fact]
    public void ShouldDetectSSN()
    {
        var input = "My SSN is 123-45-6789";

        var result = _filter.Filter(input);

        result.ContainsPII.Should().BeTrue();
        result.DetectedTypes.Should().Contain("SSN");
        result.FilteredText.Should().Contain("[SSN]");
    }

    [Fact]
    public void ShouldDetectCreditCardNumbers()
    {
        var input = "Card number: 1234-5678-9012-3456";

        var result = _filter.Filter(input);

        result.ContainsPII.Should().BeTrue();
        result.DetectedTypes.Should().Contain("CreditCard");
        result.FilteredText.Should().Contain("[CARD]");
    }

    [Fact]
    public void ShouldDetectIPAddresses()
    {
        var input = "Server is at 192.168.1.100";

        var result = _filter.Filter(input);

        result.ContainsPII.Should().BeTrue();
        result.DetectedTypes.Should().Contain("IPAddress");
        result.FilteredText.Should().Contain("[IP]");
    }

    [Fact]
    public void ShouldDetectDates()
    {
        var input = "Meeting on 2024-01-15 or 01/15/2024";

        var result = _filter.Filter(input);

        result.ContainsPII.Should().BeTrue();
        result.DetectedTypes.Should().Contain("Date");
    }

    [Fact]
    public void ShouldDetectPasswordPatterns()
    {
        var input = "password: secret123";

        var result = _filter.Filter(input);

        result.ContainsPII.Should().BeTrue();
        result.DetectedTypes.Should().Contain("Password");
        result.FilteredText.Should().Contain("[CREDENTIAL]");
    }

    [Fact]
    public void ShouldNotFlagNormalText()
    {
        var input = "Click the Save button to save your document";

        var result = _filter.Filter(input);

        result.ContainsPII.Should().BeFalse();
        result.FilteredText.Should().Be(input);
    }

    [Fact]
    public void ShouldHandleEmptyInput()
    {
        var result = _filter.Filter("");

        result.ContainsPII.Should().BeFalse();
        result.FilteredText.Should().Be("");
    }

    [Fact]
    public void ShouldHandleNullInput()
    {
        var result = _filter.Filter(null!);

        result.ContainsPII.Should().BeFalse();
    }

    [Fact]
    public void ShouldSanitizeForTransmission()
    {
        var input = "Email: test@example.com, Phone: 555-1234";

        var sanitized = _filter.SanitizeForTransmission(input);

        sanitized.Should().NotContain("test@example.com");
        sanitized.Should().Contain("[EMAIL]");
    }

    [Fact]
    public void ShouldDetectMultiplePIITypes()
    {
        var input = "Email: john@test.com, SSN: 123-45-6789, Phone: 555-1234";

        var result = _filter.Filter(input);

        result.ContainsPII.Should().BeTrue();
        result.DetectedTypes.Count.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public void ShouldDetectCommonNames()
    {
        var input = "John will handle the report";

        var result = _filter.Filter(input);

        result.ContainsPII.Should().BeTrue();
        result.DetectedTypes.Should().Contain("Name");
    }

    [Fact]
    public void ShouldDetectAddresses()
    {
        var input = "Office at 123 Main Street";

        var result = _filter.Filter(input);

        result.ContainsPII.Should().BeTrue();
        result.DetectedTypes.Should().Contain("Address");
    }

    [Fact]
    public void ContainsPII_ShouldReturnTrueForSensitiveData()
    {
        _filter.ContainsPII("my email is test@test.com").Should().BeTrue();
        _filter.ContainsPII("no sensitive data here").Should().BeFalse();
    }
}
