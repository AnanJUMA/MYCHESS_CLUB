using MYCHESS_CLUB.Models;

namespace MYCHESS_CLUB.Services;

public class BracketService
{
    public Bracket GenerateSingleElimination(int tournamentId, List<Participant> participants)
    {
        if (participants.Count < 2)
            throw new InvalidOperationException("Need at least 2 participants to generate a bracket.");

        var ordered = participants.OrderBy(p => p.Seed == 0 ? int.MaxValue : p.Seed).ToList();

        int bracketSize = NextPowerOfTwo(ordered.Count);
        int byeCount = bracketSize - ordered.Count;

        var seedOrder = BuildSeedOrder(bracketSize); // e.g. [1,16,8,9,4,13,5,12,...]

        var withByes = new List<Participant?>(ordered);
        withByes.AddRange(Enumerable.Repeat<Participant?>(null, byeCount));

        var slots = new Participant?[bracketSize];
        for (int i = 0; i < bracketSize; i++)
        {
            int seedPosition = seedOrder[i]; // 1-based
            slots[i] = seedPosition <= withByes.Count ? withByes[seedPosition - 1] : null;
        }

        var bracket = new Bracket { TournamentId = tournamentId };
        int totalRounds = (int)Math.Log2(bracketSize);
        int matchId = 1;

        var firstRound = new BracketRound { RoundNumber = 1, Name = RoundName(1, totalRounds) };
        for (int i = 0; i < bracketSize; i += 2)
        {
            var match = new Match
            {
                Id = matchId++,
                Round = 1,
                MatchNumber = i / 2,
                Player1 = slots[i],
                Player2 = slots[i + 1]
            };

            if (match.Player1 is not null && match.Player2 is null)
                match.Winner = match.Player1;
            else if (match.Player2 is not null && match.Player1 is null)
                match.Winner = match.Player2;

            firstRound.Matches.Add(match);
        }
        bracket.Rounds.Add(firstRound);

        for (int r = 2; r <= totalRounds; r++)
        {
            var round = new BracketRound { RoundNumber = r, Name = RoundName(r, totalRounds) };
            int matchesInRound = bracketSize / (int)Math.Pow(2, r);
            for (int m = 0; m < matchesInRound; m++)
                round.Matches.Add(new Match { Id = matchId++, Round = r, MatchNumber = m });

            bracket.Rounds.Add(round);
        }

        // Push round-1 byes forward; repeat in case byes cascade through multiple rounds
        for (int r = 1; r < totalRounds; r++)
            PropagateWinners(bracket, r);

        return bracket;
    }

    /// <summary>Call after a match result is known to push the winner into the next round.</summary>
    public void AdvanceWinner(Bracket bracket, int round, int matchNumber, Participant winner)
    {
        var match = bracket.Rounds.First(r => r.RoundNumber == round)
                                   .Matches.First(m => m.MatchNumber == matchNumber);
        match.Winner = winner;
        PropagateWinners(bracket, round);
    }

    private void PropagateWinners(Bracket bracket, int fromRound)
    {
        var current = bracket.Rounds.FirstOrDefault(r => r.RoundNumber == fromRound);
        var next = bracket.Rounds.FirstOrDefault(r => r.RoundNumber == fromRound + 1);
        if (current is null || next is null) return;

        foreach (var match in current.Matches.Where(m => m.Winner is not null))
        {
            int nextMatchNumber = match.MatchNumber / 2;
            var nextMatch = next.Matches.First(m => m.MatchNumber == nextMatchNumber);

            if (match.MatchNumber % 2 == 0)
                nextMatch.Player1 = match.Winner;
            else
                nextMatch.Player2 = match.Winner;

            if (nextMatch.IsBye)
                nextMatch.Winner = nextMatch.Player1 ?? nextMatch.Player2;
        }
    }

    private static int NextPowerOfTwo(int n)
    {
        int power = 1;
        while (power < n) power *= 2;
        return power;
    }

    private static string RoundName(int round, int totalRounds)
    {
        int roundsFromEnd = totalRounds - round;
        return roundsFromEnd switch
        {
            0 => "Final",
            1 => "Semifinal",
            2 => "Quarterfinal",
            _ => $"Round {round}"
        };
    }

    /// <summary>Standard bracket seeding (1v16, 8v9, 5v12, 4v13...) so top seeds meet as late as possible.</summary>
    private static List<int> BuildSeedOrder(int size)
    {
        var seeds = new List<int> { 1, 2 };
        while (seeds.Count < size)
        {
            int roundSize = seeds.Count * 2;
            var newSeeds = new List<int>();
            foreach (var s in seeds)
            {
                newSeeds.Add(s);
                newSeeds.Add(roundSize + 1 - s);
            }
            seeds = newSeeds;
        }
        return seeds;
    }
}