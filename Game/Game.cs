using System;
using System.Collections.Generic;
using WPFTris.Base;

namespace WPFTris.Game
{
    internal class Game : ITetris
    {
        public const int FieldEmpty = -1;
        public const int FieldCleared = -2;
        //should be at least the offset of the biggest piece
        public const int FieldAdditionalSpace = 2;

        private static readonly int[] scoring = { 40, 100, 300, 1200 };
        private readonly TetrominoeFactory factory;
        private PolyminoeFactory.Piece curr, next;
        private readonly int w, h;
        private readonly int[,] field;
        private Point<int> basePoint;
        private Point<int> currPoint;
        private bool linesWereCleared;
        private readonly Stack<int> clearedLines;
        private int linesClearedTillLevel;

        private int level;
        private int score;
        private int totalLines;
        private int[] totalPieces;

        #region private_methods
        private void _SetField()
        {
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h + FieldAdditionalSpace; j++)
                {
                    field[i, j] = FieldEmpty;
                }
            }
        }

        private void _Init()
        {
            _SetField();
            level = 0;
            linesClearedTillLevel = 10;
            score = 0;
            totalLines = 0;
            Array.Clear(totalPieces, 0, totalPieces.Length);
            currPoint = new Point<int>(basePoint);
            curr = factory.GetPiece();
            next = factory.GetPiece();
            linesWereCleared = false;
            clearedLines.Clear();
        }

        private ref int _FieldAt(int x, int y)
        {
            return ref field[x, y + FieldAdditionalSpace];
        }

        private bool _IsBlock(int x, int y)
        {
            return factory.PieceExists(_FieldAt(x, y));
        }

        private int _GetFilledHeight()
        {
            int top = h;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (_IsBlock(x, y) && y < top)
                    {
                        top = y;
                    }
                }
            }
            return top;
        }

        private void _DropBlocks()
        {
            int top = _GetFilledHeight() - 1;
            while (clearedLines.Count > 0)
            {
                int y = clearedLines.Pop();
                for (int y2 = y; y2 > top; y2--)
                {
                    for (int x = 0; x < w; x++)
                    {
                        _FieldAt(x, y2) = _FieldAt(x, y2 - 1);
                    }
                }
            }
        }

        private bool _CheckLines()
        {
            int top = _GetFilledHeight() - 1;
            bool ret = false;
            for (int y = h - 1; y > top; y--)
            {
                int count = 0;
                for (int x = 0; x < w; x++)
                {
                    if (_IsBlock(x, y)) count++;
                }
                if (count == w)
                {
                    clearedLines.Push(y);
                    ret = true;
                    for (int x = 0; x < w; x++)
                    {
                        _FieldAt(x, y) = FieldCleared;
                    }
                }
            }
            if (clearedLines.Count > 0)
            {
                totalLines += clearedLines.Count;
                score += scoring[clearedLines.Count - 1] * (level + 1);
                linesClearedTillLevel -= clearedLines.Count;
                if (linesClearedTillLevel < 1)
                {
                    level++;
                    int t = linesClearedTillLevel;
                    linesClearedTillLevel = Math.Max(50, (level + 1) * 10) + t;
                }
            }
            return ret;
        }

        private void _ClearField()
        {
            for (int y = 0; y < h; y++)
            {
                if (_FieldAt(0, y) == FieldCleared)
                {
                    for (int x = 0; x < w; x++)
                    {
                        _FieldAt(x, y) = FieldEmpty;
                    }
                }
            }
        }

        private bool _CheckLoss()
        {
            for (int x = 0; x < w; x++)
            {
                for (int y = -FieldAdditionalSpace; y < 0; y++)
                {
                    if (_IsBlock(x, y))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void _SetPiece(int val)
        {
            foreach (var p in curr.shape)
            {
                var pt = p + currPoint;
                _FieldAt(pt.x, pt.y) = val;
            }
        }

        private bool _PieceCollides()
        {
            foreach (var p in curr.shape)
            {
                var pt = p + currPoint;
                if (pt.x < 0) return true;
                if (pt.x > w - 1) return true;
                if (pt.y > h - 1) return true;
                if (_FieldAt(pt.x, pt.y) != FieldEmpty) return true;
            }
            return false;
        }

        private bool _CanManipulatePiece()
        {
            if (currPoint.y < 0) return false;
            return true;
        }
        #endregion

        public PolyminoeFactory.Piece NextPiece => next;

        public Point<int> CurrentPoint => new(currPoint);

        public int Score => score;

        public int Level => level;

        public int TotalLines => totalLines;

        public event ITetris.LineClearHandler? LineClear;
        public event ITetris.PieceDropHandler? PieceDrop;
        public event ITetris.BasicHandler? PieceMove;
        public event ITetris.BasicHandler? Loss;

        public enum MovementDir
        {
            Left,
            Right
        }

        public Game(int w, int h)
        {
            factory = new TetrominoeFactory();
            this.w = w;
            this.h = h;
            field = new int[w, h + FieldAdditionalSpace];
            basePoint = new Point<int>(w / 2, -1);
            clearedLines = new Stack<int>();
            totalPieces = new int[factory.GetPieces().Length];
            _Init();
        }

        public int FieldAt(int x, int y)
        {
            return _FieldAt(x, y);
        }

        public int GetPieceCount(int piece)
        {
            return totalPieces[piece];
        }

        public void RotatePiece(Polyminoe.RotationDir dir)
        {
            if (!_CanManipulatePiece()) return;
            _SetPiece(FieldEmpty);
            bool changed = true;
            curr.shape.Rotate(dir);
            if (_PieceCollides()) currPoint.x += 1;
            if (_PieceCollides()) currPoint.x -= 2;
            if (_PieceCollides())
            {
                currPoint.x += 1;
                curr.shape.Revert();
                changed = false;
            }
            _SetPiece((int)curr.name);
            if (changed) PieceMove?.Invoke(this);
        }

        public void MovePiece(MovementDir dir)
        {
            if (!_CanManipulatePiece()) return;
            _SetPiece(FieldEmpty);
            int ox = currPoint.x;
            if (MovementDir.Left == dir)
            {
                currPoint.x -= 1;
                if (_PieceCollides()) currPoint.x += 1;
            }
            else
            {
                currPoint.x += 1;
                if (_PieceCollides()) currPoint.x -= 1;
            }
            _SetPiece((int)curr.name);
            if (currPoint.x != ox) PieceMove?.Invoke(this);
        }

        public void HardDrop()
        {
            _SetPiece(FieldEmpty);
            int top = currPoint.y;
            for (int y = top + 1; y < h + 1; y++)
            {
                currPoint.y = y;
                if (_PieceCollides())
                    break;
            }
            currPoint.y -= 1;
            score += (currPoint.y - top) * 5;
            Advance();
        }

        public void Advance()
        {
            bool sw;
            if (linesWereCleared)
            {
                _ClearField();
                _DropBlocks();
                linesWereCleared = false;
            }
            _SetPiece(FieldEmpty);
            currPoint.y += 1;
            if (sw = _PieceCollides())
            {
                currPoint.y -= 1;
            }
            _SetPiece((int)curr.name);
            if (sw)
            {
                linesWereCleared = _CheckLines();
                if (_CheckLoss())
                {
                    Loss?.Invoke(this);
                    _Init();
                    return;
                }
                totalPieces[(int)curr.name]++;
                currPoint.Copy(basePoint);
                curr = next;
                next = factory.GetPiece();
                PieceDrop?.Invoke(this, curr);
                if (linesWereCleared) LineClear?.Invoke(this, clearedLines.ToArray());
            }
            else
            {
                PieceMove?.Invoke(this);
            }
        }

        public void DoMove(ITetris.Move move)
        {
            switch (move)
            {
                case ITetris.Move.Left:
                    MovePiece(MovementDir.Left);
                    break;
                case ITetris.Move.Right:
                    MovePiece(MovementDir.Right);
                    break;
                case ITetris.Move.RotateLeft:
                    RotatePiece(Polyminoe.RotationDir.Left);
                    break;
                case ITetris.Move.RotateRight:
                    RotatePiece(Polyminoe.RotationDir.Right);
                    break;
                case ITetris.Move.SoftDrop:
                    Advance();
                    break;
                case ITetris.Move.HardDrop:
                    HardDrop();
                    break;
            }
        }
    }
}
