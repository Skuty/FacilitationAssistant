# Feature: Error Handling & Recovery Mechanisms

## Problem Statement

Real-time meeting applications experience failures from network issues, server errors, invalid user actions, and data conflicts. Without explicit error handling, users are left confused when actions fail, data is lost, or the interface becomes unresponsive. Facilitators need graceful degradation to maintain meeting continuity even during technical failures.

## User Stories

- **US1**: As an attendee, I want to see clear error messages when my actions fail so that I understand what went wrong
- **US2**: As a facilitator, I want the meeting to continue with degraded functionality if real-time sync fails so that technical issues don't halt the meeting
- **US3**: As an attendee, I want automatic retry for failed actions so that transient network issues don't require manual intervention
- **US4**: As a facilitator, I want to be notified when attendees are experiencing errors so that I can adjust the meeting pace
- **US5**: As an attendee, I want validation errors to be caught before submission so that I don't waste time submitting invalid data
- **US6**: As a facilitator, I want meeting state to recover from server crashes so that meetings aren't lost due to infrastructure failures

## Acceptance Criteria

### Client-Side Validation

- **AC1**: Given I am creating a stage with an empty name, When I try to save, Then I see "Stage name is required" error message via Blazor EditForm validation without server round-trip
- **AC2**: Given I am setting stage duration to 0, When I enter the value, Then the input shows "Duration must be at least 1 minute" error immediately via Blazor InputNumber validation attribute
- **AC3**: Given I am submitting a custom concern with 9 characters, When I try to submit, Then the submit button is disabled via Blazor EditForm context and shows "Minimum 10 characters" below the input
- **AC4**: Given I exceed character limits (stage name > 100 chars), When I type beyond the limit, Then additional characters are not accepted via InputText maxlength and counter shows "0 characters remaining"

### Network Error Handling

- **AC5**: Given I submit an action via MediatR command (raise concern, answer question), When the command handler experiences timeout, Then I see "Connection timeout - Retrying automatically..." message in Blazor component
- **AC6**: Given a command fails, When automatic retry (via Polly retry policy) succeeds within 3 attempts, Then the error message is replaced with "Action completed" success message
- **AC7**: Given a command fails 3 times, When Polly retry is exhausted, Then I see "Action failed - Please try again" with manual "Retry" button in component
- **AC8**: Given Blazor circuit is disconnected, When I try to perform an action, Then I see "You are offline - Action will be retried when connection is restored" message (Blazor reconnection UI)

### Server Error Responses

- **AC9**: Given the MediatR command handler throws unhandled exception, When response is returned, Then Blazor error boundary shows "Something went wrong - Our team has been notified. Please try again in a moment."
- **AC10**: Given the command handler throws UnauthorizedAccessException (e.g., attendee tries to start stage), When I receive the response, Then I see "You don't have permission to perform this action"
- **AC11**: Given meeting query returns null (invalid meeting GUID), When I access the link, Then Blazor component shows "Meeting not found - This link may be expired or invalid"
- **AC12**: Given ASP.NET Core rate limiting middleware rejects request (429), When I receive the response, Then I see "Too many actions - Please wait X seconds before trying again" with countdown timer

### Data Conflict Resolution

- **AC13**: Given I edit a note while another update occurs, When command handler detects DbUpdateConcurrencyException, Then I see "This note was updated by someone else - Reload to see changes?" with option to reload or overwrite
- **AC14**: Given the facilitator starts Stage 3 while I'm answering a Stage 2 question, When stage transition SignalR event occurs, Then my answer command saves to Stage 2 in PostgreSQL (no data loss) and I see "Stage changed - Your answer was saved"
- **AC15**: Given I vote on a concern that was just deleted, When command handler queries PostgreSQL, Then I see "This concern was removed" and my vote command is rejected gracefully via validation

### Degraded Functionality Handling

- **AC16**: Given SignalR connection fails and long polling fallback activates, When I view the Blazor component, Then I see "Limited connectivity - Updates may be delayed" warning banner
- **AC17**: Given SignalR hub is unavailable, When I am a facilitator, Then stage commands still execute with manual refresh prompt in component: "Action saved - Attendees may need to refresh to see changes"
- **AC18**: Given the server is overloaded (high EF Core query latency), When command handlers take >5 seconds, Then I see "Server is experiencing high load - Your action is still processing" message with Blazor spinner

### State Recovery After Failures

- **AC19**: Given the .NET server crashes mid-meeting, When the server restarts and Blazor circuit reconnects, Then I receive the last persisted meeting state from PostgreSQL (stage, timer, concerns, questions)
- **AC20**: Given my browser crashes, When I reopen the attendee link, Then my session is restored via JSInterop from localStorage and Blazor circuit loads current meeting state from server
- **AC21**: Given the facilitator's Blazor circuit drops for 60 seconds, When they reconnect, Then they see "Reconnected - Meeting state restored" and retain facilitator controls validated against PostgreSQL

### Error Reporting & Monitoring

- **AC22**: Given a critical error occurs (unexpected exception in command handler), When the error is caught by MediatR pipeline behavior, Then error details are logged to Serilog/Application Insights with: error message, stack trace, user action, meeting GUID, timestamp
- **AC23**: Given I am a facilitator, When multiple attendees experience SignalR disconnections (>30% of connected circuits), Then I see an alert "Some attendees may be experiencing technical issues" in component
- **AC24**: Given errors are occurring, When Blazor component displays error messages, Then they include a unique error ID (e.g., "Error ID: ERR-{guid}") for support reference

## Out of Scope

- Automatic error recovery without user awareness (all failures are surfaced)
- Offline mode with full functionality (read-only offline access only)
- Advanced conflict resolution (e.g., operational transformation for concurrent edits)
- Facilitator-initiated attendee diagnostics (e.g., ping test, bandwidth check)
- Undo/redo functionality for user actions
- Error analytics dashboard (monitoring is out of scope for v1)

## Edge Cases & Risks

### Critical Risks

1. **Silent failures**: What if an error occurs but no error message is shown?
   - Risk: User assumes action succeeded, data is lost, meeting state diverges
   - Mitigation: Blazor ErrorBoundary component catches all uncaught exceptions, shows generic error message

2. **Error message fatigue**: What if too many errors cause users to ignore all warnings?
   - Risk: Users dismiss critical errors (e.g., data loss warnings), make destructive choices
   - Mitigation: Error severity levels (info, warning, critical), only critical errors block actions via Blazor component state

3. **Race condition on retry**: What if Polly automatic retry creates duplicate actions (e.g., two concerns raised)?
   - Risk: Spam, confused facilitator, poor user experience
   - Mitigation: Idempotency keys on all MediatR commands, PostgreSQL unique constraints deduplicate within transaction

4. **Cascading failures**: What if one component's error triggers errors in dependent components?
   - Risk: Error message avalanche, application becomes unusable
   - Mitigation: Circuit breaker pattern via Polly, disable dependent Blazor components when upstream fails

### Edge Cases

5. **Error during error reporting**: What if sending error report to server also fails?
   - Behavior: Log error to browser console, do not show nested error message to user (fail silently on error reporting)

6. **Multiple simultaneous errors**: What if network fails, and user submits 3 actions in quick succession?
   - Behavior: Show single "Connection lost" error, queue actions for retry when connection restores

7. **Stale data warning during recovery**: What if user's cached state is 10 minutes old after reconnection?
   - Behavior: Show "Your view was out of date - Refreshed to current state" with timestamp of last sync

8. **Browser back button after error**: What if user hits back button while error message is displayed?
   - Behavior: Error message dismisses, no navigation occurs (browser history not polluted by error states)

9. **Error on meeting end**: What if facilitator tries to end meeting but server fails?
   - Behavior: Show error "Could not end meeting - Please try again", meeting remains active until confirmed

10. **Partial data load failure**: What if meeting loads but concerns fail to fetch?
    - Behavior: Show main interface, display "Could not load concerns" in concerns panel with "Retry" button, other features functional

## UI/UX Requirements

### Error Message Display

1. **Toast Notifications** (non-blocking, auto-dismiss):
   - **Info**: Blue background, informational icon, auto-dismiss after 5 seconds
   - **Success**: Green background, checkmark icon, auto-dismiss after 3 seconds
   - **Warning**: Yellow background, warning icon, auto-dismiss after 7 seconds (dismissible)
   - **Error**: Red background, error icon, requires manual dismissal

2. **Inline Validation Errors**:
   - Display below input field in red text
   - Show error icon next to invalid field
   - Error persists until field is corrected

3. **Modal Error Dialogs** (blocking, require action):
   - Used for critical errors (data loss risk, permission errors)
   - Primary action: "Retry" or "Reload"
   - Secondary action: "Cancel" or "Dismiss"
   - Include error ID for support reference

4. **Banner Warnings** (persistent, dismissible):
   - Used for system-wide issues (connection degraded, server overloaded)
   - Display at top of interface
   - Dismissible but reappears if issue persists

### Error Message Language

- **Avoid Technical Jargon**: "Connection lost" not "WebSocket disconnected"
- **Actionable Guidance**: "Please refresh the page" not just "An error occurred"
- **Empathy**: "We're sorry - something went wrong" not "Error: null reference exception"
- **Time Estimates**: "Retrying in 5 seconds..." not "Retrying..."

### Loading & Processing States

- **Action in Progress**: Show spinner with text "Submitting..." on button (button disabled)
- **Page Loading**: Show skeleton screens for agenda, not blank page or spinner
- **Slow Actions** (>3 seconds): Show "This is taking longer than expected..." message after 3-second threshold

## Dependencies

### Internal Features

- **All Features**: Every interactive feature must handle errors
- **Real-Time Sync**: Error handling for connection failures, message delivery failures
- **Meeting Creation**: Error handling for link generation, storage failures

### External Systems

- **Error Tracking Service** (optional): Sentry, Rollbar, or equivalent for server-side error aggregation
- **Browser Console Logging**: All errors logged to console for debugging
- **Server Logging**: All errors logged server-side with request context

## Technical Requirements

### Error Classification

**Client-Side Errors:**
- **Validation Errors** (4xx client): User input invalid, caught before submission
- **Network Errors**: Timeout, DNS failure, connection refused
- **State Errors**: Stale data, race conditions, conflicts

**Server-Side Errors:**
- **Authorization Errors** (403): Attendee tries facilitator action
- **Not Found Errors** (404): Invalid meeting ID, expired link
- **Rate Limit Errors** (429): Too many requests from single client
- **Server Errors** (500): Unhandled exceptions, database failures

### Retry Strategy

**Exponential Backoff:**
- Attempt 1: Immediate retry (0ms delay)
- Attempt 2: 1-second delay
- Attempt 3: 2-second delay
- After 3 failures: Manual retry required

**Idempotency:**
- All mutating actions include idempotency key (UUID)
- Server deduplicates actions with same key within 5-minute window
- Idempotency key stored in localStorage for retry persistence

### Circuit Breaker Pattern

**State Machine:**
- **Closed**: Normal operation, all requests go through
- **Open**: After 5 consecutive failures, block all requests for 30 seconds
- **Half-Open**: After 30 seconds, allow 1 test request, return to Closed if successful, Open if failed

**Applied To:**
- Concern submission (prevent spam on server errors)
- Question response submission
- Note creation

### Error Reporting Schema

```json
{
  "error_id": "ERR-2026-0118-1234",
  "timestamp": 1705601234567,
  "meeting_id": "abc123",
  "session_id": "uuid",
  "user_role": "attendee" | "facilitator",
  "error_type": "network" | "validation" | "server" | "unknown",
  "action": "raise_concern" | "answer_question" | ...,
  "error_message": "Connection timeout",
  "stack_trace": "...",
  "browser": "Chrome 120",
  "url": "https://..."
}
```

## Non-Functional Requirements

### Reliability

- **Error Detection**: All network errors caught within 5 seconds (timeout)
- **Error Recovery**: 95% of transient errors resolve within 3 automatic retries
- **Data Loss Prevention**: No silent data loss (all failures surfaced to user)

### Usability

- **Error Clarity**: 90% of users understand error message without support contact (usability testing)
- **Recovery Path**: Every error message provides next step (retry, refresh, contact support)
- **No Dead Ends**: No error state leaves user unable to proceed (always offer escape hatch)

### Performance

- **Error Display Latency**: Error message appears within 500ms of failure detection
- **Retry Overhead**: Automatic retry adds <3 seconds total delay for successful recovery
- **Error Logging Impact**: Error reporting does not block user actions (async fire-and-forget)

## Testing Scenarios

### Simulation Tests

1. **Network Timeout Simulation**: Set network delay to 10 seconds → Submit concern → See "Connection timeout" error → See automatic retry
2. **Server Error Simulation**: Mock 500 response → Start stage → See "Something went wrong" error → Manual retry succeeds
3. **Offline Mode Test**: Disable network → Raise concern → See "You are offline" message → Enable network → See action complete

### Validation Tests

1. **Empty Stage Name**: Leave stage name blank → Try to save → See "Stage name is required" inline error
2. **Excessive Character Limit**: Type 200-character stage name → See text truncated at 100 characters with counter
3. **Invalid Duration**: Set stage duration to 0 → See "Duration must be at least 1 minute" error

### Recovery Tests

1. **Browser Crash Recovery**: Open meeting → Create note → Force-close browser → Reopen link → See note persisted
2. **Server Restart Recovery**: Active meeting → Restart server → Attendees auto-reconnect → Meeting state intact
3. **Stale Data Recovery**: Disconnect for 5 minutes → Reconnect → See "Refreshed to current state" message → See up-to-date agenda

### Stress Tests

1. **Rapid Action Spam**: Click "Raise Concern" 20 times in 2 seconds → See rate limiting error → Only 1 concern created
2. **Concurrent Conflicts**: Two facilitators transition stages simultaneously → Last action wins → Both see consistent result
3. **Error Avalanche**: Trigger 10 different errors in 5 seconds → See only most recent error message (no stack-up)

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-18  
**Status**: Draft - Critical Gap Analysis
