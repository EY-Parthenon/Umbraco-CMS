// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for IPublishedContent culture and language operations.
/// </summary>
public static class PublishedContentCultureExtensions
{
    /// <summary>
    /// Determines whether the content has a specific culture.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="culture">The culture to check.</param>
    /// <returns>true if the content has the specified culture; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static bool HasCulture(this IPublishedContent content, string? culture)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        // if content does not vary by culture, then it has the invariant culture,
        // and only the invariant culture
        if (!content.ContentType.VariesByCulture())
        {
            return culture == null || culture == string.Empty;
        }

        // content varies by culture, return whether it has the specified culture
        return content.Cultures.ContainsKey(culture ?? string.Empty);
    }

    /// <summary>
    /// Determines whether the content is invariant or has a specific culture.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="culture">The culture to check.</param>
    /// <returns>true if the content is invariant or has the specified culture; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    /// <exception cref="ArgumentException">Thrown when culture is null or empty.</exception>
    public static bool IsInvariantOrHasCulture(this IPublishedContent content, string culture)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (culture.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("Culture cannot be null or empty.", nameof(culture));
        }

        return !content.ContentType.VariesByCulture() || content.Cultures.ContainsKey(culture);
    }

    /// <summary>
    /// Gets the culture date of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    /// <param name="culture">The specific culture to get the date for. If null is used the current culture is used (Default is null).</param>
    /// <returns>The culture date of the content item.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static DateTime CultureDate(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string? culture = null)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        // invariant has invariant value (whatever the requested culture)
        if (!content.ContentType.VariesByCulture())
        {
            return content.Cultures.TryGetValue(string.Empty, out PublishedCultureInfo? invariantInfos)
                ? invariantInfos.Date
                : DateTime.MinValue;
        }

        // handle context culture for variant
        if (culture == null)
        {
            culture = variationContextAccessor?.VariationContext?.Culture ?? string.Empty;
        }

        // get the culture date
        return culture != string.Empty && content.Cultures.TryGetValue(culture, out PublishedCultureInfo? infos)
            ? infos.Date
            : DateTime.MinValue;
    }

    /// <summary>
    /// Gets all available cultures for the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <returns>An enumerable of culture codes available for the content item.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static IEnumerable<string> GetAvailableCultures(this IPublishedContent content)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return content.Cultures.Keys;
    }

    /// <summary>
    /// Gets the culture info for a specific culture.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="culture">The culture to get info for.</param>
    /// <returns>The culture info if available; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static PublishedCultureInfo? GetCultureInfo(this IPublishedContent content, string? culture)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return content.Cultures.TryGetValue(culture ?? string.Empty, out PublishedCultureInfo? cultureInfo) 
            ? cultureInfo 
            : null;
    }

    /// <summary>
    /// Determines whether the content varies by culture.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <returns>true if the content varies by culture; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static bool VariesByCulture(this IPublishedContent content)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return content.ContentType.VariesByCulture();
    }
}