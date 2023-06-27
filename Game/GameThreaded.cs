﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using WPFTris.Base;

namespace WPFTris.Game
{
    internal class GameThreaded
    {
        public const int PollInterval = 10;
        public const int InitialFallInterval = PollInterval * 50;
        public const int MinFallInterval = PollInterval * 5;

        private readonly Game g;
        private readonly Thread mainThread;
        private readonly Queue<Moves> moves;
        private readonly Stopwatch tickWatch;
        private int currentFallInterval;
        private int fallTimer;
        private bool operate;
        private readonly ManualResetEvent m;

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
                        if (moves.ElementAt(moves.Count - 1) != Moves.Slam)
                        {
                            moves.Enqueue(Moves.Advance);
                        }
                    }
                    else
                    {
                        moves.Enqueue(Moves.Advance);
                    }
                    fallTimer = currentFallInterval;
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
                switch (moves.Dequeue())
                {
                    case Moves.Advance:
                        g.Advance();
                        fallTimer = currentFallInterval;
                        break;
                    case Moves.RotateLeft:
                        g.RotatePiece(Polyminoe.RotationDir.Left);
                        break;
                    case Moves.RotateRight:
                        g.RotatePiece(Polyminoe.RotationDir.Right);
                        break;
                    case Moves.MoveLeft:
                        g.MovePiece(Game.MovementDir.Left);
                        break;
                    case Moves.MoveRight:
                        g.MovePiece(Game.MovementDir.Right);
                        break;
                    case Moves.Slam:
                        g.Slam();
                        fallTimer = currentFallInterval;
                        break;
                }
            }
        }

        private void _RecalculateFallInterval(int[] lines)
        {
            currentFallInterval = InitialFallInterval - g.Level * (PollInterval * 2);
            if (currentFallInterval < MinFallInterval) currentFallInterval = MinFallInterval;
        }

        public enum Moves
        {
            RotateLeft,
            RotateRight,
            MoveLeft,
            MoveRight,
            Advance,
            Slam
        }

        #region Game_fake_overrides
        public event Game.LineClearHandler LineClear
        {
            add { g.LineClear += value; }
            remove { g.LineClear -= value; }
        }

        public event Game.LossHandler Loss
        {
            add { g.Loss += value; }
            remove { g.Loss -= value; }
        }

        public event Game.PieceDropHandler PieceDrop
        {
            add { g.PieceDrop += value; }
            remove { g.PieceDrop -= value; }
        }

        public event Game.PieceMoveHandler PieceMove
        {
            add { g.PieceMove += value; }
            remove { g.PieceMove -= value; }
        }

        public event Game.RedrawHandler Redraw
        {
            add { g.Redraw += value; }
            remove { g.Redraw -= value; }
        }

        public TetrominoeFactory.Tetrominoe NextPiece => g.NextPiece;

        public Point<int> CurrentPoint => g.CurrentPoint;

        public int Score => g.Score;

        public int Level => g.Level;

        public int FieldAt(int x, int y)
        {
            return g.FieldAt(x, y);
        }
        #endregion

        public GameThreaded(int w, int h)
        {
            g = new Game(w, h);
            mainThread = new Thread(_Loop);
            moves = new Queue<Moves>();
            tickWatch = new Stopwatch();
            fallTimer = InitialFallInterval;
            currentFallInterval = InitialFallInterval;
            m = new ManualResetEvent(true);
            operate = true;
        }

        public void QueueMove(Moves move)
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
        }

        public void Resume()
        {
            m.Set();
        }

        public void Stop()
        {
            operate = false;
        }
    }
}