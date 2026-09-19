using Microsoft.AspNetCore.Identity;

namespace MYCHESS_CLUB.Data
{
    // Deliberately minimal — auth concerns only. Chess-club-specific
    // profile data (display name, rating, join date) lives in Models.Player,
    // linked via Player.IdentityUserId. This keeps the two concerns
    // separate: swapping/extending auth later doesn't touch chess data,
    // and public tournament entrants can get a Player row with no
    // IdentityUserId at all (no login required just to enter a tournament).
    public class ApplicationUser : IdentityUser
    {
    }
}