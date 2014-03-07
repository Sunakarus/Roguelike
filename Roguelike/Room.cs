using Microsoft.Xna.Framework;

namespace Roguelike
{
    internal class Room
    {
        public Map map;
        private Controller controller;
        public Point cornerNW, cornerSE;
        public int width, height;

        public Room(Controller controller, Map map, Point cornerLeftUp, Point cornerRightDown)
        {
            this.controller = controller;
            this.map = map;
            cornerNW = cornerLeftUp;
            cornerSE = cornerRightDown;
            width = cornerSE.X - cornerNW.X;
            height = cornerSE.Y - cornerNW.Y;
        }

        public void Update()
        {
        }

        public bool Intersects(Room r1)
        {
            Room r2 = this;

            return !(r2.cornerNW.X > r1.cornerSE.X - 1 ||
            r2.cornerSE.X - 1 < r1.cornerNW.X ||
            r2.cornerNW.Y > r1.cornerSE.Y - 1 ||
            r2.cornerSE.Y - 1 < r1.cornerNW.Y);
        }

        public bool ContainsPoint(Point point)
        {
            if (point.X >= cornerNW.X && point.X < cornerSE.X && point.Y >= cornerNW.Y && point.Y < cornerSE.Y)
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return map.IsIsolated(this).ToString();
        }
    }
}