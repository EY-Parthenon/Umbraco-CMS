// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for IPublishedContent URL operations.
/// </summary>
public static class PublishedContentUrlExtensions
{
    /// <summary>
    /// Gets the URL of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="publishedUrlProvider">The published URL provider.</param>
    /// <param name="culture">The specific culture to get the URL for. If null is used the current culture is used (Default is null).</param>
    /// <param name="mode">The URL mode.</param>
    /// <returns>The URL of the content item.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content or publishedUrlProvider is null.</exception>
    public static string Url(this IPublishedContent content, IPublishedUrlProvider publishedUrlProvider, string? culture = null, UrlMode mode = UrlMode.Default)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (publishedUrlProvider == null)
        {
            throw new ArgumentNullException(nameof(publishedUrlProvider));
        }

        switch (content)
        {
            case IPublishedContentWithKey withKey:
                return publishedUrlProvider.GetUrl(withKey.Key, mode, culture);
            default:
                return publishedUrlProvider.GetUrl(content.Id, mode, culture);
        }
    }

    /// <summary>
    /// Gets the URL segment of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    /// <param name="culture">The specific culture to get the URL segment for. If null is used the current culture is used (Default is null).</param>
    /// <returns>The URL segment of the content item.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    [Obsolete("Please use GetUrlSegment() on IDocumentUrlService instead. Scheduled for removal in V16.")]
    public static string? UrlSegment(this IPublishedContent content, IVariationContextAccessor? variationContextAccessor, string? culture = null)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        // invariant has invariant value (whatever the requested culture)
        if (!content.ContentType.VariesByCulture())
        {
            return content.Cultures.TryGetValue(string.Empty, out PublishedCultureInfo? invariantInfos)
                ? invariantInfos.UrlSegment
                : null;
        }

        // handle context culture for variant
        if (culture == null)
        {
            culture = variationContextAccessor?.VariationContext?.Culture ?? string.Empty;
        }

        // get
        return culture != string.Empty && content.Cultures.TryGetValue(culture, out PublishedCultureInfo? infos)
            ? infos.UrlSegment
            : null;
    }
}