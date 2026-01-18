# Feature: Onboarding & First-Run Experience

## Problem Statement
The app relies on a "Zero-Friction" model with no login. However, new facilitators and attendees may feel lost without immediate guidance, leading to high abandonment rates or misuse of features (e.g., accidental link sharing).

## User Stories
- **US1**: As a new facilitator, I want a quick tour of the interface so I know where to find the meeting link and controls.
- **US2**: As an attendee, I want to know my anonymity status immediately so I feel safe participating.
- **US3**: As a facilitator, I want to easily dismiss tutorials once I understand the tool.

## Acceptance Criteria

### Facilitator Onboarding
- **AC1**: Given a user creates their first meeting, When the dashboard loads, Then a "Welcome Tour" overlay appears.
- **AC2**: The tour highlights 3 key areas:
  1. **Meeting Link**: "Share this link with attendees (Keep the other one private!)"
  2. **Timer Controls**: "Start/Stop stages here."
  3. **Feedback Panel**: "See questions and concerns here."
- **AC3**: The tour state (seen/unseen) is stored in `localStorage`.

### Attendee Onboarding
- **AC4**: Given a user joins a meeting for the first time, When the page loads, Then a "Welcome" modal appears via `localStorage` check.
- **AC5**: The modal content includes:
  - "You are anonymous (Attendee #X)"
  - "The facilitator controls the agenda."
  - "Your feedback is visible to the facilitator."

### Persistence
- **AC6**: Once a user dismisses a tutorial, it does not reappear on page refresh.
- **AC7**: A "Help" or "?" icon in the header allows users to restart the tour manually.
