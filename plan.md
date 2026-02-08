# Implementation Plan

This document outlines the step-by-step plan for implementing the remaining features of the Facilitation Assistant. It is based on the [Feature Specifications](docs/features/README.md) and the current state described in [IMPLEMENTATION.md](IMPLEMENTATION.md).

**Status Legend:**
- [ ] Not Started
- [~] In Progress
- [x] Completed

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

- [ ] **First-run Tours**
    - Feature: Interactive guidance for new users.
    - Spec: [Onboarding Spec](docs/features/onboarding/spec.md)
    - Details:
        - Facilitator tour (Link sharing, Controls).
        - Attendee welcome (Anonymity explanation).
        - LocalStorage persistence for "seen" state.

## Phase 6: Infrastructure & Production Readiness
*Goal: Prepare for robust real-world usage.*

- [ ] **PostgreSQL Integration**
    - Feature: Switch from InMemory to robust database.
    - Spec: [Integration Matrix](docs/features/cross-cutting-concerns/integration-matrix.md)
    - Details:
        - connection strings.
        - EF Core Migrations.
        - Docker Compose support (optional but recommended).

- [ ] **Security Hardening**
    - Feature: Protect meeting data.
    - Spec: [Security Spec](docs/features/cross-cutting-concerns/security.md)
    - Details:
        - Link regeneration/invalidation.
        - Input sanitization (XSS prevention in notes/questions).
