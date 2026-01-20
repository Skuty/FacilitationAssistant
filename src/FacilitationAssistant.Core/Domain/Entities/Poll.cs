using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Domain.Enums;

namespace FacilitationAssistant.Core.Domain.Entities;

/// <summary>
/// Represents a poll for gathering votes from attendees.
/// </summary>
public class Poll : Entity
{
    public required string Question { get; set; }
    public Guid? StageId { get; init; }
    public PollStatus Status { get; set; } = PollStatus.Draft;
    public bool AllowMultipleVotes { get; init; }
    public bool ShowResultsBeforeClose { get; init; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    private readonly List<PollOption> _options = new();
    public IReadOnlyCollection<PollOption> Options => _options.AsReadOnly();

    private readonly List<Vote> _votes = new();
    public IReadOnlyCollection<Vote> Votes => _votes.AsReadOnly();

    public void AddOption(string text, int sortOrder)
    {
        if (Status != PollStatus.Draft)
            throw new InvalidOperationException("Cannot add options to a poll that is not in draft status");

        _options.Add(new PollOption
        {
            Text = text,
            SortOrder = sortOrder
        });
    }

    public void Open(DateTime openTime)
    {
        if (Status != PollStatus.Draft)
            throw new InvalidOperationException("Poll can only be opened from draft status");

        Status = PollStatus.Active;
        OpenedAt = openTime;
    }

    public void Close(DateTime closeTime)
    {
        if (Status != PollStatus.Active)
            throw new InvalidOperationException("Poll can only be closed when active");

        Status = PollStatus.Closed;
        ClosedAt = closeTime;
    }

    public void AddVote(string voterSessionId, Guid optionId)
    {
        if (Status != PollStatus.Active)
            throw new InvalidOperationException("Cannot vote on inactive poll");

        var option = _options.FirstOrDefault(o => o.Id == optionId);
        if (option == null)
            throw new ArgumentException("Invalid option ID", nameof(optionId));

        if (!AllowMultipleVotes && _votes.Any(v => v.VoterSessionId == voterSessionId))
            throw new InvalidOperationException("Multiple votes not allowed");

        _votes.Add(new Vote
        {
            VoterSessionId = voterSessionId,
            OptionId = optionId
        });
    }
}
