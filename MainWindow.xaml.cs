using System;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WPFTris.Game;
using WPFTris.Base;
using WPFTris.UserInteraction;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WPFTris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private static readonly Dictionary<int, Color> pieceColor = new Dictionary<int, Color>
        {
            [TetrominoeFactory.Tetrominoes.I] = Colors.Red,
            [TetrominoeFactory.Tetrominoes.O] = Colors.Red,
            [TetrominoeFactory.Tetrominoes.T] = Colors.Red,
            [TetrominoeFactory.Tetrominoes.L] = Colors.Blue,
            [TetrominoeFactory.Tetrominoes.J] = Colors.Blue,
            [TetrominoeFactory.Tetrominoes.S] = Colors.Blue,
            [TetrominoeFactory.Tetrominoes.Z] = Colors.Blue,
        };

        private readonly GameThreaded g;
        private readonly int w, h;
        private readonly Label[] pieceCount;
        private readonly PieceImage[] pieceDisplays;
        private readonly Controller c;

        private struct PieceImage
        {
            public Image Big;
            public Image Small;
        }

        public MainWindow()
        {
            InitializeComponent();
            w = FieldView.WidthInTiles;
            h = FieldView.HeightInTiles;
            g = new GameThreaded(new Game.Game(w, h));
            c = new Controller(g, 50);

            pieceDisplays = new PieceImage[pieceColor.Keys.Count];
            _SetPieceDisplays();
            _DisplayNext(g.NextPiece.name);
            pieceCount = new Label[pieceColor.Keys.Count];
            _FillPieceStats();

            g.PieceDrop += _NewPiece;
            g.LineClear += _LineClear;
            g.PieceMove += _DrawFieldLocal;
            g.Loss += (ITetris t) => { Dispatcher.BeginInvoke(_DrawField); };
            FieldView.LineClearAnimCompleted += _Resume;

            LevelLabel.Content = $"Level: {g.Level}";
            ScoreLabel.Content = $"Score: {g.Score}";

            g.Start();
            c.Start();
        }

        private void _FillPieceStats()
        {
            int i = 0;
            foreach (var piece in pieceColor.Keys)
            {
                int pInt = (int)piece;
                PieceStatGrid.RowDefinitions.Add(new RowDefinition());
                pieceCount[pInt] = new Label
                {
                    Content = "0",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                };
                pieceDisplays[pInt].Small.HorizontalAlignment = HorizontalAlignment.Center;
                pieceDisplays[pInt].Small.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetRow(pieceCount[pInt], i);
                Grid.SetRow(pieceDisplays[pInt].Small, i);
                Grid.SetColumn(pieceCount[pInt], 1);
                Grid.SetColumn(pieceDisplays[pInt].Small, 0);
                PieceStatGrid.Children.Add(pieceCount[pInt]);
                PieceStatGrid.Children.Add(pieceDisplays[pInt].Small);
                i++;
            }
        }

        private void _SetPieceDisplays()
        {
            TetrominoeFactory f = new TetrominoeFactory();
            foreach (var piece in pieceColor.Keys)
            {
                pieceDisplays[(int)piece] = _CreatePieceDisplay(f.GetPiece(piece).shape, piece);
            }
        }

        private PieceImage _CreatePieceDisplay(Polyminoe piece, int name)
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
            f.TileSize = MarkupPieceDisplay.TileSize;
            f.TileOverlay = @"img/tile_overlay.png";
            f.BackgroundTile = @"img/tile_transparent.png";
            f.WidthInTiles = bottomRight.x;
            f.HeightInTiles = bottomRight.y;
            foreach (var p in piece)
            {
                var pt = p + topLeft;
                f.TileBlock(pt.x, pt.y, pieceColor[name]);
            }
            PieceImage ret = new PieceImage();
            Size s = new Size(f.Width, f.Height);
            f.Measure(s);
            f.Arrange(new Rect(s));
            RenderTargetBitmap rt = new((int)f.Width,
                (int)f.Height,
                96,
                96,
                PixelFormats.Pbgra32);
            rt.Render(f);
            ret.Big = new Image
            {
                Source = rt,
                Width = f.Width,
                Height = f.Height,
            };
            RenderOptions.SetBitmapScalingMode(ret.Big, BitmapScalingMode.NearestNeighbor);
            double scale = (double)FieldView.TileSize / MarkupPieceDisplay.TileSize;
            rt = new((int)(f.Width * scale),
                (int)(f.Height * scale),
                (int)(96.0 * scale),
                (int)(96.0 * scale),
                PixelFormats.Pbgra32);
            rt.Render(f);
            ret.Small = new Image
            {
                Source = rt,
                Width = f.Width * scale,
                Height = f.Height * scale,
            };
            RenderOptions.SetBitmapScalingMode(ret.Small, BitmapScalingMode.NearestNeighbor);
            return ret;
        }

        private void _DisplayNext(int p)
        {
            NextPieceView.Child = pieceDisplays[p].Big;
        }

        private void _NewPiece(ITetris t, PolyminoeFactory.Piece p)
        {
            Dispatcher.BeginInvoke(_DrawField);
            Dispatcher.BeginInvoke(_DisplayNext, g.NextPiece.name);
            Dispatcher.BeginInvoke(() => { ScoreLabel.Content = $"Score: {g.Score}"; });
            Dispatcher.BeginInvoke(() => { pieceCount[p.name].Content = g.GetPieceCount(p.name).ToString(); });
        }

        private void _LineClear(ITetris t, int[] lines)
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
                    int tVal = g.FieldAt(x, y);
                    switch (tVal)
                    {
                        case Game.Game.FieldCleared:
                            FieldView.TileBlock(x, y, Colors.White);
                            break;
                        case Game.Game.FieldEmpty:
                            FieldView.TileBackground(x, y);
                            break;
                        default:
                            FieldView.TileBlock(x, y, pieceColor[tVal]);
                            break;
                    }
                }
            }
        }

        private void _DrawField()
        {
            _DrawFieldIn(0, 0, w - 1, h - 1);
        }

        private void _DrawFieldLocal(ITetris t)
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
            c.Stop();
            g.Stop();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Action<ITetris.Move> kh;
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                kh = (ITetris.Move m) => { c.Add(m); };
            }
            else
            {
                kh = g.DoMove;
            }
            switch (e.Key)
            {
                case Key.Left:
                    kh(ITetris.Move.Left);
                    break;
                case Key.Right:
                    kh(ITetris.Move.Right);
                    break;
                case Key.Down:
                    kh(ITetris.Move.SoftDrop);
                    break;
                case Key.Z:
                    kh(ITetris.Move.RotateLeft);
                    break;
                case Key.X:
                    kh(ITetris.Move.RotateRight);
                    break;
                case Key.Space:
                    kh(ITetris.Move.HardDrop);
                    break;  
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    c.Remove(ITetris.Move.Left);
                    break;
                case Key.Right:
                    c.Remove(ITetris.Move.Right);
                    break;
                case Key.Down:
                    c.Remove(ITetris.Move.SoftDrop);
                    break;
                case Key.Z:
                    c.Remove(ITetris.Move.RotateLeft);
                    break;
                case Key.X:
                    c.Remove(ITetris.Move.RotateRight);
                    break;
                case Key.Space:
                    c.Remove(ITetris.Move.HardDrop);
                    break;
            }
        }
    }
}
