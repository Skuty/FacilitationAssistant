# Feature: User Settings & Preferences

## Problem Statement
Users have different environment needs (dark rooms vs bright offices) and accessibility requirements. Since there are no accounts, we need a robust way to persist these preferences locally.

## User Stories
- **US1**: As a user, I want to switch to Dark Mode so the screen doesn't glare during a presentation.
- **US2**: As a user, I want to disable sound notifications so I don't disturb others.
- **US3**: As a user, I want to view the interface in my preferred language (if supported later).

## Acceptance Criteria

### Interface
- **AC1**: A "Settings" gear icon is visible in the header for both Facilitators and Attendees.
- **AC2**: Clicking the gear opens a modal/panel with the following sections:
  - **Appearance**: Theme (System / Light / Dark)
  - **Notifications**: Sound Effects (On / Off), Toast Popups (On / Off)
  - **Accessibility**: Reduced Motion (On / Off), High Contrast (On / Off)

### Behavior
- **AC3**: **Theme**: Defaults to "System". Changes apply immediately without reload.
- **AC4**: **Sound**: Defaults to "On". Controls sounds for: Stage timer ending, New Message received, New Concern raised (Facilitator only).
- **AC5**: **Persistence**: All settings are saved to `localStorage` key `user_preferences`.
- **AC6**: Settings are device-specific; changing them on mobile does not affect desktop.

### Edge Cases
- **AC7**: If `localStorage` is disabled/full, settings revert to defaults on every page load.
- **AC8**: If "Reduced Motion" is enabled in OS, the app automatically respects it regardless of the in-app toggle default.
