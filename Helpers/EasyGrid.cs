using System.Windows;
using System.Windows.Controls;

namespace WPFTris.Helpers
{
    internal class EasyGrid<T> where T : UIElement, new()
    {
        private readonly Grid g;
        public Grid Grid { get { return g; } }
        private T[,] elements;
        public T this[int x, int y]
        {
            get { return elements[x, y]; }
            set
            {
                g.Children.Remove(elements[x, y]);
                elements[x, y] = value;
                Grid.SetRow(elements[x, y], y);
                Grid.SetColumn(elements[x, y], x);
                g.Children.Add(elements[x, y]);
            }
        }

#pragma warning disable CS8618
        public EasyGrid(int w, int h)
        {
            g = new Grid();
            for (int x = 0; x < w; x++)
            {
                g.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int y = 0; y < h; y++)
            {
                g.RowDefinitions.Add(new RowDefinition());
            }
            _PopulateGrid(g);
        }

        public EasyGrid(Grid g)
        {
            this.g = g;
            _PopulateGrid(g);
        }
#pragma warning restore CS8618

        private void _PopulateGrid(Grid g)
        {
            elements = new T[g.ColumnDefinitions.Count, g.RowDefinitions.Count];
            for (int x = 0; x < g.ColumnDefinitions.Count; x++)
            {
                for (int y = 0; y < g.RowDefinitions.Count; y++)
                {
                    elements[x, y] = new T();
                    Grid.SetRow(elements[x, y], y);
                    Grid.SetColumn(elements[x, y], x);
                    g.Children.Add(elements[x, y]);
                }
            }
        }
    }
}
