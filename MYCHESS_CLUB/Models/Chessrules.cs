namespace MYCHESS_CLUB.Models
{
    /// <summary>
    /// Pure chess rules logic operating on a plain ChessPiece?[8,8] board.
    /// Has no dependency on Blazor or any UI — used by both the Play page
    /// (for human moves) and ChessAi (for search), so there's a single
    /// source of truth for what's legal.
    /// </summary>
    public static class ChessRules
    {
        public static PieceColor Opponent(PieceColor c) =>
            c == PieceColor.White ? PieceColor.Black : PieceColor.White;

        // ===================== Move-shape validation =====================

        public static bool IsMoveValid(ChessPiece?[,] board, ChessPiece piece, Square from, Square to, Square? enPassantTarget)
        {
            var target = board[to.Row, to.Col];
            if (target != null && target.Color == piece.Color) return false;

            int rowDiff = to.Row - from.Row;
            int colDiff = to.Col - from.Col;
            int absRow = Math.Abs(rowDiff);
            int absCol = Math.Abs(colDiff);

            switch (piece.Type)
            {
                case PieceType.Pawn:
                    return IsValidPawnMove(board, piece, from, to, rowDiff, colDiff, target, enPassantTarget);

                case PieceType.Knight:
                    return (absRow == 2 && absCol == 1) || (absRow == 1 && absCol == 2);

                case PieceType.Bishop:
                    return absRow == absCol && absRow > 0 && IsPathClear(board, from, to);

                case PieceType.Rook:
                    return ((rowDiff == 0) != (colDiff == 0)) && IsPathClear(board, from, to);

                case PieceType.Queen:
                    bool diagonal = absRow == absCol && absRow > 0;
                    bool straight = (rowDiff == 0) != (colDiff == 0);
                    return (diagonal || straight) && IsPathClear(board, from, to);

                case PieceType.King:
                    // Castling (2-square king move) is generated/validated separately
                    return absRow <= 1 && absCol <= 1 && (absRow + absCol > 0);

                default:
                    return false;
            }
        }

        private static bool IsValidPawnMove(ChessPiece?[,] board, ChessPiece piece, Square from, Square to, int rowDiff, int colDiff, ChessPiece? target, Square? enPassantTarget)
        {
            int direction = piece.Color == PieceColor.White ? -1 : 1;
            int startRow = piece.Color == PieceColor.White ? 6 : 1;

            if (colDiff == 0)
            {
                if (target != null) return false;
                if (rowDiff == direction) return true;

                if (from.Row == startRow && rowDiff == 2 * direction)
                {
                    var midRow = from.Row + direction;
                    return board[midRow, from.Col] == null;
                }
                return false;
            }

            if (Math.Abs(colDiff) == 1 && rowDiff == direction)
            {
                if (target != null) return true;
                return enPassantTarget != null && to.Row == enPassantTarget.Row && to.Col == enPassantTarget.Col;
            }

            return false;
        }

        public static bool IsPathClear(ChessPiece?[,] board, Square from, Square to)
        {
            int rowStep = Math.Sign(to.Row - from.Row);
            int colStep = Math.Sign(to.Col - from.Col);

            int r = from.Row + rowStep;
            int c = from.Col + colStep;

            while (r != to.Row || c != to.Col)
            {
                if (board[r, c] != null) return false;
                r += rowStep;
                c += colStep;
            }
            return true;
        }

        // ===================== Check detection =====================

        public static Square? FindKing(ChessPiece?[,] board, PieceColor color)
        {
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    var p = board[r, c];
                    if (p != null && p.Type == PieceType.King && p.Color == color)
                        return new Square(r, c);
                }
            return null;
        }

        public static bool IsKingInCheck(ChessPiece?[,] board, PieceColor color)
        {
            var kingSquare = FindKing(board, color);
            if (kingSquare == null) return false;
            return IsSquareAttacked(board, kingSquare, Opponent(color));
        }

        public static bool IsSquareAttacked(ChessPiece?[,] board, Square target, PieceColor byColor)
        {
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    var p = board[r, c];
                    if (p == null || p.Color != byColor) continue;
                    if (CanAttack(board, p, new Square(r, c), target)) return true;
                }
            return false;
        }

        private static bool CanAttack(ChessPiece?[,] board, ChessPiece piece, Square from, Square to)
        {
            int rowDiff = to.Row - from.Row;
            int colDiff = to.Col - from.Col;
            int absRow = Math.Abs(rowDiff);
            int absCol = Math.Abs(colDiff);

            switch (piece.Type)
            {
                case PieceType.Pawn:
                    int direction = piece.Color == PieceColor.White ? -1 : 1;
                    return absCol == 1 && rowDiff == direction;

                case PieceType.Knight:
                    return (absRow == 2 && absCol == 1) || (absRow == 1 && absCol == 2);

                case PieceType.Bishop:
                    return absRow == absCol && absRow > 0 && IsPathClear(board, from, to);

                case PieceType.Rook:
                    return ((rowDiff == 0) != (colDiff == 0)) && IsPathClear(board, from, to);

                case PieceType.Queen:
                    bool diagonal = absRow == absCol && absRow > 0;
                    bool straight = (rowDiff == 0) != (colDiff == 0);
                    return (diagonal || straight) && IsPathClear(board, from, to);

                case PieceType.King:
                    return absRow <= 1 && absCol <= 1 && (absRow + absCol > 0);

                default:
                    return false;
            }
        }

        // ===================== Move simulation =====================

        public static (ChessPiece? capturedAtTo, ChessPiece? capturedEnPassant, int epRow, int epCol) ApplyMove(ChessPiece?[,] board, ChessPiece piece, Square from, Square to)
        {
            ChessPiece? capturedAtTo = board[to.Row, to.Col];
            ChessPiece? capturedEnPassant = null;
            int epRow = -1, epCol = -1;

            bool isEnPassant = piece.Type == PieceType.Pawn && to.Col != from.Col && capturedAtTo == null;
            if (isEnPassant)
            {
                epRow = from.Row;
                epCol = to.Col;
                capturedEnPassant = board[epRow, epCol];
                board[epRow, epCol] = null;
            }

            board[to.Row, to.Col] = piece;
            board[from.Row, from.Col] = null;

            return (capturedAtTo, capturedEnPassant, epRow, epCol);
        }

        public static void UndoMove(ChessPiece?[,] board, ChessPiece piece, Square from, Square to, ChessPiece? capturedAtTo, ChessPiece? capturedEnPassant, int epRow, int epCol)
        {
            board[from.Row, from.Col] = piece;
            board[to.Row, to.Col] = capturedAtTo;

            if (capturedEnPassant != null)
            {
                board[epRow, epCol] = capturedEnPassant;
            }
        }

        public static bool WouldLeaveKingInCheck(ChessPiece?[,] board, ChessPiece piece, Square from, Square to)
        {
            var (capturedAtTo, capturedEP, epRow, epCol) = ApplyMove(board, piece, from, to);
            bool inCheck = IsKingInCheck(board, piece.Color);
            UndoMove(board, piece, from, to, capturedAtTo, capturedEP, epRow, epCol);
            return inCheck;
        }

        public static bool HasAnyLegalMoves(ChessPiece?[,] board, PieceColor color, Square? enPassantTarget)
        {
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    var p = board[r, c];
                    if (p == null || p.Color != color) continue;
                    var from = new Square(r, c);

                    for (int tr = 0; tr < 8; tr++)
                        for (int tc = 0; tc < 8; tc++)
                        {
                            if (tr == r && tc == c) continue;
                            var to = new Square(tr, tc);
                            if (IsMoveValid(board, p, from, to, enPassantTarget) && !WouldLeaveKingInCheck(board, p, from, to))
                                return true;
                        }
                }

            return CanCastle(board, color, true) || CanCastle(board, color, false);
        }

        // ===================== Castling =====================

        public static bool CanCastle(ChessPiece?[,] board, PieceColor color, bool kingSide)
        {
            int homeRow = color == PieceColor.White ? 7 : 0;
            int kingCol = 4;
            int rookCol = kingSide ? 7 : 0;
            int newKingCol = kingSide ? 6 : 2;

            var king = board[homeRow, kingCol];
            var rook = board[homeRow, rookCol];

            if (king == null || king.Type != PieceType.King || king.Color != color || king.HasMoved) return false;
            if (rook == null || rook.Type != PieceType.Rook || rook.Color != color || rook.HasMoved) return false;

            int step = kingCol < rookCol ? 1 : -1;
            for (int c = kingCol + step; c != rookCol; c += step)
                if (board[homeRow, c] != null) return false;

            if (IsKingInCheck(board, color)) return false;

            var opponent = Opponent(color);
            int kingStep = newKingCol > kingCol ? 1 : -1;
            for (int c = kingCol; c != newKingCol + kingStep; c += kingStep)
                if (IsSquareAttacked(board, new Square(homeRow, c), opponent)) return false;

            return true;
        }

        public static void PerformCastle(ChessPiece?[,] board, PieceColor color, bool kingSide)
        {
            int homeRow = color == PieceColor.White ? 7 : 0;
            int kingCol = 4;
            int rookCol = kingSide ? 7 : 0;
            int newKingCol = kingSide ? 6 : 2;
            int newRookCol = kingSide ? 5 : 3;

            var king = board[homeRow, kingCol];
            var rook = board[homeRow, rookCol];

            board[homeRow, newKingCol] = king;
            board[homeRow, kingCol] = null;
            board[homeRow, newRookCol] = rook;
            board[homeRow, rookCol] = null;

            if (king != null) king.HasMoved = true;
            if (rook != null) rook.HasMoved = true;
        }

        /// <summary>
        /// Reverses PerformCastle. Only valid to call immediately after a
        /// PerformCastle on the same board — used by the AI search to back
        /// out of a simulated castle. Safe to reset HasMoved to false because
        /// CanCastle guarantees both pieces were unmoved before castling.
        /// </summary>
        public static void UndoCastle(ChessPiece?[,] board, PieceColor color, bool kingSide)
        {
            int homeRow = color == PieceColor.White ? 7 : 0;
            int kingCol = 4;
            int rookCol = kingSide ? 7 : 0;
            int newKingCol = kingSide ? 6 : 2;
            int newRookCol = kingSide ? 5 : 3;

            var king = board[homeRow, newKingCol];
            var rook = board[homeRow, newRookCol];

            board[homeRow, kingCol] = king;
            board[homeRow, newKingCol] = null;
            board[homeRow, rookCol] = rook;
            board[homeRow, newRookCol] = null;

            if (king != null) king.HasMoved = false;
            if (rook != null) rook.HasMoved = false;
        }

        // ===================== Move generation (for the AI) =====================

        public static List<Move> GenerateLegalMoves(ChessPiece?[,] board, PieceColor color, Square? enPassantTarget)
        {
            var moves = new List<Move>();

            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    var piece = board[r, c];
                    if (piece == null || piece.Color != color) continue;
                    var from = new Square(r, c);

                    for (int tr = 0; tr < 8; tr++)
                        for (int tc = 0; tc < 8; tc++)
                        {
                            if (tr == r && tc == c) continue;
                            var to = new Square(tr, tc);

                            if (!IsMoveValid(board, piece, from, to, enPassantTarget)) continue;
                            if (WouldLeaveKingInCheck(board, piece, from, to)) continue;

                            bool isPromotion = piece.Type == PieceType.Pawn && (tr == 0 || tr == 7);
                            var capturedPiece = board[tr, tc];

                            moves.Add(new Move
                            {
                                From = from,
                                To = to,
                                Piece = piece,
                                CapturedPiece = capturedPiece,
                                Promotion = isPromotion ? PieceType.Queen : null
                            });
                        }

                    if (piece.Type == PieceType.King)
                    {
                        if (CanCastle(board, color, true))
                            moves.Add(new Move { From = from, To = new Square(r, c + 2), Piece = piece, IsCastle = true, KingSide = true });
                        if (CanCastle(board, color, false))
                            moves.Add(new Move { From = from, To = new Square(r, c - 2), Piece = piece, IsCastle = true, KingSide = false });
                    }
                }

            return moves;
        }
    }
}