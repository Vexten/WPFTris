using Microsoft.Windows.Themes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WPFTris.Base
{
    /// <summary>
    /// Implements basic polyminoe factory functionality<para/>
    /// To implement correctly, define a public <see cref="Pieces"/> subclass and use <c>_InitPiecesObject</c> to assign it to <c>this.p</c>
    /// </summary>
    internal abstract class PolyminoeFactory
    {
        protected static readonly string ShapeIdentifier = "Shape";

        private readonly Random rnd;
        private int last;
        private Dictionary<int, Polyminoe.State[]> polyminoes;
        protected Pieces p;

        /// <summary>
        /// Marker class for piece definitions<para/>
        /// Piece definition format:
        /// <code>
        /// public const int %NAME% = ...;
        /// private static readonly Polyminoe.State[] %NAME% + <see cref="ShapeIdentifier"/> = ...;
        /// </code>
        /// </summary>
        public abstract class Pieces { }

        public readonly struct Piece
        {
            public readonly Polyminoe shape;
            public readonly int name;

            public Piece(Polyminoe shape, int name)
            {
                this.shape = shape;
                this.name = name;
            }
        }

        private int Roll()
        {
            int c = rnd.Next(polyminoes.Count);
            if (last == c) c = rnd.Next(polyminoes.Count);
            last = c;
            return c;
        }

        private Dictionary<int, Polyminoe.State[]> _ParseClass(System.Type pieceEnum)
        {
            Dictionary<int, Polyminoe.State[]> pieces = new Dictionary<int, Polyminoe.State[]>();
            Dictionary<string, int> names = new Dictionary<string, int>();
            Dictionary<string, Polyminoe.State[]> states = new Dictionary<string, Polyminoe.State[]>();
            FieldInfo[] fields = pieceEnum.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (var fi in fields)
            {
                if (fi.FieldType == typeof(int))
                {
                    names[fi.Name] = (int)fi.GetValue(null);
                }
                else
                {
                    int i = fi.Name.IndexOf(ShapeIdentifier);
                    states[fi.Name.Remove(i,ShapeIdentifier.Length)] = (Polyminoe.State[])fi.GetValue(null);
                }
            }
            foreach (var kvp in names)
            {
                pieces[kvp.Value] = states[kvp.Key];
            }
            return pieces;
        }

        protected abstract void _InitPiecesObject();

        protected PolyminoeFactory(int seed)
        {
            _InitPiecesObject();
            rnd = new Random(seed);
            polyminoes = _ParseClass(p.GetType());
        }

        protected PolyminoeFactory()
        {
            _InitPiecesObject();
            rnd = new Random();
            polyminoes = _ParseClass(p.GetType());
        }

        public Piece GetPiece()
        {
            int roll = Roll();
            return GetPiece(roll);
        }

        public bool PieceExists(int piece)
        {
            return polyminoes.Keys.Contains(piece);
        }

        public int[] GetPieces()
        {
            return polyminoes.Keys.ToArray();
        }

        public Piece GetPiece(int piece)
        {
            if (!polyminoes.ContainsKey(piece)) throw new ArgumentOutOfRangeException("Such piece does not exist. Please use constants from the provided class.");
            Polyminoe.State[] states = polyminoes[piece];
            return new Piece(new Polyminoe(ref states), piece);
        }
    }
}
