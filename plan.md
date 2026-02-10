# Implementation Plan

This document outlines the step-by-step plan for implementing the remaining features of the Facilitation Assistant. It is based on the [Feature Specifications](docs/features/README.md) and the current state described in [IMPLEMENTATION.md](IMPLEMENTATION.md).

**Status Legend:**
- [ ] Not Started
- [~] In Progress
- [x] Completed

## Phase 0: Communication & Messaging
*Goal: Enable facilitators to broadcast messages and questions to attendees during meetings.*

- [x] **Messaging & Communication System**
    - Feature: Broadcast messages, announcements, and quick questions to attendees.
    - Spec: [Messaging Spec](docs/features/messaging/spec.md) (AC1-AC38)
    - Details:
        - Message types (Announcement/Question).
        - Response types (Reactions, Predefined Answers, Free Text).
        - Real-time notifications for attendees.
        - Message history and response tracking.
    - Implementation:
        - Created `Message`, `MessageOption`, and `MessageResponse` entities
        - Created `CreateMessageCommand`, `RespondToMessageCommand`, and `CloseMessageCommand`
        - Created query handlers: `GetMessagesByMeetingQuery`, `GetMessageByIdQuery`, `GetPendingMessagesQuery`
        - Added `MessageManager` component to Facilitator view for composing and sending messages
        - Added `AttendeeMessages` component to Attendee view with notification modals
        - Messages support Announcements (no response) and Questions (with reactions, predefined answers, or free text)
        - Attendees see modal notifications for new messages with response options
        - Facilitator can view response counts and close messages
        - Successfully tested: application builds, all 8 tests pass, UI verified

## Phase 1: Meeting Lifecycle & Summary
*Goal: Allow meetings to be formally ended and provide a persistent record of what happened.*

- [x] **End Meeting Functionality**
    - Feature: Facilitator can permanently end a meeting.
    - Spec: [Meeting Summary Spec](docs/features/meeting-summary/spec.md) (AC1-AC4)
    - Details:
        - "End Meeting" button for facilitator.
        - Confirmation dialog.
        - Stops all timers and freezes state.
        - Redirects all users to Summary view.

- [x] **Meeting Summary View**
    - Feature: Read-only view of a concluded meeting.
    - Spec: [Meeting Summary Spec](docs/features/meeting-summary/spec.md) (AC5-AC39)
    - Details:
        - Metadata (Duration, Date).
        - Agenda breakdown (Actual vs Planned durations).
        - List of Public Notes.
        - List of Raised Concerns (and responses).
        - Statistics.

## Phase 2: Enhanced Agenda Management
*Goal: Give facilitators full control over the agenda structure during setup and execution.*

- [x] **Reorder Agenda Stages**
    - Feature: Drag-and-drop or Move Up/Down controls for stages.
    - Spec: [Agenda Management](docs/features/agenda-management/spec.md) (Implied/Future Enhancement)
    - Details: Allow changing `OrderIndex` of stages.
    - Implementation:
        - Created `ReorderAgendaStagesCommand` and handler
        - Added Move Up/Down buttons to Facilitator UI in Setup phase
        - Buttons are disabled appropriately (top stage can't move up, bottom can't move down)
        - Successfully tested with Playwright automation

- [x] **Delete Agenda Stages**
    - Feature: Remove a stage that was added by mistake.
    - Spec: [Agenda Management](docs/features/agenda-management/spec.md) (Implied/Future Enhancement)
    - Details: Soft or hard delete of stages not yet started.
    - Implementation:
        - Created `DeleteAgendaStageCommand` and handler
        - Added delete (🗑️) button to Facilitator UI in Setup phase
        - Only stages with status `NotStarted` can be deleted
        - Remaining stages are automatically reordered after deletion
        - Successfully tested with Playwright automation

## Phase 3: Polling & Questions System
*Goal: Enable structured feedback collection beyond simple concerns.*

- [x] **Question Management (Facilitator)**
    - Feature: Define questions before or during meeting.
    - Spec: [Polling System Spec](docs/features/polling-system/spec.md) (AC1-AC11)
    - Details:
        - Types: Single Choice, Multiple Choice, Free Text, Scale.
        - Association with specific stages.
        - Ad-hoc question creation.
    - Implementation:
        - Created Question, QuestionOption, and QuestionResponse entities
        - Created CreateQuestionCommand, TriggerQuestionCommand, CloseQuestionCommand, and SubmitQuestionResponseCommand
        - Created query handlers: GetQuestionsByMeetingQuery, GetQuestionByIdQuery, GetActiveQuestionsForAttendeeQuery, GetQuestionResultsQuery
        - Added question management UI to Facilitator view (both Setup and Active phases)
        - Questions can be added with different types (Single Choice, Multiple Choice, Free Text, Scale)
        - Questions can be triggered manually or set to auto-trigger at meeting/stage start
        - Results are shown in real-time with response counts and percentages
        - Successfully tested with Playwright automation

- [x] **Attendee Question Interface**
    - Feature: Attendees see and answer questions.
    - Spec: [Polling System Spec](docs/features/polling-system/spec.md) (AC12-AC20)
    - Details:
        - Modal/Panel overlay when question is triggered.
        - Answer submission logic.
    - Implementation:
        - Added question modal overlay to Attendee view
        - Displays active questions one at a time (FIFO queue)
        - Supports all question types: Single Choice, Multiple Choice, Free Text, Scale
        - Submit and Skip functionality with response tracking
        - Shows "pending questions" badge in meeting progress card
        - Confirmation toast message after successful submission
        - Successfully tested: application builds and all tests pass

- [x] **Real-time Results**
    - Feature: Visualization of poll results.
    - Spec: [Polling System Spec](docs/features/polling-system/spec.md) (AC21-AC31)
    - Details:
        - Facilitator view of results.
        - Optional shared view for attendees.
        - Charts/Graphs for structured data, List for text.
    - Implementation:
        - Enhanced scale results in Facilitator view with distribution histogram
        - Added ResultVisibility setting in question creation (Facilitator Only, Aggregated, All Responses)
        - Implemented attendee results display based on ResultVisibility setting
        - Results show after submission: aggregated charts/percentages for choice/scale questions
        - Facilitator view updates results every 5 seconds for real-time display
        - Free text answers shown as list (based on visibility setting)
        - Fixed Mediator DI configuration issue (changed to Singleton with DbContext)
        - Successfully tested: application builds and all 8 tests pass

## Phase 4: Enhanced Engagement (Concerns & Notes)
*Goal: Deepen the interaction capabilities for all participants.*

- [x] **Concern Voting & Interaction**
    - Feature: Attendees can vote on concerns; Facilitators can respond.
    - Spec: [Concerns & Feedback Spec](docs/features/concerns-feedback/spec.md) (AC10-AC26)
    - Details:
        - Like/Dislike/Neutral votes.
        - Facilitator text response to concerns.
        - Withdraw concern functionality.
    - Implementation:
        - Created `ConcernVote` entity with unique constraint per session/concern
        - Updated `Concern` entity with acknowledgment, response, and withdrawal tracking
        - Created commands: `VoteConcernCommand`, `AcknowledgeConcernCommand`, `RespondToConcernCommand`, `WithdrawConcernCommand`
        - Created `GetConcernsByMeetingQuery` to load concerns with votes
        - Implemented handlers for all concern interaction commands
        - Added voting UI to Attendee view with real-time vote counts and visual feedback
        - Added concern management UI to Facilitator view with acknowledge/respond functionality
        - Attendees can vote (like/dislike/neutral) and toggle votes
        - Facilitators can acknowledge concerns and provide text responses
        - Attendees can withdraw their own concerns
        - All changes persist and sync in real-time across users
        - Successfully tested: application builds and all 8 tests pass

- [x] **Private Notes & Note Management**
    - Feature: Private personal notes and editing capabilities.
    - Spec: [Notes System Spec](docs/features/notes-system/spec.md) (AC9-AC22)
    - Details:
        - Toggle Public/Private visibility.
        - Edit existing notes.
        - Delete notes.
    - Implementation:
        - Created `UpdateNoteCommand` and handler to update note content and visibility
        - Created `DeleteNoteCommand` and handler to remove notes
        - Added note management UI to Facilitator view with visibility toggle, edit, and delete
        - Added note management UI to Attendee view with same capabilities
        - Notes show visibility icons: 👁️ for public, 🔒 for private
        - Edit mode with inline textarea and save/cancel buttons
        - Delete confirmation modal for both facilitator and attendee views
        - Updated Summary view to filter private notes based on viewer role
        - Facilitators see all notes; attendees see only public notes + their own
        - Successfully tested: application builds, all tests pass, UI validated with Playwright

## Phase 5: Onboarding & Experience
*Goal: Reduce friction for first-time users.*

- [x] **First-run Tours**
    - Feature: Interactive guidance for new users.
    - Spec: [Onboarding Spec](docs/features/onboarding/spec.md)
    - Details:
        - Facilitator tour (Link sharing, Controls).
        - Attendee welcome (Anonymity explanation).
        - LocalStorage persistence for "seen" state.
    - Implementation:
        - Created JavaScript interop (`onboarding.js`) for localStorage management
        - Created `TourOverlay` component with 5-step facilitator tour
        - Created `AttendeeWelcome` modal component
        - Added help (❓) button to both pages to manually restart tours
        - Tour highlights: meeting link sharing, timer controls, feedback panel
        - Welcome modal explains: anonymity, facilitator control, feedback visibility
        - Tours persist state in localStorage (won't show again after dismissal)
        - Successfully tested: application builds, all tests pass, UI validated with Playwright

## Phase 6: Infrastructure & Production Readiness
*Goal: Prepare for robust real-world usage.*

- [x] **PostgreSQL Integration**
    - Feature: Switch from InMemory to robust database.
    - Spec: [Integration Matrix](docs/features/cross-cutting-concerns/integration-matrix.md)
    - Details:
        - connection strings.
        - EF Core Migrations.
        - Docker Compose support (optional but recommended).
    - Implementation:
        - Added Npgsql.EntityFrameworkCore.PostgreSQL 9.0.0 package
        - Added Microsoft.EntityFrameworkCore.InMemory 9.0.0 for testing
        - Added Microsoft.EntityFrameworkCore.Design 9.0.0 for migrations
        - Updated appsettings.json with database provider configuration and connection strings
        - Updated Program.cs to dynamically select database provider (PostgreSQL or InMemory) based on configuration
        - Created FacilitationDbContextFactory for design-time DbContext creation during migrations
        - Created initial EF Core migration (InitialCreate) with full schema
        - Created docker-compose.yml for easy PostgreSQL setup with health checks
        - Created DATABASE_SETUP.md with comprehensive setup instructions
        - Development mode uses InMemory database by default; Production can use PostgreSQL
        - Successfully tested: application builds, all 8 tests pass, UI verified with Playwright

- [x] **Security Hardening**
    - Feature: Protect meeting data.
    - Spec: [Security Spec](docs/features/cross-cutting-concerns/security.md)
    - Details:
        - Link regeneration/invalidation.
        - Input sanitization (XSS prevention in notes/questions).
        - Rate limiting (60 requests/minute per IP).
        - Resource creation limits (max 50 concerns, 100 notes per meeting).
    - Implementation:
        - Created `RegenerateFacilitatorTokenCommand` and handler to generate new cryptographically secure tokens
        - Added input sanitization using `HtmlEncoder.Default.Encode()` to all user input handlers:
          - AddNoteHandler, UpdateNoteHandler (note content)
          - RaiseConcernHandler, RespondToConcernHandler (concern text, responses)
          - CreateMeetingHandler (meeting title)
          - AddAgendaStageHandler (stage name, description)
          - CreateQuestionHandler (question text, options, scale labels)
          - SubmitQuestionResponseHandler (free text answers)
        - Configured ASP.NET Core rate limiting middleware with FixedWindowRateLimiter (60 requests/minute per IP)
        - Added resource creation limits validation:
          - Max 100 notes per meeting (enforced in AddNoteHandler)
          - Max 50 concerns per meeting (enforced in RaiseConcernHandler)
        - Enhanced Facilitator UI with link security features:
          - Facilitator link obfuscation (masked with bullets until revealed)
          - "Reveal/Hide" toggle button for facilitator link
          - "Regenerate Link" button with loading spinner
          - Copy button disabled when link is hidden
          - Success/error messages for regeneration
          - Automatic navigation to new URL after regeneration
        - Successfully tested: application builds, all 8 tests pass, UI validated with Playwright
        - Verified: link obfuscation, reveal/hide toggle, and link regeneration all working correctly

## Phase 7: User Experience & Customization
*Goal: Improve usability and allow personalization.*

- [x] **User Settings & Preferences**
    - Feature: Personalize interface appearance and behavior.
    - Spec: [User Settings Spec](docs/features/user-settings/spec.md) (AC1-AC8)
    - Details:
        - Theme selection (System/Light/Dark).
        - Sound and toast notification controls.
        - Accessibility options (Reduced Motion, High Contrast).
        - LocalStorage persistence.
    - Implementation:
        - Created `settings.js` JavaScript interop for localStorage management
        - Implemented theme system with CSS variables supporting light/dark modes
        - Created `SettingsModal` component with 3 sections: Appearance, Notifications, Accessibility
        - Added settings gear (⚙️) button to both Facilitator and Attendee views
        - Theme options: System (auto-detect), Light, Dark with instant preview
        - Notification toggles: Sound Effects and Toast Notifications
        - Accessibility options: Reduced Motion (with OS-level detection) and High Contrast
        - All settings persist to localStorage and apply immediately without reload
        - Respects OS-level reduced motion preference automatically
        - "Reset to Defaults" button to restore original settings
        - Successfully tested: application builds, all 8 tests pass, UI validated with Playwright
        - Verified: settings modal opens/closes correctly, all controls functional

## Phase 8: Messaging & Communication System
*Goal: Enable facilitators to broadcast messages and gather structured responses from attendees.*

- [x] **Messaging System**
    - Feature: Facilitators can broadcast announcements and questions; attendees can respond.
    - Spec: [Messaging System Spec](docs/features/messaging/spec.md) (AC1-AC38)
    - Details:
        - Message types: Announcement and Question.
        - Response types: Reactions (👍 👎 ❤️ 😂 😮), Predefined Answers (2-6 options), Free Text (max 500 chars).
        - Message history and response tracking.
        - Real-time notifications for attendees.
        - Facilitator view of aggregated responses.
    - Implementation:
        - Created `Message`, `MessageOption`, and `MessageResponse` entities with proper enums
        - Created commands: `CreateMessageCommand`, `RespondToMessageCommand`, `CloseMessageCommand`
        - Created queries: `GetMessagesByMeetingQuery`, `GetMessageByIdQuery`, `GetPendingMessagesQuery`
        - Implemented handlers with input sanitization and validation:
          - Message text max 1000 chars
          - Predefined options 2-6, each max 100 chars
          - Free text responses max 500 chars
          - Reaction validation for emoji set
        - Created `MessageManager` component for Facilitator view:
          - Message composer with type selection (Announcement/Question)
          - Response type configuration (Reactions/Predefined/FreeText)
          - Dynamic option builder for predefined answers
          - Message list with response counts
          - Close message functionality
        - Created `AttendeeMessages` component for Attendee view:
          - Toast notification banner for new messages
          - Modal overlay for message viewing and responding
          - Support for all response types with proper UI
          - FIFO queue for multiple pending messages
          - Auto-refresh integration with polling timer
        - Updated `FacilitationDbContext` with DbSets and entity configurations
        - Updated `Meeting` entity with Messages navigation property
        - Integrated components into Facilitator and Attendee pages
        - Successfully tested: application builds, all 8 tests pass, UI components render correctly
        - Verified: message system is functional and ready for use

## Phase 9: Accessibility (WCAG 2.1 AA Compliance)
*Goal: Ensure application is accessible to all users including those using assistive technologies.*

- [x] **Accessibility Features Implementation**
    - Feature: Full WCAG 2.1 Level AA compliance for improved usability.
    - Spec: [Accessibility Spec](docs/features/cross-cutting-concerns/accessibility.md)
    - Details:
        - Skip links for keyboard navigation
        - ARIA live regions for dynamic content updates
        - Full ARIA attributes for all interactive elements
        - Enhanced focus states with high contrast support
        - Proper semantic landmarks (banner, main, etc.)
        - Icon accessibility with aria-hidden for decorative elements
        - Screen reader compatible progress bars
    - Implementation:
        - Added CSS styles for skip links with focus-visible support
        - Enhanced focus states for WCAG 2.1 AA compliance (2-3px outlines)
        - Added `.sr-only` and `.sr-only-focusable` utility classes
        - Added skip link to all pages (Attendee, Facilitator, Summary)
        - Implemented proper ARIA landmarks:
          - `role="banner"` for page headers
          - `role="main"` with `id="main-content"` for main content areas
          - `role="dialog"` and `aria-modal="true"` for modal dialogs
        - Added ARIA labels to all icon buttons:
          - Settings buttons: `aria-label="Open settings"`
          - Help buttons: `aria-label="Show tour/welcome"`
          - Action buttons with clear descriptive labels
        - Decorated all emoji/icon elements with `aria-hidden="true"`
        - Implemented ARIA live regions:
          - Timer displays: `aria-live="off"` (updates too frequently for screen readers)
          - Stage time: `aria-live="polite"` for status updates
        - Added full progress bar ARIA attributes:
          - `role="progressbar"`
          - `aria-valuenow`, `aria-valuemin`, `aria-valuemax`
          - `aria-label` with descriptive text
        - Applied to all progress bars:
          - Stage progress bars in Attendee/Facilitator views
          - Question result visualization bars
          - Scale distribution histograms
        - Enhanced modal dialogs with proper ARIA:
          - `aria-labelledby` pointing to modal titles
          - Close buttons with `aria-label="Close"`
        - Successfully tested: application builds, all 8 tests pass
        - Verified: Focus states, skip links, and ARIA attributes functional

