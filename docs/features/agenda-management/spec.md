# Feature: Agenda Management & Real-Time Timing

## Problem Statement

Meeting participants lose track of time and current focus during online meetings. Facilitators need manual control over stage transitions while all participants must see synchronized progress, overruns, and remaining time to maintain discipline and awareness.

## User Stories

- **US1**: As a facilitator, I want to manually transition between agenda stages so that I control meeting flow based on actual discussion needs
- **US2**: As an attendee, I want to see all agenda stages at once so that I understand the full meeting structure
- **US3**: As an attendee, I want to see which stage is currently active so that I know what topic we're discussing
- **US4**: As an attendee, I want to see time remaining in the current stage so that I can prepare for transitions
- **US5**: As an attendee, I want to see when a stage is overrunning so that I understand why the meeting is delayed
- **US6**: As a facilitator, I want to skip stages or go back to previous stages so that I can adapt to meeting dynamics
- **US7**: As an attendee, I want to see overall meeting progress independent of stages so that I know when the meeting will actually end
- **US8**: As a facilitator, I want to extend stage duration on-the-fly so that I can accommodate important discussions
- **US9**: As an attendee, I want time displays to update in real-time so that I see accurate countdowns

## Acceptance Criteria

### Agenda Display

- **AC1**: Given a meeting is active, When I access the attendee link, Then I see all agenda stages displayed as tiles with: stage name, planned duration, stage order number
- **AC2**: Given multiple stages exist, When I view the agenda, Then stages are displayed in the order defined by the facilitator
- **AC3**: Given I am viewing the agenda, When no stage is active yet, Then the first stage is highlighted as "Next"
- **AC4**: Given a stage is active, When I view the agenda, Then the active stage is visually distinct (border, background color, "Active" label)

### Time Display - Current Stage

- **AC5**: Given a stage is active, When I view its tile, Then I see a progress bar showing elapsed time vs planned duration
- **AC6**: Given a stage is active and within planned duration, When I view time remaining, Then it shows format "Xm Ys remaining" (e.g., "5m 30s remaining")
- **AC7**: Given a stage has exceeded planned duration, When I view the tile, Then I see "Overrunning by Xm Ys" in warning color (e.g., red/orange)
- **AC8**: Given a stage is overrunning, When I view the progress bar, Then it extends beyond 100% with visual distinction (different color after 100% mark)
- **AC9**: Given I am viewing time displays, When time updates, Then it refreshes every 1 second without visible lag

### Overall Meeting Progress

- **AC10**: Given a meeting has started, When I view the interface, Then I see total meeting elapsed time in format "Total: Xh Ym" or "Total: Xm"
- **AC11**: Given the facilitator set total meeting duration, When I view overall progress, Then I see "Total: Xm of Ym" with overall progress bar
- **AC12**: Given the facilitator did not set total meeting duration, When I view overall progress, Then I see "Total: Xm elapsed" (open-ended)
- **AC13**: Given overall meeting exceeds planned duration, When I view overall progress, Then I see "Meeting overrunning by Xm" in warning color
- **AC14**: Given all stages are completed but total meeting duration not reached, When I view overall progress, Then it continues counting (total time independent of stages)

### Facilitator Stage Control

- **AC15**: Given I am a facilitator viewing the agenda, When I click "Start" on a stage, Then that stage becomes active and timer starts
- **AC16**: Given a stage is active, When I click "End Stage", Then the stage is marked complete and timer stops
- **AC17**: Given a stage is active, When I click "Start" on a different stage, Then the current stage ends and the new stage starts immediately
- **AC18**: Given a stage was previously completed, When I click "Restart" on it, Then it becomes active again with timer reset to 0
- **AC19**: Given stages exist, When I skip a stage by starting a later one, Then the skipped stage remains in "Not Started" status (visible to attendees)

### Stage Status Indicators

- **AC20**: Given a stage has not started, When I view it, Then it shows "Not Started" status (neutral color)
- **AC21**: Given a stage is currently active, When I view it, Then it shows "Active" status (primary color)
- **AC22**: Given a stage has completed, When I view it, Then it shows "Completed" status with actual duration taken (e.g., "Completed in 12m", green)
- **AC23**: Given a stage was skipped, When I view it, Then it shows "Skipped" status (gray, no duration)

### Real-Time Synchronization

- **AC24**: Given I am an attendee, When the facilitator starts a new stage, Then my display updates within 2 seconds to show the new active stage
- **AC25**: Given I am an attendee, When the facilitator extends a stage duration, Then my progress bar and remaining time update within 2 seconds
- **AC26**: Given multiple attendees are viewing, When the facilitator transitions stages, Then all attendees see the same active stage within 2 seconds

### Stage Duration Adjustment

- **AC27**: Given a stage is active, When the facilitator clicks "Add 5 Minutes", Then the planned duration increases by 5 minutes and progress bar adjusts
- **AC28**: Given a stage is active, When the facilitator adjusts duration, Then attendees see updated "time remaining" immediately
- **AC29**: Given a stage is overrunning, When the facilitator adds time, Then the "overrunning" warning disappears if new duration exceeds elapsed time

## Out of Scope

- Automatic stage transitions based on time
- Countdown timer with alarm/notification at stage end
- Stage duration templates or presets
- Percentage-based progress (e.g., "50% complete") - only time-based
- Stage dependencies (must complete X before starting Y)
- Parallel stages or concurrent tracks
- Stage time analytics or recommendations

## Edge Cases & Risks

### Critical Risks

1. **Clock skew between facilitator and attendees**: What if attendee's device clock is 5 minutes off?
   - Risk: Attendees see incorrect time remaining
   - Mitigation: Use server-side timestamps, send elapsed time (not target end time)

2. **Stage transition spam**: What if facilitator rapidly clicks through stages?
   - Risk: Attendees overwhelmed with updates, data loss
   - Mitigation: Debounce stage transition actions (1-second cooldown), queue updates

3. **Browser tab inactive**: What if attendee switches tabs and timers stop updating (browser throttling)?
   - Risk: Stale time displays, confusion on return
   - Mitigation: On tab focus, force refresh from server

4. **Facilitator disconnects mid-stage**: What if facilitator loses connection during active stage?
   - Risk: Stage timer continues but no one can end it
   - Mitigation: Stage continues, facilitator can reconnect and control

### Edge Cases

5. **Zero stages completed**: What if facilitator never starts any stage?
   - Behavior: Overall timer still runs, all stages show "Not Started"

6. **All stages completed early**: What if all stages done in 30min but meeting planned for 60min?
   - Behavior: Overall timer continues, facilitator can manually end meeting

7. **Extremely long stage**: What if a stage runs for 3 hours?
   - Behavior: Support up to 24-hour display, show "Xh Ym" format

8. **Negative time remaining**: What if clock adjustment causes negative time?
   - Behavior: Treat as overrunning, show "Overrunning by 0m" minimum

9. **Rapid stage switching**: What if facilitator goes Stage 1 → Stage 3 → Stage 1 → Stage 2?
   - Behavior: Each transition logged, last active stage is current, previous completions preserved

10. **Stage completed instantly**: What if facilitator starts and immediately ends a stage?
    - Behavior: Show "Completed in <1m" or "Completed in 0m"

## UI/UX Requirements

### Agenda Display Layout

1. **Stage Tiles**:
   - Display as vertical list or horizontal timeline (responsive)
   - Each tile contains: Stage number, Name, Planned duration, Status, Progress bar (if active/completed)
   - Visual hierarchy: Active stage largest/most prominent

2. **Current Stage Emphasis**:
   - Active stage: Bold border, elevated shadow, pulse animation (subtle)
   - Completed stages: Muted colors, checkmark icon
   - Not started: Neutral gray

3. **Progress Bars**:
   - Active stage: Animated fill from left to right
   - Color transitions: Green (0-80%) → Yellow (80-100%) → Red (>100% overrun)
   - Width: Full width of tile

4. **Overall Meeting Progress**:
   - Sticky header or footer (always visible)
   - Format: "Total Meeting: 45m of 60m" + progress bar
   - If overrunning: "Meeting overrunning by 5m" (red background)

### Facilitator Controls

1. **Stage Action Buttons** (visible only to facilitator):
   - "Start" button on inactive stages
   - "End Stage" button on active stage
   - "Add 5 Minutes" button on active stage
   - "Restart" button on completed stages

2. **Visual Feedback**:
   - Button clicks show loading spinner for 0.5s
   - Stage transitions show brief animation (fade in/out)

3. **Confirmation Dialogs**:
   - No confirmation for stage start/end (fast operation)
   - Confirmation if facilitator tries to end meeting with active stage

### Validation Rules

- **Stage Transition**: Cannot start a stage if no meeting is active
- **Duration Adjustment**: Cannot reduce duration below elapsed time (only add time)
- **Time Display**: Show "0m" minimum, never negative values
- **Stage Restart**: Cannot restart a stage that is currently active (must end first)

### Interaction Patterns

- **Hover State**: Stage tiles show facilitator controls on hover (if facilitator)
- **Click Stage Tile**: No action for attendees, facilitator sees quick actions menu
- **Progress Bar Animation**: Smooth fill, no jank on 1-second updates
- **Mobile Touch**: Stage tiles scrollable horizontally (swipe)

## Dependencies

### Internal Features

- **Meeting Creation**: Provides initial agenda structure and planned durations
- **Real-Time Sync**: WebSocket connection for stage transition broadcasts
- **Notes System**: Notes are associated with specific stages
- **Polling System**: Questions can be triggered at stage start

### External Systems

- **Server Time Sync**: Authoritative clock for elapsed time calculations
- **WebSocket/Polling**: For real-time stage updates to attendees

## Data Model (Conceptual)

```
MeetingState {
  current_stage_id: uuid | null
  current_stage_started_at: timestamp | null
  meeting_started_at: timestamp
  meeting_ended_at: timestamp | null
}

StageExecution {
  stage_id: foreign_key
  started_at: timestamp | null
  ended_at: timestamp | null
  status: 'not_started' | 'active' | 'completed' | 'skipped'
  actual_duration_seconds: integer | null
  adjusted_planned_duration_minutes: integer (can differ from original)
}
```

## Non-Functional Requirements

### Performance

- Time display updates every 1 second
- Stage transition broadcast to all attendees < 2 seconds
- Progress bar animation at 60fps (smooth)

### Reliability

- Timer continues if facilitator disconnects (stored on server)
- Attendees auto-reconnect if disconnected (see current stage state)
- No timer drift over 2-hour meetings (server authoritative)

### Usability

- Time formats consistent (prefer "Xm Ys" over "X:Y" for clarity)
- Color-blind safe progress indicators (not just color, also patterns/labels)
- Large touch targets for mobile facilitators (48x48px minimum)

## Testing Scenarios

### Happy Path

1. Facilitator starts Stage 1 → Attendees see active stage with countdown → Stage completes in planned time
2. Facilitator starts Stage 2 → Lets it overrun by 3 minutes → Attendees see red "Overrunning by 3m"
3. Facilitator adds 5 minutes to overrunning stage → Warning clears → Stage completes in adjusted time

### Complex Flows

1. Facilitator skips Stage 2 → Starts Stage 3 → Goes back to Stage 2 → Both show correct status
2. All stages complete in 40min → Overall meeting continues to 60min → Meeting still active
3. Attendee joins mid-meeting → Sees current active stage, previous completed, future not started

### Edge Cases

1. Stage runs for 120 minutes → Display shows "2h 0m"
2. Facilitator starts stage, immediately ends → Shows "Completed in <1m"
3. Attendee browser tab inactive for 10 minutes → Returns → Display updates to current time

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-17  
**Status**: Draft - Pending Review
