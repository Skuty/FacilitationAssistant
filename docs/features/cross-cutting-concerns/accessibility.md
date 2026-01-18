# Cross-Cutting Concern: Accessibility (A11y)

## Standard
All features must comply with **WCAG 2.1 Level AA**.

## Core Requirements

### 1. Keyboard Navigation
- **Focus States**: All interactive elements (Buttons, Inputs, Tiles) must have a visible `:focus` ring (min 2px solid contrast color).
- **Tab Order**: Logical flow (Header -> Main Content -> Sidebar -> Footer).
- **Skip Links**: "Skip to Main Content" must be the first tabable element.
- **No Trap**: No modal or component should trap keyboard focus without an escape key exit.

### 2. Screen Reader Support (ARIA)
- **Live Regions**:
  - Timer updates: Use `aria-live="off"` for the second-by-second countdown (too noisy), but `aria-live="assertive"` for "Stage Ended" alerts.
  - New Messages: Use `aria-live="polite"` for incoming messages/concerns.
- **Progress Bars**: Must use `role="progressbar"`, `aria-valuenow`, `aria-valuemin`, `aria-valuemax`.
- **Icons**: All decorative icons must have `aria-hidden="true"`. Functional icons must have `aria-label`.

### 3. Visual Design
- **Color Contrast**: Text must maintain 4.5:1 ratio against background.
- **Color Independence**: State (e.g., "Overrunning") must not rely solely on color (Red). Use text labels ("Overrun") or icons alongside color changes.
- **Text Resizing**: UI must support 200% text zoom without breaking layout or overlapping.

## Testing Checklist
- [ ] Pass automated audit (e.g., Lighthouse / axe-core)
- [ ] Manual keyboard-only run-through of the "Critical Path" (Create meeting -> Start stage -> Vote).
- [ ] NVDA/VoiceOver test for Timer announcements.
