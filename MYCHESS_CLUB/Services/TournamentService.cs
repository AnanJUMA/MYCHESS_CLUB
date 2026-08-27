
namespace MYCHESS_CLUB.Services
{
    public class TournamentService
    {
        public List<Tournament> GetTournaments()
        {
            return new List<Tournament>
            {
                new Tournament
                {
                    Id = 1,
                    Name = "Lucky Stars Club Championship",
                    Date = DateTime.Now.AddMonths(1), // replace with real date
                    Location = "To be announced",
                    Format = TournamentFormat.Classical,
                    Eligibility = "Club Members",
                    Description = "Our main club championship tournament."
                },
                new Tournament
                {
                    Id = 2,
                    Name = "Lucky Stars Rapid Tournament",
                    Date = DateTime.Now.AddMonths(2),
                    Location = "To be announced",
                    Format = TournamentFormat.Rapid,
                    Eligibility = "Open to Members",
                    Description = "A fast-paced tournament for our members."
                },
                new Tournament
                {
                    Id = 3,
                    Name = "Lucky Stars Junior Championship",
                    Date = DateTime.Now.AddMonths(3),
                    Location = "To be announced",
                    Format = TournamentFormat.Blitz,
                    Eligibility = "Junior Members",
                    Description = "A tournament designed for our young players."
                }
            };
        }
    }
}