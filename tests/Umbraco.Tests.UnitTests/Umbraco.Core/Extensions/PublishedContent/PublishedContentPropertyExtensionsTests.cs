using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions.PublishedContent;

[TestFixture]
public class PublishedContentPropertyExtensionsTests
{
    private Mock<IPublishedContent> _contentMock;
    private Mock<IPublishedContentType> _contentTypeMock;
    private Mock<IVariationContextAccessor> _variationContextAccessorMock;
    private Mock<IPublishedValueFallback> _publishedValueFallbackMock;

    [SetUp]
    public void Setup()
    {
        _contentMock = new Mock<IPublishedContent>();
        _contentTypeMock = new Mock<IPublishedContentType>();
        _variationContextAccessorMock = new Mock<IVariationContextAccessor>();
        _publishedValueFallbackMock = new Mock<IPublishedValueFallback>();

        _contentMock.Setup(x => x.ContentType).Returns(_contentTypeMock.Object);
    }

    #region Name Tests

    [Test]
    public void Name_ThrowsArgumentNullException_WhenContentIsNull()
    {
        IPublishedContent content = null!;

        Assert.Throws<ArgumentNullException>(() =>
            content.Name(_variationContextAccessorMock.Object));
    }

    [Test]
    public void Name_ReturnsInvariantName_WhenContentDoesNotVaryByCulture()
    {
        const string expectedName = "Test Content";
        var cultures = new Dictionary<string, PublishedCultureInfo>
        {
            [string.Empty] = new PublishedCultureInfo("en-US", expectedName, string.Empty, DateTime.Now)
        };

        _contentTypeMock.Setup(x => x.VariesByCulture()).Returns(false);
        _contentMock.Setup(x => x.Cultures).Returns(cultures);

        var result = _contentMock.Object.Name(_variationContextAccessorMock.Object, "fr-FR");

        Assert.That(result, Is.EqualTo(expectedName));
    }

    [Test]
    public void Name_ReturnsCultureSpecificName_WhenContentVariesByCulture()
    {
        const string englishName = "English Content";
        const string frenchName = "Contenu Français";
        var cultures = new Dictionary<string, PublishedCultureInfo>
        {
            ["en-US"] = new PublishedCultureInfo("en-US", englishName, string.Empty, DateTime.Now),
            ["fr-FR"] = new PublishedCultureInfo("fr-FR", frenchName, string.Empty, DateTime.Now)
        };

        _contentTypeMock.Setup(x => x.VariesByCulture()).Returns(true);
        _contentMock.Setup(x => x.Cultures).Returns(cultures);

        var result = _contentMock.Object.Name(_variationContextAccessorMock.Object, "fr-FR");

        Assert.That(result, Is.EqualTo(frenchName));
    }

    [Test]
    public void Name_UsesVariationContext_WhenCultureIsNull()
    {
        const string expectedName = "Context Culture Content";
        var cultures = new Dictionary<string, PublishedCultureInfo>
        {
            ["de-DE"] = new PublishedCultureInfo("de-DE", expectedName, string.Empty, DateTime.Now)
        };

        var variationContext = new Mock<IVariationContext>();
        variationContext.Setup(x => x.Culture).Returns("de-DE");
        _variationContextAccessorMock.Setup(x => x.VariationContext).Returns(variationContext.Object);

        _contentTypeMock.Setup(x => x.VariesByCulture()).Returns(true);
        _contentMock.Setup(x => x.Cultures).Returns(cultures);

        var result = _contentMock.Object.Name(_variationContextAccessorMock.Object);

        Assert.That(result, Is.EqualTo(expectedName));
    }

    [Test]
    public void Name_ReturnsEmptyString_WhenCultureNotFound()
    {
        var cultures = new Dictionary<string, PublishedCultureInfo>
        {
            ["en-US"] = new PublishedCultureInfo("en-US", "English", string.Empty, DateTime.Now)
        };

        _contentTypeMock.Setup(x => x.VariesByCulture()).Returns(true);
        _contentMock.Setup(x => x.Cultures).Returns(cultures);

        var result = _contentMock.Object.Name(_variationContextAccessorMock.Object, "jp-JP");

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    #endregion

    #region HasValue Tests

    [Test]
    public void HasValue_ThrowsArgumentNullException_WhenContentIsNull()
    {
        IPublishedContent content = null!;

        Assert.Throws<ArgumentNullException>(() =>
            content.HasValue(_publishedValueFallbackMock.Object, "alias"));
    }

    [Test]
    public void HasValue_ReturnsFalse_WhenPropertyDoesNotExist()
    {
        _contentMock.Setup(x => x.HasProperty("nonExistent")).Returns(false);

        var result = _contentMock.Object.HasValue(_publishedValueFallbackMock.Object, "nonExistent");

        Assert.That(result, Is.False);
    }

    [Test]
    public void HasValue_ReturnsTrue_WhenPropertyExistsAndHasValue()
    {
        var propertyMock = new Mock<IPublishedProperty>();
        propertyMock.Setup(x => x.HasValue(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        _contentMock.Setup(x => x.HasProperty("existingProperty")).Returns(true);
        _contentMock.Setup(x => x.GetProperty("existingProperty")).Returns(propertyMock.Object);

        var result = _contentMock.Object.HasValue(_publishedValueFallbackMock.Object, "existingProperty");

        Assert.That(result, Is.True);
    }

    [Test]
    public void HasValue_UsesFallback_WhenPropertyValueIsNull()
    {
        var propertyMock = new Mock<IPublishedProperty>();
        propertyMock.Setup(x => x.HasValue(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        _contentMock.Setup(x => x.HasProperty("property")).Returns(true);
        _contentMock.Setup(x => x.GetProperty("property")).Returns(propertyMock.Object);

        _publishedValueFallbackMock
            .Setup(x => x.TryGetValue(_contentMock.Object, "property", It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<Fallback>(), It.IsAny<object>(), out It.Ref<object?>.IsAny))
            .Returns((IPublishedContent c, string a, string? cu, string? s, Fallback f, object? d, out object? v) =>
            {
                v = "fallback value";
                return true;
            });

        var result = _contentMock.Object.HasValue(_publishedValueFallbackMock.Object, "property", 
            fallback: Fallback.ToLanguage);

        Assert.That(result, Is.True);
    }

    #endregion

    #region Value Tests

    [Test]
    public void Value_ThrowsArgumentNullException_WhenContentIsNull()
    {
        IPublishedContent content = null!;

        Assert.Throws<ArgumentNullException>(() =>
            content.Value(_publishedValueFallbackMock.Object, "alias"));
    }

    [Test]
    public void Value_ReturnsPropertyValue_WhenPropertyExists()
    {
        const string expectedValue = "Test Value";
        var propertyMock = new Mock<IPublishedProperty>();
        propertyMock.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<string>())).Returns(expectedValue);

        _contentMock.Setup(x => x.GetProperty("property")).Returns(propertyMock.Object);

        var result = _contentMock.Object.Value(_publishedValueFallbackMock.Object, "property");

        Assert.That(result, Is.EqualTo(expectedValue));
    }

    [Test]
    public void Value_ReturnsDefaultValue_WhenPropertyDoesNotExist()
    {
        const string defaultValue = "Default";
        _contentMock.Setup(x => x.GetProperty("nonExistent")).Returns((IPublishedProperty?)null);

        var result = _contentMock.Object.Value(_publishedValueFallbackMock.Object, "nonExistent", 
            defaultValue: defaultValue);

        Assert.That(result, Is.EqualTo(defaultValue));
    }

    [Test]
    public void Value_UsesFallback_WhenConfigured()
    {
        const string fallbackValue = "Fallback Value";
        var propertyMock = new Mock<IPublishedProperty>();
        propertyMock.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<string>())).Returns(null);

        _contentMock.Setup(x => x.GetProperty("property")).Returns(propertyMock.Object);

        _publishedValueFallbackMock
            .Setup(x => x.TryGetValue<object>(_contentMock.Object, "property", It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<Fallback>(), It.IsAny<object>(), out It.Ref<object?>.IsAny))
            .Returns((IPublishedContent c, string a, string? cu, string? s, Fallback f, object? d, out object? v) =>
            {
                v = fallbackValue;
                return true;
            });

        var result = _contentMock.Object.Value(_publishedValueFallbackMock.Object, "property", 
            fallback: Fallback.ToAncestors);

        Assert.That(result, Is.EqualTo(fallbackValue));
    }

    #endregion

    #region Generic Value Tests

    [Test]
    public void ValueGeneric_ReturnsTypedValue_WhenPropertyExists()
    {
        const int expectedValue = 42;
        var propertyMock = new Mock<IPublishedProperty>();
        propertyMock.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<string>())).Returns(expectedValue);

        _contentMock.Setup(x => x.GetProperty("intProperty")).Returns(propertyMock.Object);

        var result = _contentMock.Object.Value<int>(_publishedValueFallbackMock.Object, "intProperty");

        Assert.That(result, Is.EqualTo(expectedValue));
    }

    [Test]
    public void ValueGeneric_ReturnsConvertedValue_WhenConversionPossible()
    {
        const string stringValue = "123";
        const int expectedValue = 123;
        var propertyMock = new Mock<IPublishedProperty>();
        propertyMock.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<string>())).Returns(stringValue);

        _contentMock.Setup(x => x.GetProperty("property")).Returns(propertyMock.Object);

        _publishedValueFallbackMock
            .Setup(x => x.TryGetValue<int>(_contentMock.Object, "property", It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<Fallback>(), It.IsAny<int>(), out It.Ref<int>.IsAny))
            .Returns((IPublishedContent c, string a, string? cu, string? s, Fallback f, int d, out int v) =>
            {
                v = expectedValue;
                return true;
            });

        var result = _contentMock.Object.Value<int>(_publishedValueFallbackMock.Object, "property");

        Assert.That(result, Is.EqualTo(expectedValue));
    }

    [Test]
    public void ValueGeneric_ReturnsDefaultValue_WhenConversionFails()
    {
        const int defaultValue = 99;
        _contentMock.Setup(x => x.GetProperty("property")).Returns((IPublishedProperty?)null);

        var result = _contentMock.Object.Value<int>(_publishedValueFallbackMock.Object, "property", 
            defaultValue: defaultValue);

        Assert.That(result, Is.EqualTo(defaultValue));
    }

    [Test]
    public void ValueGeneric_HandlesNullableTypes()
    {
        int? expectedValue = 42;
        var propertyMock = new Mock<IPublishedProperty>();
        propertyMock.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<string>())).Returns(expectedValue);

        _contentMock.Setup(x => x.GetProperty("nullableInt")).Returns(propertyMock.Object);

        var result = _contentMock.Object.Value<int?>(_publishedValueFallbackMock.Object, "nullableInt");

        Assert.That(result, Is.EqualTo(expectedValue));
    }

    [Test]
    public void ValueGeneric_ReturnsNull_ForNullableTypeWhenNoValue()
    {
        _contentMock.Setup(x => x.GetProperty("nullableInt")).Returns((IPublishedProperty?)null);

        var result = _contentMock.Object.Value<int?>(_publishedValueFallbackMock.Object, "nullableInt");

        Assert.That(result, Is.Null);
    }

    #endregion

    #region Culture and Segment Tests

    [Test]
    public void HasValue_PassesCultureAndSegment_ToProperty()
    {
        const string culture = "fr-FR";
        const string segment = "segment1";
        var propertyMock = new Mock<IPublishedProperty>();
        
        propertyMock.Setup(x => x.HasValue(culture, segment)).Returns(true);
        _contentMock.Setup(x => x.HasProperty("property")).Returns(true);
        _contentMock.Setup(x => x.GetProperty("property")).Returns(propertyMock.Object);

        var result = _contentMock.Object.HasValue(_publishedValueFallbackMock.Object, "property", culture, segment);

        Assert.That(result, Is.True);
        propertyMock.Verify(x => x.HasValue(culture, segment), Times.Once);
    }

    [Test]
    public void Value_PassesCultureAndSegment_ToProperty()
    {
        const string culture = "de-DE";
        const string segment = "segment2";
        const string expectedValue = "Localized Value";
        var propertyMock = new Mock<IPublishedProperty>();
        
        propertyMock.Setup(x => x.GetValue(culture, segment)).Returns(expectedValue);
        _contentMock.Setup(x => x.GetProperty("property")).Returns(propertyMock.Object);

        var result = _contentMock.Object.Value(_publishedValueFallbackMock.Object, "property", culture, segment);

        Assert.That(result, Is.EqualTo(expectedValue));
        propertyMock.Verify(x => x.GetValue(culture, segment), Times.Once);
    }

    #endregion
}