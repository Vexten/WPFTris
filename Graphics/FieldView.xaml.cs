using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;

namespace WPFTris.Graphics
{
    /// <summary>
    /// Логика взаимодействия для FieldView.xaml
    /// </summary>
    public partial class FieldView : UserControl
    {
        public static readonly DependencyProperty
            TileSizeProperty,
            WidthInTilesProperty,
            HeightInTilesProperty,
            TileOverlayProperty,
            BackgroundTileProperty,
            LineClearFadeOutProperty;

        private int w;
        private int h;
        private Tile[,] tiles;
        private BitmapImage tileOverlay;
        private BitmapImage backgroundTile;
        private Dictionary<Color, SolidColorBrush> brushCache;
        private DoubleAnimation tileSizeAnim;
        private DoubleAnimation tilePosAnim;
        private Storyboard tileFadeOut;
        private int animatedTileCounter;

        private struct Tile
        {
            public FrameworkElement foreground;
            public FrameworkElement background;
        }

        static FieldView()
        {
            TileSizeProperty = DependencyProperty.Register(
                "TileSize",
                typeof(int),
                typeof(FieldView),
                new FrameworkPropertyMetadata(
                    1,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsParentArrange |
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure,
                    new PropertyChangedCallback(_TileSizeChanged),
                    new CoerceValueCallback(_ClampToOne))
                );
            WidthInTilesProperty = DependencyProperty.Register(
                "WidthInTiles",
                typeof(int),
                typeof(FieldView),
                new FrameworkPropertyMetadata(
                    1,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsParentArrange |
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure,
                    new PropertyChangedCallback(_WidthInTilesChanged),
                    new CoerceValueCallback(_ClampToOne))
                );
            HeightInTilesProperty = DependencyProperty.Register(
                "HeightInTiles",
                typeof(int),
                typeof(FieldView),
                new FrameworkPropertyMetadata(
                    1,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsParentArrange |
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure,
                    new PropertyChangedCallback(_HeightInTilesChanged),
                    new CoerceValueCallback(_ClampToOne))
                );
            TileOverlayProperty = DependencyProperty.Register(
                "TileOverlay",
                typeof(string),
                typeof(FieldView),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(_TileOverlayChanged))
                );
            BackgroundTileProperty = DependencyProperty.Register(
                "BackgroundTile",
                typeof(string),
                typeof(FieldView),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(_BackgroundTileChanged))
                );
            LineClearFadeOutProperty = DependencyProperty.Register(
                "LineClearFadeOut",
                typeof(int),
                typeof(FieldView),
                new FrameworkPropertyMetadata(
                    200,
                    new PropertyChangedCallback(_LineFadeOutChanged))
                );
        }

        #region DependencyPropertyHandlers
        private static void _LineFadeOutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.NewValue == (int)e.OldValue) return;
            FieldView f = (FieldView)d;
            f.LineClearFadeOut = (int)e.NewValue;
            f._ChangeAnimTime();
        }

        private static void _BackgroundTileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FieldView f = (FieldView)d;
            string path = (string)e.NewValue;
            if (path != string.Empty)
            {
                if (path != (string)e.OldValue)
                {
                    f.backgroundTile = _BMPFromContentByUri($"pack://application:,,,/{path}");
                }
            }
            f.BackgroundTile = path;
            f._SetBackground();
        }

        private static void _TileOverlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FieldView f = (FieldView)d;
            string path = (string)e.NewValue;
            if (path != string.Empty)
            {
                if (path != (string)e.OldValue)
                {
                    f.tileOverlay = _BMPFromContentByUri($"pack://application:,,,/{path}");
                }
            }
            f.TileOverlay = path;
            f.Field.Children.Clear();
            f._CreateTiles(f.TileSize);
        }

        private static object _ClampToOne(DependencyObject d, object baseValue)
        {
            return (int)baseValue > 0 ? (int)baseValue : 1;
        }

        private static void _HeightInTilesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.NewValue == (int)e.OldValue) return;
            FieldView f = ((FieldView)d);
            f.HeightInTiles = (int)e.NewValue;
            f.Height = f.HeightInTiles * f.TileSize;
            f._ImGoingInsane();
        }

        private static void _WidthInTilesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.NewValue == (int)e.OldValue) return;
            FieldView f = ((FieldView)d);
            f.WidthInTiles = (int)e.NewValue;
            f.Width = f.WidthInTiles * f.TileSize;
            f._ImGoingInsane();
        }

        private static void _TileSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.NewValue == (int)e.OldValue) return;
            FieldView f = ((FieldView)d);
            f.TileSize = (int)e.NewValue;
            f.Height = f.HeightInTiles * f.TileSize;
            f.Width = f.WidthInTiles * f.TileSize;
            f._ImGoingInsane();
            f._SetBackground();
            f._ChangeAnimScale();
        }
        #endregion

        private static BitmapImage _BMPFromContentByUri(string uri)
        {
            Uri imageUri = new(uri);
            StreamResourceInfo inf;
            inf = Application.GetContentStream(imageUri);
            if (inf == null)
            {
                Trace.WriteLine($"{uri} doesn't exist, or you are currently inside of Designer.\n" +
                    $"Falling back on error sprite.");
                inf = Application.GetResourceStream(new Uri(@"pack://application:,,,/WPFTris;component/img/error_sprite.png"));
            }
            BitmapImage image = new();
            image.BeginInit();
            image.StreamSource = inf.Stream;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            return image;
        }

        public int TileSize
        {
            get { return (int)GetValue(TileSizeProperty); }
            set { SetValue(TileSizeProperty, value); }
        }

        public int WidthInTiles
        {
            get { return (int)GetValue(WidthInTilesProperty); }
            set { SetValue(WidthInTilesProperty, value); }
        }

        public int HeightInTiles
        {
            get { return (int)GetValue(HeightInTilesProperty); }
            set { SetValue(HeightInTilesProperty, value); }
        }

        public string BackgroundTile
        {
            get { return (string)GetValue(BackgroundTileProperty); }
            set { SetValue(BackgroundTileProperty, value); }
        }

        public string TileOverlay
        {
            get { return (string)GetValue(TileOverlayProperty); }
            set { SetValue(TileOverlayProperty, value); }
        }

        public int LineClearFadeOut
        {
            get { return (int)GetValue(LineClearFadeOutProperty); }
            set { SetValue(LineClearFadeOutProperty, value); }
        }

        public event EventHandler LineClearAnimCompleted;

        public FieldView()
        {
            InitializeComponent();
            brushCache = new();
            tileSizeAnim = new();
            tilePosAnim = new();
            tileSizeAnim.To = 0;
            tileSizeAnim.FillBehavior = FillBehavior.Stop;
            tilePosAnim.FillBehavior = FillBehavior.Stop;
            Storyboard.SetTargetProperty(tileSizeAnim, new PropertyPath(WidthProperty));
            Storyboard.SetTargetProperty(tileSizeAnim, new PropertyPath(HeightProperty));
            Storyboard.SetTargetProperty(tilePosAnim, new PropertyPath(Canvas.LeftProperty));
            Storyboard.SetTargetProperty(tilePosAnim, new PropertyPath(Canvas.TopProperty));
            tileFadeOut = new();
            tileFadeOut.Children.Add(tileSizeAnim);
            tileFadeOut.Children.Add(tilePosAnim);
            tileFadeOut.Completed += _TileAnimCompleted;
            animatedTileCounter = 0;
        }

        public void TileBlock(int x, int y, Color color)
        {
            Rectangle r = (Rectangle)tiles[x, y].background;
            if (r.Fill is SolidColorBrush b)
            {
                if (b.Color != color)
                {
                    SolidColorBrush? brush;
                    if (brushCache.TryGetValue(color, out brush))
                    {
                        r.Fill = brush;
                    }
                    else
                    {
                        brushCache[color] = new SolidColorBrush(color);
                        r.Fill = brushCache[color];
                    }
                }
            }
            tiles[x, y].foreground.Visibility = Visibility.Visible;
            tiles[x ,y].background.Visibility = Visibility.Visible;
        }

        public void TileBackground(int x, int y)
        {
            tiles[x, y].foreground.Visibility = Visibility.Hidden;
            tiles[x, y].background.Visibility = Visibility.Hidden;
        }

        public void LineClear(int line)
        {
            if (line < 0) return;
            for (int x = 0; x < w; x++)
            {
                Tile t = tiles[x, line];
                ((Rectangle)t.background).Fill = Brushes.White;
                tileFadeOut.Seek(t.foreground, TimeSpan.Zero, TimeSeekOrigin.BeginTime);
                tileFadeOut.Resume(t.foreground);
                tileFadeOut.Seek(t.background, TimeSpan.Zero, TimeSeekOrigin.BeginTime);
                tileFadeOut.Resume(t.background);
                animatedTileCounter += 2;
            }
        }

        private void _CreateTiles(int tileSize)
        {
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int xc = x * tileSize;
                    int yc = y * tileSize;
                    Image i = new Image
                    {
                        Source = tileOverlay,
                        Width = tileSize,
                        Height = tileSize,
                    };
                    RenderOptions.SetBitmapScalingMode(i, BitmapScalingMode.NearestNeighbor);
                    Canvas.SetLeft(i, xc);
                    Canvas.SetTop(i, yc);
                    Canvas.SetZIndex(i, 1);
                    Rectangle r = new Rectangle
                    {
                        Fill = Brushes.White,
                        Height = tileSize,
                        Width = tileSize,
                    };
                    Canvas.SetLeft(r, xc);
                    Canvas.SetTop(r, yc);
                    Canvas.SetZIndex(r, 0);
                    Field.Children.Add(i);
                    tileFadeOut.Begin(i, true);
                    tileFadeOut.Pause(i);
                    i.Visibility = Visibility.Hidden;
                    Field.Children.Add(r);
                    tileFadeOut.Begin(r, true);
                    tileFadeOut.Pause(r);
                    r.Visibility = Visibility.Hidden;
                    tiles[x, y] = new Tile { foreground = i, background = r };
                }
            }
        }

        private void _ImGoingInsane()
        {
            w = WidthInTiles;
            h = HeightInTiles;
            Field.Children.Clear();
            tiles = new Tile[WidthInTiles, HeightInTiles];
            _CreateTiles(TileSize);
        }

        private void _SetBackground()
        {
            Field.Background = new ImageBrush
            {
                ImageSource = backgroundTile,
                TileMode = TileMode.Tile,
                Viewport = new Rect(new Point(0, 0), new Point(TileSize, TileSize)),
                ViewportUnits = BrushMappingMode.Absolute
            };
        }

        private void _ChangeAnimTime()
        {
            Duration d = new Duration(TimeSpan.FromMilliseconds(LineClearFadeOut));
            tileSizeAnim.Duration = d;
            tilePosAnim.Duration = d;
            foreach (var tile in tiles)
            {
                tileFadeOut.Remove(tile.foreground);
                tileFadeOut.Begin(tile.foreground, true);
                tileFadeOut.Pause(tile.foreground);
                tileFadeOut.Remove(tile.foreground);
                tileFadeOut.Begin(tile.foreground, true);
                tileFadeOut.Pause(tile.foreground);
            }
        }

        private void _ChangeAnimScale()
        {
            tileSizeAnim.From = TileSize;
            tilePosAnim.By = TileSize / 2;
        }

        private void _TileAnimCompleted(object? sender, EventArgs e)
        {
            animatedTileCounter--;
            if (animatedTileCounter == 0)
            {
                LineClearAnimCompleted?.Invoke(this, new EventArgs());
            }
        }
    }
}
