using System;

namespace WPFTris.Base
{
    internal class TetrominoeFactory : PolyminoeFactory
    {    
        public class Tetrominoes : Pieces
        {
            public const int I = 0;
            public const int O = 1;
            public const int T = 2;
            public const int S = 3;
            public const int Z = 4;
            public const int J = 5;
            public const int L = 6;
            private static readonly Polyminoe.State[] IShape =
            {
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(-2,0), new Point<int>(-1,0), new Point<int>(0,0), new Point<int>(1,0)
                }),
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(0,1), new Point<int>(0,0), new Point<int>(0,-1), new Point<int>(0,-2)
                }),
            };
            private static readonly Polyminoe.State[] OShape =
            {
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(-1,-1), new Point<int>(-1,0), new Point<int>(0,0), new Point<int>(0,-1)
                })
            };
            private static readonly Polyminoe.State[] TShape =
            {
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(0,0), new Point<int>(1,0), new Point<int>(0,-1), new Point<int>(-1,0)
                }),
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(0,0), new Point<int>(0,1), new Point<int>(0,-1), new Point<int>(1,0)
                }),
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(0,1), new Point<int>(1,0), new Point<int>(0,0), new Point<int>(-1,0)
                }),
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(0,0), new Point<int>(-1,0), new Point<int>(0,1), new Point<int>(0,-1)
                }),
            };
            private static readonly Polyminoe.State[] SShape =
            {
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(1,0), new Point<int>(0,0), new Point<int>(0,1), new Point<int>(-1,1)
                }),
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(0,-1), new Point<int>(0,0), new Point<int>(1,0), new Point<int>(1,1)
                }),
            };
            private static readonly Polyminoe.State[] ZShape =
            {
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(-1,0), new Point<int>(0,0), new Point<int>(0,1), new Point<int>(1,1)
                }),
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(1,-1), new Point<int>(1,0), new Point<int>(0,0), new Point<int>(0,1)
                }),
            };
            private static readonly Polyminoe.State[] JShape =
            {
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(1,0), new Point<int>(0,0), new Point<int>(-1,0), new Point<int>(-1,-1)
                }),
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(0,1), new Point<int>(0,0), new Point<int>(0,-1), new Point<int>(1,-1)
                }),
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(-1,0), new Point<int>(0,0), new Point<int>(1,0), new Point<int>(1,1)
                }),
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(0,-1), new Point<int>(0,0), new Point<int>(0,1), new Point<int>(-1,1)
                }),
            };
            private static readonly Polyminoe.State[] LShape =
            {
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(1,-1), new Point<int>(1,0), new Point<int>(0,0), new Point<int>(-1,0)
                }),
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(1,1), new Point<int>(0,1), new Point<int>(0,0), new Point<int>(0,-1)
                }),
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(-1,1), new Point<int>(-1,0), new Point<int>(0,0), new Point<int>(1,0)
                }),
                new Polyminoe.State(new Point<int>[]
                {
                    new Point<int>(-1,-1), new Point<int>(0,-1), new Point<int>(0,0), new Point<int>(0,1)
                }),
            };
        }

        protected override void _InitPiecesObject()
        {
            p = new Tetrominoes();
        }

        public TetrominoeFactory(int seed) : base(seed) { }

        public TetrominoeFactory() : base() { }
    }
}
