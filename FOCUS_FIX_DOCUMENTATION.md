# Focus Management Fix for Document Editor

## Issue Description
When editing a document with mandatory fields in Umbraco CMS, the cursor focus was being pulled back to mandatory fields when trying to edit the document title. This created a frustrating user experience where users couldn't freely edit fields without the focus being stolen.

**Issue URL**: https://github.com/umbraco/umbraco-cms/issues/19099  
**Affected URI**: `/section/content/workspace/document/edit/{id}`

## Root Cause Analysis

The issue was caused by a conflict between three components:

1. **Validation Controller** (`validation.controller.ts`): 
   - Automatically called `focusFirstInvalidElement()` whenever validation failed
   - This happened during live validation while users were typing

2. **umbFocus Directive** (`focus.lit-directive.ts`):
   - Retried focus every 100ms if an element didn't have focus
   - Multiple elements with this directive competed for focus

3. **Workspace Validation**:
   - Triggered validation on property changes
   - Caused focus to jump to invalid mandatory fields

## Solution Implementation

### 1. Conditional Focus Management in Validation Controller
**File**: `/packages/core/validation/controllers/validation.controller.ts`

Added conditional focus control:
- New private property `#autoFocusOnValidation` (default: false)
- New methods: `setAutoFocusOnValidation()` and `getAutoFocusOnValidation()`
- Updated `validate()` method to accept options parameter with `focusOnError` flag
- Only focuses on invalid elements when explicitly requested

### 2. Updated Workspace Context
**File**: `/packages/core/workspace/submittable/submittable-workspace-context-base.ts`

Modified validation flow:
- Updated `validate()` method to pass focus options to validation contexts
- Modified `_validateAndLog()` to control focus behavior
- Focus is enabled during save/submit actions but disabled during live editing

### 3. Enhanced Focus Directive
**File**: `/packages/core/lit-element/directives/focus.lit-directive.ts`

Improved focus management:
- Added user interaction detection (mouse and keyboard events)
- Prevents focus stealing during user interaction (500ms window)
- Added retry limit (max 10 attempts) to prevent infinite loops
- Properly cleans up timeouts on disconnect

## Changes Summary

### Modified Files:
1. `/src/Umbraco.Web.UI.Client/src/packages/core/validation/controllers/validation.controller.ts`
2. `/src/Umbraco.Web.UI.Client/src/packages/core/workspace/submittable/submittable-workspace-context-base.ts`
3. `/src/Umbraco.Web.UI.Client/src/packages/core/lit-element/directives/focus.lit-directive.ts`

### Key Changes:
- Added `#autoFocusOnValidation` flag to validation controller
- Added `focusOnError` option to validation methods
- Enhanced focus directive with user interaction detection
- Added retry limits to prevent infinite focus loops

## Testing

A test page has been created at `test-focus-fix.html` to demonstrate the fix works correctly.

### Test Scenarios:
1. **Focus Stealing Prevention**: Edit title field with empty mandatory fields - focus should remain on title
2. **Save with Validation**: Click save with empty mandatory fields - focus moves to first invalid field
3. **Live Validation**: Validation messages appear without stealing focus during typing

## Expected Behavior After Fix

1. **During Editing**:
   - Users can freely edit any field without focus being stolen
   - Validation messages appear in real-time without affecting focus
   - User interaction is respected and not interrupted

2. **During Save/Submit**:
   - Focus automatically moves to first invalid field
   - Helps users quickly identify and fix validation errors
   - Maintains expected form submission behavior

3. **Focus Directive**:
   - Respects user interaction and doesn't steal focus during typing
   - Limits retry attempts to prevent infinite loops
   - Properly cleans up resources when elements are removed

## Migration Notes

For developers extending or customizing the validation system:

1. The validation controller now supports optional focus control
2. Use `validate({ focusOnError: true })` to enable focus on validation errors
3. The focus directive is now aware of user interactions
4. Default behavior is to NOT steal focus during live validation

## Performance Improvements

1. Reduced unnecessary focus attempts during user interaction
2. Limited retry attempts prevent infinite loops
3. Proper cleanup of timeouts prevents memory leaks

## Browser Compatibility

The fix uses standard DOM events and APIs that are supported in all modern browsers:
- `addEventListener` for mouse/keyboard events
- `setTimeout/clearTimeout` for timing control
- Standard focus management APIs

## Future Considerations

1. Consider making the user interaction timeout configurable
2. Add telemetry to track focus management issues
3. Consider adding focus management preferences in user settings
4. Investigate alternative validation UX patterns (inline vs. summary)