using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions.PublishedContent;

/// <summary>
/// Integration tests for the PublishedContentExtensions class.
/// These tests verify that the original extension methods continue to work correctly
/// alongside the new specialized extension classes.
/// </summary>
[TestFixture]
public class PublishedContentExtensionsTests
{
    private Mock<IPublishedContent> _contentMock;
    private Mock<IVariationContextAccessor> _variationContextAccessorMock;
    private Mock<IPublishedValueFallback> _publishedValueFallbackMock;
    private Mock<IPublishedUrlProvider> _publishedUrlProviderMock;
    private Mock<INavigationQueryService> _navigationQueryServiceMock;
    private Mock<IPublishedStatusFilteringService> _publishedStatusFilteringServiceMock;
    private Mock<IFileService> _fileServiceMock;
    private Mock<IContentTypeService> _contentTypeServiceMock;
    private Mock<IPublishedContentType> _contentTypeMock;

    [SetUp]
    public void Setup()
    {
        _contentMock = new Mock<IPublishedContent>();
        _variationContextAccessorMock = new Mock<IVariationContextAccessor>();
        _publishedValueFallbackMock = new Mock<IPublishedValueFallback>();
        _publishedUrlProviderMock = new Mock<IPublishedUrlProvider>();
        _navigationQueryServiceMock = new Mock<INavigationQueryService>();
        _publishedStatusFilteringServiceMock = new Mock<IPublishedStatusFilteringService>();
        _fileServiceMock = new Mock<IFileService>();
        _contentTypeServiceMock = new Mock<IContentTypeService>();
        _contentTypeMock = new Mock<IPublishedContentType>();

        _contentMock.Setup(x => x.ContentType).Returns(_contentTypeMock.Object);
        _contentMock.Setup(x => x.Id).Returns(1);
        _contentMock.Setup(x => x.Key).Returns(Guid.NewGuid());
    }

    #region Integration Tests

    [Test]
    public void SpecializedExtensions_WorkAlongsideOriginalExtensions()
    {
        // This test verifies that both the original PublishedContentExtensions methods
        // and the new specialized extension methods can be used in the same codebase
        
        const string propertyAlias = "title";
        const string propertyValue = "Page Title";
        var propertyMock = new Mock<IPublishedProperty>();
        propertyMock.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<string>())).Returns(propertyValue);
        _contentMock.Setup(x => x.GetProperty(propertyAlias)).Returns(propertyMock.Object);

        // Use the specialized extension directly
        var hasValueSpecialized = PublishedContentPropertyExtensions.HasValue(
            _contentMock.Object, 
            _publishedValueFallbackMock.Object, 
            propertyAlias);

        // Use the original extension method
        var hasValueOriginal = _contentMock.Object.HasValue(
            _publishedValueFallbackMock.Object,
            propertyAlias);

        // Both should work correctly
        Assert.That(hasValueSpecialized, Is.EqualTo(hasValueOriginal));
    }

    [Test]
    public void AllExtensionClasses_HandleNullContent_Consistently()
    {
        IPublishedContent nullContent = null!;

        // All specialized extension classes should throw ArgumentNullException for null content
        Assert.Throws<ArgumentNullException>(() => 
            PublishedContentPropertyExtensions.Name(nullContent, _variationContextAccessorMock.Object));
        
        Assert.Throws<ArgumentNullException>(() => 
            PublishedContentPropertyExtensions.HasValue(nullContent, _publishedValueFallbackMock.Object, "alias"));
        
        Assert.Throws<ArgumentNullException>(() => 
            PublishedContentUrlExtensions.Url(nullContent, _publishedUrlProviderMock.Object));
        
        Assert.Throws<ArgumentNullException>(() => 
            PublishedContentCultureExtensions.HasCulture(nullContent, "en-US"));
        
        Assert.Throws<ArgumentNullException>(() => 
            PublishedContentTypeExtensions.IsComposedOf(nullContent, "alias"));
        
        Assert.Throws<ArgumentNullException>(() => 
            PublishedContentTemplateExtensions.GetTemplateAlias(nullContent, _fileServiceMock.Object));
    }

    [Test]
    public void SpecializedExtensions_CanBeUsedDirectly()
    {
        // Setup
        const string culture = "fr-FR";
        var cultures = new Dictionary<string, PublishedCultureInfo>
        {
            [culture] = new PublishedCultureInfo(culture, "French", "french", DateTime.Now)
        };
        _contentTypeMock.Setup(x => x.VariesByCulture()).Returns(true);
        _contentMock.Setup(x => x.Cultures).Returns(cultures);

        const string compositionAlias = "seoComposition";
        _contentTypeMock.Setup(x => x.IsComposedOf(compositionAlias)).Returns(true);

        // Test using specialized extensions directly
        var hasCulture = PublishedContentCultureExtensions.HasCulture(_contentMock.Object, culture);
        var isComposedOf = PublishedContentTypeExtensions.IsComposedOf(_contentMock.Object, compositionAlias);

        Assert.That(hasCulture, Is.True);
        Assert.That(isComposedOf, Is.True);
    }

    [Test]
    public void NavigationExtensions_WorkCorrectly()
    {
        // Test that navigation extensions can be used directly
        _contentMock.Setup(x => x.Path).Returns("-1,2,3,4");
        
        var ancestorMock = new Mock<IPublishedContent>();
        ancestorMock.Setup(x => x.Id).Returns(2);
        ancestorMock.Setup(x => x.Path).Returns("-1,2");
        
        var isDescendant = PublishedContentNavigationExtensions.IsDescendant(_contentMock.Object, ancestorMock.Object);
        
        Assert.That(isDescendant, Is.True);
    }

    [Test]
    public void PropertyExtensions_WorkCorrectly()
    {
        const string expectedName = "Test Content";
        const string culture = "en-US";
        var cultures = new Dictionary<string, PublishedCultureInfo>
        {
            [culture] = new PublishedCultureInfo(culture, expectedName, string.Empty, DateTime.Now)
        };

        _contentTypeMock.Setup(x => x.VariesByCulture()).Returns(true);
        _contentMock.Setup(x => x.Cultures).Returns(cultures);

        var name = PublishedContentPropertyExtensions.Name(_contentMock.Object, _variationContextAccessorMock.Object, culture);

        Assert.That(name, Is.EqualTo(expectedName));
    }

    [Test]
    public void UrlExtensions_WorkCorrectly()
    {
        const string expectedUrl = "/test-page";
        
        _publishedUrlProviderMock
            .Setup(x => x.GetUrl(_contentMock.Object, UrlMode.Default, null, It.IsAny<Uri>()))
            .Returns(expectedUrl);

        var url = PublishedContentUrlExtensions.Url(_contentMock.Object, _publishedUrlProviderMock.Object);

        Assert.That(url, Is.EqualTo(expectedUrl));
    }

    [Test]
    public void TemplateExtensions_WorkCorrectly()
    {
        const int templateId = 123;
        const string expectedAlias = "ArticleTemplate";
        var templateMock = new Mock<ITemplate>();

        _contentMock.Setup(x => x.TemplateId).Returns(templateId);
        templateMock.Setup(x => x.Alias).Returns(expectedAlias);
        _fileServiceMock.Setup(x => x.GetTemplate(templateId)).Returns(templateMock.Object);

        var alias = PublishedContentTemplateExtensions.GetTemplateAlias(_contentMock.Object, _fileServiceMock.Object);

        Assert.That(alias, Is.EqualTo(expectedAlias));
    }

    #endregion

    #region Backward Compatibility Tests

    [Test]
    public void OriginalExtensionMethods_StillExist()
    {
        // This test verifies that all the original extension methods still exist
        // and can be called through the main PublishedContentExtensions class
        
        // These should all compile and run without errors
        Assert.DoesNotThrow(() =>
        {
            // Property methods
            _ = _contentMock.Object.Name(_variationContextAccessorMock.Object);
            _ = _contentMock.Object.HasValue(_publishedValueFallbackMock.Object, "alias");
            _ = _contentMock.Object.Value(_publishedValueFallbackMock.Object, "alias");
            _ = _contentMock.Object.Value<int>(_publishedValueFallbackMock.Object, "alias");
            
            // URL methods
            _ = _contentMock.Object.Url(_publishedUrlProviderMock.Object);
            
            // Culture methods
            _ = _contentMock.Object.HasCulture("en-US");
            _ = _contentMock.Object.IsInvariantOrHasCulture("en-US");
            _ = _contentMock.Object.CultureDate(_variationContextAccessorMock.Object);
            
            // Type methods
            _ = _contentMock.Object.IsComposedOf("alias");
            _ = _contentMock.Object.IsDocumentType("docType");
            
            // Template methods
            _ = _contentMock.Object.GetTemplateAlias(_fileServiceMock.Object);
        });
    }

    #endregion
}