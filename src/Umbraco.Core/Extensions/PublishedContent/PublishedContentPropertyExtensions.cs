// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for IPublishedContent property access and value retrieval.
/// </summary>
public static class PublishedContentPropertyExtensions
{
    /// <summary>
    /// Gets the name of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    /// <param name="culture">The specific culture to get the name for. If null is used the current culture is used (Default is null).</param>
    /// <returns>The name of the content item.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static string Name(this IPublishedContent content, IVariationContextAccessor? variationContextAccessor, string? culture = null)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        // invariant has invariant value (whatever the requested culture)
        if (!content.ContentType.VariesByCulture())
        {
            return content.Cultures.TryGetValue(string.Empty, out PublishedCultureInfo? invariantInfos)
                ? invariantInfos.Name
                : string.Empty;
        }

        // handle context culture for variant
        if (culture == null)
        {
            culture = variationContextAccessor?.VariationContext?.Culture ?? string.Empty;
        }

        // get
        return culture != string.Empty && content.Cultures.TryGetValue(culture, out PublishedCultureInfo? infos)
            ? infos.Name
            : string.Empty;
    }

    /// <summary>
    /// Determines whether the content has a value for a property identified by its alias.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="segment">The segment.</param>
    /// <param name="fallback">The fallback options.</param>
    /// <returns>true if the content has a value for the property; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static bool HasValue(
        this IPublishedContent content,
        IPublishedValueFallback publishedValueFallback,
        string alias,
        string? culture = null,
        string? segment = null,
        Fallback fallback = default)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        IPublishedProperty? property = content.GetProperty(alias);

        // if we have a property, and it has a value, return that value
        if (property != null && property.HasValue(culture, segment))
        {
            return true;
        }

        // else let fallback try to get a value
        return publishedValueFallback.TryGetValue(content, alias, culture, segment, fallback, null, out _);
    }

    /// <summary>
    /// Gets the value of a content's property identified by its alias.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="segment">The segment.</param>
    /// <param name="fallback">The fallback options.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The value of the content's property identified by the alias.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static object? Value(
        this IPublishedContent content,
        IPublishedValueFallback publishedValueFallback,
        string alias,
        string? culture = null,
        string? segment = null,
        Fallback fallback = default,
        object? defaultValue = default)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        IPublishedProperty? property = content.GetProperty(alias);

        // if we have a property, and it has a value, return that value
        if (property != null && property.HasValue(culture, segment))
        {
            return property.GetValue(culture, segment);
        }

        // else let fallback try to get a value
        if (publishedValueFallback.TryGetValue(content, alias, culture, segment, fallback, defaultValue, out var value))
        {
            return value;
        }

        // else... if we have a property, at least let the converter return its own
        // vision of 'no value' (could be an empty enumerable) - otherwise, default
        return property?.GetValue(culture, segment);
    }

    /// <summary>
    /// Gets the value of a content's property identified by its alias, converted to a specified type.
    /// </summary>
    /// <typeparam name="T">The target property type.</typeparam>
    /// <param name="content">The content item.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="segment">The segment.</param>
    /// <param name="fallback">The fallback options.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The value of the content's property identified by the alias, converted to the specified type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static T? Value<T>(
        this IPublishedContent content,
        IPublishedValueFallback publishedValueFallback,
        string alias,
        string? culture = null,
        string? segment = null,
        Fallback fallback = default,
        T? defaultValue = default)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        IPublishedProperty? property = content.GetProperty(alias);

        // if we have a property, and it has a value, return that value
        if (property != null && property.HasValue(culture, segment))
        {
            return property.Value<T>(publishedValueFallback, culture, segment);
        }

        // else let fallback try to get a value
        if (publishedValueFallback.TryGetValue<T>(content, alias, culture, segment, fallback, defaultValue, out var value))
        {
            return value;
        }

        // else... if we have a property, at least let the converter return its own
        // vision of 'no value' (could be an empty enumerable) - otherwise, default
        return property != null ? property.Value<T>(publishedValueFallback, culture, segment) : default;
    }
}