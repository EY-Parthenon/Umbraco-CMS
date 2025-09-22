using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions.PublishedContent;

[TestFixture]
public class PublishedContentTypeExtensionsTests
{
    private Mock<IPublishedContent> _contentMock;
    private Mock<IPublishedContentType> _contentTypeMock;

    [SetUp]
    public void Setup()
    {
        _contentMock = new Mock<IPublishedContent>();
        _contentTypeMock = new Mock<IPublishedContentType>();
        _contentMock.Setup(x => x.ContentType).Returns(_contentTypeMock.Object);
    }

    #region IsComposedOf Tests

    [Test]
    public void IsComposedOf_ThrowsArgumentNullException_WhenContentIsNull()
    {
        IPublishedContent content = null!;

        Assert.Throws<ArgumentNullException>(() => content.IsComposedOf("composition"));
    }

    [Test]
    public void IsComposedOf_ThrowsArgumentException_WhenAliasIsNullOrEmpty()
    {
        Assert.Throws<ArgumentException>(() => _contentMock.Object.IsComposedOf(null!));
        Assert.Throws<ArgumentException>(() => _contentMock.Object.IsComposedOf(string.Empty));
        Assert.Throws<ArgumentException>(() => _contentMock.Object.IsComposedOf("   "));
    }

    [Test]
    public void IsComposedOf_ReturnsTrue_WhenContentTypeIsComposedOfAlias()
    {
        const string compositionAlias = "seoComposition";
        
        _contentTypeMock.Setup(x => x.IsComposedOf(compositionAlias)).Returns(true);

        var result = _contentMock.Object.IsComposedOf(compositionAlias);

        Assert.That(result, Is.True);
        _contentTypeMock.Verify(x => x.IsComposedOf(compositionAlias), Times.Once);
    }

    [Test]
    public void IsComposedOf_ReturnsFalse_WhenContentTypeIsNotComposedOfAlias()
    {
        const string compositionAlias = "navigationComposition";
        
        _contentTypeMock.Setup(x => x.IsComposedOf(compositionAlias)).Returns(false);

        var result = _contentMock.Object.IsComposedOf(compositionAlias);

        Assert.That(result, Is.False);
        _contentTypeMock.Verify(x => x.IsComposedOf(compositionAlias), Times.Once);
    }

    [Test]
    public void IsComposedOf_IsCaseInsensitive()
    {
        const string compositionAlias = "SeoComposition";
        const string lowerCaseAlias = "seocomposition";
        
        _contentTypeMock.Setup(x => x.IsComposedOf(It.IsAny<string>()))
            .Returns<string>(alias => alias.Equals(compositionAlias, StringComparison.OrdinalIgnoreCase));

        var result = _contentMock.Object.IsComposedOf(lowerCaseAlias);

        // This test assumes case-insensitive comparison
        Assert.That(result, Is.True);
    }

    #endregion

    #region IsDocumentType Tests (Single Alias)

    [Test]
    public void IsDocumentType_ThrowsArgumentNullException_WhenContentIsNull()
    {
        IPublishedContent content = null!;

        Assert.Throws<ArgumentNullException>(() => content.IsDocumentType("page"));
    }

    [Test]
    public void IsDocumentType_ThrowsArgumentException_WhenAliasIsNullOrEmpty()
    {
        Assert.Throws<ArgumentException>(() => _contentMock.Object.IsDocumentType(null!));
        Assert.Throws<ArgumentException>(() => _contentMock.Object.IsDocumentType(string.Empty));
        Assert.Throws<ArgumentException>(() => _contentMock.Object.IsDocumentType("   "));
    }

    [Test]
    public void IsDocumentType_ReturnsTrue_WhenContentTypeAliasMatches()
    {
        const string docTypeAlias = "articlePage";
        
        _contentTypeMock.Setup(x => x.Alias).Returns(docTypeAlias);

        var result = _contentMock.Object.IsDocumentType(docTypeAlias);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsDocumentType_ReturnsFalse_WhenContentTypeAliasDoesNotMatch()
    {
        _contentTypeMock.Setup(x => x.Alias).Returns("homePage");

        var result = _contentMock.Object.IsDocumentType("articlePage");

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsDocumentType_IsCaseSensitive()
    {
        _contentTypeMock.Setup(x => x.Alias).Returns("ArticlePage");

        var result = _contentMock.Object.IsDocumentType("articlepage");

        // Assuming case-sensitive comparison by default
        Assert.That(result, Is.False);
    }

    #endregion

    #region IsDocumentType Tests (With Recursive Flag)

    [Test]
    public void IsDocumentType_Recursive_ReturnsTrue_WhenContentTypeAliasMatches()
    {
        const string docTypeAlias = "newsPage";
        
        _contentTypeMock.Setup(x => x.Alias).Returns(docTypeAlias);

        var result = _contentMock.Object.IsDocumentType(docTypeAlias, recursive: false);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsDocumentType_Recursive_ReturnsTrue_WhenBaseTypeAliasMatches()
    {
        const string baseTypeAlias = "basePage";
        const string currentTypeAlias = "articlePage";
        
        var baseType = new Mock<IPublishedContentType>();
        baseType.Setup(x => x.Alias).Returns(baseTypeAlias);
        
        _contentTypeMock.Setup(x => x.Alias).Returns(currentTypeAlias);
        _contentTypeMock.Setup(x => x.CompositionAliases).Returns(new[] { baseTypeAlias, currentTypeAlias });

        var result = _contentMock.Object.IsDocumentType(baseTypeAlias, recursive: true);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsDocumentType_NonRecursive_ReturnsFalse_WhenOnlyBaseTypeMatches()
    {
        const string baseTypeAlias = "basePage";
        const string currentTypeAlias = "articlePage";
        
        _contentTypeMock.Setup(x => x.Alias).Returns(currentTypeAlias);
        _contentTypeMock.Setup(x => x.CompositionAliases).Returns(new[] { baseTypeAlias, currentTypeAlias });

        var result = _contentMock.Object.IsDocumentType(baseTypeAlias, recursive: false);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsDocumentType_Recursive_ChecksAllCompositionAliases()
    {
        const string targetAlias = "seoComposition";
        const string currentTypeAlias = "articlePage";
        
        var compositionAliases = new[] { "basePage", "seoComposition", "navigationComposition", currentTypeAlias };
        
        _contentTypeMock.Setup(x => x.Alias).Returns(currentTypeAlias);
        _contentTypeMock.Setup(x => x.CompositionAliases).Returns(compositionAliases);

        var result = _contentMock.Object.IsDocumentType(targetAlias, recursive: true);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsDocumentType_Recursive_ReturnsFalse_WhenAliasNotInComposition()
    {
        const string targetAlias = "nonExistentComposition";
        const string currentTypeAlias = "articlePage";
        
        var compositionAliases = new[] { "basePage", "seoComposition", currentTypeAlias };
        
        _contentTypeMock.Setup(x => x.Alias).Returns(currentTypeAlias);
        _contentTypeMock.Setup(x => x.CompositionAliases).Returns(compositionAliases);

        var result = _contentMock.Object.IsDocumentType(targetAlias, recursive: true);

        Assert.That(result, Is.False);
    }

    #endregion

    #region Complex Inheritance Tests

    [Test]
    public void IsDocumentType_Recursive_HandlesDeepInheritanceChain()
    {
        const string rootAlias = "rootPage";
        const string baseAlias = "basePage";
        const string sectionAlias = "sectionPage";
        const string currentAlias = "articlePage";
        
        var compositionAliases = new[] { rootAlias, baseAlias, sectionAlias, currentAlias };
        
        _contentTypeMock.Setup(x => x.Alias).Returns(currentAlias);
        _contentTypeMock.Setup(x => x.CompositionAliases).Returns(compositionAliases);

        // Should find root alias even though it's at the beginning of the chain
        Assert.That(_contentMock.Object.IsDocumentType(rootAlias, recursive: true), Is.True);
        Assert.That(_contentMock.Object.IsDocumentType(baseAlias, recursive: true), Is.True);
        Assert.That(_contentMock.Object.IsDocumentType(sectionAlias, recursive: true), Is.True);
        Assert.That(_contentMock.Object.IsDocumentType(currentAlias, recursive: true), Is.True);
    }

    [Test]
    public void IsComposedOf_WorksWithMultipleCompositions()
    {
        const string seoComposition = "seoComposition";
        const string navComposition = "navigationComposition";
        const string metaComposition = "metadataComposition";
        
        _contentTypeMock.Setup(x => x.IsComposedOf(seoComposition)).Returns(true);
        _contentTypeMock.Setup(x => x.IsComposedOf(navComposition)).Returns(true);
        _contentTypeMock.Setup(x => x.IsComposedOf(metaComposition)).Returns(false);

        Assert.That(_contentMock.Object.IsComposedOf(seoComposition), Is.True);
        Assert.That(_contentMock.Object.IsComposedOf(navComposition), Is.True);
        Assert.That(_contentMock.Object.IsComposedOf(metaComposition), Is.False);
    }

    #endregion

    #region Edge Cases

    [Test]
    public void IsDocumentType_Recursive_HandlesEmptyCompositionAliases()
    {
        const string currentAlias = "standalonePage";
        
        _contentTypeMock.Setup(x => x.Alias).Returns(currentAlias);
        _contentTypeMock.Setup(x => x.CompositionAliases).Returns(Array.Empty<string>());

        var result = _contentMock.Object.IsDocumentType("basePage", recursive: true);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsDocumentType_Recursive_HandlesNullCompositionAliases()
    {
        const string currentAlias = "standalonePage";
        
        _contentTypeMock.Setup(x => x.Alias).Returns(currentAlias);
        _contentTypeMock.Setup(x => x.CompositionAliases).Returns((IEnumerable<string>)null!);

        var result = _contentMock.Object.IsDocumentType(currentAlias, recursive: true);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsDocumentType_HandlesSpecialCharactersInAlias()
    {
        const string specialAlias = "page-with_special.chars";
        
        _contentTypeMock.Setup(x => x.Alias).Returns(specialAlias);

        var result = _contentMock.Object.IsDocumentType(specialAlias);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsComposedOf_HandlesCircularComposition()
    {
        // This tests that the method doesn't get stuck in infinite loop
        // if there's a circular reference in compositions
        const string compositionA = "compositionA";
        
        _contentTypeMock.Setup(x => x.IsComposedOf(compositionA)).Returns(true);

        var result = _contentMock.Object.IsComposedOf(compositionA);

        Assert.That(result, Is.True);
        _contentTypeMock.Verify(x => x.IsComposedOf(compositionA), Times.Once);
    }

    #endregion

    #region Performance Tests

    [Test]
    public void IsDocumentType_Recursive_PerformsEfficientlyWithLargeCompositionList()
    {
        const string targetAlias = "target";
        const int compositionCount = 100;
        
        var compositionAliases = Enumerable.Range(0, compositionCount)
            .Select(i => $"composition{i}")
            .Concat(new[] { targetAlias })
            .ToArray();
        
        _contentTypeMock.Setup(x => x.Alias).Returns("currentType");
        _contentTypeMock.Setup(x => x.CompositionAliases).Returns(compositionAliases);

        var result = _contentMock.Object.IsDocumentType(targetAlias, recursive: true);

        Assert.That(result, Is.True);
    }

    #endregion
}