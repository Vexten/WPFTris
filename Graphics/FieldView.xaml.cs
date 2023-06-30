using System;
using System.Collections.Generic;
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
        private UIElement[,] tiles;

        static FieldView()
        {
            TileSizeProperty = DependencyProperty.Register(
                "TileSize",
                typeof(int),
                typeof(FieldView),
                new FrameworkPropertyMetadata(
                    0,
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
                    0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(_WidthInTilesChanged))
                );
            HeightInTilesProperty = DependencyProperty.Register(
                "HeightInTiles",
                typeof(int),
                typeof(FieldView),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(_HeightInTilesChanged))
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

        }

        public void TileBackground(int x, int y)
        {

        }

        private BitmapImage _LoadImageFromContentByUri(string uri)
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
                    Image t = new Image
                    {
                        Source = tileSprite,
                        Width = tileSize,
                        Height = tileSize,
                    };
                    RenderOptions.SetBitmapScalingMode(t, BitmapScalingMode.NearestNeighbor);
                    Canvas.SetLeft(t, x * tileSize);
                    Canvas.SetTop(t, y * tileSize);
                    Field.Children.Add(t);
                    tiles[x, y] = t;
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
                tiles = new Image[WidthInTiles, HeightInTiles];
                _CreateTiles(TileSize);
            }
            catch (Exception)
            {
                //tiles = new Border[WidthInTiles, HeightInTiles];
                _CreateDesignerPreview(TileSize);
            }
        }
    }
}
