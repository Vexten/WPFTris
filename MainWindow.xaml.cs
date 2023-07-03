using System;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WPFTris.Game;
using WPFTris.Base;
using WPFTris.Graphics;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace WPFTris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private const int displayCellSize = 40;

        private readonly GameThreaded g;
        private readonly int w, h;
        private readonly FieldView[] pieceDisplays;

#pragma warning disable CS8618
        public MainWindow()
        {
            InitializeComponent();
            w = FieldView.WidthInTiles;
            h = FieldView.HeightInTiles;
            g = new GameThreaded(w, h);

            pieceDisplays = new FieldView[Enum.GetNames(typeof(TetrominoeFactory.Pieces)).Length];
            _SetPieceDisplays();
            _DisplayNext(g.NextPiece.name);

            g.PieceDrop += _NewPiece;
            g.LineClear += _LineClear;
            g.PieceMove += _DrawFieldLocal;
            g.Redraw += () => { Dispatcher.BeginInvoke(_DrawField); };
            g.Loss += () => { Dispatcher.BeginInvoke(_DrawField); };
            FieldView.LineClearAnimCompleted += _Resume;

            LevelLabel.Content = $"Level: {g.Level}";
            ScoreLabel.Content = $"Score: {g.Score}";

            g.Start();
        }
#pragma warning restore CS8618

        private void _SetPieceDisplays()
        {
            FieldView field = new();
            field.TileSize = displayCellSize;
            field.TileOverlay = @"img/tile_overlay.png";
            field.BackgroundTile = @"img/tile_black.png";
            foreach (int piece in Enum.GetValues(typeof(TetrominoeFactory.Pieces)).Cast<int>())
            {
                pieceDisplays[piece] = _CreatePieceDisplay(TetrominoeFactory.GetTetrominoe((TetrominoeFactory.Pieces)piece).shape);
            }
        }

        private FieldView _CreatePieceDisplay(Polyminoe piece)
        {
            Point<int> topLeft = new(0,0);
            Point<int> bottomRight = new(0,0);
            foreach (var p in piece)
            {
                if (p.x < topLeft.x) topLeft.x = p.x;
                else if (p.x > bottomRight.x) bottomRight.x = p.x;
                if (p.y < topLeft.y) topLeft.y = p.y;
                else if (p.y > bottomRight.y) bottomRight.y = p.y;
            }
            topLeft = -topLeft;
            bottomRight = topLeft + bottomRight + new Point<int>(1,1);
            FieldView f = new();
            f.TileSize = displayCellSize;
            f.TileOverlay = @"img/tile_overlay.png";
            f.BackgroundTile = @"img/tile_black.png";
            f.WidthInTiles = bottomRight.x;
            f.HeightInTiles = bottomRight.y;
            Size s = new Size(f.Width, f.Height);
            for (int x = 0; x < f.WidthInTiles; x++)
            {
                for (int y = 0; y < f.HeightInTiles; y++)
                {
                    f.TileBackground(x, y);
                }
            }
            foreach (var p in piece)
            {
                var pt = p + topLeft;
                f.TileBlock(pt.x, pt.y, Color.FromRgb(0, 0, 255));
            }
            return f;
        }

        private void _DisplayNext(TetrominoeFactory.Pieces p)
        {
            NextPieceView.Child = pieceDisplays[(int)p];
        }

        private void _NewPiece()
        {
            Dispatcher.BeginInvoke(_DrawField);
            Dispatcher.BeginInvoke(_DisplayNext, g.NextPiece.name);
            Dispatcher.BeginInvoke(() => { ScoreLabel.Content = $"Score: {g.Score}"; });
        }

        private void _LineClear(int[] lines)
        {
            g.Pause();
            Dispatcher.BeginInvoke(() => { LevelLabel.Content = $"Level: {g.Level}"; });
            Dispatcher.BeginInvoke(() => { ScoreLabel.Content = $"Score: {g.Score}"; });
            Dispatcher.BeginInvoke(() => { TotalLinesDisplay.Content = $"Lines: {g.TotalLines}"; });
            foreach (var line in lines)
            {
                Dispatcher.BeginInvoke(() => { FieldView.LineClear(line); });
            }
        }

        private void _Resume(object? sender, EventArgs e)
        {
            g.Resume();
            _DrawField();
        }

        private void _DrawFieldIn(int x1, int y1, int x2, int y2)
        {
            for (int x = x1; x < x2 + 1; x++)
            {
                for (int y = y1; y < y2 + 1; y++)
                {
                    switch (g.FieldAt(x, y))
                    {
                        case Game.Game.FieldCleared:
                            FieldView.TileBlock(x, y, Color.FromRgb(255,0,0));
                            break;
                        case Game.Game.FieldEmpty:
                            FieldView.TileBackground(x, y);
                            break;
                        default:
                            FieldView.TileBlock(x, y, Color.FromRgb(0, 0, 255));
                            break;
                    }
                }
            }
        }

        private void _DrawField()
        {
            _DrawFieldIn(0, 0, w - 1, h - 1);
        }

        private void _DrawFieldLocal()
        {
            Point<int> p = g.CurrentPoint;
            Dispatcher.BeginInvoke(_DrawFieldIn,
                _Clamp(0, p.x - 3, w - 1),
                _Clamp(0, p.y - 3, h - 1),
                _Clamp(0, p.x + 3, w - 1),
                _Clamp(0, p.y + 3, h - 1));
        }

        private static T _Clamp<T>(T min, T n, T max) where T : INumber<T>
        {
            if (n < min) return min;
            if (n > max) return max;
            return n;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            g.Stop();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    g.QueueMove(GameThreaded.Move.MoveLeft);
                    break;
                case Key.Right:
                    g.QueueMove(GameThreaded.Move.MoveRight);
                    break;
                case Key.Down:
                    g.QueueMove(GameThreaded.Move.Advance);
                    break;
                case Key.Z:
                    g.QueueMove(GameThreaded.Move.RotateLeft);
                    break;
                case Key.X:
                    g.QueueMove(GameThreaded.Move.RotateRight);
                    break;
                case Key.Space:
                    g.QueueMove(GameThreaded.Move.Slam);
                    break;  
            }
        }
    }
}
