// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Extensions;

/// <summary>
/// Main extension methods for IPublishedContent.
/// This class acts as a facade that delegates to specialized extension classes for better maintainability.
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
    #region Name - Delegates to PublishedContentPropertyExtensions

    /// <summary>
    ///     Gets the name of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="culture">
    ///     The specific culture to get the name for. If null is used the current culture is used (Default is
    ///     null).
    /// </param>
    public static string Name(this IPublishedContent content, IVariationContextAccessor? variationContextAccessor, string? culture = null)
        => PublishedContentPropertyExtensions.Name(content, variationContextAccessor, culture);

    #endregion

    #region Url segment - Delegates to PublishedContentUrlExtensions

    /// <summary>
    ///     Gets the URL segment of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="culture">
    ///     The specific culture to get the URL segment for. If null is used the current culture is used
    ///     (Default is null).
    /// </param>
    [Obsolete("Please use GetUrlSegment() on IDocumentUrlService instead. Scheduled for removal in V16.")]
    public static string? UrlSegment(this IPublishedContent content, IVariationContextAccessor? variationContextAccessor, string? culture = null)
        => PublishedContentUrlExtensions.UrlSegment(content, variationContextAccessor, culture);

    #endregion

    #region IsComposedOf - Delegates to PublishedContentTypeExtensions

    /// <summary>
    ///     Gets a value indicating whether the content is of a content type composed of the given alias
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="alias">The content type alias.</param>
    /// <returns>
    ///     A value indicating whether the content is of a content type composed of a content type identified by the
    ///     alias.
    /// </returns>
    public static bool IsComposedOf(this IPublishedContent content, string alias) =>
        PublishedContentTypeExtensions.IsComposedOf(content, alias);

    #endregion

    #region Axes: parent - Delegates to PublishedContentNavigationExtensions

    // Parent is native

    /// <summary>
    ///     Gets the parent of the content, of a given content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <returns>The parent of content, of the given content type, else null.</returns>
    public static T? Parent<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        where T : class, IPublishedContent
        => PublishedContentNavigationExtensions.Parent<T>(content, navigationQueryService, publishedStatusFilteringService);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static T? Parent<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        where T : class, IPublishedContent
        => content.Parent<T>(navigationQueryService, GetPublishedStatusFilteringService(content));

    #endregion

    #region Url - Delegates to PublishedContentUrlExtensions

    /// <summary>
    ///     Gets the url of the content item.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If the content item is a document, then this method returns the url of the
    ///         document. If it is a media, then this methods return the media url for the
    ///         'umbracoFile' property. Use the MediaUrl() method to get the media url for other
    ///         properties.
    ///     </para>
    ///     <para>
    ///         The value of this property is contextual. It depends on the 'current' request uri,
    ///         if any. In addition, when the content type is multi-lingual, this is the url for the
    ///         specified culture. Otherwise, it is the invariant url.
    ///     </para>
    /// </remarks>
    public static string Url(this IPublishedContent content, IPublishedUrlProvider publishedUrlProvider, string? culture = null, UrlMode mode = UrlMode.Default)
        => PublishedContentUrlExtensions.Url(content, publishedUrlProvider, culture, mode);

    #endregion

    #region Cultures - Delegates to PublishedContentCultureExtensions

    /// <summary>
    ///     Determines whether the content has a culture.
    /// </summary>
    /// <remarks>
    ///     <para>Culture is case-insensitive.</para>
    /// </remarks>
    public static bool HasCulture(this IPublishedContent content, string? culture)
        => PublishedContentCultureExtensions.HasCulture(content, culture);

    /// <summary>
    ///     Determines whether the content is invariant, or has a culture.
    /// </summary>
    /// <remarks>
    ///     <para>Culture is case-insensitive.</para>
    /// </remarks>
    public static bool IsInvariantOrHasCulture(this IPublishedContent content, string culture)
        => PublishedContentCultureExtensions.IsInvariantOrHasCulture(content, culture);

    /// <summary>
    ///     Gets the date of the content item for the specified culture.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="variationContextAccessor"></param>
    /// <param name="culture">
    ///     The specific culture to get the name for. If null is used the current culture is used (Default is
    ///     null).
    /// </param>
    public static DateTime CultureDate(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string? culture = null)
        => PublishedContentCultureExtensions.CultureDate(content, variationContextAccessor, culture);

    #endregion

    #region IsAllowedTemplate - Delegates to PublishedContentTemplateExtensions

    /// <summary>
    ///     Gets the template alias of the content item.
    /// </summary>
    public static string GetTemplateAlias(this IPublishedContent content, IFileService fileService)
        => PublishedContentTemplateExtensions.GetTemplateAlias(content, fileService);

    /// <summary>
    ///     Determines if the content item has a template that is an allowed template for the document type.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="contentTypeService"></param>
    /// <param name="webRoutingSettings"></param>
    /// <param name="templateId"></param>
    /// <returns></returns>
    public static bool IsAllowedTemplate(this IPublishedContent content, IContentTypeService contentTypeService, WebRoutingSettings webRoutingSettings, int templateId) =>
        PublishedContentTemplateExtensions.IsAllowedTemplate(content, contentTypeService, webRoutingSettings, templateId);

    /// <summary>
    ///     Determines if the content item has a template that is an allowed template for the document type.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="contentTypeService"></param>
    /// <param name="disableAlternativeTemplates"></param>
    /// <param name="validateAlternativeTemplates"></param>
    /// <param name="templateId"></param>
    /// <returns></returns>
    public static bool IsAllowedTemplate(this IPublishedContent content, IContentTypeService contentTypeService, bool disableAlternativeTemplates, bool validateAlternativeTemplates, int templateId)
        => PublishedContentTemplateExtensions.IsAllowedTemplate(content, contentTypeService, disableAlternativeTemplates, validateAlternativeTemplates, templateId);

    /// <summary>
    ///     Determines if the content item has a template that is an allowed template for the document type.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="fileService"></param>
    /// <param name="contentTypeService"></param>
    /// <param name="disableAlternativeTemplates"></param>
    /// <param name="validateAlternativeTemplates"></param>
    /// <param name="templateAlias"></param>
    /// <returns></returns>
    public static bool IsAllowedTemplate(this IPublishedContent content, IFileService fileService, IContentTypeService contentTypeService, bool disableAlternativeTemplates, bool validateAlternativeTemplates, string templateAlias)
        => PublishedContentTemplateExtensions.IsAllowedTemplate(content, fileService, contentTypeService, disableAlternativeTemplates, validateAlternativeTemplates, templateAlias);

    #endregion

    #region HasValue - Delegates to PublishedContentPropertyExtensions

    /// <summary>
    ///     Gets a value indicating whether the content has a value for a property identified by its alias.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedValueFallback"></param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The variation language.</param>
    /// <param name="segment">The variation segment.</param>
    /// <param name="fallback">Optional fallback strategy.</param>
    /// <returns>A value indicating whether the content has a value for the property identified by the alias.</returns>
    /// <remarks>
    ///     Returns true if HasValue is true, or a fallback strategy can provide a value.
    /// </remarks>
    public static bool HasValue(this IPublishedContent content, IPublishedValueFallback publishedValueFallback, string alias, string? culture = null, string? segment = null, Fallback fallback = default)
        => PublishedContentPropertyExtensions.HasValue(content, publishedValueFallback, alias, culture, segment, fallback);

    #endregion

    #region Value - Delegates to PublishedContentPropertyExtensions

    /// <summary>
    ///     Gets the value of a content's property identified by its alias.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedValueFallback"></param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The variation language.</param>
    /// <param name="segment">The variation segment.</param>
    /// <param name="fallback">Optional fallback strategy.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The value of the content's property identified by the alias, if it exists, otherwise a default value.</returns>
    /// <remarks>
    ///     <para>
    ///         The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering
    ///         content.
    ///     </para>
    ///     <para>
    ///         If no property with the specified alias exists, or if the property has no value, returns
    ///         <paramref name="defaultValue" />.
    ///     </para>
    ///     <para>
    ///         If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the
    ///         converter.
    ///     </para>
    ///     <para>The alias is case-insensitive.</para>
    /// </remarks>
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
    ///     Gets the value of a content's property identified by its alias, converted to a specified type.
    /// </summary>
    /// <typeparam name="T">The target property type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="publishedValueFallback"></param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The variation language.</param>
    /// <param name="segment">The variation segment.</param>
    /// <param name="fallback">Optional fallback strategy.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The value of the content's property identified by the alias, converted to the specified type.</returns>
    /// <remarks>
    ///     <para>
    ///         The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering
    ///         content.
    ///     </para>
    ///     <para>
    ///         If no property with the specified alias exists, or if the property has no value, or if it could not be
    ///         converted, returns <c>default(T)</c>.
    ///     </para>
    ///     <para>
    ///         If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the
    ///         converter.
    ///     </para>
    ///     <para>The alias is case-insensitive.</para>
    /// </remarks>
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

    #region IsDocumentType - Delegates to PublishedContentTypeExtensions

    /// <summary>
    ///     Returns true if the content is of a content type that is of a specified content type
    /// </summary>
    /// <param name="content"></param>
    /// <param name="docTypeAlias"></param>
    /// <returns></returns>
    public static bool IsDocumentType(this IPublishedContent content, string docTypeAlias) =>
        PublishedContentTypeExtensions.IsDocumentType(content, docTypeAlias);

    /// <summary>
    ///     Returns true if the content is of a content type that is of a specified alias or it's derived types
    /// </summary>
    /// <param name="content"></param>
    /// <param name="docTypeAlias"></param>
    /// <param name="recursive"></param>
    /// <returns></returns>
    public static bool IsDocumentType(this IPublishedContent content, string docTypeAlias, bool recursive)
        => PublishedContentTypeExtensions.IsDocumentType(content, docTypeAlias, recursive);

    #endregion

    #region Axes: equality - Delegates to PublishedContentNavigationExtensions

    public static bool IsEqual(this IPublishedContent content, IPublishedContent other) 
        => PublishedContentNavigationExtensions.IsEqual(content, other);

    public static bool IsNotEqual(this IPublishedContent content, IPublishedContent other) =>
        PublishedContentNavigationExtensions.IsNotEqual(content, other);

    public static bool IsDescendant(this IPublishedContent content, IPublishedContent other) =>
        PublishedContentNavigationExtensions.IsDescendant(content, other);

    public static bool IsDescendantOrSelf(this IPublishedContent content, IPublishedContent other) =>
        PublishedContentNavigationExtensions.IsDescendantOrSelf(content, other);

    public static bool IsAncestor(this IPublishedContent content, IPublishedContent other) =>
        PublishedContentNavigationExtensions.IsAncestor(content, other);

    public static bool IsAncestorOrSelf(this IPublishedContent content, IPublishedContent other) =>
        PublishedContentNavigationExtensions.IsAncestorOrSelf(content, other);

    #endregion

    #region Axes: ancestors - Delegates to PublishedContentNavigationExtensions

    /// <summary>
    ///     Returns all ancestors of the current page.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <returns>All ancestors of the current page.</returns>
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        => PublishedContentNavigationExtensions.Ancestors(content, navigationQueryService, publishedStatusFilteringService);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        => content.Ancestors(navigationQueryService, GetPublishedStatusFilteringService(content));

    /// <summary>
    ///     Returns all ancestors of the current page, at a level lesser or equal to a specified level.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>All ancestors of the current page, at a level lesser or equal to the specified level.</returns>
    /// <remarks>
    ///     Does not consider the level of the current page. So it may return ancestors at levels greater than the
    ///     current page's level.
    /// </remarks>
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int maxLevel)
        => PublishedContentNavigationExtensions.Ancestors(content, navigationQueryService, publishedStatusFilteringService, maxLevel);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel)
        => content.Ancestors(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    /// <summary>
    ///     Returns all ancestors of the current page, of a specified content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <returns>All ancestors of the current page, of the specified content type.</returns>
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string contentTypeAlias)
        => PublishedContentNavigationExtensions.Ancestors(content, navigationQueryService, publishedStatusFilteringService, contentTypeAlias);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias)
        => content.Ancestors(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias);

    /// <summary>
    ///     Returns all ancestors of the current page, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <returns>All ancestors of the current page, of the specified content type.</returns>
    public static IEnumerable<T> Ancestors<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        where T : class, IPublishedContent
        => PublishedContentNavigationExtensions.Ancestors<T>(content, navigationQueryService, publishedStatusFilteringService);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> Ancestors<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        where T : class, IPublishedContent
        => content.Ancestors<T>(navigationQueryService, GetPublishedStatusFilteringService(content));

    #endregion

    #region Axes: ancestors-or-self - Delegates to PublishedContentNavigationExtensions

    /// <summary>
    ///     Returns all ancestors of the current page including the current page itself.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <returns>All ancestors of the current page including the current page itself.</returns>
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        => PublishedContentNavigationExtensions.AncestorsOrSelf(content, navigationQueryService, publishedStatusFilteringService);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        => content.AncestorsOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content));

    /// <summary>
    ///     Returns all ancestors of the current page including the current page itself, at a level lesser or equal to a
    ///     specified level.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="maxLevel">The level.</param>
    /// <returns>
    ///     All ancestors of the current page including the current page itself, at a level lesser or equal to the
    ///     specified level.
    /// </returns>
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        int maxLevel)
        => PublishedContentNavigationExtensions.AncestorsOrSelf(content, navigationQueryService, publishedStatusFilteringService, maxLevel);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        int maxLevel)
        => content.AncestorsOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), maxLevel);

    /// <summary>
    ///     Returns all ancestors of the current page including the current page itself, of a specified content type.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <returns>All ancestors of the current page including the current page itself, of the specified content type.</returns>
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string contentTypeAlias)
        => PublishedContentNavigationExtensions.AncestorsOrSelf(content, navigationQueryService, publishedStatusFilteringService, contentTypeAlias);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService,
        string contentTypeAlias)
        => content.AncestorsOrSelf(navigationQueryService, GetPublishedStatusFilteringService(content), contentTypeAlias);

    /// <summary>
    ///     Returns all ancestors of the current page including the current page itself, of a specified content type.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="navigationQueryService">The query service for the in-memory navigation structure.</param>
    /// <param name="publishedStatusFilteringService"></param>
    /// <returns>All ancestors of the current page including the current page itself, of the specified content type.</returns>
    public static IEnumerable<T> AncestorsOrSelf<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        where T : class, IPublishedContent
        => PublishedContentNavigationExtensions.AncestorsOrSelf<T>(content, navigationQueryService, publishedStatusFilteringService);

    [Obsolete("Use the overload with IPublishedStatusFilteringService, scheduled for removal in v17")]
    public static IEnumerable<T> AncestorsOrSelf<T>(
        this IPublishedContent content,
        IPublishedCache publishedCache,
        INavigationQueryService navigationQueryService)
        where T : class, IPublishedContent
        => content.AncestorsOrSelf<T>(navigationQueryService, GetPublishedStatusFilteringService(content));

    #endregion

    #region Helper method for obsolete overloads

    private static IPublishedStatusFilteringService GetPublishedStatusFilteringService(IPublishedContent content) =>
        StaticServiceProvider.Instance.GetRequiredService<IPublishedStatusFilteringService>();

    #endregion
}