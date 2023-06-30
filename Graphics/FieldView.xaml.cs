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
        private ImageDrawing[,] tiles;

        static FieldView()
        {
            TileSizeProperty = DependencyProperty.Register(
                "TileSize",
                typeof(int),
                typeof(FieldView),
                new FrameworkPropertyMetadata(
                    20,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender)
                );
            WidthInTilesProperty = DependencyProperty.Register(
                "Width",
                typeof(int),
                typeof(FieldView),
                new FrameworkPropertyMetadata(
                    10,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender)
                );
            HeightInTilesProperty = DependencyProperty.Register(
                "Height",
                typeof(int),
                typeof(FieldView),
                new FrameworkPropertyMetadata(
                    20,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender)
                );
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
            tiles = new ImageDrawing[WidthInTiles, HeightInTiles];
            _CreateTiles(TileSize);
        }

        public void TileBlock(int x, int y, Color color)
        {

        }

        public void TileBackground(int x, int y)
        {

        }

        private void _CreateTiles(int tileSize)
        {
            Uri tileSpriteUri = new("pack://application:,,,/img/tile_overlay.png");
            BitmapImage tileSprite = new(tileSpriteUri);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    tiles[x, y] = new(tileSprite, new Rect(x*tileSize, y*tileSize, tileSize, tileSize));
                    AddChild(tiles[x, y]);
                }
            }
        }
    }
}
