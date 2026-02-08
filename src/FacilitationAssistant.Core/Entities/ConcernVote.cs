namespace FacilitationAssistant.Core.Entities;

public enum VoteType
{
    Like,
    Dislike,
    Neutral
}

public class ConcernVote
{
    public Guid Id { get; set; }
    public Guid ConcernId { get; set; }
    public Concern Concern { get; set; } = null!;
    
    public string SessionId { get; set; } = string.Empty;
    public VoteType VoteType { get; set; }
    public DateTime VotedAt { get; set; }
}
