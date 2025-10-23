import { UmbLinkPickerModalElement } from './link-picker-modal.element.js';
import { expect } from '@open-wc/testing';
import { css } from '@umbraco-cms/backoffice/external/lit';

describe('UmbLinkPickerModalElement', () => {
	it('is defined as a custom element', () => {
		expect(customElements.get('umb-link-picker-modal')).to.equal(UmbLinkPickerModalElement);
	});

	describe('CSS styles for readonly URL display', () => {
		it('should include text-overflow ellipsis styles for readonly input', () => {
			// Get the static styles from the component
			const styles = UmbLinkPickerModalElement.styles;

			// Convert styles to string for inspection
			let stylesString = '';
			if (Array.isArray(styles)) {
				stylesString = styles.map((s) => s.cssText).join('\n');
			} else if (styles && typeof styles === 'object' && 'cssText' in styles) {
				stylesString = (styles as any).cssText;
			}

			// Check that the styles include the necessary CSS for preventing overflow
			// The styles should include text-overflow: ellipsis, overflow: hidden, and white-space: nowrap
			// for the readonly input
			expect(stylesString).to.include('text-overflow');
			expect(stylesString).to.include('ellipsis');
			expect(stylesString).to.include('overflow');
			expect(stylesString).to.include('hidden');
			expect(stylesString).to.include('white-space');
			expect(stylesString).to.include('nowrap');
		});
	});
});
