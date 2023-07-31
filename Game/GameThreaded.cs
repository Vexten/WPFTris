using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using WPFTris.Base;
using System;

namespace WPFTris.Game
{
    internal class GameThreaded : ITetris, ITetrisThreaded
    {
        public const int PollInterval = 10;
        public const int InitialFallInterval = PollInterval * 50;
        public const int MinFallInterval = PollInterval * 5;

        private readonly ITetris g;
        private readonly Thread mainThread;
        private readonly Queue<ITetris.Move> moves;
        private readonly Stopwatch tickWatch;
        private int currentFallInterval;
        private int fallTimer;
        private bool operate;
        private readonly ManualResetEvent m;
        private bool isPaused;

        private void _Loop()
        {
            int sleepTime;
            g.LineClear += _RecalculateFallInterval;

            while (operate)
            {
                m.WaitOne();
                tickWatch.Start();

                fallTimer -= PollInterval;
                if (fallTimer < 1)
                {
                    if (moves.Count > 0)
                    {
                        ITetris.Move last = moves.ElementAt(moves.Count - 1);
                        if (last != ITetris.Move.HardDrop && last != ITetris.Move.SoftDrop)
                        {
                            moves.Enqueue(ITetris.Move.SoftDrop);
                        }
                    }
                    else
                    {
                        moves.Enqueue(ITetris.Move.SoftDrop);
                    }
                }
                _DoMoves();

                sleepTime = PollInterval - (int)tickWatch.ElapsedMilliseconds;
                if (sleepTime < 0) sleepTime = 0;
                tickWatch.Reset();
                Thread.Sleep(sleepTime);
            }
        }

        private void _DoMoves()
        {
            while (moves.Count > 0)
            {
                ITetris.Move move = moves.Dequeue();
                g.DoMove(move);
                if (move == ITetris.Move.SoftDrop || move == ITetris.Move.HardDrop)
                {
                    fallTimer = currentFallInterval;
                }
            }
        }

        private void _RecalculateFallInterval(ITetris t, int[] lines)
        {
            currentFallInterval = InitialFallInterval - g.Level * (PollInterval * 2);
            if (currentFallInterval < MinFallInterval) currentFallInterval = MinFallInterval;
        }

        #region Game_fake_overrides
        public event ITetris.LineClearHandler? LineClear
        {
            add { g.LineClear += value; }
            remove { g.LineClear -= value; }
        }

        public event ITetris.BasicHandler? Loss
        {
            add { g.Loss += value; }
            remove { g.Loss -= value; }
        }

        public event ITetris.PieceDropHandler? PieceDrop
        {
            add { g.PieceDrop += value; }
            remove { g.PieceDrop -= value; }
        }

        public event ITetris.BasicHandler? PieceMove
        {
            add { g.PieceMove += value; }
            remove { g.PieceMove -= value; }
        }

        public PolyminoeFactory.Piece NextPiece => g.NextPiece;

        public Point<int> CurrentPoint => g.CurrentPoint;

        public int Score => g.Score;

        public int Level => g.Level;

        public int TotalLines => g.TotalLines;

        public int FieldAt(int x, int y) => g.FieldAt(x, y);

        public int GetPieceCount(int piece) => g.GetPieceCount(piece);
        #endregion

        public bool IsPaused
        {
            get => isPaused;
        }

        public GameThreaded(ITetris g)
        {
            if (g is ITetrisThreaded)
            {
                throw new ArgumentException("Only use nonthreaded Tetris with this wrapper.");
            }
            this.g = g;
            mainThread = new Thread(_Loop);
            moves = new Queue<ITetris.Move>();
            tickWatch = new Stopwatch();
            fallTimer = InitialFallInterval;
            currentFallInterval = InitialFallInterval;
            m = new ManualResetEvent(true);
            operate = true;
            isPaused = false;
        }

        public void DoMove(ITetris.Move move)
        {
            moves.Enqueue(move);
        }

        public void Start()
        {
            mainThread.Start();
        }

        public void Pause()
        {
            m.Reset();
            isPaused = true;
        }

        public void Resume()
        {
            g.DoMove(ITetris.Move.SoftDrop);
            m.Set();
            isPaused = false;
        }

        public void Stop()
        {
            operate = false;
        }
    }
}
