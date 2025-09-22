// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for IPublishedContent type and composition operations.
/// </summary>
public static class PublishedContentTypeExtensions
{
    /// <summary>
    /// Determines whether the content type is composed of the specified alias.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="alias">The content type alias to check.</param>
    /// <returns>true if the content type is composed of the alias; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static bool IsComposedOf(this IPublishedContent content, string alias)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return content.ContentType.CompositionAliases.InvariantContains(alias);
    }

    /// <summary>
    /// Determines whether the content is of a specific document type.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="docTypeAlias">The document type alias to check.</param>
    /// <returns>true if the content is of the specified document type; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static bool IsDocumentType(this IPublishedContent content, string docTypeAlias)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return content.ContentType.Alias.InvariantEquals(docTypeAlias);
    }

    /// <summary>
    /// Determines whether the content is of a specific document type, optionally checking recursively.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="docTypeAlias">The document type alias to check.</param>
    /// <param name="recursive">If true, checks the content type and all its compositions.</param>
    /// <returns>true if the content is of the specified document type; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static bool IsDocumentType(this IPublishedContent content, string docTypeAlias, bool recursive)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return recursive
            ? content.IsComposedOf(docTypeAlias)
            : content.IsDocumentType(docTypeAlias);
    }

    /// <summary>
    /// Gets the content type alias of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <returns>The content type alias.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static string GetContentTypeAlias(this IPublishedContent content)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return content.ContentType.Alias;
    }

    /// <summary>
    /// Gets all composition aliases of the content type.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <returns>An enumerable of composition aliases.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static IEnumerable<string> GetCompositionAliases(this IPublishedContent content)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return content.ContentType.CompositionAliases;
    }

    /// <summary>
    /// Determines whether the content type has a specific property.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="propertyAlias">The property alias to check.</param>
    /// <returns>true if the content type has the property; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    public static bool HasProperty(this IPublishedContent content, string propertyAlias)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return content.ContentType.GetPropertyType(propertyAlias) != null;
    }
}