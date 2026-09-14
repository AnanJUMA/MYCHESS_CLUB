namespace MYCHESS_CLUB.Models
{
    public class Square
    {
        public int Row { get; set; }
        public int Col { get; set; }

        public Square(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public override bool Equals(object? obj) =>
            obj is Square other && other.Row == Row && other.Col == Col;

        public override int GetHashCode() => (Row, Col).GetHashCode();
    }
}