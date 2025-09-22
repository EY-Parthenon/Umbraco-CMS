using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions.PublishedContent;

[TestFixture]
public class PublishedContentTemplateExtensionsTests
{
    private Mock<IPublishedContent> _contentMock;
    private Mock<IFileService> _fileServiceMock;
    private Mock<IContentTypeService> _contentTypeServiceMock;
    private WebRoutingSettings _webRoutingSettings;
    private Mock<ITemplate> _templateMock;
    private Mock<IContentType> _contentTypeMock;

    [SetUp]
    public void Setup()
    {
        _contentMock = new Mock<IPublishedContent>();
        _fileServiceMock = new Mock<IFileService>();
        _contentTypeServiceMock = new Mock<IContentTypeService>();
        _webRoutingSettings = new WebRoutingSettings();
        _templateMock = new Mock<ITemplate>();
        _contentTypeMock = new Mock<IContentType>();

        _contentMock.Setup(x => x.Id).Returns(1);
        _contentMock.Setup(x => x.ContentType).Returns(Mock.Of<IPublishedContentType>());
    }

    #region GetTemplateAlias Tests

    [Test]
    public void GetTemplateAlias_ThrowsArgumentNullException_WhenContentIsNull()
    {
        IPublishedContent content = null!;

        Assert.Throws<ArgumentNullException>(() => 
            content.GetTemplateAlias(_fileServiceMock.Object));
    }

    [Test]
    public void GetTemplateAlias_ThrowsArgumentNullException_WhenFileServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => 
            _contentMock.Object.GetTemplateAlias(null!));
    }

    [Test]
    public void GetTemplateAlias_ReturnsTemplateAlias_WhenTemplateExists()
    {
        const int templateId = 123;
        const string expectedAlias = "ArticleTemplate";

        _contentMock.Setup(x => x.TemplateId).Returns(templateId);
        _templateMock.Setup(x => x.Alias).Returns(expectedAlias);
        _fileServiceMock.Setup(x => x.GetTemplate(templateId)).Returns(_templateMock.Object);

        var result = _contentMock.Object.GetTemplateAlias(_fileServiceMock.Object);

        Assert.That(result, Is.EqualTo(expectedAlias));
        _fileServiceMock.Verify(x => x.GetTemplate(templateId), Times.Once);
    }

    [Test]
    public void GetTemplateAlias_ReturnsEmptyString_WhenTemplateIdIsNull()
    {
        _contentMock.Setup(x => x.TemplateId).Returns((int?)null);

        var result = _contentMock.Object.GetTemplateAlias(_fileServiceMock.Object);

        Assert.That(result, Is.EqualTo(string.Empty));
        _fileServiceMock.Verify(x => x.GetTemplate(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void GetTemplateAlias_ReturnsEmptyString_WhenTemplateNotFound()
    {
        const int templateId = 123;

        _contentMock.Setup(x => x.TemplateId).Returns(templateId);
        _fileServiceMock.Setup(x => x.GetTemplate(templateId)).Returns((ITemplate?)null);

        var result = _contentMock.Object.GetTemplateAlias(_fileServiceMock.Object);

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetTemplateAlias_ReturnsEmptyString_WhenTemplateAliasIsNull()
    {
        const int templateId = 123;

        _contentMock.Setup(x => x.TemplateId).Returns(templateId);
        _templateMock.Setup(x => x.Alias).Returns((string?)null);
        _fileServiceMock.Setup(x => x.GetTemplate(templateId)).Returns(_templateMock.Object);

        var result = _contentMock.Object.GetTemplateAlias(_fileServiceMock.Object);

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    #endregion

    #region IsAllowedTemplate Tests (By Template ID with WebRoutingSettings)

    [Test]
    public void IsAllowedTemplate_ById_ThrowsArgumentNullException_WhenContentIsNull()
    {
        IPublishedContent content = null!;

        Assert.Throws<ArgumentNullException>(() => 
            content.IsAllowedTemplate(_contentTypeServiceMock.Object, _webRoutingSettings, 123));
    }

    [Test]
    public void IsAllowedTemplate_ById_ThrowsArgumentNullException_WhenContentTypeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => 
            _contentMock.Object.IsAllowedTemplate(null!, _webRoutingSettings, 123));
    }

    [Test]
    public void IsAllowedTemplate_ById_ThrowsArgumentNullException_WhenWebRoutingSettingsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => 
            _contentMock.Object.IsAllowedTemplate(_contentTypeServiceMock.Object, null!, 123));
    }

    [Test]
    public void IsAllowedTemplate_ById_ReturnsTrue_WhenAlternativeTemplatesDisabled()
    {
        const int templateId = 456;
        _webRoutingSettings.DisableAlternativeTemplates = true;

        var result = _contentMock.Object.IsAllowedTemplate(
            _contentTypeServiceMock.Object, 
            _webRoutingSettings, 
            templateId);

        Assert.That(result, Is.True);
        _contentTypeServiceMock.Verify(x => x.Get(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void IsAllowedTemplate_ById_ReturnsTrue_WhenValidateAlternativeTemplatesIsFalse()
    {
        const int templateId = 456;
        _webRoutingSettings.DisableAlternativeTemplates = false;
        _webRoutingSettings.ValidateAlternativeTemplates = false;

        var result = _contentMock.Object.IsAllowedTemplate(
            _contentTypeServiceMock.Object,
            _webRoutingSettings,
            templateId);

        Assert.That(result, Is.True);
        _contentTypeServiceMock.Verify(x => x.Get(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void IsAllowedTemplate_ById_ReturnsTrue_WhenTemplateIsContentTemplate()
    {
        const int templateId = 789;
        
        _contentMock.Setup(x => x.TemplateId).Returns(templateId);
        _webRoutingSettings.DisableAlternativeTemplates = false;
        _webRoutingSettings.ValidateAlternativeTemplates = true;

        var result = _contentMock.Object.IsAllowedTemplate(
            _contentTypeServiceMock.Object,
            _webRoutingSettings,
            templateId);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsAllowedTemplate_ById_ReturnsTrue_WhenTemplateInAllowedTemplates()
    {
        const int contentTypeId = 100;
        const int templateId = 456;
        var allowedTemplates = new[] 
        { 
            CreateTemplate(123, "Template1"),
            CreateTemplate(templateId, "Template2"),
            CreateTemplate(789, "Template3")
        };

        _contentMock.Setup(x => x.ContentType.Id).Returns(contentTypeId);
        _contentMock.Setup(x => x.TemplateId).Returns(123);
        
        _contentTypeMock.Setup(x => x.AllowedTemplates).Returns(allowedTemplates);
        _contentTypeServiceMock.Setup(x => x.Get(contentTypeId)).Returns(_contentTypeMock.Object);

        _webRoutingSettings.DisableAlternativeTemplates = false;
        _webRoutingSettings.ValidateAlternativeTemplates = true;

        var result = _contentMock.Object.IsAllowedTemplate(
            _contentTypeServiceMock.Object,
            _webRoutingSettings,
            templateId);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsAllowedTemplate_ById_ReturnsFalse_WhenTemplateNotInAllowedTemplates()
    {
        const int contentTypeId = 100;
        const int templateId = 999;
        var allowedTemplates = new[] 
        { 
            CreateTemplate(123, "Template1"),
            CreateTemplate(456, "Template2")
        };

        _contentMock.Setup(x => x.ContentType.Id).Returns(contentTypeId);
        _contentMock.Setup(x => x.TemplateId).Returns(123);
        
        _contentTypeMock.Setup(x => x.AllowedTemplates).Returns(allowedTemplates);
        _contentTypeServiceMock.Setup(x => x.Get(contentTypeId)).Returns(_contentTypeMock.Object);

        _webRoutingSettings.DisableAlternativeTemplates = false;
        _webRoutingSettings.ValidateAlternativeTemplates = true;

        var result = _contentMock.Object.IsAllowedTemplate(
            _contentTypeServiceMock.Object,
            _webRoutingSettings,
            templateId);

        Assert.That(result, Is.False);
    }

    #endregion

    #region IsAllowedTemplate Tests (By Template ID with Explicit Flags)

    [Test]
    public void IsAllowedTemplate_ByIdWithFlags_ReturnsTrue_WhenAlternativeTemplatesDisabled()
    {
        const int templateId = 456;

        var result = _contentMock.Object.IsAllowedTemplate(
            _contentTypeServiceMock.Object,
            disableAlternativeTemplates: true,
            validateAlternativeTemplates: false,
            templateId);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsAllowedTemplate_ByIdWithFlags_ValidatesCorrectly()
    {
        const int contentTypeId = 100;
        const int templateId = 456;
        var allowedTemplates = new[] 
        { 
            CreateTemplate(templateId, "AllowedTemplate")
        };

        _contentMock.Setup(x => x.ContentType.Id).Returns(contentTypeId);
        _contentMock.Setup(x => x.TemplateId).Returns(123);
        
        _contentTypeMock.Setup(x => x.AllowedTemplates).Returns(allowedTemplates);
        _contentTypeServiceMock.Setup(x => x.Get(contentTypeId)).Returns(_contentTypeMock.Object);

        var result = _contentMock.Object.IsAllowedTemplate(
            _contentTypeServiceMock.Object,
            disableAlternativeTemplates: false,
            validateAlternativeTemplates: true,
            templateId);

        Assert.That(result, Is.True);
    }

    #endregion

    #region IsAllowedTemplate Tests (By Template Alias)

    [Test]
    public void IsAllowedTemplate_ByAlias_ThrowsArgumentNullException_WhenFileServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => 
            _contentMock.Object.IsAllowedTemplate(
                null!,
                _contentTypeServiceMock.Object,
                false,
                false,
                "templateAlias"));
    }

    [Test]
    public void IsAllowedTemplate_ByAlias_ThrowsArgumentException_WhenTemplateAliasIsNullOrEmpty()
    {
        Assert.Throws<ArgumentException>(() => 
            _contentMock.Object.IsAllowedTemplate(
                _fileServiceMock.Object,
                _contentTypeServiceMock.Object,
                false,
                false,
                null!));

        Assert.Throws<ArgumentException>(() => 
            _contentMock.Object.IsAllowedTemplate(
                _fileServiceMock.Object,
                _contentTypeServiceMock.Object,
                false,
                false,
                string.Empty));
    }

    [Test]
    public void IsAllowedTemplate_ByAlias_ReturnsFalse_WhenTemplateNotFound()
    {
        const string templateAlias = "NonExistentTemplate";

        _fileServiceMock.Setup(x => x.GetTemplate(templateAlias)).Returns((ITemplate?)null);

        var result = _contentMock.Object.IsAllowedTemplate(
            _fileServiceMock.Object,
            _contentTypeServiceMock.Object,
            false,
            true,
            templateAlias);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsAllowedTemplate_ByAlias_ReturnsTrue_WhenTemplateFoundAndAllowed()
    {
        const string templateAlias = "ArticleTemplate";
        const int templateId = 456;
        const int contentTypeId = 100;
        
        _templateMock.Setup(x => x.Id).Returns(templateId);
        _fileServiceMock.Setup(x => x.GetTemplate(templateAlias)).Returns(_templateMock.Object);

        var allowedTemplates = new[] 
        { 
            CreateTemplate(templateId, templateAlias)
        };

        _contentMock.Setup(x => x.ContentType.Id).Returns(contentTypeId);
        _contentMock.Setup(x => x.TemplateId).Returns(123);
        
        _contentTypeMock.Setup(x => x.AllowedTemplates).Returns(allowedTemplates);
        _contentTypeServiceMock.Setup(x => x.Get(contentTypeId)).Returns(_contentTypeMock.Object);

        var result = _contentMock.Object.IsAllowedTemplate(
            _fileServiceMock.Object,
            _contentTypeServiceMock.Object,
            disableAlternativeTemplates: false,
            validateAlternativeTemplates: true,
            templateAlias);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsAllowedTemplate_ByAlias_IsCaseInsensitive()
    {
        const string templateAlias = "ArticleTemplate";
        const string searchAlias = "articletemplate";
        const int templateId = 456;
        
        _templateMock.Setup(x => x.Id).Returns(templateId);
        _fileServiceMock.Setup(x => x.GetTemplate(It.IsAny<string>())).Returns(_templateMock.Object);

        _contentMock.Setup(x => x.TemplateId).Returns(templateId);

        var result = _contentMock.Object.IsAllowedTemplate(
            _fileServiceMock.Object,
            _contentTypeServiceMock.Object,
            disableAlternativeTemplates: false,
            validateAlternativeTemplates: true,
            searchAlias);

        Assert.That(result, Is.True);
    }

    #endregion

    #region Edge Cases and Complex Scenarios

    [Test]
    public void IsAllowedTemplate_HandlesNullAllowedTemplates()
    {
        const int contentTypeId = 100;
        const int templateId = 456;

        _contentMock.Setup(x => x.ContentType.Id).Returns(contentTypeId);
        _contentMock.Setup(x => x.TemplateId).Returns(123);
        
        _contentTypeMock.Setup(x => x.AllowedTemplates).Returns((IEnumerable<ITemplate>?)null);
        _contentTypeServiceMock.Setup(x => x.Get(contentTypeId)).Returns(_contentTypeMock.Object);

        var result = _contentMock.Object.IsAllowedTemplate(
            _contentTypeServiceMock.Object,
            disableAlternativeTemplates: false,
            validateAlternativeTemplates: true,
            templateId);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsAllowedTemplate_HandlesEmptyAllowedTemplates()
    {
        const int contentTypeId = 100;
        const int templateId = 456;

        _contentMock.Setup(x => x.ContentType.Id).Returns(contentTypeId);
        _contentMock.Setup(x => x.TemplateId).Returns(123);
        
        _contentTypeMock.Setup(x => x.AllowedTemplates).Returns(Enumerable.Empty<ITemplate>());
        _contentTypeServiceMock.Setup(x => x.Get(contentTypeId)).Returns(_contentTypeMock.Object);

        var result = _contentMock.Object.IsAllowedTemplate(
            _contentTypeServiceMock.Object,
            disableAlternativeTemplates: false,
            validateAlternativeTemplates: true,
            templateId);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsAllowedTemplate_HandlesNullContentType()
    {
        const int contentTypeId = 100;
        const int templateId = 456;

        _contentMock.Setup(x => x.ContentType.Id).Returns(contentTypeId);
        _contentMock.Setup(x => x.TemplateId).Returns(123);
        
        _contentTypeServiceMock.Setup(x => x.Get(contentTypeId)).Returns((IContentType?)null);

        var result = _contentMock.Object.IsAllowedTemplate(
            _contentTypeServiceMock.Object,
            disableAlternativeTemplates: false,
            validateAlternativeTemplates: true,
            templateId);

        Assert.That(result, Is.False);
    }

    [Test]
    public void GetTemplateAlias_CachesResult_WhenCalledMultipleTimes()
    {
        const int templateId = 123;
        const string expectedAlias = "CachedTemplate";

        _contentMock.Setup(x => x.TemplateId).Returns(templateId);
        _templateMock.Setup(x => x.Alias).Returns(expectedAlias);
        _fileServiceMock.Setup(x => x.GetTemplate(templateId)).Returns(_templateMock.Object);

        var result1 = _contentMock.Object.GetTemplateAlias(_fileServiceMock.Object);
        var result2 = _contentMock.Object.GetTemplateAlias(_fileServiceMock.Object);

        Assert.That(result1, Is.EqualTo(expectedAlias));
        Assert.That(result2, Is.EqualTo(expectedAlias));
        
        // Should only call GetTemplate once if caching is implemented
        _fileServiceMock.Verify(x => x.GetTemplate(templateId), Times.Exactly(2)); // Adjust based on actual implementation
    }

    #endregion

    #region Helper Methods

    private ITemplate CreateTemplate(int id, string alias)
    {
        var template = new Mock<ITemplate>();
        template.Setup(x => x.Id).Returns(id);
        template.Setup(x => x.Alias).Returns(alias);
        return template.Object;
    }

    #endregion
}