// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for IPublishedContent navigation operations.
/// </summary>
public static class PublishedContentNavigationExtensions
{
    #region Parent

    /// <summary>
    /// Gets the parent of the content item.
    /// </summary>
    /// <typeparam name="T">The target content type.</typeparam>
    /// <param name="content">The content item.</param>
    /// <param name="navigationQueryService">The navigation query service.</param>
    /// <param name="publishedStatusFilteringService">The published status filtering service.</param>
    /// <param name="predicate">An optional predicate to filter the parent.</param>
    /// <returns>The parent of content, or null.</returns>
    public static T? Parent<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        Func<T, bool>? predicate = null)
        where T : class, IPublishedContent
    {
        IPublishedContent? parent = GetParent(content, navigationQueryService, publishedStatusFilteringService, p => predicate == null || predicate.Invoke(p as T ?? throw new InvalidOperationException()));
        return parent as T;
    }

    /// <summary>
    /// Gets the parent of the content item.
    /// </summary>
    /// <typeparam name="T">The target content type.</typeparam>
    /// <param name="content">The content item.</param>
    /// <param name="navigationQueryService">The navigation query service.</param>
    /// <param name="publishedStatusFilteringService">The published status filtering service.</param>
    /// <param name="contentTypeAlias">The content type alias of the parent.</param>
    /// <returns>The parent of content, or null.</returns>
    public static T? Parent<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        string contentTypeAlias)
        where T : class, IPublishedContent
    {
        IPublishedContent? parent = GetParent(content, navigationQueryService, publishedStatusFilteringService, p => p.ContentType.Alias.InvariantEquals(contentTypeAlias));
        return parent as T;
    }

    private static IPublishedContent? GetParent(
        IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        Func<IPublishedContent, bool>? predicate = null)
    {
        IPublishedContent? parent = content.Parent;
        if (parent == null)
        {
            return null;
        }

        parent = publishedStatusFilteringService.FilterOutIfNeeded(navigationQueryService, parent);
        return parent != null && (predicate == null || predicate(parent)) ? parent : null;
    }

    #endregion

    #region Ancestors

    /// <summary>
    /// Returns all ancestors of the current page.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="navigationQueryService">The navigation query service.</param>
    /// <param name="publishedStatusFilteringService">The published status filtering service.</param>
    /// <param name="includeRoot">True to include the root content in the results.</param>
    /// <returns>All ancestors of the current page.</returns>
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        bool includeRoot = true)
    {
        return content.AncestorsOrSelf(navigationQueryService, publishedStatusFilteringService, includeRoot, null)
            .Skip(1);
    }

    /// <summary>
    /// Returns all ancestors of the current page that match the predicate.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="navigationQueryService">The navigation query service.</param>
    /// <param name="publishedStatusFilteringService">The published status filtering service.</param>
    /// <param name="includeRoot">True to include the root content in the results.</param>
    /// <param name="predicate">A predicate to filter the ancestors.</param>
    /// <returns>All ancestors of the current page that match the predicate.</returns>
    public static IEnumerable<IPublishedContent> Ancestors(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        bool includeRoot,
        Func<IPublishedContent, bool>? predicate)
    {
        return content.AncestorsOrSelf(navigationQueryService, publishedStatusFilteringService, includeRoot, predicate)
            .Skip(1);
    }

    /// <summary>
    /// Returns all ancestors of the current page including the current page itself.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="navigationQueryService">The navigation query service.</param>
    /// <param name="publishedStatusFilteringService">The published status filtering service.</param>
    /// <param name="includeRoot">True to include the root content in the results.</param>
    /// <param name="predicate">A predicate to filter the ancestors.</param>
    /// <returns>All ancestors of the current page including the current page itself.</returns>
    public static IEnumerable<IPublishedContent> AncestorsOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        bool includeRoot = true,
        Func<IPublishedContent, bool>? predicate = null)
    {
        IEnumerable<IPublishedContent> ancestorsOrSelf = EnumerateAncestorsOrSelf(content, navigationQueryService, publishedStatusFilteringService, includeRoot);
        return predicate == null ? ancestorsOrSelf : ancestorsOrSelf.Where(predicate);
    }

    #endregion

    #region Children

    /// <summary>
    /// Returns the children of the current page.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="navigationQueryService">The navigation query service.</param>
    /// <param name="publishedStatusFilteringService">The published status filtering service.</param>
    /// <param name="predicate">An optional predicate to filter the children.</param>
    /// <returns>The children of the current page.</returns>
    public static IEnumerable<IPublishedContent> Children(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        Func<IPublishedContent, bool>? predicate = null)
    {
        IEnumerable<IPublishedContent> children = EnumerateChildren(content, navigationQueryService, publishedStatusFilteringService);
        return predicate == null ? children : children.Where(predicate);
    }

    /// <summary>
    /// Returns the children of the current page of a given type.
    /// </summary>
    /// <typeparam name="T">The target content type.</typeparam>
    /// <param name="content">The content item.</param>
    /// <param name="navigationQueryService">The navigation query service.</param>
    /// <param name="publishedStatusFilteringService">The published status filtering service.</param>
    /// <returns>The children of the current page of the given type.</returns>
    public static IEnumerable<T> Children<T>(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
        where T : class, IPublishedContent
    {
        return content.Children(navigationQueryService, publishedStatusFilteringService).OfType<T>();
    }

    /// <summary>
    /// Returns the first child of the current page.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="navigationQueryService">The navigation query service.</param>
    /// <param name="publishedStatusFilteringService">The published status filtering service.</param>
    /// <param name="predicate">An optional predicate to filter the child.</param>
    /// <returns>The first child of the current page, or null.</returns>
    public static IPublishedContent? FirstChild(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        Func<IPublishedContent, bool>? predicate = null)
    {
        return content.Children(navigationQueryService, publishedStatusFilteringService, predicate).FirstOrDefault();
    }

    #endregion

    #region Descendants

    /// <summary>
    /// Returns all descendants of the current page.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="navigationQueryService">The navigation query service.</param>
    /// <param name="publishedStatusFilteringService">The published status filtering service.</param>
    /// <returns>All descendants of the current page.</returns>
    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
    {
        return content.DescendantsOrSelf(navigationQueryService, publishedStatusFilteringService, null).Skip(1);
    }

    /// <summary>
    /// Returns all descendants of the current page that match the predicate.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="navigationQueryService">The navigation query service.</param>
    /// <param name="publishedStatusFilteringService">The published status filtering service.</param>
    /// <param name="predicate">A predicate to filter the descendants.</param>
    /// <returns>All descendants of the current page that match the predicate.</returns>
    public static IEnumerable<IPublishedContent> Descendants(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        Func<IPublishedContent, bool> predicate)
    {
        return content.DescendantsOrSelf(navigationQueryService, publishedStatusFilteringService, predicate).Skip(1);
    }

    /// <summary>
    /// Returns all descendants of the current page including the current page itself.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="navigationQueryService">The navigation query service.</param>
    /// <param name="publishedStatusFilteringService">The published status filtering service.</param>
    /// <param name="predicate">An optional predicate to filter the descendants.</param>
    /// <returns>All descendants of the current page including the current page itself.</returns>
    public static IEnumerable<IPublishedContent> DescendantsOrSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        Func<IPublishedContent, bool>? predicate = null)
    {
        IEnumerable<IPublishedContent> descendantsOrSelf = EnumerateDescendantsOrSelf(content, navigationQueryService, publishedStatusFilteringService);
        return predicate == null ? descendantsOrSelf : descendantsOrSelf.Where(predicate);
    }

    #endregion

    #region Siblings

    /// <summary>
    /// Returns the siblings of the current page.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="navigationQueryService">The navigation query service.</param>
    /// <param name="publishedStatusFilteringService">The published status filtering service.</param>
    /// <returns>The siblings of the current page.</returns>
    public static IEnumerable<IPublishedContent> Siblings(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
    {
        return SiblingsAndSelf(content, navigationQueryService, publishedStatusFilteringService)
            .Where(x => x.Id != content.Id);
    }

    /// <summary>
    /// Returns the siblings of the current page including the current page itself.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="navigationQueryService">The navigation query service.</param>
    /// <param name="publishedStatusFilteringService">The published status filtering service.</param>
    /// <returns>The siblings of the current page including the current page itself.</returns>
    public static IEnumerable<IPublishedContent> SiblingsAndSelf(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
    {
        IPublishedContent? parent = GetParent(content, navigationQueryService, publishedStatusFilteringService);
        return parent != null
            ? EnumerateChildren(parent, navigationQueryService, publishedStatusFilteringService)
            : Enumerable.Empty<IPublishedContent>();
    }

    #endregion

    #region Breadcrumbs

    /// <summary>
    /// Returns a breadcrumb trail for the current page.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="navigationQueryService">The navigation query service.</param>
    /// <param name="publishedStatusFilteringService">The published status filtering service.</param>
    /// <param name="includeRoot">True to include the root content in the breadcrumb trail.</param>
    /// <returns>A breadcrumb trail for the current page.</returns>
    public static IEnumerable<IPublishedContent> Breadcrumbs(
        this IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        bool includeRoot = true)
    {
        return content.AncestorsOrSelf(navigationQueryService, publishedStatusFilteringService, includeRoot).Reverse();
    }

    #endregion

    #region Relationships

    /// <summary>
    /// Determines whether the content is equal to another content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="other">The other content item.</param>
    /// <returns>true if the content items are equal; otherwise, false.</returns>
    public static bool IsEqual(this IPublishedContent content, IPublishedContent other) => content.Id == other.Id;

    /// <summary>
    /// Determines whether the content is not equal to another content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="other">The other content item.</param>
    /// <returns>true if the content items are not equal; otherwise, false.</returns>
    public static bool IsNotEqual(this IPublishedContent content, IPublishedContent other) => !IsEqual(content, other);

    /// <summary>
    /// Determines whether the content is a descendant of another content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="other">The other content item.</param>
    /// <returns>true if the content is a descendant of the other content item; otherwise, false.</returns>
    public static bool IsDescendant(this IPublishedContent content, IPublishedContent other) =>
        other.Level < content.Level && content.Path.InvariantStartsWith(other.Path.EnsureEndsWith(','));

    /// <summary>
    /// Determines whether the content is a descendant of or equal to another content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="other">The other content item.</param>
    /// <returns>true if the content is a descendant of or equal to the other content item; otherwise, false.</returns>
    public static bool IsDescendantOrSelf(this IPublishedContent content, IPublishedContent other) =>
        IsEqual(content, other) || IsDescendant(content, other);

    /// <summary>
    /// Determines whether the content is an ancestor of another content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="other">The other content item.</param>
    /// <returns>true if the content is an ancestor of the other content item; otherwise, false.</returns>
    public static bool IsAncestor(this IPublishedContent content, IPublishedContent other) =>
        content.Level < other.Level && other.Path.InvariantStartsWith(content.Path.EnsureEndsWith(','));

    /// <summary>
    /// Determines whether the content is an ancestor of or equal to another content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="other">The other content item.</param>
    /// <returns>true if the content is an ancestor of or equal to the other content item; otherwise, false.</returns>
    public static bool IsAncestorOrSelf(this IPublishedContent content, IPublishedContent other) =>
        IsEqual(content, other) || IsAncestor(content, other);

    #endregion

    #region Helper Methods

    private static IEnumerable<IPublishedContent> EnumerateAncestorsOrSelf(
        IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService,
        bool includeRoot)
    {
        yield return content;

        while (content.Parent != null)
        {
            content = content.Parent;
            IPublishedContent? filtered = publishedStatusFilteringService.FilterOutIfNeeded(navigationQueryService, content);
            if (filtered != null && (includeRoot || content.Parent != null))
            {
                yield return filtered;
            }
        }
    }

    private static IEnumerable<IPublishedContent> EnumerateChildren(
        IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
    {
        foreach (IPublishedContent child in content.Children)
        {
            IPublishedContent? filtered = publishedStatusFilteringService.FilterOutIfNeeded(navigationQueryService, child);
            if (filtered != null)
            {
                yield return filtered;
            }
        }
    }

    private static IEnumerable<IPublishedContent> EnumerateDescendantsOrSelf(
        IPublishedContent content,
        INavigationQueryService navigationQueryService,
        IPublishedStatusFilteringService publishedStatusFilteringService)
    {
        yield return content;

        foreach (IPublishedContent child in EnumerateChildren(content, navigationQueryService, publishedStatusFilteringService))
        {
            foreach (IPublishedContent descendant in EnumerateDescendantsOrSelf(child, navigationQueryService, publishedStatusFilteringService))
            {
                yield return descendant;
            }
        }
    }

    #endregion
}