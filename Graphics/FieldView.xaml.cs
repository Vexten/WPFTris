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
            BackgroundTileProperty;

        private int w;
        private int h;
        private Tile[,] tiles;
        private BitmapImage tileOverlay;
        private BitmapImage backgroundTile;

        private struct Tile
        {
            public UIElement foreground;
            public UIElement background;
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
                    FrameworkPropertyMetadataOptions.AffectsRender,
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
                    FrameworkPropertyMetadataOptions.AffectsRender,
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
                    FrameworkPropertyMetadataOptions.AffectsRender,
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
            FieldView f = ((FieldView)d);
            f.HeightInTiles = (int)e.NewValue;
            f.Height = f.HeightInTiles * f.TileSize;
            f._ImGoingInsane();
        }

        private static void _WidthInTilesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FieldView f = ((FieldView)d);
            f.WidthInTiles = (int)e.NewValue;
            f.Width = f.WidthInTiles * f.TileSize;
            f._ImGoingInsane();
        }

        private static void _TileSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FieldView f = ((FieldView)d);
            f.TileSize = (int)e.NewValue;
            f.Height = f.HeightInTiles * f.TileSize;
            f.Width = f.WidthInTiles * f.TileSize;
            f._ImGoingInsane();
            f._SetBackground();
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

        public FieldView()
        {
            InitializeComponent();
        }

        public void TileBlock(int x, int y, Color color)
        {
            Rectangle r = (Rectangle)tiles[x, y].background;
            if (r.Fill is SolidColorBrush b)
            {
                if (b.Color != color)
                { 
                    r.Fill = new SolidColorBrush(color);
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
                    Field.Children.Add(r);
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
    }
}
