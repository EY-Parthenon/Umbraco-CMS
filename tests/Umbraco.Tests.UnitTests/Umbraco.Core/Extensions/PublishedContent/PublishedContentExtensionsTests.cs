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
/// Tests for the main PublishedContentExtensions facade class.
/// This class verifies that all methods correctly delegate to the specialized extension classes.
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

    #region Property Access Delegation Tests

    [Test]
    public void Name_DelegatesToPropertyExtensions()
    {
        const string expectedName = "Test Content";
        const string culture = "en-US";
        var cultures = new Dictionary<string, PublishedCultureInfo>
        {
            [culture] = new PublishedCultureInfo(culture, expectedName, string.Empty, DateTime.Now)
        };

        _contentTypeMock.Setup(x => x.VariesByCulture()).Returns(true);
        _contentMock.Setup(x => x.Cultures).Returns(cultures);

        // Call through facade
        var result = PublishedContentExtensions.Name(_contentMock.Object, _variationContextAccessorMock.Object, culture);

        Assert.That(result, Is.EqualTo(expectedName));
    }

    [Test]
    public void HasValue_DelegatesToPropertyExtensions()
    {
        const string alias = "property";
        var propertyMock = new Mock<IPublishedProperty>();
        propertyMock.Setup(x => x.HasValue(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        _contentMock.Setup(x => x.HasProperty(alias)).Returns(true);
        _contentMock.Setup(x => x.GetProperty(alias)).Returns(propertyMock.Object);

        // Call through facade
        var result = PublishedContentExtensions.HasValue(
            _contentMock.Object, 
            _publishedValueFallbackMock.Object, 
            alias);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Value_DelegatesToPropertyExtensions()
    {
        const string alias = "property";
        const string expectedValue = "Test Value";
        var propertyMock = new Mock<IPublishedProperty>();
        propertyMock.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<string>())).Returns(expectedValue);

        _contentMock.Setup(x => x.GetProperty(alias)).Returns(propertyMock.Object);

        // Call through facade
        var result = PublishedContentExtensions.Value(
            _contentMock.Object,
            _publishedValueFallbackMock.Object,
            alias);

        Assert.That(result, Is.EqualTo(expectedValue));
    }

    [Test]
    public void ValueGeneric_DelegatesToPropertyExtensions()
    {
        const string alias = "intProperty";
        const int expectedValue = 42;
        var propertyMock = new Mock<IPublishedProperty>();
        propertyMock.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<string>())).Returns(expectedValue);

        _contentMock.Setup(x => x.GetProperty(alias)).Returns(propertyMock.Object);

        // Call through facade
        var result = PublishedContentExtensions.Value<int>(
            _contentMock.Object,
            _publishedValueFallbackMock.Object,
            alias);

        Assert.That(result, Is.EqualTo(expectedValue));
    }

    #endregion

    #region URL Operations Delegation Tests

    [Test]
    public void Url_DelegatesToUrlExtensions()
    {
        const string expectedUrl = "/test-page";
        const string culture = "fr-FR";
        const UrlMode mode = UrlMode.Absolute;

        _publishedUrlProviderMock
            .Setup(x => x.GetUrl(_contentMock.Object, mode, culture, It.IsAny<Uri>()))
            .Returns(expectedUrl);

        // Call through facade
        var result = PublishedContentExtensions.Url(
            _contentMock.Object,
            _publishedUrlProviderMock.Object,
            culture,
            mode);

        Assert.That(result, Is.EqualTo(expectedUrl));
    }

    [Test]
    [Obsolete("Testing obsolete method")]
    public void UrlSegment_DelegatesToUrlExtensions()
    {
        const string expectedSegment = "test-segment";
        const string culture = "en-US";

        _contentMock.Setup(x => x.GetUrlSegment(_variationContextAccessorMock.Object, culture))
            .Returns(expectedSegment);
        _contentTypeMock.Setup(x => x.VariesByCulture()).Returns(true);

        // Call through facade
        var result = PublishedContentExtensions.UrlSegment(
            _contentMock.Object,
            _variationContextAccessorMock.Object,
            culture);

        Assert.That(result, Is.EqualTo(expectedSegment));
    }

    #endregion

    #region Navigation Delegation Tests

    [Test]
    public void Parent_DelegatesToNavigationExtensions()
    {
        var parentMock = new Mock<IPublishedContent>();
        parentMock.Setup(x => x.Id).Returns(0);
        parentMock.Setup(x => x.Name).Returns("Parent");

        _navigationQueryServiceMock
            .Setup(x => x.TryGetParentKey(_contentMock.Object.Key, out It.Ref<Guid?>.IsAny))
            .Returns((Guid key, out Guid? parentKey) =>
            {
                parentKey = parentMock.Object.Key;
                return true;
            });

        _navigationQueryServiceMock
            .Setup(x => x.TryGetPublishedContent(It.IsAny<Guid>(), out It.Ref<IPublishedContent?>.IsAny))
            .Returns((Guid key, out IPublishedContent? content) =>
            {
                content = parentMock.Object;
                return true;
            });

        _publishedStatusFilteringServiceMock
            .Setup(x => x.FilterContent(It.IsAny<IPublishedContent>()))
            .Returns(parentMock.Object);

        // Call through facade
        var result = PublishedContentExtensions.Parent<IPublishedContent>(
            _contentMock.Object,
            _navigationQueryServiceMock.Object,
            _publishedStatusFilteringServiceMock.Object);

        Assert.That(result, Is.EqualTo(parentMock.Object));
    }

    [Test]
    public void Ancestors_DelegatesToNavigationExtensions()
    {
        var ancestor1 = CreateMockContent(2, "Ancestor1");
        var ancestor2 = CreateMockContent(3, "Ancestor2");
        var ancestors = new[] { ancestor1, ancestor2 };

        _navigationQueryServiceMock
            .Setup(x => x.TryGetAncestorsKeys(_contentMock.Object.Key, out It.Ref<IEnumerable<Guid>>.IsAny))
            .Returns((Guid key, out IEnumerable<Guid> ancestorKeys) =>
            {
                ancestorKeys = ancestors.Select(a => a.Key);
                return true;
            });

        foreach (var ancestor in ancestors)
        {
            var localAncestor = ancestor;
            _navigationQueryServiceMock
                .Setup(x => x.TryGetPublishedContent(localAncestor.Key, out It.Ref<IPublishedContent?>.IsAny))
                .Returns((Guid key, out IPublishedContent? content) =>
                {
                    content = localAncestor;
                    return true;
                });
        }

        _publishedStatusFilteringServiceMock
            .Setup(x => x.FilterContent(It.IsAny<IEnumerable<IPublishedContent>>()))
            .Returns<IEnumerable<IPublishedContent>>(items => items);

        // Call through facade
        var result = PublishedContentExtensions.Ancestors(
            _contentMock.Object,
            _navigationQueryServiceMock.Object,
            _publishedStatusFilteringServiceMock.Object).ToList();

        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public void Children_DelegatesToNavigationExtensions()
    {
        var child1 = CreateMockContent(10, "Child1");
        var child2 = CreateMockContent(11, "Child2");
        var children = new[] { child1, child2 };

        _navigationQueryServiceMock
            .Setup(x => x.TryGetChildrenKeys(_contentMock.Object.Key, out It.Ref<IEnumerable<Guid>>.IsAny))
            .Returns((Guid key, out IEnumerable<Guid> childKeys) =>
            {
                childKeys = children.Select(c => c.Key);
                return true;
            });

        foreach (var child in children)
        {
            var localChild = child;
            _navigationQueryServiceMock
                .Setup(x => x.TryGetPublishedContent(localChild.Key, out It.Ref<IPublishedContent?>.IsAny))
                .Returns((Guid key, out IPublishedContent? content) =>
                {
                    content = localChild;
                    return true;
                });
        }

        _publishedStatusFilteringServiceMock
            .Setup(x => x.FilterContent(It.IsAny<IEnumerable<IPublishedContent>>()))
            .Returns<IEnumerable<IPublishedContent>>(items => items);

        // Call through facade
        var result = PublishedContentExtensions.Children(
            _contentMock.Object,
            _navigationQueryServiceMock.Object,
            _publishedStatusFilteringServiceMock.Object).ToList();

        Assert.That(result.Count, Is.EqualTo(2));
    }

    #endregion

    #region Relationship Delegation Tests

    [Test]
    public void IsEqual_DelegatesToNavigationExtensions()
    {
        var otherContent = _contentMock.Object;

        // Call through facade
        var result = PublishedContentExtensions.IsEqual(_contentMock.Object, otherContent);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsDescendant_DelegatesToNavigationExtensions()
    {
        _contentMock.Setup(x => x.Path).Returns("-1,2,3,4");

        var ancestorMock = new Mock<IPublishedContent>();
        ancestorMock.Setup(x => x.Id).Returns(2);
        ancestorMock.Setup(x => x.Path).Returns("-1,2");

        // Call through facade
        var result = PublishedContentExtensions.IsDescendant(_contentMock.Object, ancestorMock.Object);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsAncestor_DelegatesToNavigationExtensions()
    {
        _contentMock.Setup(x => x.Path).Returns("-1,2");

        var descendantMock = new Mock<IPublishedContent>();
        descendantMock.Setup(x => x.Id).Returns(4);
        descendantMock.Setup(x => x.Path).Returns("-1,2,3,4");

        // Call through facade
        var result = PublishedContentExtensions.IsAncestor(_contentMock.Object, descendantMock.Object);

        Assert.That(result, Is.True);
    }

    #endregion

    #region Culture Delegation Tests

    [Test]
    public void HasCulture_DelegatesToCultureExtensions()
    {
        const string culture = "fr-FR";
        var cultures = new Dictionary<string, PublishedCultureInfo>
        {
            [culture] = new PublishedCultureInfo(culture, "French", "french", DateTime.Now)
        };

        _contentTypeMock.Setup(x => x.VariesByCulture()).Returns(true);
        _contentMock.Setup(x => x.Cultures).Returns(cultures);

        // Call through facade
        var result = PublishedContentExtensions.HasCulture(_contentMock.Object, culture);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsInvariantOrHasCulture_DelegatesToCultureExtensions()
    {
        _contentTypeMock.Setup(x => x.VariesByCulture()).Returns(false);

        // Call through facade
        var result = PublishedContentExtensions.IsInvariantOrHasCulture(_contentMock.Object, "any-culture");

        Assert.That(result, Is.True);
    }

    [Test]
    public void CultureDate_DelegatesToCultureExtensions()
    {
        var expectedDate = new DateTime(2024, 1, 15);
        var cultures = new Dictionary<string, PublishedCultureInfo>
        {
            ["en-US"] = new PublishedCultureInfo("en-US", "English", "english", expectedDate)
        };

        _contentTypeMock.Setup(x => x.VariesByCulture()).Returns(true);
        _contentMock.Setup(x => x.Cultures).Returns(cultures);

        // Call through facade
        var result = PublishedContentExtensions.CultureDate(
            _contentMock.Object,
            _variationContextAccessorMock.Object,
            "en-US");

        Assert.That(result, Is.EqualTo(expectedDate));
    }

    #endregion

    #region Content Type Delegation Tests

    [Test]
    public void IsComposedOf_DelegatesToTypeExtensions()
    {
        const string compositionAlias = "seoComposition";

        _contentTypeMock.Setup(x => x.IsComposedOf(compositionAlias)).Returns(true);

        // Call through facade
        var result = PublishedContentExtensions.IsComposedOf(_contentMock.Object, compositionAlias);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsDocumentType_DelegatesToTypeExtensions()
    {
        const string docTypeAlias = "articlePage";

        _contentTypeMock.Setup(x => x.Alias).Returns(docTypeAlias);

        // Call through facade
        var result = PublishedContentExtensions.IsDocumentType(_contentMock.Object, docTypeAlias);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsDocumentType_WithRecursive_DelegatesToTypeExtensions()
    {
        const string baseTypeAlias = "basePage";
        const string currentTypeAlias = "articlePage";

        _contentTypeMock.Setup(x => x.Alias).Returns(currentTypeAlias);
        _contentTypeMock.Setup(x => x.CompositionAliases).Returns(new[] { baseTypeAlias, currentTypeAlias });

        // Call through facade
        var result = PublishedContentExtensions.IsDocumentType(_contentMock.Object, baseTypeAlias, recursive: true);

        Assert.That(result, Is.True);
    }

    #endregion

    #region Template Delegation Tests

    [Test]
    public void GetTemplateAlias_DelegatesToTemplateExtensions()
    {
        const int templateId = 123;
        const string expectedAlias = "ArticleTemplate";
        var templateMock = new Mock<ITemplate>();

        _contentMock.Setup(x => x.TemplateId).Returns(templateId);
        templateMock.Setup(x => x.Alias).Returns(expectedAlias);
        _fileServiceMock.Setup(x => x.GetTemplate(templateId)).Returns(templateMock.Object);

        // Call through facade
        var result = PublishedContentExtensions.GetTemplateAlias(_contentMock.Object, _fileServiceMock.Object);

        Assert.That(result, Is.EqualTo(expectedAlias));
    }

    [Test]
    public void IsAllowedTemplate_WithWebRoutingSettings_DelegatesToTemplateExtensions()
    {
        const int templateId = 456;
        var webRoutingSettings = new WebRoutingSettings
        {
            DisableAlternativeTemplates = true
        };

        // Call through facade
        var result = PublishedContentExtensions.IsAllowedTemplate(
            _contentMock.Object,
            _contentTypeServiceMock.Object,
            webRoutingSettings,
            templateId);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsAllowedTemplate_WithFlags_DelegatesToTemplateExtensions()
    {
        const int templateId = 456;

        // Call through facade
        var result = PublishedContentExtensions.IsAllowedTemplate(
            _contentMock.Object,
            _contentTypeServiceMock.Object,
            disableAlternativeTemplates: true,
            validateAlternativeTemplates: false,
            templateId);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsAllowedTemplate_ByAlias_DelegatesToTemplateExtensions()
    {
        const string templateAlias = "ArticleTemplate";
        const int templateId = 456;
        var templateMock = new Mock<ITemplate>();

        templateMock.Setup(x => x.Id).Returns(templateId);
        _fileServiceMock.Setup(x => x.GetTemplate(templateAlias)).Returns(templateMock.Object);
        _contentMock.Setup(x => x.TemplateId).Returns(templateId);

        // Call through facade
        var result = PublishedContentExtensions.IsAllowedTemplate(
            _contentMock.Object,
            _fileServiceMock.Object,
            _contentTypeServiceMock.Object,
            disableAlternativeTemplates: false,
            validateAlternativeTemplates: false,
            templateAlias);

        Assert.That(result, Is.True);
    }

    #endregion

    #region Integration Tests

    [Test]
    public void AllDelegationMethods_HandleNullContent_Consistently()
    {
        IPublishedContent nullContent = null!;

        // All methods should throw ArgumentNullException for null content
        Assert.Throws<ArgumentNullException>(() => 
            PublishedContentExtensions.Name(nullContent, _variationContextAccessorMock.Object));
        
        Assert.Throws<ArgumentNullException>(() => 
            PublishedContentExtensions.HasValue(nullContent, _publishedValueFallbackMock.Object, "alias"));
        
        Assert.Throws<ArgumentNullException>(() => 
            PublishedContentExtensions.Url(nullContent, _publishedUrlProviderMock.Object));
        
        Assert.Throws<ArgumentNullException>(() => 
            PublishedContentExtensions.HasCulture(nullContent, "en-US"));
        
        Assert.Throws<ArgumentNullException>(() => 
            PublishedContentExtensions.IsComposedOf(nullContent, "alias"));
        
        Assert.Throws<ArgumentNullException>(() => 
            PublishedContentExtensions.GetTemplateAlias(nullContent, _fileServiceMock.Object));
    }

    [Test]
    public void FacadeMethods_PreserveOriginalBehavior()
    {
        // This test ensures that the facade methods maintain the same behavior
        // as the original implementation would have had
        
        // Setup a complete scenario
        const string propertyAlias = "title";
        const string propertyValue = "Page Title";
        const string url = "/page";
        const string culture = "en-US";
        
        var propertyMock = new Mock<IPublishedProperty>();
        propertyMock.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<string>())).Returns(propertyValue);
        _contentMock.Setup(x => x.GetProperty(propertyAlias)).Returns(propertyMock.Object);
        
        _publishedUrlProviderMock
            .Setup(x => x.GetUrl(_contentMock.Object, It.IsAny<UrlMode>(), It.IsAny<string>(), It.IsAny<Uri>()))
            .Returns(url);
        
        var cultures = new Dictionary<string, PublishedCultureInfo>
        {
            [culture] = new PublishedCultureInfo(culture, "English", "english", DateTime.Now)
        };
        _contentTypeMock.Setup(x => x.VariesByCulture()).Returns(true);
        _contentMock.Setup(x => x.Cultures).Returns(cultures);
        
        // Test multiple facade methods work together
        var titleValue = PublishedContentExtensions.Value(_contentMock.Object, _publishedValueFallbackMock.Object, propertyAlias);
        var pageUrl = PublishedContentExtensions.Url(_contentMock.Object, _publishedUrlProviderMock.Object);
        var hasCulture = PublishedContentExtensions.HasCulture(_contentMock.Object, culture);
        
        Assert.That(titleValue, Is.EqualTo(propertyValue));
        Assert.That(pageUrl, Is.EqualTo(url));
        Assert.That(hasCulture, Is.True);
    }

    #endregion

    #region Helper Methods

    private IPublishedContent CreateMockContent(int id, string name)
    {
        var mock = new Mock<IPublishedContent>();
        mock.Setup(x => x.Id).Returns(id);
        mock.Setup(x => x.Name).Returns(name);
        mock.Setup(x => x.Key).Returns(Guid.NewGuid());
        return mock.Object;
    }

    #endregion
}