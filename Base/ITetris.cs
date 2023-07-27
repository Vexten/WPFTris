using WPFTris.Game;

namespace WPFTris.Base
{

    internal interface ITetris
    {

        public PolyminoeFactory.Piece NextPiece
        {
            get;
        }
        public Point<int> CurrentPoint
        {
            get;
        }
        public int Score
        {
            get;
        }
        public int Level
        {
            get;
        }
        public int TotalLines
        {
            get;
        }

        public delegate void BasicHandler(ITetris sender);
        public event BasicHandler? PieceMove;
        public event BasicHandler? Loss;

        public delegate void LineClearHandler(ITetris sender, int[] lines);
        public event LineClearHandler? LineClear;

        public delegate void PieceDropHandler(ITetris sender, TetrominoeFactory);

        public enum Move
        {
            Left,
            Right,
            RotateLeft,
            RotateRight,
            SoftDrop,
            HardDrop
        }

        public void DoMove(Move move);

        public void FieldAt(int x, int y);

        public int GetPieceCount(int piece);
    }
}
