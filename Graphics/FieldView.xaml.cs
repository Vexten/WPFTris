using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static readonly DependencyProperty TileSizeProperty, WidthInTilesProperty, HeightInTilesProperty;

        private int w;
        private int h;
        private Tile[,] tiles;

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

        private static BitmapImage _LoadImageFromContentByUri(string uri)
        {
            Uri tileSpriteUri = new(uri);
            StreamResourceInfo inf = Application.GetContentStream(tileSpriteUri);
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
            BitmapImage tileSprite = _LoadImageFromContentByUri(@"pack://application:,,,/img/tile_overlay.png");
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int xc = x * tileSize;
                    int yc = y * tileSize;
                    Image i = new Image
                    {
                        Source = tileSprite,
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
                    TileBackground(x, y);
                }
            }
        }

        private void _CreateDesignerPreview(int tileSize)
        {
            Thickness th = new Thickness(2);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Border b = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = th,
                        Width = tileSize,
                        Height = tileSize,
                    };
                    Canvas.SetLeft(b, x * tileSize);
                    Canvas.SetTop(b, y * tileSize);
                    Field.Children.Add(b);
                }
            }
        }

        private void _ImGoingInsane()
        {
            w = WidthInTiles;
            h = HeightInTiles;
            Field.Children.Clear();
            try
            {
                tiles = new Tile[WidthInTiles, HeightInTiles];
                _CreateTiles(TileSize);
            }
            catch (System.NullReferenceException ex)
            {
                Trace.WriteLine($"Exception {ex} on /img/tile_overlay.png, either the file is missing or you are currently inside of a designer");
                _CreateDesignerPreview(TileSize);
            }
        }
    }
}
