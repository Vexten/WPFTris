using System.Numerics;

namespace WPFTris.Base
{
    internal struct Point<T> where T : INumber<T>
    {
        public T x;
        public T y;

        public Point(T x, T y)
        {
            this.x = x;
            this.y = y;
        }

        public Point(Point<T> p)
        {
            x = p.x;
            y = p.y;
        }

        public void Copy<Tp>(Point<Tp> p) where Tp : INumber<Tp>
        {
            x = T.CreateChecked(p.x);
            y = T.CreateChecked(p.y);
        }

        public static Point<T> operator +(Point<T> left, Point<T> right) => new(left.x + right.x, left.y + right.y);
        public static Point<T> operator -(Point<T> left, Point<T> right) => new(left.x - right.x, left.y - right.y);
        public static Point<T> operator -(Point<T> p) => new(-p.x, -p.y);
    }
}
