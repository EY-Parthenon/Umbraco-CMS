# Focus Management Fix for Mandatory Fields

## Issue Fixed
The document editor was incorrectly pulling focus back to mandatory fields when users tried to edit other fields like the title, preventing normal editing workflow.

## Root Cause
The validation controller (`validation.controller.ts`) was automatically calling `focusFirstInvalidElement()` whenever validation failed, including during field navigation when validators were being added or removed.

## Solution Implemented

### 1. Modified Validation Controller
- Added optional `focusOnError` parameter to the `validate()` method
- Default is `false` for regular validation, preventing focus hijacking during field navigation
- Only focuses on invalid elements when explicitly requested

### 2. Updated Submission Contexts
- Modified `submittable-workspace-context-base.ts` to pass `focusOnError: true` during submit validation
- Updated package builder and templating modals to pass the option during save/update operations

## Changes Made

### Core Files Modified:
1. `/src/packages/core/validation/controllers/validation.controller.ts`
   - Line 385: Added optional `options` parameter to `validate()` method
   - Line 423: Only calls `focusFirstInvalidElement()` when `options?.focusOnError === true`

2. `/src/packages/core/workspace/submittable/submittable-workspace-context-base.ts`
   - Line 77-78: Updated `validate()` method to accept and pass `focusOnError` parameter
   - Line 90: Passes `true` for `focusOnError` during submit validation

3. `/src/packages/packages/package-builder/workspace/workspace-package-builder.element.ts`
   - Updated `#save()` and `#update()` methods to pass `{ focusOnError: true }`

4. `/src/packages/templating/modals/templating-page-field-builder/templating-page-field-builder-modal.element.ts`
   - Updated `#submit()` method to pass `{ focusOnError: true }`

## Testing Instructions

1. Create a document type with multiple fields, including at least one mandatory field
2. Create or edit a document of this type
3. Leave the mandatory field empty
4. Try to edit the title or other fields
5. **Expected Result**: You should be able to freely edit any field without focus being pulled back
6. Try to save/publish the document
7. **Expected Result**: Focus should jump to the first invalid field and validation messages should appear

## Validation Behavior

- **During editing**: Validation messages appear but focus is not stolen
- **On save/submit**: Focus jumps to first invalid field if validation fails
- **Keyboard navigation**: Tab/Shift-Tab work normally without interference
- **Screen readers**: Validation messages are still announced without focus disruption