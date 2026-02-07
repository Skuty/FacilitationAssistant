# Implementation Plan

This document outlines the step-by-step plan for implementing the remaining features of the Facilitation Assistant. It is based on the [Feature Specifications](docs/features/README.md) and the current state described in [IMPLEMENTATION.md](IMPLEMENTATION.md).

**Status Legend:**
- [ ] Not Started
- [~] In Progress
- [x] Completed

## Phase 1: Meeting Lifecycle & Summary
*Goal: Allow meetings to be formally ended and provide a persistent record of what happened.*

- [ ] **End Meeting Functionality**
    - Feature: Facilitator can permanently end a meeting.
    - Spec: [Meeting Summary Spec](docs/features/meeting-summary/spec.md) (AC1-AC4)
    - Details:
        - "End Meeting" button for facilitator.
        - Confirmation dialog.
        - Stops all timers and freezes state.
        - Redirects all users to Summary view.

- [ ] **Meeting Summary View**
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

- [ ] **Reorder Agenda Stages**
    - Feature: Drag-and-drop or Move Up/Down controls for stages.
    - Spec: [Agenda Management](docs/features/agenda-management/spec.md) (Implied/Future Enhancement)
    - Details: Allow changing `OrderIndex` of stages.

- [ ] **Delete Agenda Stages**
    - Feature: Remove a stage that was added by mistake.
    - Spec: [Agenda Management](docs/features/agenda-management/spec.md) (Implied/Future Enhancement)
    - Details: Soft or hard delete of stages not yet started.

## Phase 3: Polling & Questions System
*Goal: Enable structured feedback collection beyond simple concerns.*

- [ ] **Question Management (Facilitator)**
    - Feature: Define questions before or during meeting.
    - Spec: [Polling System Spec](docs/features/polling-system/spec.md) (AC1-AC11)
    - Details:
        - Types: Single Choice, Multiple Choice, Free Text, Scale.
        - Association with specific stages.
        - Ad-hoc question creation.

- [ ] **Attendee Question Interface**
    - Feature: Attendees see and answer questions.
    - Spec: [Polling System Spec](docs/features/polling-system/spec.md) (AC12-AC20)
    - Details:
        - Modal/Panel overlay when question is triggered.
        - Answer submission logic.

- [ ] **Real-time Results**
    - Feature: Visualization of poll results.
    - Spec: [Polling System Spec](docs/features/polling-system/spec.md) (AC21-AC31)
    - Details:
        - Facilitator view of results.
        - Optional shared view for attendees.
        - Charts/Graphs for structured data, List for text.

## Phase 4: Enhanced Engagement (Concerns & Notes)
*Goal: Deepen the interaction capabilities for all participants.*

- [ ] **Concern Voting & Interaction**
    - Feature: Attendees can vote on concerns; Facilitators can respond.
    - Spec: [Concerns & Feedback Spec](docs/features/concerns-feedback/spec.md) (AC10-AC26)
    - Details:
        - Like/Dislike/Neutral votes.
        - Facilitator text response to concerns.
        - Withdraw concern functionality.

- [ ] **Private Notes & Note Management**
    - Feature: Private personal notes and editing capabilities.
    - Spec: [Notes System Spec](docs/features/notes-system/spec.md) (AC9-AC22)
    - Details:
        - Toggle Public/Private visibility.
        - Edit existing notes.
        - Delete notes.

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
