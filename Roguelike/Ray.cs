using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike
{
    class Ray
    {
        public Point origin;
        public Vector2 direction;
        Controller controller;
        int viewDistance;

        public Ray(Controller controller, Point origin, Vector2 direction, int viewDistance)
        {
            this.origin = origin;
            this.direction = direction;
            this.direction.Normalize();
            this.controller = controller;
            this.viewDistance = viewDistance;
        }

        public void LineSight(ref int[,] sightArray)
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
                if (controller.map.GetElementAtPos(pos) != (int)Map.Element.Door && controller.map.GetElementAtPos(pos) !=(int)Map.Element.Wall)
                {
                    sightArray[pos.X, pos.Y] = 0 + counter/10;
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
