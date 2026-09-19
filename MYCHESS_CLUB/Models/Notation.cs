namespace MYCHESS_CLUB.Models
{
    /// <summary>
    /// Converts between FEN strings / algebraic square names and this
    /// project's board representation. Deliberately only parses piece
    /// placement and side-to-move for now — castling rights and en
    /// passant aren't needed for static diagrams or single-move puzzles.
    /// (Full FEN round-tripping, including those fields, is what the
    /// tournament Game persistence will need later — this is a starting
    /// point for that, not the final version.)
    /// </summary>
    public static class Notation
    {
        public static (ChessPiece?[,] Board, PieceColor Turn) ParseFen(string fen)
        {
            var board = new ChessPiece?[8, 8];
            var parts = fen.Trim().Split(' ');
            var ranks = parts[0].Split('/');

            for (int row = 0; row < 8 && row < ranks.Length; row++)
            {
                int col = 0;
                foreach (char ch in ranks[row])
                {
                    if (char.IsDigit(ch))
                    {
                        col += ch - '0';
                        continue;
                    }

                    var color = char.IsUpper(ch) ? PieceColor.White : PieceColor.Black;
                    var type = char.ToLower(ch) switch
                    {
                        'p' => PieceType.Pawn,
                        'n' => PieceType.Knight,
                        'b' => PieceType.Bishop,
                        'r' => PieceType.Rook,
                        'q' => PieceType.Queen,
                        'k' => PieceType.King,
                        _ => PieceType.Pawn
                    };

                    if (col < 8)
                    {
                        board[row, col] = new ChessPiece { Color = color, Type = type, HasMoved = true };
                    }
                    col++;
                }
            }

            var turn = parts.Length > 1 && parts[1] == "b" ? PieceColor.Black : PieceColor.White;
            return (board, turn);
        }

        public static Square FromAlgebraic(string square)
        {
            int col = square[0] - 'a';
            int row = 8 - (square[1] - '0');
            return new Square(row, col);
        }

        public static string ToAlgebraic(Square square) =>
            $"{(char)('a' + square.Col)}{8 - square.Row}";
    }
}