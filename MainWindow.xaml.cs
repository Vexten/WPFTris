using System;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WPFTris.Game;
using WPFTris.Helpers;
using WPFTris.Base;
using WPFTris.Graphics;

namespace WPFTris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    using Cell = Label;

    public partial class MainWindow : Window
    {
        private const int displayCellSize = 50;

        private delegate void SetLineColor(int line, Brush b);

        private readonly GameThreaded g;
        private readonly int w, h;
        private readonly EasyGrid<Cell>[] pieceDisplays;
        private readonly EasyGrid<Cell> easyField;
        private readonly DispatcherTimer lineAnimTimer;
        private readonly int baseAnimTicks;
        private int animTicks;
        private int[] lines;

#pragma warning disable CS8618
        public MainWindow()
        {
            InitializeComponent();
            w = Field.ColumnDefinitions.Count;
            h = Field.RowDefinitions.Count;
            g = new GameThreaded(Field.ColumnDefinitions.Count, Field.RowDefinitions.Count);

            pieceDisplays = new EasyGrid<Cell>[Enum.GetNames(typeof(TetrominoeFactory.Pieces)).Length];
            easyField = new EasyGrid<Cell>(Field);
            _SetPieceDisplays();
            _DisplayNext(g.NextPiece.name);

            g.PieceDrop += _NewPiece;
            g.LineClear += _LineClear;
            g.PieceMove += _DrawFieldLocal;
            g.Redraw += () => { Dispatcher.BeginInvoke(_DrawField); };
            g.Loss += () => { Dispatcher.BeginInvoke(_DrawField); };

            lineAnimTimer = new();
            lineAnimTimer.Interval = TimeSpan.FromMilliseconds(20);
            baseAnimTicks = 100;
            animTicks = baseAnimTicks;
            lineAnimTimer.Tick += _Animation;

            LevelLabel.Content = $"Level: {g.Level}";
            ScoreLabel.Content = $"Score: {g.Score}";

            g.Start();
        }
#pragma warning restore CS8618

        private void _SetPieceDisplays()
        {
            foreach (int piece in Enum.GetValues(typeof(TetrominoeFactory.Pieces)).Cast<int>())
            {
                pieceDisplays[piece] = _CreatePieceDisplay(TetrominoeFactory.GetTetrominoe((TetrominoeFactory.Pieces)piece).shape);
            }
        }

        private EasyGrid<Cell> _CreatePieceDisplay(Polyminoe piece)
        {
            Grid g = new();
            g.RowDefinitions.Add(new RowDefinition());
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.Height = displayCellSize;
            g.Width = displayCellSize;
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
            bottomRight = topLeft + bottomRight;
            for (int i = 0; i < bottomRight.x; i++)
            {
                g.ColumnDefinitions.Add(new ColumnDefinition());
                g.Width += displayCellSize;
            }
            for (int i = 0; i < bottomRight.y; i++)
            {
                g.RowDefinitions.Add(new RowDefinition());
                g.Height += displayCellSize;
            }
            EasyGrid<Cell> ret = new(g);
            foreach (var p in piece)
            {
                var pt = p + topLeft;
                _SetCellBackground(pt.x, pt.y, Brushes.Black, ret);
            }
            ret.Grid.HorizontalAlignment = HorizontalAlignment.Center;
            ret.Grid.VerticalAlignment = VerticalAlignment.Center;
            return ret;
        }

        private void _DisplayNext(TetrominoeFactory.Pieces p)
        {
            NextPieceView.Child = pieceDisplays[(int)p].Grid;
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
            this.lines = lines;
            Dispatcher.BeginInvoke(() => { LevelLabel.Content = $"Level: {g.Level}"; });
            Dispatcher.BeginInvoke(() => { ScoreLabel.Content = $"Score: {g.Score}"; });
            Dispatcher.BeginInvoke(() => { TotalLinesDisplay.Content = $"Lines: {g.TotalLines}"; });
            foreach (var line in lines)
            {
                Dispatcher.BeginInvoke(() => { FieldView.LineClear(line); });
            }
            lineAnimTimer.Start();
        }

        private void _Animation(object? sender, EventArgs e)
        {
            if (animTicks < 1)
            {
                animTicks = baseAnimTicks;
                lineAnimTimer.Stop();
                g.QueueMove(GameThreaded.Move.Advance);
                g.Resume();
                return;
            }
            byte c = (byte)(255 - (255 * animTicks / baseAnimTicks));
            SolidColorBrush b = new(Color.FromRgb(255, c, c));
            foreach (var line in lines)
            {
                for (int x = 0; x < w; x++)
                {
                    _SetCellBackground(x, line, b, easyField);
                }
            }
            animTicks--;
        }

        private void _SetCellBackground(int x, int y, Brush b, EasyGrid<Cell> g)
        {
            Cell l = g[x, y];
            l.Background = b;
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
                            _SetCellBackground(x, y, Brushes.Red, easyField);
                            FieldView.TileBlock(x, y, Color.FromRgb(255,0,0));
                            break;
                        case Game.Game.FieldEmpty:
                            _SetCellBackground(x, y, Brushes.White, easyField);
                            FieldView.TileBackground(x, y);
                            break;
                        default:
                            _SetCellBackground(x, y, Brushes.Black, easyField);
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
