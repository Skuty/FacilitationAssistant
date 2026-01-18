## Plan: Facilitation Assistant Implementation Roadmap

This plan outlines the development path for the Facilitation Assistant, assuming a manually established 3-layer Clean Architecture (Core, Infrastructure, Web). The focus is on establishing the Domain model and Real-time synchronization patterns first, utilizing **MudBlazor** for the UI and **SQLite** for persistence.

### Steps
1.  **Core & Infrastructure Wiring**: Configure `AppDbContext` with **SQLite**, setup `MediatR` pipelines, and define base `AggregateRoot` abstractions within the pre-existing `Core` and `Infrastructure` projects.
2.  **UI Foundation & Meeting Identity**: Install **MudBlazor** (Layouts/Providers). Implement the `Meeting` aggregate, `CreateMeeting` command, and unique URL generation (`/m/{id}`) with basic `MeetingHub` group management.
3.  **Agenda & Time Sync**: Implement `AgendaStage` domain logic and the "Push State" strategy (UTC start times) for synchronized timers across clients.
4.  **Interaction Modules**: Build `Poll`, `Message` (Q&A/Announcements), and `Concern` aggregates. Update `MeetingHub` to broadcast these specific domain events.
5.  **Private State & Summary**: Implement `Notes` system using a "Pull" strategy (separate queries for private data) and the read-only `MeetingSummary` view.

### Further Considerations
1.  **MudBlazor Setup**: Ensure `MudThemeProvider`, `MudDialogProvider`, and `MudSnackbarProvider` are registered in the main layout immediately to support feature development.
2.  **SQLite Constraints**: Ensure `DbContextFactory` is strictly used for all command handlers to avoid threading issues with SQLite in a Server-side Blazor environment.
