namespace MYCHESS_CLUB.Models;

public class Participant
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Seed { get; set; } // 0 = unseeded
}