# Product Requirements Document: Facilitation Assistant

## Executive Summary

Facilitation Assistant is a web-based meeting management tool that enables facilitators to run structured, time-boxed meetings with real-time attendee engagement. The application operates without mandatory authentication, using unique link-based access control to separate facilitator and attendee capabilities.

## Problem Statement

Online meetings lack structure, timing awareness, and real-time feedback mechanisms. Facilitators struggle to keep meetings on track while attendees lose context about meeting progress and have limited channels to provide input without disrupting the flow. Existing tools require account creation, complex setup, or lack real-time synchronization between facilitator and attendees.

## Target Users

### Primary Personas

1. **Meeting Facilitator** (Primary Actor)
   - Runs structured meetings (retrospectives, workshops, planning sessions)
   - Needs to control meeting flow, gather input, and maintain time discipline
   - May be external consultant or internal team lead
   - Expects minimal setup friction

2. **Meeting Attendee** (Participant)
   - Joins meetings via shared link (no account required)
   - Needs to know current agenda status and time remaining
   - Wants to provide feedback without verbally interrupting
   - May join from various devices/browsers

### Secondary Personas

3. **Anonymous Drop-In Attendee**
   - Joins meeting late or leaves early
   - Limited context about meeting purpose
   - Needs immediate orientation to current stage

## Core Value Propositions

- **Zero-Friction Entry**: No login required for attendees
- **Time Transparency**: All participants see real-time progress and overruns
- **Structured Feedback**: Attendees provide input through polls, reactions, and concerns without interrupting
- **Facilitator Control**: Single source of truth for meeting state, manually controlled by facilitator
- **Post-Meeting Persistence**: Meeting summary and notes available after session ends

## Business Goals

1. Enable effective remote facilitation for teams of 5-50 participants
2. Reduce meeting overruns by making time visible to all participants
3. Increase attendee engagement through low-friction feedback mechanisms
4. Support future live/hybrid meeting scenarios

## Success Metrics

- Meeting completion rate (% of agendas completed within planned time)
- Attendee feedback submission rate (% of attendees who interact with polls/concerns)
- Link sharing success (% of attendees who successfully join without errors)
- Post-meeting summary access rate

## High-Level Functional Requirements

### Core Capabilities

1. **Meeting Generation**
   - Create meeting with unique facilitator and attendee links
   - Define agenda with time-boxed stages
   - No authentication required

2. **Real-Time Agenda Display**
   - Visual representation of all stages
   - Current stage highlighting with progress bar
   - Time remaining/overrun indicators
   - Overall meeting progress tracker

3. **Stage Management**
   - Facilitator-controlled stage transitions
   - Ability to skip, revisit, or extend stages
   - Independent overall meeting timer

4. **Attendee Input Collection**
   - Predefined question polls (per-meeting, per-stage, ad-hoc)
   - Free-text responses
   - Template question repository
   - Configurable result visibility

5. **Concerns & Feedback System**
   - Attendees raise concerns (fixed options + custom text)
   - Real-time visibility to facilitator
   - Peer voting (like/dislike/neutral)
   - Facilitator acknowledgment and response

6. **Messaging & Communication**
   - Facilitator broadcasts messages to all attendees
   - Messages appear as notifications
   - Attendees respond via reactions/text/polls
   - Message history accessible during meeting

7. **Notes Management**
   - Per-stage and per-meeting notes
   - Public and private note visibility
   - Attendee note-taking capability
   - Pre-meeting note preparation

8. **Post-Meeting Summary**
   - Persistent link access to meeting results
   - Notes aggregation
   - Poll results
   - Exportable/viewable format

## Non-Functional Requirements

### Performance
- Real-time synchronization latency < 2 seconds
- Support 50 concurrent attendees per meeting
- Page load time < 3 seconds on standard broadband

### Reliability
- Meeting state must persist if facilitator/attendees disconnect
- Auto-reconnection on network interruption
- No data loss on browser refresh

### Usability
- Responsive design (desktop, tablet, mobile)
- Keyboard navigation support
- Clear visual hierarchy for time-critical information
- Consistent with modern web UX patterns

### Security
- Unique, non-guessable meeting links
- Separation of facilitator and attendee permissions
- No sensitive data storage (optional anonymous usage)
- Rate limiting on concern/message submission
- **(See `cross-cutting-concerns/security.md` for Link Regeneration and Token specs)**

### Accessibility
- WCAG 2.1 AA compliance
- Screen reader compatibility for agenda and timing
- High contrast mode support
- **(See `cross-cutting-concerns/accessibility.md` for WCAG/ARIA implementation details)**

## Out of Scope (v1.0)

- Video/audio conferencing integration
- User accounts and authentication
- Meeting templates or saved agendas
- Facilitator collaboration (multiple facilitators per meeting)
- Breakout rooms or sub-groups
- Calendar integration
- Meeting recording or transcription
- Mobile native applications
- Real-time chat between attendees
- File uploads or screen sharing
- Integration with third-party tools (Slack, Teams, etc.)
- Analytics dashboard or historical meeting trends

## Technical Constraints

- Must work in modern browsers (Chrome, Firefox, Safari, Edge - last 2 versions)
- No browser plugins or extensions required
- Graceful degradation for older browsers
- Mobile browser support (iOS Safari, Android Chrome)

## Risk Assessment

### High-Priority Risks

1. **Real-time synchronization failures** → Attendees see stale data
   - Mitigation: WebSocket fallback to polling, visible sync status indicator

2. **Link sharing security** → Unauthorized attendee access, link leakage
   - Mitigation: Time-limited links, facilitator can end meeting, no sensitive defaults

3. **Facilitator device failure** → Meeting becomes uncontrollable
   - Mitigation: Facilitator link reusable from different device, meeting state persists

4. **Overwhelming concern notifications** → Facilitator distraction
   - Mitigation: Collapsible concern panel, digest view, mute option

5. **Time zone confusion** → Meeting timing displayed incorrectly
   - Mitigation: Use relative timers (countdown), not absolute timestamps

### Medium-Priority Risks

6. **Browser compatibility issues** → Features break on specific browsers
7. **Network latency** → Rural/low-bandwidth attendees experience delays
8. **Attendee confusion on first use** → High drop-off rate
9. **Poll fatigue** → Attendees ignore questions if overused
10. **Note visibility misconfiguration** → Accidental exposure of private notes

## Dependencies

- **Real-time communication infrastructure**: WebSocket or equivalent
- **Unique ID generation**: For meeting links
- **Client-side time synchronization**: For accurate countdowns
- **Browser notification API**: For facilitator messages (optional enhancement)

## Future Enhancements (Post-v1.0)

- Meeting templates and agenda presets
- Live meeting support (physical room display)
- Facilitator co-host capability
- Attendee role assignment (observer vs participant)
- Meeting analytics and trend reports
- Integration with calendar systems
- AI-powered meeting insights (topic clustering from notes)

## Approval & Sign-Off

_This section to be completed by stakeholders_

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-17  
**Owner**: Product Team  
**Status**: Draft - Pending Review
