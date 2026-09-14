namespace MYCHESS_CLUB.Models
{
    public enum AiDifficulty
    {
        Beginner,
        Club,
        Expert,
        Master,
        Grandmaster
    }

    /// <summary>
    /// Self-contained chess AI: minimax search with alpha-beta pruning,
    /// material + piece-square-table evaluation, and move ordering.
    /// No external engine/binary — deploys anywhere .NET runs (including
    /// restricted hosts like Azure App Service that block spawning
    /// external processes).
    ///
    /// Realistic strength ceiling is club/expert level (roughly
    /// 1800-2200 ELO at "Grandmaster" difficulty), not literal
    /// grandmaster-strength play — a true GM-strength engine (Stockfish,
    /// etc.) is a different, far larger undertaking than a from-scratch
    /// minimax engine can realistically reach.
    /// </summary>
    public class ChessAi
    {
        private static readonly Random Rng = new();

        private record Preset(int Depth, double BlunderChance, int BlunderPoolSize);

        private static readonly Dictionary<AiDifficulty, Preset> Presets = new()
        {
            [AiDifficulty.Beginner] = new Preset(1, 0.45, 6),
            [AiDifficulty.Club] = new Preset(2, 0.20, 4),
            [AiDifficulty.Expert] = new Preset(3, 0.08, 2),
            [AiDifficulty.Master] = new Preset(4, 0.0, 1),
            [AiDifficulty.Grandmaster] = new Preset(5, 0.0, 1),
        };

        private static readonly Dictionary<PieceType, int> PieceValues = new()
        {
            [PieceType.Pawn] = 100,
            [PieceType.Knight] = 320,
            [PieceType.Bishop] = 330,
            [PieceType.Rook] = 500,
            [PieceType.Queen] = 900,
            [PieceType.King] = 20000
        };

        private const int MateScore = 1_000_000;

        // Piece-square tables: row 0 = rank 8 (Black's home), row 7 = rank 1
        // (White's home) — matching this project's board convention. Values
        // are from White's point of view; Black's are read with the row
        // mirrored (7 - row).
        private static readonly int[,] PawnTable = {
            {  0,  0,  0,  0,  0,  0,  0,  0 },
            { 50, 50, 50, 50, 50, 50, 50, 50 },
            { 10, 10, 20, 30, 30, 20, 10, 10 },
            {  5,  5, 10, 25, 25, 10,  5,  5 },
            {  0,  0,  0, 20, 20,  0,  0,  0 },
            {  5, -5,-10,  0,  0,-10, -5,  5 },
            {  5, 10, 10,-20,-20, 10, 10,  5 },
            {  0,  0,  0,  0,  0,  0,  0,  0 },
        };

        private static readonly int[,] KnightTable = {
            { -50,-40,-30,-30,-30,-30,-40,-50 },
            { -40,-20,  0,  0,  0,  0,-20,-40 },
            { -30,  0, 10, 15, 15, 10,  0,-30 },
            { -30,  5, 15, 20, 20, 15,  5,-30 },
            { -30,  0, 15, 20, 20, 15,  0,-30 },
            { -30,  5, 10, 15, 15, 10,  5,-30 },
            { -40,-20,  0,  5,  5,  0,-20,-40 },
            { -50,-40,-30,-30,-30,-30,-40,-50 },
        };

        private static readonly int[,] BishopTable = {
            { -20,-10,-10,-10,-10,-10,-10,-20 },
            { -10,  0,  0,  0,  0,  0,  0,-10 },
            { -10,  0,  5, 10, 10,  5,  0,-10 },
            { -10,  5,  5, 10, 10,  5,  5,-10 },
            { -10,  0, 10, 10, 10, 10,  0,-10 },
            { -10, 10, 10, 10, 10, 10, 10,-10 },
            { -10,  5,  0,  0,  0,  0,  5,-10 },
            { -20,-10,-10,-10,-10,-10,-10,-20 },
        };

        private static readonly int[,] RookTable = {
            {  0,  0,  0,  0,  0,  0,  0,  0 },
            {  5, 10, 10, 10, 10, 10, 10,  5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            {  0,  0,  0,  5,  5,  0,  0,  0 },
        };

        private static readonly int[,] QueenTable = {
            { -20,-10,-10, -5, -5,-10,-10,-20 },
            { -10,  0,  0,  0,  0,  0,  0,-10 },
            { -10,  0,  5,  5,  5,  5,  0,-10 },
            {  -5,  0,  5,  5,  5,  5,  0, -5 },
            {   0,  0,  5,  5,  5,  5,  0, -5 },
            { -10,  5,  5,  5,  5,  5,  0,-10 },
            { -10,  0,  5,  0,  0,  0,  0,-10 },
            { -20,-10,-10, -5, -5,-10,-10,-20 },
        };

        private static readonly int[,] KingTable = {
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -20,-30,-30,-40,-40,-30,-30,-20 },
            { -10,-20,-20,-20,-20,-20,-20,-10 },
            {  20, 20,  0,  0,  0,  0, 20, 20 },
            {  20, 30, 10,  0,  0, 10, 30, 20 },
        };

        /// <summary>
        /// Picks a move for `aiColor` given the current position. Returns
        /// null if there's no legal move (shouldn't happen if the caller
        /// checks game-over state first).
        /// </summary>
        public Move? GetBestMove(ChessPiece?[,] originalBoard, PieceColor aiColor, Square? enPassantTarget, AiDifficulty difficulty)
        {
            var board = CloneBoard(originalBoard);
            var preset = Presets[difficulty];

            var moves = ChessRules.GenerateLegalMoves(board, aiColor, enPassantTarget);
            if (moves.Count == 0) return null;

            OrderMoves(moves);

            var scored = new List<(Move move, int score)>();
            int alpha = int.MinValue + 1;
            int beta = int.MaxValue - 1;

            foreach (var move in moves)
            {
                var (nextEnPassant, undo) = MakeMove(board, move);
                int score = Minimax(board, ChessRules.Opponent(aiColor), nextEnPassant, preset.Depth - 1, alpha, beta, aiColor);
                undo();

                scored.Add((move, score));
                if (score > alpha) alpha = score;
            }

            scored.Sort((a, b) => b.score.CompareTo(a.score));

            // Weaker presets sometimes play a move from a wider pool instead
            // of always the computer-perfect top choice, so low difficulties
            // actually feel beatable rather than just "slow but perfect".
            if (preset.BlunderChance > 0 && Rng.NextDouble() < preset.BlunderChance)
            {
                int poolCount = Math.Min(preset.BlunderPoolSize, scored.Count);
                return scored[Rng.Next(poolCount)].move;
            }

            return scored[0].move;
        }

        private int Minimax(ChessPiece?[,] board, PieceColor colorToMove, Square? enPassantTarget, int depth, int alpha, int beta, PieceColor aiColor)
        {
            if (depth <= 0)
            {
                return Evaluate(board, aiColor);
            }

            var moves = ChessRules.GenerateLegalMoves(board, colorToMove, enPassantTarget);
            if (moves.Count == 0)
            {
                bool inCheck = ChessRules.IsKingInCheck(board, colorToMove);
                if (!inCheck) return 0; // stalemate

                // Checkmate: very bad for whoever is stuck, from the AI's
                // perspective. Add `depth` so faster mates score higher —
                // the AI prefers the quickest forced win / slowest forced loss.
                int mateScore = MateScore + depth;
                return colorToMove == aiColor ? -mateScore : mateScore;
            }

            OrderMoves(moves);
            bool maximizing = colorToMove == aiColor;
            int best = maximizing ? int.MinValue + 1 : int.MaxValue - 1;

            foreach (var move in moves)
            {
                var (nextEnPassant, undo) = MakeMove(board, move);
                int eval = Minimax(board, ChessRules.Opponent(colorToMove), nextEnPassant, depth - 1, alpha, beta, aiColor);
                undo();

                if (maximizing)
                {
                    if (eval > best) best = eval;
                    if (eval > alpha) alpha = eval;
                }
                else
                {
                    if (eval < best) best = eval;
                    if (eval < beta) beta = eval;
                }

                if (beta <= alpha) break; // prune
            }

            return best;
        }

        /// <summary>
        /// Applies a move (normal, en passant, castle, or promotion) to the
        /// board and returns an Action that undoes exactly that change.
        /// </summary>
        private (Square? nextEnPassant, Action undo) MakeMove(ChessPiece?[,] board, Move move)
        {
            if (move.IsCastle)
            {
                var color = move.Piece.Color;
                bool kingSide = move.KingSide;
                ChessRules.PerformCastle(board, color, kingSide);
                return (null, () => ChessRules.UndoCastle(board, color, kingSide));
            }

            var (capturedAtTo, capturedEP, epRow, epCol) = ChessRules.ApplyMove(board, move.Piece, move.From, move.To);

            bool prevHasMoved = move.Piece.HasMoved;
            move.Piece.HasMoved = true;

            PieceType? prevType = null;
            if (move.Promotion.HasValue)
            {
                prevType = move.Piece.Type;
                move.Piece.Type = move.Promotion.Value;
            }

            Square? nextEnPassant = null;
            if (move.Piece.Type == PieceType.Pawn && !move.Promotion.HasValue && Math.Abs(move.To.Row - move.From.Row) == 2)
            {
                nextEnPassant = new Square((move.To.Row + move.From.Row) / 2, move.To.Col);
            }

            void Undo()
            {
                if (prevType.HasValue) move.Piece.Type = prevType.Value;
                move.Piece.HasMoved = prevHasMoved;
                ChessRules.UndoMove(board, move.Piece, move.From, move.To, capturedAtTo, capturedEP, epRow, epCol);
            }

            return (nextEnPassant, Undo);
        }

        private void OrderMoves(List<Move> moves)
        {
            // Captures first (highest-value victim first) — this makes
            // alpha-beta pruning far more effective since strong moves
            // are considered before weak ones.
            moves.Sort((a, b) =>
            {
                int av = a.CapturedPiece != null ? PieceValues[a.CapturedPiece.Type] : -1;
                int bv = b.CapturedPiece != null ? PieceValues[b.CapturedPiece.Type] : -1;
                return bv.CompareTo(av);
            });
        }

        /// <summary>Material + positional score from `perspective`'s point of view.</summary>
        private int Evaluate(ChessPiece?[,] board, PieceColor perspective)
        {
            int score = 0;

            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    var piece = board[r, c];
                    if (piece == null) continue;

                    int value = PieceValues[piece.Type] + PositionalValue(piece, r, c);
                    score += piece.Color == perspective ? value : -value;
                }

            return score;
        }

        private int PositionalValue(ChessPiece piece, int row, int col)
        {
            // White reads the table as-is (row 0 = rank 8, row 7 = rank 1,
            // matching the board). Black mirrors it vertically.
            int r = piece.Color == PieceColor.White ? row : 7 - row;

            return piece.Type switch
            {
                PieceType.Pawn => PawnTable[r, col],
                PieceType.Knight => KnightTable[r, col],
                PieceType.Bishop => BishopTable[r, col],
                PieceType.Rook => RookTable[r, col],
                PieceType.Queen => QueenTable[r, col],
                PieceType.King => KingTable[r, col],
                _ => 0
            };
        }

        private static ChessPiece?[,] CloneBoard(ChessPiece?[,] board)
        {
            var clone = new ChessPiece?[8, 8];
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    var p = board[r, c];
                    clone[r, c] = p == null ? null : new ChessPiece { Color = p.Color, Type = p.Type, HasMoved = p.HasMoved };
                }
            return clone;
        }
    }
}