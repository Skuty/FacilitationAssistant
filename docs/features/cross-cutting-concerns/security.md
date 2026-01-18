# Cross-Cutting Concern: Security & Access Control

## Overview
While the Facilitation Assistant avoids user accounts, it handles meeting state management which requires a robust authorization model based on "Capability URLs".

## Security Model

### 1. Capability URLs (Access Tokens)
- **Facilitator Link**: Contains a high-entropy secret (e.g., `/meeting/m-{uuid}/admin/{secret_token}`). Grants Write access to meeting state.
- **Attendee Link**: Contains only the public ID (e.g., `/meeting/m-{uuid}`). Grants Read-Only access + Write access to own session data.

### 2. Threat Model: Link Hijacking
**Risk**: Facilitator link is leaked.
**Mitigation Strategy**:
- **Link Regeneration**:
  - Facilitator can click "Regenerate Admin Link" in settings.
  - Server invalidates the old `secret_token` immediately.
  - Server issues a new `secret_token`.
  - Active WebSocket sessions using the old token are forcibly disconnected.
- **Read-Once/Obfuscation**:
  - The admin link is masked in the UI ("••••••••") until revealed by click.
  - Clipboard copy action includes a warning.

### 3. Rate Limiting & Abuse Prevention
- **API Rate Limiting**: Max 60 requests/minute per IP.
- **Resource Creation Limits**:
  - Max 50 concerns per meeting.
  - Max 100 notes per meeting.
- **Input Sanitization**:
  - All user inputs (Display Name, Note content, Question answers) must be stripped of HTML tags/scripts on the Server before broadcast.

## Implementation Requirements
- Use `crypto.randomUUID()` or server-side CSPRNG for token generation.
- Tokens must be at least 32 characters alphanumeric.
- All WebSocket messages must validate the token payload.
