using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions.PublishedContent;

[TestFixture]
public class PublishedContentNavigationExtensionsTests
{
    private Mock<IPublishedContent> _contentMock;
    private Mock<INavigationQueryService> _navigationQueryServiceMock;
    private Mock<IPublishedStatusFilteringService> _publishedStatusFilteringServiceMock;
    private List<IPublishedContent> _testContentList;

    [SetUp]
    public void Setup()
    {
        _contentMock = new Mock<IPublishedContent>();
        _navigationQueryServiceMock = new Mock<INavigationQueryService>();
        _publishedStatusFilteringServiceMock = new Mock<IPublishedStatusFilteringService>();
        _testContentList = new List<IPublishedContent>();

        // Setup default content properties
        _contentMock.Setup(x => x.Id).Returns(1);
        _contentMock.Setup(x => x.Key).Returns(Guid.NewGuid());
        _contentMock.Setup(x => x.Path).Returns("-1,1");
        _contentMock.Setup(x => x.Level).Returns(1);
    }

    #region Parent Tests

    [Test]
    public void Parent_ThrowsArgumentNullException_WhenContentIsNull()
    {
        IPublishedContent content = null!;

        Assert.Throws<ArgumentNullException>(() =>
            content.Parent<IPublishedContent>(_navigationQueryServiceMock.Object, _publishedStatusFilteringServiceMock.Object));
    }

    [Test]
    public void Parent_ReturnsParent_WhenParentExists()
    {
        var parentMock = new Mock<IPublishedContent>();
        parentMock.Setup(x => x.Id).Returns(0);
        
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

        var result = _contentMock.Object.Parent<IPublishedContent>(
            _navigationQueryServiceMock.Object, 
            _publishedStatusFilteringServiceMock.Object);

        Assert.That(result, Is.EqualTo(parentMock.Object));
    }

    [Test]
    public void Parent_ReturnsNull_WhenParentDoesNotExist()
    {
        _navigationQueryServiceMock
            .Setup(x => x.TryGetParentKey(_contentMock.Object.Key, out It.Ref<Guid?>.IsAny))
            .Returns((Guid key, out Guid? parentKey) =>
            {
                parentKey = null;
                return false;
            });

        var result = _contentMock.Object.Parent<IPublishedContent>(
            _navigationQueryServiceMock.Object, 
            _publishedStatusFilteringServiceMock.Object);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Parent_AppliesPredicate_WhenProvided()
    {
        var parentMock = new Mock<IPublishedContent>();
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

        var result = _contentMock.Object.Parent<IPublishedContent>(
            _navigationQueryServiceMock.Object,
            _publishedStatusFilteringServiceMock.Object,
            p => p.Name == "Parent");

        Assert.That(result, Is.EqualTo(parentMock.Object));

        var noMatchResult = _contentMock.Object.Parent<IPublishedContent>(
            _navigationQueryServiceMock.Object,
            _publishedStatusFilteringServiceMock.Object,
            p => p.Name == "NonExistent");

        Assert.That(noMatchResult, Is.Null);
    }

    #endregion

    #region Ancestors Tests

    [Test]
    public void Ancestors_ThrowsArgumentNullException_WhenContentIsNull()
    {
        IPublishedContent content = null!;

        Assert.Throws<ArgumentNullException>(() =>
            content.Ancestors(_navigationQueryServiceMock.Object, _publishedStatusFilteringServiceMock.Object));
    }

    [Test]
    public void Ancestors_ReturnsAncestors_InCorrectOrder()
    {
        var grandparent = CreateMockContent(1, "Grandparent", -1);
        var parent = CreateMockContent(2, "Parent", 1);
        var ancestors = new[] { parent, grandparent };

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

        var result = _contentMock.Object.Ancestors(
            _navigationQueryServiceMock.Object,
            _publishedStatusFilteringServiceMock.Object).ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0], Is.EqualTo(parent));
        Assert.That(result[1], Is.EqualTo(grandparent));
    }

    [Test]
    public void Ancestors_ExcludesRoot_WhenIncludeRootIsFalse()
    {
        var root = CreateMockContent(-1, "Root", null);
        var parent = CreateMockContent(2, "Parent", -1);
        var ancestors = new[] { parent, root };

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
            .Returns<IEnumerable<IPublishedContent>>(items => items.Where(i => i.Id != -1));

        var result = _contentMock.Object.Ancestors(
            _navigationQueryServiceMock.Object,
            _publishedStatusFilteringServiceMock.Object,
            includeRoot: false).ToList();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(parent));
    }

    #endregion

    #region AncestorsOrSelf Tests

    [Test]
    public void AncestorsOrSelf_IncludesSelf_InResult()
    {
        var parent = CreateMockContent(2, "Parent", -1);
        var ancestors = new[] { parent };

        _navigationQueryServiceMock
            .Setup(x => x.TryGetAncestorsKeys(_contentMock.Object.Key, out It.Ref<IEnumerable<Guid>>.IsAny))
            .Returns((Guid key, out IEnumerable<Guid> ancestorKeys) =>
            {
                ancestorKeys = ancestors.Select(a => a.Key);
                return true;
            });

        _navigationQueryServiceMock
            .Setup(x => x.TryGetPublishedContent(parent.Key, out It.Ref<IPublishedContent?>.IsAny))
            .Returns((Guid key, out IPublishedContent? content) =>
            {
                content = parent;
                return true;
            });

        _publishedStatusFilteringServiceMock
            .Setup(x => x.FilterContent(It.IsAny<IEnumerable<IPublishedContent>>()))
            .Returns<IEnumerable<IPublishedContent>>(items => items);

        var result = _contentMock.Object.AncestorsOrSelf(
            _navigationQueryServiceMock.Object,
            _publishedStatusFilteringServiceMock.Object).ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0], Is.EqualTo(_contentMock.Object));
        Assert.That(result[1], Is.EqualTo(parent));
    }

    [Test]
    public void AncestorsOrSelf_AppliesPredicate_WhenProvided()
    {
        _contentMock.Setup(x => x.Name).Returns("Current");
        var parent = CreateMockContent(2, "Parent", -1);
        var ancestors = new[] { parent };

        _navigationQueryServiceMock
            .Setup(x => x.TryGetAncestorsKeys(_contentMock.Object.Key, out It.Ref<IEnumerable<Guid>>.IsAny))
            .Returns((Guid key, out IEnumerable<Guid> ancestorKeys) =>
            {
                ancestorKeys = ancestors.Select(a => a.Key);
                return true;
            });

        _navigationQueryServiceMock
            .Setup(x => x.TryGetPublishedContent(parent.Key, out It.Ref<IPublishedContent?>.IsAny))
            .Returns((Guid key, out IPublishedContent? content) =>
            {
                content = parent;
                return true;
            });

        _publishedStatusFilteringServiceMock
            .Setup(x => x.FilterContent(It.IsAny<IEnumerable<IPublishedContent>>()))
            .Returns<IEnumerable<IPublishedContent>>(items => items);

        var result = _contentMock.Object.AncestorsOrSelf(
            _navigationQueryServiceMock.Object,
            _publishedStatusFilteringServiceMock.Object,
            predicate: c => c.Name.StartsWith("P")).ToList();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(parent));
    }

    #endregion

    #region Children Tests

    [Test]
    public void Children_ReturnsChildren_WhenTheyExist()
    {
        var child1 = CreateMockContent(10, "Child1", 1);
        var child2 = CreateMockContent(11, "Child2", 1);
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

        var result = _contentMock.Object.Children(
            _navigationQueryServiceMock.Object,
            _publishedStatusFilteringServiceMock.Object).ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Contains(child1), Is.True);
        Assert.That(result.Contains(child2), Is.True);
    }

    [Test]
    public void Children_AppliesPredicate_WhenProvided()
    {
        var child1 = CreateMockContent(10, "Apple", 1);
        var child2 = CreateMockContent(11, "Banana", 1);
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

        var result = _contentMock.Object.Children(
            _navigationQueryServiceMock.Object,
            _publishedStatusFilteringServiceMock.Object,
            c => c.Name.StartsWith("A")).ToList();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(child1));
    }

    #endregion

    #region Relationship Tests

    [Test]
    public void IsEqual_ReturnsTrue_WhenContentIsSame()
    {
        var otherContent = _contentMock.Object;
        
        var result = _contentMock.Object.IsEqual(otherContent);
        
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsEqual_ReturnsFalse_WhenContentIsDifferent()
    {
        var otherMock = new Mock<IPublishedContent>();
        otherMock.Setup(x => x.Id).Returns(2);
        
        var result = _contentMock.Object.IsEqual(otherMock.Object);
        
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsNotEqual_ReturnsTrue_WhenContentIsDifferent()
    {
        var otherMock = new Mock<IPublishedContent>();
        otherMock.Setup(x => x.Id).Returns(2);
        
        var result = _contentMock.Object.IsNotEqual(otherMock.Object);
        
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsDescendant_ReturnsTrue_WhenContentIsDescendant()
    {
        _contentMock.Setup(x => x.Path).Returns("-1,2,3,4");
        
        var ancestorMock = new Mock<IPublishedContent>();
        ancestorMock.Setup(x => x.Id).Returns(2);
        ancestorMock.Setup(x => x.Path).Returns("-1,2");
        
        var result = _contentMock.Object.IsDescendant(ancestorMock.Object);
        
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsDescendant_ReturnsFalse_WhenContentIsNotDescendant()
    {
        _contentMock.Setup(x => x.Path).Returns("-1,3,4");
        
        var otherMock = new Mock<IPublishedContent>();
        otherMock.Setup(x => x.Id).Returns(2);
        otherMock.Setup(x => x.Path).Returns("-1,2");
        
        var result = _contentMock.Object.IsDescendant(otherMock.Object);
        
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsDescendantOrSelf_ReturnsTrue_WhenContentIsSelf()
    {
        var result = _contentMock.Object.IsDescendantOrSelf(_contentMock.Object);
        
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsAncestor_ReturnsTrue_WhenContentIsAncestor()
    {
        _contentMock.Setup(x => x.Path).Returns("-1,2");
        
        var descendantMock = new Mock<IPublishedContent>();
        descendantMock.Setup(x => x.Id).Returns(4);
        descendantMock.Setup(x => x.Path).Returns("-1,2,3,4");
        
        var result = _contentMock.Object.IsAncestor(descendantMock.Object);
        
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsAncestorOrSelf_ReturnsTrue_WhenContentIsSelf()
    {
        var result = _contentMock.Object.IsAncestorOrSelf(_contentMock.Object);
        
        Assert.That(result, Is.True);
    }

    #endregion

    #region Siblings Tests

    [Test]
    public void Siblings_ReturnsSiblings_ExcludingSelf()
    {
        var sibling1 = CreateMockContent(2, "Sibling1", 0);
        var sibling2 = CreateMockContent(3, "Sibling2", 0);
        var allSiblings = new[] { _contentMock.Object, sibling1, sibling2 };

        // Setup parent
        var parentKey = Guid.NewGuid();
        _navigationQueryServiceMock
            .Setup(x => x.TryGetParentKey(_contentMock.Object.Key, out It.Ref<Guid?>.IsAny))
            .Returns((Guid key, out Guid? pKey) =>
            {
                pKey = parentKey;
                return true;
            });

        // Setup children of parent (siblings)
        _navigationQueryServiceMock
            .Setup(x => x.TryGetChildrenKeys(parentKey, out It.Ref<IEnumerable<Guid>>.IsAny))
            .Returns((Guid key, out IEnumerable<Guid> childKeys) =>
            {
                childKeys = allSiblings.Select(s => s.Key);
                return true;
            });

        foreach (var sibling in allSiblings)
        {
            var localSibling = sibling;
            _navigationQueryServiceMock
                .Setup(x => x.TryGetPublishedContent(localSibling.Key, out It.Ref<IPublishedContent?>.IsAny))
                .Returns((Guid key, out IPublishedContent? content) =>
                {
                    content = localSibling;
                    return true;
                });
        }

        _publishedStatusFilteringServiceMock
            .Setup(x => x.FilterContent(It.IsAny<IEnumerable<IPublishedContent>>()))
            .Returns<IEnumerable<IPublishedContent>>(items => items);

        var result = _contentMock.Object.Siblings(
            _navigationQueryServiceMock.Object,
            _publishedStatusFilteringServiceMock.Object).ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Contains(sibling1), Is.True);
        Assert.That(result.Contains(sibling2), Is.True);
        Assert.That(result.Contains(_contentMock.Object), Is.False);
    }

    #endregion

    #region Breadcrumbs Tests

    [Test]
    public void Breadcrumbs_ReturnsBreadcrumbTrail_InCorrectOrder()
    {
        var root = CreateMockContent(-1, "Root", null);
        var parent = CreateMockContent(2, "Parent", -1);
        var ancestors = new[] { parent, root };

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

        var result = _contentMock.Object.Breadcrumbs(
            _navigationQueryServiceMock.Object,
            _publishedStatusFilteringServiceMock.Object).ToList();

        // Breadcrumbs should be in reverse order (root to current)
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result[0], Is.EqualTo(root));
        Assert.That(result[1], Is.EqualTo(parent));
        Assert.That(result[2], Is.EqualTo(_contentMock.Object));
    }

    #endregion

    #region Helper Methods

    private IPublishedContent CreateMockContent(int id, string name, int? parentId)
    {
        var mock = new Mock<IPublishedContent>();
        mock.Setup(x => x.Id).Returns(id);
        mock.Setup(x => x.Name).Returns(name);
        mock.Setup(x => x.Key).Returns(Guid.NewGuid());
        
        var path = parentId.HasValue ? $"-1,{parentId},{id}" : $"-1,{id}";
        mock.Setup(x => x.Path).Returns(path);
        
        return mock.Object;
    }

    #endregion
}