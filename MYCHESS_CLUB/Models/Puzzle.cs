namespace MYCHESS_CLUB.Models
{
    public class Puzzle
    {
        public required string Fen { get; set; }
        public required string SolutionFrom { get; set; } // algebraic, e.g. "a6"
        public required string SolutionTo { get; set; }   // algebraic, e.g. "c7"
        public required string Description { get; set; }
        public string Theme { get; set; } = "Tactics";
    }
}