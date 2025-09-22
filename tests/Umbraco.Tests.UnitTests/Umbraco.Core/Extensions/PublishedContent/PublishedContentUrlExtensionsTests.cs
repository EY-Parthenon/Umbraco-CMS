using System;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions.PublishedContent;

[TestFixture]
public class PublishedContentUrlExtensionsTests
{
    private Mock<IPublishedContent> _contentMock;
    private Mock<IPublishedUrlProvider> _publishedUrlProviderMock;
    private Mock<IVariationContextAccessor> _variationContextAccessorMock;
    private Mock<IPublishedContentType> _contentTypeMock;

    [SetUp]
    public void Setup()
    {
        _contentMock = new Mock<IPublishedContent>();
        _publishedUrlProviderMock = new Mock<IPublishedUrlProvider>();
        _variationContextAccessorMock = new Mock<IVariationContextAccessor>();
        _contentTypeMock = new Mock<IPublishedContentType>();

        _contentMock.Setup(x => x.ContentType).Returns(_contentTypeMock.Object);
    }

    #region Url Tests

    [Test]
    public void Url_ThrowsArgumentNullException_WhenContentIsNull()
    {
        IPublishedContent content = null!;

        Assert.Throws<ArgumentNullException>(() =>
            content.Url(_publishedUrlProviderMock.Object));
    }

    [Test]
    public void Url_ThrowsArgumentNullException_WhenPublishedUrlProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _contentMock.Object.Url(null!));
    }

    [Test]
    public void Url_ReturnsUrl_FromPublishedUrlProvider()
    {
        const string expectedUrl = "/test-page";
        
        _publishedUrlProviderMock
            .Setup(x => x.GetUrl(_contentMock.Object, UrlMode.Default, null, It.IsAny<Uri>()))
            .Returns(expectedUrl);

        var result = _contentMock.Object.Url(_publishedUrlProviderMock.Object);

        Assert.That(result, Is.EqualTo(expectedUrl));
    }

    [Test]
    public void Url_PassesCulture_ToPublishedUrlProvider()
    {
        const string culture = "fr-FR";
        const string expectedUrl = "/fr/test-page";
        
        _publishedUrlProviderMock
            .Setup(x => x.GetUrl(_contentMock.Object, UrlMode.Default, culture, It.IsAny<Uri>()))
            .Returns(expectedUrl);

        var result = _contentMock.Object.Url(_publishedUrlProviderMock.Object, culture);

        Assert.That(result, Is.EqualTo(expectedUrl));
        _publishedUrlProviderMock.Verify(x => x.GetUrl(_contentMock.Object, UrlMode.Default, culture, It.IsAny<Uri>()), Times.Once);
    }

    [Test]
    public void Url_PassesUrlMode_ToPublishedUrlProvider()
    {
        const string expectedUrl = "https://example.com/test-page";
        const UrlMode mode = UrlMode.Absolute;
        
        _publishedUrlProviderMock
            .Setup(x => x.GetUrl(_contentMock.Object, mode, null, It.IsAny<Uri>()))
            .Returns(expectedUrl);

        var result = _contentMock.Object.Url(_publishedUrlProviderMock.Object, mode: mode);

        Assert.That(result, Is.EqualTo(expectedUrl));
        _publishedUrlProviderMock.Verify(x => x.GetUrl(_contentMock.Object, mode, null, It.IsAny<Uri>()), Times.Once);
    }

    [Test]
    public void Url_PassesCultureAndUrlMode_ToPublishedUrlProvider()
    {
        const string culture = "de-DE";
        const string expectedUrl = "https://example.de/de/test-seite";
        const UrlMode mode = UrlMode.Absolute;
        
        _publishedUrlProviderMock
            .Setup(x => x.GetUrl(_contentMock.Object, mode, culture, It.IsAny<Uri>()))
            .Returns(expectedUrl);

        var result = _contentMock.Object.Url(_publishedUrlProviderMock.Object, culture, mode);

        Assert.That(result, Is.EqualTo(expectedUrl));
        _publishedUrlProviderMock.Verify(x => x.GetUrl(_contentMock.Object, mode, culture, It.IsAny<Uri>()), Times.Once);
    }

    [Test]
    public void Url_ReturnsHashUrl_WhenProviderReturnsNull()
    {
        _publishedUrlProviderMock
            .Setup(x => x.GetUrl(_contentMock.Object, It.IsAny<UrlMode>(), It.IsAny<string>(), It.IsAny<Uri>()))
            .Returns((string?)null);

        var result = _contentMock.Object.Url(_publishedUrlProviderMock.Object);

        Assert.That(result, Is.EqualTo("#"));
    }

    [Test]
    public void Url_ReturnsHashUrl_WhenProviderReturnsEmptyString()
    {
        _publishedUrlProviderMock
            .Setup(x => x.GetUrl(_contentMock.Object, It.IsAny<UrlMode>(), It.IsAny<string>(), It.IsAny<Uri>()))
            .Returns(string.Empty);

        var result = _contentMock.Object.Url(_publishedUrlProviderMock.Object);

        Assert.That(result, Is.EqualTo("#"));
    }

    #endregion

    #region UrlSegment Tests

    [Test]
    [Obsolete("Testing obsolete method for backward compatibility")]
    public void UrlSegment_ThrowsArgumentNullException_WhenContentIsNull()
    {
        IPublishedContent content = null!;

        Assert.Throws<ArgumentNullException>(() =>
            content.UrlSegment(_variationContextAccessorMock.Object));
    }

    [Test]
    [Obsolete("Testing obsolete method for backward compatibility")]
    public void UrlSegment_ReturnsUrlSegment_ForInvariantContent()
    {
        const string expectedSegment = "test-segment";
        
        _contentMock.Setup(x => x.UrlSegment).Returns(expectedSegment);
        _contentTypeMock.Setup(x => x.VariesByCulture()).Returns(false);

        var result = _contentMock.Object.UrlSegment(_variationContextAccessorMock.Object);

        Assert.That(result, Is.EqualTo(expectedSegment));
    }

    [Test]
    [Obsolete("Testing obsolete method for backward compatibility")]
    public void UrlSegment_ReturnsUrlSegment_ForSpecificCulture()
    {
        const string culture = "fr-FR";
        const string expectedSegment = "segment-francais";
        
        _contentMock.Setup(x => x.GetUrlSegment(_variationContextAccessorMock.Object, culture))
            .Returns(expectedSegment);
        _contentTypeMock.Setup(x => x.VariesByCulture()).Returns(true);

        var result = _contentMock.Object.UrlSegment(_variationContextAccessorMock.Object, culture);

        Assert.That(result, Is.EqualTo(expectedSegment));
    }

    [Test]
    [Obsolete("Testing obsolete method for backward compatibility")]
    public void UrlSegment_UsesVariationContext_WhenCultureIsNull()
    {
        const string expectedSegment = "context-segment";
        const string contextCulture = "de-DE";
        
        var variationContext = new Mock<IVariationContext>();
        variationContext.Setup(x => x.Culture).Returns(contextCulture);
        _variationContextAccessorMock.Setup(x => x.VariationContext).Returns(variationContext.Object);
        
        _contentMock.Setup(x => x.GetUrlSegment(_variationContextAccessorMock.Object, contextCulture))
            .Returns(expectedSegment);
        _contentTypeMock.Setup(x => x.VariesByCulture()).Returns(true);

        var result = _contentMock.Object.UrlSegment(_variationContextAccessorMock.Object);

        Assert.That(result, Is.EqualTo(expectedSegment));
    }

    [Test]
    [Obsolete("Testing obsolete method for backward compatibility")]
    public void UrlSegment_ReturnsNull_WhenUrlSegmentNotAvailable()
    {
        _contentMock.Setup(x => x.GetUrlSegment(It.IsAny<IVariationContextAccessor>(), It.IsAny<string>()))
            .Returns((string?)null);
        _contentTypeMock.Setup(x => x.VariesByCulture()).Returns(true);

        var result = _contentMock.Object.UrlSegment(_variationContextAccessorMock.Object, "en-US");

        Assert.That(result, Is.Null);
    }

    #endregion

    #region Different UrlMode Tests

    [Test]
    public void Url_WithDefaultMode_ReturnsRelativeUrl()
    {
        const string expectedUrl = "/products/item-1";
        
        _publishedUrlProviderMock
            .Setup(x => x.GetUrl(_contentMock.Object, UrlMode.Default, null, It.IsAny<Uri>()))
            .Returns(expectedUrl);

        var result = _contentMock.Object.Url(_publishedUrlProviderMock.Object, mode: UrlMode.Default);

        Assert.That(result, Is.EqualTo(expectedUrl));
    }

    [Test]
    public void Url_WithAbsoluteMode_ReturnsAbsoluteUrl()
    {
        const string expectedUrl = "https://www.example.com/products/item-1";
        
        _publishedUrlProviderMock
            .Setup(x => x.GetUrl(_contentMock.Object, UrlMode.Absolute, null, It.IsAny<Uri>()))
            .Returns(expectedUrl);

        var result = _contentMock.Object.Url(_publishedUrlProviderMock.Object, mode: UrlMode.Absolute);

        Assert.That(result, Is.EqualTo(expectedUrl));
    }

    [Test]
    public void Url_WithAutoMode_ReturnsAppropriateUrl()
    {
        const string expectedUrl = "/auto-determined-url";
        
        _publishedUrlProviderMock
            .Setup(x => x.GetUrl(_contentMock.Object, UrlMode.Auto, null, It.IsAny<Uri>()))
            .Returns(expectedUrl);

        var result = _contentMock.Object.Url(_publishedUrlProviderMock.Object, mode: UrlMode.Auto);

        Assert.That(result, Is.EqualTo(expectedUrl));
    }

    #endregion

    #region Edge Cases

    [Test]
    public void Url_HandlesSpecialCharacters_InUrl()
    {
        const string expectedUrl = "/products/special-item-&-more";
        
        _publishedUrlProviderMock
            .Setup(x => x.GetUrl(_contentMock.Object, UrlMode.Default, null, It.IsAny<Uri>()))
            .Returns(expectedUrl);

        var result = _contentMock.Object.Url(_publishedUrlProviderMock.Object);

        Assert.That(result, Is.EqualTo(expectedUrl));
    }

    [Test]
    public void Url_HandlesUnicodeCharacters_InUrl()
    {
        const string expectedUrl = "/products/日本語-item";
        
        _publishedUrlProviderMock
            .Setup(x => x.GetUrl(_contentMock.Object, UrlMode.Default, null, It.IsAny<Uri>()))
            .Returns(expectedUrl);

        var result = _contentMock.Object.Url(_publishedUrlProviderMock.Object);

        Assert.That(result, Is.EqualTo(expectedUrl));
    }

    [Test]
    public void Url_HandlesQueryString_InUrl()
    {
        const string expectedUrl = "/products/item?category=electronics&sort=price";
        
        _publishedUrlProviderMock
            .Setup(x => x.GetUrl(_contentMock.Object, UrlMode.Default, null, It.IsAny<Uri>()))
            .Returns(expectedUrl);

        var result = _contentMock.Object.Url(_publishedUrlProviderMock.Object);

        Assert.That(result, Is.EqualTo(expectedUrl));
    }

    [Test]
    public void Url_HandlesFragment_InUrl()
    {
        const string expectedUrl = "/products/item#reviews";
        
        _publishedUrlProviderMock
            .Setup(x => x.GetUrl(_contentMock.Object, UrlMode.Default, null, It.IsAny<Uri>()))
            .Returns(expectedUrl);

        var result = _contentMock.Object.Url(_publishedUrlProviderMock.Object);

        Assert.That(result, Is.EqualTo(expectedUrl));
    }

    #endregion
}