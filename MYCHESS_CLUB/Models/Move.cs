namespace MYCHESS_CLUB.Models
{
    public class Move
    {
        public required Square From { get; set; }
        public required Square To { get; set; }
        public required ChessPiece Piece { get; set; }

        // The piece sitting on the destination square before the move,
        // if any — used for move ordering (search captures first) and
        // by the caller to know whether this move is a capture. Not set
        // for en passant (that captured piece isn't on the destination
        // square — ChessRules.ApplyMove handles that case separately).
        public ChessPiece? CapturedPiece { get; set; }

        public bool IsCastle { get; set; }
        public bool KingSide { get; set; }

        // Only queen promotions are generated (see ChessRules.GenerateLegalMoves)
        // to keep the AI's branching factor reasonable — under-promotion is a
        // rare enough tactic that skipping it barely affects playing strength.
        public PieceType? Promotion { get; set; }
    }
}