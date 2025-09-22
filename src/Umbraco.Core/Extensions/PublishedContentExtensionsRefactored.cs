// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Extensions;

/// <summary>
/// Main extension methods for IPublishedContent.
/// This class acts as a facade that delegates to specialized extension classes.
/// </summary>
/// <remarks>
/// This class maintains backward compatibility while delegating to specialized extension classes:
/// - PublishedContentPropertyExtensions: Property access and values
/// - PublishedContentUrlExtensions: URL generation
/// - PublishedContentNavigationExtensions: Navigation (ancestors, descendants, children, siblings)
/// - PublishedContentCultureExtensions: Culture and language operations
/// - PublishedContentTypeExtensions: Content type operations
/// - PublishedContentTemplateExtensions: Template operations
/// </remarks>
public static partial class PublishedContentExtensions
{
    #region Property Access - Delegates to PublishedContentPropertyExtensions

    /// <summary>
    /// Gets the name of the content item.
    /// </summary>
    public static string Name(this IPublishedContent content, IVariationContextAccessor? variationContextAccessor, string? culture = null)
        => PublishedContentPropertyExtensions.Name(content, variationContextAccessor, culture);

    /// <summary>
    /// Determines whether the content has a value for a property.
    /// </summary>
    public static bool HasValue(
        this IPublishedContent content,
        IPublishedValueFallback publishedValueFallback,
        string alias,
        string? culture = null,
        string? segment = null,
        Fallback fallback = default)
        => PublishedContentPropertyExtensions.HasValue(content, publishedValueFallback, alias, culture, segment, fallback);

    /// <summary>
    /// Gets the value of a content's property.
    /// </summary>
    public static object? Value(
        this IPublishedContent content,
        IPublishedValueFallback publishedValueFallback,
        string alias,
        string? culture = null,
        string? segment = null,
        Fallback fallback = default,
        object? defaultValue = default)
        => PublishedContentPropertyExtensions.Value(content, publishedValueFallback, alias, culture, segment, fallback, defaultValue);

    /// <summary>
    /// Gets the value of a content's property converted to a specified type.
    /// </summary>
    public static T? Value<T>(
        this IPublishedContent content,
        IPublishedValueFallback publishedValueFallback,
        string alias,
        string? culture = null,
        string? segment = null,
        Fallback fallback = default,
        T? defaultValue = default)
        => PublishedContentPropertyExtensions.Value<T>(content, publishedValueFallback, alias, culture, segment, fallback, defaultValue);

    #endregion

    #region URL Operations - Delegates to PublishedContentUrlExtensions

    /// <summary>
    /// Gets the URL of the content item.
    /// </summary>
    public static string Url(this IPublishedContent content, IPublishedUrlProvider publishedUrlProvider, string? culture = null, UrlMode mode = UrlMode.Default)
        => PublishedContentUrlExtensions.Url(content, publishedUrlProvider, culture, mode);

    /// <summary>
    /// Gets the URL segment of the content item.
    /// </summary>
    [Obsolete("Please use GetUrlSegment() on IDocumentUrlService instead. Scheduled for removal in V16.")]
    public static string? UrlSegment(this IPublishedContent content, IVariationContextAccessor? variationContextAccessor, string? culture = null)
        => PublishedContentUrlExtensions.UrlSegment(content, variationContextAccessor, culture);

    #endregion

    #region Navigation - Delegates to PublishedContentNavigationExtensions

    /// <summary>
    /// Gets the parent of the content item.
    /// </summary>
    public static T? Parent<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        Func<T, bool>? predicate = null)
        where T : class, IPublishedContent
        => PublishedContentNavigationExtensions.Parent<T>(content, navigationQueryService, publishedStatusFilteringService, predicate);

    /// <summary>
    /// Returns all ancestors of the current page.
    /// </summary>
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        bool includeRoot = true)
        => PublishedContentNavigationExtensions.Ancestors(content, navigationQueryService, publishedStatusFilteringService, includeRoot);

    /// <summary>
    /// Returns all ancestors of the current page including the current page itself.
    /// </summary>
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        bool includeRoot = true,
        Func<IPublishedContent, bool>? predicate = null)
        => PublishedContentNavigationExtensions.AncestorsOrSelf(content, navigationQueryService, publishedStatusFilteringService, includeRoot, predicate);

    /// <summary>
    /// Returns the children of the current page.
    /// </summary>
    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        Func<IPublishedContent, bool>? predicate = null)
        => PublishedContentNavigationExtensions.Children(content, navigationQueryService, publishedStatusFilteringService, predicate);

    /// <summary>
    /// Returns all descendants of the current page.
    /// </summary>
    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        => PublishedContentNavigationExtensions.Descendants(content, navigationQueryService, publishedStatusFilteringService);

    /// <summary>
    /// Returns all descendants of the current page including the current page itself.
    /// </summary>
    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        Func<IPublishedContent, bool>? predicate = null)
        => PublishedContentNavigationExtensions.DescendantsOrSelf(content, navigationQueryService, publishedStatusFilteringService, predicate);

    /// <summary>
    /// Returns the siblings of the current page.
    /// </summary>
    public static IEnumerable<IPublishedContent> Siblings(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        => PublishedContentNavigationExtensions.Siblings(content, navigationQueryService, publishedStatusFilteringService);

    /// <summary>
    /// Returns a breadcrumb trail for the current page.
    /// </summary>
    public static IEnumerable<IPublishedContent> Breadcrumbs(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        bool includeRoot = true)
        => PublishedContentNavigationExtensions.Breadcrumbs(content, navigationQueryService, publishedStatusFilteringService, includeRoot);

    #endregion

    #region Relationships - Delegates to PublishedContentNavigationExtensions

    /// <summary>
    /// Determines whether the content is equal to another content item.
    /// </summary>
    public static bool IsEqual(this IPublishedContent content, IPublishedContent other)
        => PublishedContentNavigationExtensions.IsEqual(content, other);

    /// <summary>
    /// Determines whether the content is not equal to another content item.
    /// </summary>
    public static bool IsNotEqual(this IPublishedContent content, IPublishedContent other)
        => PublishedContentNavigationExtensions.IsNotEqual(content, other);

    /// <summary>
    /// Determines whether the content is a descendant of another content item.
    /// </summary>
    public static bool IsDescendant(this IPublishedContent content, IPublishedContent other)
        => PublishedContentNavigationExtensions.IsDescendant(content, other);

    /// <summary>
    /// Determines whether the content is a descendant of or equal to another content item.
    /// </summary>
    public static bool IsDescendantOrSelf(this IPublishedContent content, IPublishedContent other)
        => PublishedContentNavigationExtensions.IsDescendantOrSelf(content, other);

    /// <summary>
    /// Determines whether the content is an ancestor of another content item.
    /// </summary>
    public static bool IsAncestor(this IPublishedContent content, IPublishedContent other)
        => PublishedContentNavigationExtensions.IsAncestor(content, other);

    /// <summary>
    /// Determines whether the content is an ancestor of or equal to another content item.
    /// </summary>
    public static bool IsAncestorOrSelf(this IPublishedContent content, IPublishedContent other)
        => PublishedContentNavigationExtensions.IsAncestorOrSelf(content, other);

    #endregion

    #region Culture - Delegates to PublishedContentCultureExtensions

    /// <summary>
    /// Determines whether the content has a specific culture.
    /// </summary>
    public static bool HasCulture(this IPublishedContent content, string? culture)
        => PublishedContentCultureExtensions.HasCulture(content, culture);

    /// <summary>
    /// Determines whether the content is invariant or has a specific culture.
    /// </summary>
    public static bool IsInvariantOrHasCulture(this IPublishedContent content, string culture)
        => PublishedContentCultureExtensions.IsInvariantOrHasCulture(content, culture);

    /// <summary>
    /// Gets the culture date of the content item.
    /// </summary>
    public static DateTime CultureDate(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string? culture = null)
        => PublishedContentCultureExtensions.CultureDate(content, variationContextAccessor, culture);

    #endregion

    #region Content Type - Delegates to PublishedContentTypeExtensions

    /// <summary>
    /// Determines whether the content type is composed of the specified alias.
    /// </summary>
    public static bool IsComposedOf(this IPublishedContent content, string alias)
        => PublishedContentTypeExtensions.IsComposedOf(content, alias);

    /// <summary>
    /// Determines whether the content is of a specific document type.
    /// </summary>
    public static bool IsDocumentType(this IPublishedContent content, string docTypeAlias)
        => PublishedContentTypeExtensions.IsDocumentType(content, docTypeAlias);

    /// <summary>
    /// Determines whether the content is of a specific document type, optionally checking recursively.
    /// </summary>
    public static bool IsDocumentType(this IPublishedContent content, string docTypeAlias, bool recursive)
        => PublishedContentTypeExtensions.IsDocumentType(content, docTypeAlias, recursive);

    #endregion

    #region Templates - Delegates to PublishedContentTemplateExtensions

    /// <summary>
    /// Gets the template alias of the content item.
    /// </summary>
    public static string GetTemplateAlias(this IPublishedContent content, IFileService fileService)
        => PublishedContentTemplateExtensions.GetTemplateAlias(content, fileService);

    /// <summary>
    /// Determines whether a template is allowed for the content item.
    /// </summary>
    public static bool IsAllowedTemplate(
        this IPublishedContent content,
        IContentTypeService contentTypeService,
        WebRoutingSettings webRoutingSettings,
        int templateId)
        => PublishedContentTemplateExtensions.IsAllowedTemplate(content, contentTypeService, webRoutingSettings, templateId);

    /// <summary>
    /// Determines whether a template is allowed for the content item.
    /// </summary>
    public static bool IsAllowedTemplate(
        this IPublishedContent content,
        IContentTypeService contentTypeService,
        bool disableAlternativeTemplates,
        bool validateAlternativeTemplates,
        int templateId)
        => PublishedContentTemplateExtensions.IsAllowedTemplate(content, contentTypeService, disableAlternativeTemplates, validateAlternativeTemplates, templateId);

    /// <summary>
    /// Determines whether a template is allowed for the content item by alias.
    /// </summary>
    public static bool IsAllowedTemplate(
        this IPublishedContent content,
        IFileService fileService,
        IContentTypeService contentTypeService,
        bool disableAlternativeTemplates,
        bool validateAlternativeTemplates,
        string templateAlias)
        => PublishedContentTemplateExtensions.IsAllowedTemplate(content, fileService, contentTypeService, disableAlternativeTemplates, validateAlternativeTemplates, templateAlias);

    #endregion

    // Note: Additional convenience overloads and obsolete methods would be added here for full backward compatibility
    // This is a simplified version showing the delegation pattern
}