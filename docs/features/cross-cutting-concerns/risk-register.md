# Risk Register & Mitigation Strategy

## Purpose

This document catalogs all identified risks across the Facilitation Assistant application, prioritized by severity and likelihood, with concrete mitigation strategies.

---

## Risk Assessment Matrix

**Severity Scale:**
- **Critical**: System unusable, data loss, security breach
- **High**: Major feature broken, poor UX, user abandonment
- **Medium**: Feature degradation, workarounds available
- **Low**: Minor inconvenience, cosmetic issues

**Likelihood Scale:**
- **Very High**: >70% chance of occurring
- **High**: 40-70% chance
- **Medium**: 15-40% chance
- **Low**: <15% chance

**Priority**: Severity × Likelihood (Critical/Very High = P0, Low/Low = P4)

---

## P0: Critical Priority Risks

### R-001: Facilitator Link Hijacking
- **Category**: Security
- **Severity**: Critical
- **Likelihood**: High (50%)
- **Description**: Facilitator accidentally shares private link publicly; attacker gains control of meeting
- **Impact**: Unauthorized control, meeting disruption, data tampering, reputational damage
- **Mitigation**:
  1. Visual distinction: Red color-coded link, "⚠️ Private - Do Not Share" warning label
  2. Confirmation dialog when copying facilitator link: "This link gives full control - Share carefully"
  3. Facilitator can regenerate link (invalidates old one) - **NEW REQUIREMENT**
  4. Access logs: Show "Last accessed from IP X" - **NEW REQUIREMENT**
  5. Documentation: Onboarding tutorial emphasizes link security
- **Residual Risk**: Medium (even with mitigations, user error possible)
- **Owner**: Product Security + UX Team

### R-002: Real-Time Sync Failure During Critical Moment
- **Category**: Technical
- **Severity**: Critical
- **Likelihood**: Medium (30%)
- **Description**: WebSocket connection drops during stage transition; attendees desync, see stale data
- **Impact**: Attendees confused about meeting status, data loss (late question responses), meeting chaos
- **Mitigation**:
  1. Aggressive reconnection strategy (1s, 2s, 5s, 10s retries)
  2. Full state snapshot on reconnect (not incremental updates)
  3. Visible sync status indicator (green/yellow/red dot)
  4. Polling fallback when WebSocket unavailable
  5. Disable facilitator controls when disconnected (prevent split-brain)
- **Residual Risk**: Low (with robust fallback)
- **Owner**: Backend Team

### R-003: Clock Skew Between Clients
- **Category**: Technical
- **Severity**: High
- **Likelihood**: High (60%)
- **Description**: Client's system clock is wrong; timers show incorrect remaining time, countdowns desync
- **Impact**: Trust undermined, attendees don't know when stage ends, timing discipline fails
- **Mitigation**:
  1. Server-authoritative timestamps: Server sends meeting start time
  2. Client calculates elapsed time: `elapsed = Date.now() - serverStartTime + serverTimeOffset`
  3. NTP-style time sync: Heartbeat messages include server timestamp, client calculates offset
  4. Display relative time ("5m 30s remaining") not absolute ("Ends at 2:45 PM")
  5. Alert facilitator if client clocks diverge >5 seconds
- **Residual Risk**: Low (server-authoritative time eliminates issue)
- **Owner**: Frontend + Backend Team

### R-004: Data Loss on Server Crash
- **Category**: Reliability
- **Severity**: Critical
- **Likelihood**: Medium (25%)
- **Description**: Server crashes mid-meeting; in-memory state lost, meeting cannot continue
- **Impact**: Meeting destroyed, user frustration, data loss (concerns, notes, responses)
- **Mitigation**:
  1. Persistent storage for all state changes (PostgreSQL, Redis with persistence)
  2. Write-ahead logging (WAL) for state mutations
  3. Auto-save every action (no batch commits)
  4. Server restart recovery: Clients reconnect, pull last state from DB
  5. Load balancing: Multiple server instances, stateless design (state in DB, not memory)
- **Residual Risk**: Low (with proper architecture)
- **Owner**: Infrastructure Team

---

## P1: High Priority Risks

### R-005: Concern Spam Overwhelms Facilitator
- **Category**: User Behavior
- **Severity**: High
- **Likelihood**: High (50%)
- **Description**: Attendees spam concerns (trolling or legitimate); facilitator panel unusable
- **Impact**: Facilitator ignores all concerns, feature becomes useless
- **Mitigation**:
  1. Rate limiting: 1 concern per attendee per 2 minutes
  2. Facilitator can hide/dismiss concerns (not delete, for audit)
  3. Collapsible concerns panel (facilitator can minimize to focus)
  4. Digest view: Group concerns by type ("5 attendees flagged 'Topic Unclear'")
  5. Mute option: Facilitator can disable concern notifications temporarily
- **Residual Risk**: Medium (user behavior unpredictable)
- **Owner**: Product Team

### R-006: Question Fatigue Leads to Low Response Rates
- **Category**: UX
- **Severity**: High
- **Likelihood**: High (60%)
- **Description**: Facilitator asks too many questions; attendees skip or ignore, data quality poor
- **Impact**: Invalid data collection, facilitator misled by low sample size
- **Mitigation**:
  1. Show response rate: "5/12 attendees answered (42%)"
  2. Warn facilitator: "3 questions pending - Consider waiting for responses"
  3. Limit active question queue to 3 simultaneously
  4. Question priority levels: "Optional" vs "Required" (required blocks stage progression)
  5. Auto-close questions after 5 minutes if no engagement
- **Residual Risk**: Medium (design can reduce but not eliminate fatigue)
- **Owner**: Product Team

### R-007: Private Note Accidentally Made Public
- **Category**: Privacy
- **Severity**: High
- **Likelihood**: Medium (35%)
- **Description**: Facilitator toggles note from private to public without realizing; confidential info exposed
- **Impact**: Privacy breach, trust loss, potential legal issues (sensitive data exposure)
- **Mitigation**:
  1. Visual distinction: Private notes have red background/border, public notes green
  2. Confirmation dialog: "Make this note public? All attendees will see it. This cannot be undone."
  3. Default to private (secure by default)
  4. Audit log: Track visibility changes (timestamp, who changed)
  5. "Undo" option: 10-second window to revert visibility change
- **Residual Risk**: Low (with UI safeguards)
- **Owner**: UX + Privacy Team

### R-008: Mobile UX Breakdown
- **Category**: UX
- **Severity**: High
- **Likelihood**: High (55%)
- **Description**: On mobile, 10+ agenda stages create scroll chaos; attendees lose context
- **Impact**: Mobile users disengaged, poor meeting participation from 30%+ of users
- **Mitigation**:
  1. Collapsed view: Show only current stage + next 2, "Expand All" button
  2. Sticky "Current Stage" header at top
  3. Quick navigation: Tap top bar to jump to current stage
  4. Responsive design testing: iOS Safari, Android Chrome required
  5. Progressive disclosure: Hide completed stages by default
- **Residual Risk**: Medium (mobile constraints limit UX)
- **Owner**: Frontend Team

---

## P2: Medium Priority Risks

### R-009: Meeting Link Loss (User Closes Browser)
- **Category**: UX
- **Severity**: Medium
- **Likelihood**: Very High (80%)
- **Description**: Facilitator creates meeting, closes browser, loses both links, cannot recover
- **Impact**: Orphaned meetings, user frustration, repeat sign-ups, poor first impression
- **Mitigation**:
  1. Browser warning: "Are you sure? Meeting links will be lost if not saved"
  2. SessionStorage backup: Store last-created meeting for 24 hours
  3. Email delivery option: "Email me the links" (requires email input) - **NEW FEATURE**
  4. "Recover Meeting" feature: Enter email → receive links if match found - **NEW FEATURE**
  5. Tutorial: First-time users see "Save your links" instruction
- **Residual Risk**: Medium (user behavior difficult to control)
- **Owner**: Product Team

### R-010: Message Notification Fatigue
- **Category**: UX
- **Severity**: Medium
- **Likelihood**: High (50%)
- **Description**: Facilitator sends too many messages; attendees mute or ignore, miss critical updates
- **Impact**: Communication breakdown, important messages not seen
- **Mitigation**:
  1. Rate limiting: 5 messages per minute (hard limit)
  2. Read receipts: Facilitator sees "8/12 attendees viewed"
  3. Urgency levels: "Normal" (toast) vs "Urgent" (modal)
  4. Message preview: Attendees see first 50 chars, expand to read full
  5. Discourage overuse: Show warning "You've sent 5 messages in the last 10 minutes"
- **Residual Risk**: Low (rate limits enforce discipline)
- **Owner**: Product Team

### R-011: Late Joiner Disorientation
- **Category**: UX
- **Severity**: Medium
- **Likelihood**: High (70%)
- **Description**: Attendee joins 30 minutes into meeting; sees 20 historical messages, unclear context
- **Impact**: Confusion, attendee disengaged, asks questions already addressed
- **Mitigation**:
  1. Onboarding screen: "Meeting in progress - Currently on Stage 3 of 5"
  2. Historical message summary: "5 messages sent earlier" (collapsed by default)
  3. Current stage highlight: Auto-scroll to active stage on join
  4. "What You Missed" panel: Summary of completed stages, key poll results
  5. Facilitator notification: "New attendee joined" (can offer recap)
- **Residual Risk**: Low (good onboarding reduces confusion)
- **Owner**: UX Team

### R-012: Poll Results Bias (Early Concerns Get More Votes)
- **Category**: Data Quality
- **Severity**: Medium
- **Likelihood**: High (65%)
- **Description**: Concerns raised early get more votes than later ones; not reflective of true priority
- **Impact**: Facilitator prioritizes wrong issues, misreads attendee sentiment
- **Mitigation**:
  1. Sort by vote-to-time ratio: "votes per minute active" (recency-adjusted)
  2. Show timestamp: "(raised 5m ago)" to give context
  3. Facilitator can manually pin/prioritize concerns
  4. Default sort: "Most Recent" (not "Most Votes")
  5. Analytics: Track voting patterns, adjust algorithm post-launch
- **Residual Risk**: Medium (bias difficult to eliminate entirely)
- **Owner**: Data Science + Product Team

---

## P3: Lower Priority Risks

### R-013: Markdown/HTML Injection in User Content
- **Category**: Security
- **Severity**: Medium
- **Likelihood**: Medium (30%)
- **Description**: Attendee enters `<script>alert('XSS')</script>` in note or concern text
- **Impact**: XSS attack, session hijacking, data theft
- **Mitigation**:
  1. Content sanitization: Strip HTML tags, encode special characters
  2. Use DOMPurify library for sanitization
  3. Content Security Policy (CSP) headers to block inline scripts
  4. Validation: Reject input with `<script>`, `<iframe>`, `onclick=` patterns
  5. Security testing: Automated XSS penetration tests
- **Residual Risk**: Low (standard web security practices)
- **Owner**: Security Team

### R-014: Database Query Performance Degradation
- **Category**: Performance
- **Severity**: Medium
- **Likelihood**: Low (20%)
- **Description**: Fetching 100 concerns + 200 notes + 50 poll responses becomes slow (>3s)
- **Impact**: Slow page load, poor UX, user abandonment
- **Mitigation**:
  1. Database indexing: Index on meeting_id, session_id, created_at
  2. Pagination: Load 20 concerns at a time, "Load More" button
  3. Lazy loading: Fetch poll results on demand (not page load)
  4. Caching: Redis cache for active meeting state
  5. Query optimization: N+1 query detection, eager loading
- **Residual Risk**: Low (standard optimization techniques)
- **Owner**: Backend Team

### R-015: Browser Compatibility Issues
- **Category**: Technical
- **Severity**: Medium
- **Likelihood**: Medium (35%)
- **Description**: Feature breaks on Safari, or older Chrome versions; attendees locked out
- **Impact**: User frustration, support burden, accessibility barriers
- **Mitigation**:
  1. Browser support policy: Chrome/Firefox/Safari/Edge (last 2 versions)
  2. Polyfills: For WebSocket, localStorage, modern JS features
  3. Graceful degradation: Core features work without WebSocket (polling fallback)
  4. Browser detection: Warn users if unsupported browser detected
  5. Testing matrix: Automated tests on Selenium Grid (multiple browsers)
- **Residual Risk**: Low (with proper testing)
- **Owner**: QA Team

---

## P4: Low Priority Risks

### R-016: Meeting Runs for 12 Hours Without Cleanup
- **Category**: Operational
- **Severity**: Low
- **Likelihood**: Low (10%)
- **Description**: Facilitator forgets to end meeting; timer runs indefinitely
- **Impact**: Database clutter, abandoned meeting state
- **Mitigation**:
  1. Auto-end meeting after 48 hours of inactivity
  2. Facilitator receives email reminder: "Your meeting has been running for 8 hours"
  3. Soft limit: Warn facilitator after 6 hours "Meeting running long - End meeting?"
  4. Background cleanup job: Archive meetings inactive >48 hours
- **Residual Risk**: Negligible
- **Owner**: Backend Team

### R-017: Attendee Uses Gibberish Display Name
- **Category**: Data Quality
- **Severity**: Low
- **Likelihood**: Medium (40%)
- **Description**: Attendee sets name to "asdfjkl" or emoji spam
- **Impact**: Facilitator confused by unprofessional names, poor meeting culture
- **Mitigation**:
  1. Character validation: Alphanumeric + basic punctuation only
  2. Emoji stripping: Remove emoji characters
  3. Placeholder suggestion: "Leave blank for 'Attendee #X'"
  4. Facilitator can see session ID: "asdfjkl (Session #A12)" for reference
- **Residual Risk**: Negligible (minor inconvenience)
- **Owner**: Product Team

---

## Risk Monitoring & Review

### Review Cadence
- **Pre-Launch**: Weekly risk review during development
- **Post-Launch**: Monthly risk assessment for first 6 months
- **Ongoing**: Quarterly risk review, update register

### Metrics to Track
1. **R-001 (Link Hijacking)**: # of facilitator link regenerations, # of unauthorized access attempts
2. **R-002 (Sync Failure)**: Reconnection rate, avg reconnection time, % of users experiencing desync
3. **R-005 (Concern Spam)**: Avg concerns per meeting, % of meetings with >20 concerns
4. **R-006 (Question Fatigue)**: Avg response rate, % of questions with <50% response rate
5. **R-009 (Link Loss)**: % of meetings accessed <2 times (orphan rate)

### Escalation Path
- **P0 Risks Realized**: Immediate hotfix, post-mortem within 24 hours
- **P1 Risks Realized**: Patch within 48 hours, incident review
- **P2 Risks Realized**: Fix in next sprint, add to backlog
- **P3/P4 Risks Realized**: Log and monitor, fix when time permits

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-18  
**Status**: Draft - Comprehensive Risk Catalog
