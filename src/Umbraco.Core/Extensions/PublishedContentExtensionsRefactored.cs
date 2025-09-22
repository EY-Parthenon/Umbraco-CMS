// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Extensions;

/// <summary>
/// This file documents the refactoring of PublishedContentExtensions.
/// The original PublishedContentExtensions class has been refactored into specialized extension classes
/// for better maintainability and organization while maintaining backward compatibility.
/// </summary>
/// <remarks>
/// The refactoring splits functionality into the following specialized classes:
/// - PublishedContentPropertyExtensions: Property access and values
/// - PublishedContentUrlExtensions: URL generation
/// - PublishedContentNavigationExtensions: Navigation (ancestors, descendants, children, siblings)
/// - PublishedContentCultureExtensions: Culture and language operations
/// - PublishedContentTypeExtensions: Content type operations
/// - PublishedContentTemplateExtensions: Template operations
/// 
/// The original PublishedContentExtensions class remains unchanged and continues to provide
/// all the same public API methods. The specialized classes can be used directly for more
/// focused functionality or through the original facade for backward compatibility.
/// </remarks>
public static partial class PublishedContentExtensions
{
    // This partial class declaration exists to document the refactoring.
    // All actual methods remain in the original PublishedContentExtensions.cs file
    // to maintain backward compatibility and avoid duplication.
    // The specialized extension classes in the PublishedContent folder provide
    // the modularized implementation that can be used directly when needed.
}