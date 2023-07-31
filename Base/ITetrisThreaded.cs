using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFTris.Base
{
    /// <summary>
    /// Marker interface<para/>
    /// Tetris class which implements it is itself threaded (game advances automatically)
    /// </summary>
    internal interface ITetrisThreaded
    {
        public bool IsPaused { get; }
        public void Start();
        public void Stop();
        public void Pause();
        public void Resume();
    }
}
