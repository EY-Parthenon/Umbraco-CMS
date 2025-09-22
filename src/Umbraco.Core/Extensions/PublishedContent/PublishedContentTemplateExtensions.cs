// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for IPublishedContent template operations.
/// </summary>
public static class PublishedContentTemplateExtensions
{
    /// <summary>
    /// Gets the template alias of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="fileService">The file service.</param>
    /// <returns>The template alias, or an empty string if the template is not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content or fileService is null.</exception>
    public static string GetTemplateAlias(this IPublishedContent content, IFileService fileService)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (fileService == null)
        {
            throw new ArgumentNullException(nameof(fileService));
        }

        if (content.TemplateId.HasValue)
        {
            var template = fileService.GetTemplate(content.TemplateId.Value);
            return template?.Alias ?? string.Empty;
        }

        return string.Empty;
    }

    /// <summary>
    /// Determines whether a template is allowed for the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="webRoutingSettings">The web routing settings.</param>
    /// <param name="templateId">The template ID to check.</param>
    /// <returns>true if the template is allowed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content or contentTypeService is null.</exception>
    public static bool IsAllowedTemplate(
        this IPublishedContent content,
        IContentTypeService contentTypeService,
        WebRoutingSettings webRoutingSettings,
        int templateId)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (contentTypeService == null)
        {
            throw new ArgumentNullException(nameof(contentTypeService));
        }

        return content.IsAllowedTemplate(
            contentTypeService,
            webRoutingSettings.DisableAlternativeTemplates,
            webRoutingSettings.ValidateAlternativeTemplates,
            templateId);
    }

    /// <summary>
    /// Determines whether a template is allowed for the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="disableAlternativeTemplates">Whether alternative templates are disabled.</param>
    /// <param name="validateAlternativeTemplates">Whether to validate alternative templates.</param>
    /// <param name="templateId">The template ID to check.</param>
    /// <returns>true if the template is allowed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content or contentTypeService is null.</exception>
    public static bool IsAllowedTemplate(
        this IPublishedContent content,
        IContentTypeService contentTypeService,
        bool disableAlternativeTemplates,
        bool validateAlternativeTemplates,
        int templateId)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (contentTypeService == null)
        {
            throw new ArgumentNullException(nameof(contentTypeService));
        }

        if (disableAlternativeTemplates)
        {
            return content.TemplateId == templateId;
        }

        if (!validateAlternativeTemplates)
        {
            return true;
        }

        var contentType = contentTypeService.Get(content.ContentType.Id);
        if (contentType == null)
        {
            return false;
        }

        return contentType.IsAllowedTemplate(templateId);
    }

    /// <summary>
    /// Determines whether a template is allowed for the content item by alias.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="fileService">The file service.</param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="disableAlternativeTemplates">Whether alternative templates are disabled.</param>
    /// <param name="validateAlternativeTemplates">Whether to validate alternative templates.</param>
    /// <param name="templateAlias">The template alias to check.</param>
    /// <returns>true if the template is allowed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content, fileService, or contentTypeService is null.</exception>
    public static bool IsAllowedTemplate(
        this IPublishedContent content,
        IFileService fileService,
        IContentTypeService contentTypeService,
        bool disableAlternativeTemplates,
        bool validateAlternativeTemplates,
        string templateAlias)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (fileService == null)
        {
            throw new ArgumentNullException(nameof(fileService));
        }

        if (contentTypeService == null)
        {
            throw new ArgumentNullException(nameof(contentTypeService));
        }

        var template = fileService.GetTemplate(templateAlias);
        return template != null && content.IsAllowedTemplate(
            contentTypeService,
            disableAlternativeTemplates,
            validateAlternativeTemplates,
            template.Id);
    }

    /// <summary>
    /// Gets the template ID of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <returns>The template ID if set; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static int? GetTemplateId(this IPublishedContent content)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return content.TemplateId;
    }

    /// <summary>
    /// Determines whether the content has a template.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <returns>true if the content has a template; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static bool HasTemplate(this IPublishedContent content)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return content.TemplateId.HasValue;
    }
}