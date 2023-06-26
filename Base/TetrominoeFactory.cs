using System;

namespace WPFTris.Base
{
    internal class TetrominoeFactory
    {
        private static readonly Polyminoe.State[] I =
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
        private static readonly Polyminoe.State[] O =
        {
            new Polyminoe.State(new Point<int>[]
            {
                new Point<int>(-1,-1), new Point<int>(-1,0), new Point<int>(0,0), new Point<int>(0,-1)
            })
        };
        private static readonly Polyminoe.State[] T =
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
        private static readonly Polyminoe.State[] S =
        {
            new Polyminoe.State(new Point<int>[]
            {
                new Point<int>(1,-1), new Point<int>(0,-1), new Point<int>(0,0), new Point<int>(-1,0)
            }),
            new Polyminoe.State(new Point<int>[]
            {
                new Point<int>(0,-1), new Point<int>(0,0), new Point<int>(1,0), new Point<int>(1,1)
            }),
        };
        private static readonly Polyminoe.State[] Z =
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
        private static readonly Polyminoe.State[] J =
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
        private static readonly Polyminoe.State[] L =
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
        private static readonly Polyminoe.State[][] tetrominoes = { I, O, T, S, Z, L, J };
        private static readonly int tetrominoe_count = tetrominoes.Length;

        private Random rnd;
        private int last;

        public struct Tetrominoe
        {
            public readonly Polyminoe shape;
            public readonly Pieces name;

            public Tetrominoe(Polyminoe shape, Pieces name)
            {
                this.shape = shape;
                this.name = name;
            }
        }

        private int Roll()
        {
            int c = rnd.Next(tetrominoe_count);
            if (last == c) c = rnd.Next(tetrominoe_count);
            last = c;
            return c;
        }

        public enum Pieces
        {
            I = 0,
            O,
            T,
            S,
            Z,
            L,
            J
        }

        public TetrominoeFactory(int seed)
        {
            rnd = new Random(seed);
        }

        public TetrominoeFactory()
        {
            rnd = new Random();
        }

        public Tetrominoe GetTetrominoe()
        {
            int roll = Roll();
            return new Tetrominoe(new Polyminoe(ref tetrominoes[roll]), (Pieces)roll);
        }

        public Tetrominoe GetTetrominoe(Pieces piece)
        {
            return new Tetrominoe(new Polyminoe(ref tetrominoes[(int)piece]), piece);
        }
    }
}
