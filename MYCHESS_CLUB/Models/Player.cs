namespace MYCHESS_CLUB.Models
{
    /// <summary>
    /// A chess-club profile. Members have IdentityUserId set (created
    /// automatically during registration). Public tournament entrants who
    /// haven't joined as members get a Player row with IdentityUserId left
    /// null — same table, same Tournament/Entry/Pairing schema either way.
    /// </summary>
    public class Player
    {
        public int Id { get; set; }

        // Null for a public tournament entrant who isn't a logged-in member.
        public string? IdentityUserId { get; set; }

        public string Name { get; set; } = "";
        public string? Email { get; set; }
        public int Rating { get; set; } = 1200;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}