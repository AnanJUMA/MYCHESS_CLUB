using MYCHESS_CLUB.Models;

namespace MYCHESS_CLUB.Services
{
    public class TournamentService
    {
        private readonly List<Tournament> _tournaments;
        private readonly List<Participant> _participants = new();
        private int _nextParticipantId = 1;

        public TournamentService()
        {
            _tournaments = new List<Tournament>
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

        public List<Tournament> GetTournaments() => _tournaments;

        public Tournament? GetTournament(int id) =>
            _tournaments.FirstOrDefault(t => t.Id == id);

        public List<Participant> GetParticipants(int tournamentId) =>
            _participants.Where(p => p.TournamentId == tournamentId)
                          .OrderBy(p => p.Seed == 0 ? int.MaxValue : p.Seed)
                          .ToList();

        public Participant RegisterParticipant(int tournamentId, string name, int seed = 0)
        {
            var participant = new Participant
            {
                Id = _nextParticipantId++,
                TournamentId = tournamentId,
                Name = name,
                Seed = seed
            };
            _participants.Add(participant);
            return participant;
        }

        public void RemoveParticipant(int participantId)
        {
            _participants.RemoveAll(p => p.Id == participantId);
        }

        public void SetSeed(int participantId, int seed)
        {
            var p = _participants.FirstOrDefault(x => x.Id == participantId);
            if (p is not null) p.Seed = seed;
        }
    }
}