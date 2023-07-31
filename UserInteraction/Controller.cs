using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WPFTris.Base;

namespace WPFTris.UserInteraction
{
    internal class Controller
    {
        private ITetris g;
        private Thread t;

        [Flags]
        protected enum MoveFlag
        {
            NONE = 0,
            L = 1,
            R = 2,
            LR = L | R,
            RL = 4,
            RR = 8,
            RLRR = RR | RL,
            SD = 16,
            HD = 32,
        }

        protected MoveFlag moveFlags;
        protected bool operate;
        protected int intervalMs;
        protected int msTillInput;

        protected static MoveFlag _Cast(ITetris.Move m)
        {
            switch (m)
            {
                case ITetris.Move.Left: return MoveFlag.L;
                case ITetris.Move.Right: return MoveFlag.R;
                case ITetris.Move.RotateLeft: return MoveFlag.RL;
                case ITetris.Move.RotateRight: return MoveFlag.RR;
                case ITetris.Move.SoftDrop: return MoveFlag.SD;
                case ITetris.Move.HardDrop: return MoveFlag.HD;
                default: return MoveFlag.NONE;
            }
        }

        protected void _Loop()
        {
            while (operate)
            {
                if (DoInput)
                {
                    if (msTillInput < 1)
                    {
                        _InputMoves();
                        msTillInput = intervalMs;
                    }
                    else
                    {
                        msTillInput -= 10;
                    }
                }
                Thread.Sleep(10);
            }
        }

        protected void _InputMoves()
        {
            if (moveFlags == MoveFlag.NONE) return;
            if ((moveFlags & MoveFlag.HD) == MoveFlag.HD)
            {
                g.DoMove(ITetris.Move.HardDrop);
                Remove(ITetris.Move.HardDrop);
                return;
            }
            MoveFlag lr = moveFlags & MoveFlag.LR;
            if (lr != 0 && lr != MoveFlag.LR)
            {
                if (lr == MoveFlag.L) g.DoMove(ITetris.Move.Left);
                else g.DoMove(ITetris.Move.Right);
            }
            MoveFlag rot = moveFlags & MoveFlag.RLRR;
            if (rot != 0 && rot != MoveFlag.RLRR)
            {
                if (rot == MoveFlag.LR) g.DoMove(ITetris.Move.RotateLeft);
                else g.DoMove(ITetris.Move.RotateRight);
            }
            if ((moveFlags & MoveFlag.SD) == MoveFlag.SD)
            {
                g.DoMove(ITetris.Move.SoftDrop);
            }
        }

        public bool DoInput;

        public Controller(ITetris game, int interval)
        {
            g = game;
            moveFlags = MoveFlag.NONE;
            DoInput = true;
            intervalMs = interval;
            operate = true;
            t = new Thread(_Loop);
        }

        public void Start() => t.Start();
        public void Stop()
        {
            operate = false;
        }

        public bool Add(ITetris.Move move)
        {
            MoveFlag m = _Cast(move);
            if ((moveFlags & m) == m) return false;
            moveFlags |= m;
            msTillInput = 0;
            return true;
        }

        public bool Remove(ITetris.Move move)
        {
            MoveFlag m = _Cast(move);
            if ((moveFlags & m) != m) return false;
            moveFlags &= ~m;
            msTillInput = 0;
            return true;
        }
    }
}
