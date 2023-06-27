using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WPFTris.Base
{
    internal class Polyminoe : IEnumerable<Point<int>>
    {
#pragma warning disable CS8618
        public class State
        {

            public State Right;
            public State Left;

            public Point<int>[] points;

            public State(Point<int>[] points)
            {
                this.points = points;
            }

            public State Get()
            {
                return this;
            }
        }
#pragma warning restore CS8618

        public enum RotationDir
        {
            Left,
            Right
        }

        private State currentState;
        private RotationDir revertDir;

        public State CurrentState { get { return currentState; } }

        public Polyminoe(ref State[] states)
        {
            for (int i = 0; i < states.Length - 1; i++)
            {
                states[i].Right = states[i + 1];
                states[i + 1].Left = states[i];
            }
            states[^1].Right = states[0];
            states[0].Left = states[^1];
            currentState = states[0];

        }

        public void Rotate(RotationDir dir)
        {
            switch (dir)
            {
                case RotationDir.Left:
                    currentState = currentState.Left;
                    revertDir = RotationDir.Right;
                    break;
                case RotationDir.Right:
                    currentState = currentState.Right;
                    revertDir = RotationDir.Left;
                    break;
            }
        }

        public void Revert()
        {
            Rotate(revertDir);
        }

        public IEnumerator<Point<int>> GetEnumerator()
        {
            return new PolyminoeEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class PolyminoeEnumerator : IEnumerator<Point<int>>
    {
        private readonly Polyminoe p;
        private readonly Polyminoe.State enumState;
        private readonly IEnumerator<Point<int>> enumerator;

        private Point<int> GetCurrent()
        {
            if (p.CurrentState == enumState)
            {
                return enumerator.Current;
            }
            throw new InvalidOperationException("Polyminoe state has changed during iteration.");
        }

        public PolyminoeEnumerator(Polyminoe p)
        {
            this.p = p;
            enumState = p.CurrentState;
            enumerator = enumState.points.Cast<Point<int>>().GetEnumerator();
        }

        public object Current => GetCurrent();

        Point<int> IEnumerator<Point<int>>.Current => GetCurrent();

        public void Dispose()
        {
            enumerator.Dispose();
        }

        public bool MoveNext()
        {
            if (p.CurrentState == enumState)
            {
                return enumerator.MoveNext();
            }
            throw new InvalidOperationException("Polyminoe state has changed during iteration.");
        }

        public void Reset()
        {
            enumerator.Reset();
        }
    }
}
