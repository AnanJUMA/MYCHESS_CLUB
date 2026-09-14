namespace MYCHESS_CLUB.Models;

public class Match
{
    public int Id { get; set; }
    public int Round { get; set; }          // 1-based
    public int MatchNumber { get; set; }    // position within the round, 0-based
    public Participant? Player1 { get; set; }
    public Participant? Player2 { get; set; }
    public Participant? Winner { get; set; }

    public bool IsBye => (Player1 is null) != (Player2 is null); // exactly one side empty
    public bool IsComplete => Winner is not null;
}

public class BracketRound
{
    public int RoundNumber { get; set; }
    public string Name { get; set; } = string.Empty; // "Final", "Semifinal", "Round 1"...
    public List<Match> Matches { get; set; } = new();
}

public class Bracket
{
    public int TournamentId { get; set; }
    public List<BracketRound> Rounds { get; set; } = new();
}