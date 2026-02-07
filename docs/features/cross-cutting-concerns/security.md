# Cross-Cutting Concern: Security & Access Control

## Overview
While the Facilitation Assistant avoids user accounts, it handles meeting state management which requires a robust authorization model based on "Capability URLs".

## Security Model

### 1. Capability URLs (Access Tokens)
- **Facilitator Link**: Contains a high-entropy secret (e.g., `/meeting/m-{guid}/admin/{secret_token}`). Grants Write access to meeting state.
- **Attendee Link**: Contains only the public ID (e.g., `/meeting/m-{guid}`). Grants Read-Only access + Write access to own session data.

### 2. Threat Model: Link Hijacking
**Risk**: Facilitator link is leaked.
**Mitigation Strategy**:
- **Link Regeneration**:
  - Facilitator can click "Regenerate Admin Link" in settings.
  - Server invalidates the old `secret_token` in PostgreSQL immediately via command handler.
  - Server issues a new `secret_token` (cryptographically secure GUID).
  - Active SignalR connections using the old token are forcibly disconnected via hub notification.
- **Read-Once/Obfuscation**:
  - The admin link is masked in Blazor component UI ("••••••••") until revealed by click.
  - Clipboard copy action includes a warning via JSInterop.

### 3. Rate Limiting & Abuse Prevention
- **Middleware Rate Limiting**: Max 60 requests/minute per IP using ASP.NET Core rate limiting middleware
- **Resource Creation Limits** (enforced in MediatR command validators):
  - Max 50 concerns per meeting.
  - Max 100 notes per meeting.
- **Input Sanitization**:
  - All user inputs (Display Name, Note content, Question answers) must be sanitized using `HtmlEncoder` in validation pipeline before persisting to PostgreSQL.

## Implementation Requirements
- Use `Guid.NewGuid()` with cryptographically secure random number generator in .NET for token generation.
- Tokens must be at least 32 characters alphanumeric (GUID is 36 characters with hyphens).
- All SignalR hub invocations must validate the token payload against PostgreSQL via scoped service.
- Store secret tokens hashed in PostgreSQL using ASP.NET Core Data Protection API.
