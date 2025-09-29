export interface UmbValidator extends EventTarget {
	/**
	 * The path to the data that the validator is validating.
	 */
	//readonly dataPath?: string;

	/**
	 * Validate the form, will return a promise that resolves to true if what the Validator represents is valid.
	 * @param shouldFocusOnError - If true, focus will be moved to the first invalid element when validation fails. Defaults to false.
	 */
	validate(shouldFocusOnError?: boolean): Promise<void>;

	/**
	 * Reset the validator to its initial state.
	 */
	reset(): void;

	/**
	 * Returns true if the validator is valid.
	 * This might represent last known state and might first be updated when validate() is called.
	 */
	isValid: boolean;

	/**
	 * Focus the first invalid element.
	 */
	focusFirstInvalidElement(): void;

	//getMessage(): string;
	//getMessages(): string[]; // Should we enable bringing multiple messages?

	destroy(): void;
}
