namespace MYCHESS_CLUB.Models
{

    public enum PieceColor { White, Black }
    public enum PieceType { Pawn, Knight, Bishop, Rook, Queen, King }

    public class ChessPiece
    {
        public PieceColor Color { get; set; }
        public PieceType Type { get; set; }

        // Tracks whether this piece has ever moved — needed to determine
        // castling eligibility (king and rook must both be unmoved).
        public bool HasMoved { get; set; } = false;

        public string UnicodeSymbol => (Color, Type) switch
        {
            (PieceColor.White, PieceType.King) => "♔",
            (PieceColor.White, PieceType.Queen) => "♕",
            (PieceColor.White, PieceType.Rook) => "♖",
            (PieceColor.White, PieceType.Bishop) => "♗",
            (PieceColor.White, PieceType.Knight) => "♘",
            (PieceColor.White, PieceType.Pawn) => "♙",
            (PieceColor.Black, PieceType.King) => "♚",
            (PieceColor.Black, PieceType.Queen) => "♛",
            (PieceColor.Black, PieceType.Rook) => "♜",
            (PieceColor.Black, PieceType.Bishop) => "♝",
            (PieceColor.Black, PieceType.Knight) => "♞",
            (PieceColor.Black, PieceType.Pawn) => "♟",
            _ => ""
        };

        public string CssClass => $"piece-{Color.ToString().ToLower()}-{Type.ToString().ToLower()}";
    }
}