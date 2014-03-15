using Microsoft.Xna.Framework;
using System;

namespace Roguelike
{
    internal class Ray
    {
        public Point origin;
        public Vector2 direction;
        private Controller controller;
        private int viewDistance;

        public Ray(Controller controller, Point origin, Vector2 direction, int viewDistance)
        {
            this.origin = origin;
            this.direction = direction;
            this.direction.Normalize();
            this.controller = controller;
            this.viewDistance = viewDistance;
        }

        public bool CanSee(Point pointPos)
        {
            int counter = -1;
            do
            {
                counter++;
                Vector2 newPos = new Vector2(origin.X + direction.X * counter, origin.Y + direction.Y * counter);

                newPos.X = (int)Math.Floor((float)newPos.X);
                newPos.Y = (int)Math.Floor((float)newPos.Y);

                Point pos = new Point((int)newPos.X, (int)newPos.Y);

                if (controller.map.OutOfBounds(pos))
                {
                    break;
                }
                if (pointPos == pos)
                {
                    return true;
                }
                if (controller.map.GetElementAtPos(pos) == (int)Map.Element.Door || controller.map.GetElementAtPos(pos) == (int)Map.Element.Wall)
                {
                    return false;
                }
            }
            while (counter != viewDistance);
            return false;
        }

        public void CreateSightArray(ref int[,] sightArray)
        {
            int counter = -1;
            do
            {
                counter++;
                Vector2 newPos = new Vector2(origin.X + direction.X * counter, origin.Y + direction.Y * counter);

                newPos.X = (int)Math.Floor((float)newPos.X);
                newPos.Y = (int)Math.Floor((float)newPos.Y);

                Point pos = new Point((int)newPos.X, (int)newPos.Y);

                if (controller.map.OutOfBounds(pos))
                {
                    break;
                }
                if (controller.map.GetElementAtPos(pos) != (int)Map.Element.Door && controller.map.GetElementAtPos(pos) != (int)Map.Element.Wall)
                {
                    sightArray[pos.X, pos.Y] = 0 + counter / 10;
                }
                else
                {
                    sightArray[pos.X, pos.Y] = 0 + counter / 10;
                    break;
                }
            }
            while (counter != viewDistance);
        }
    }
}