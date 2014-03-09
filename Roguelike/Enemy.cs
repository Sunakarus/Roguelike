using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Roguelike
{
    internal class Enemy
    {
        public Texture2D texture;
        public Point position;
        public Controller controller;

        public enum Movement { Left, Up, Right, Down }

        public Enemy(Controller controller, Texture2D texture, Point position)
        {
            this.texture = texture;
            this.position = position;
            this.controller = controller;
        }

        public void Move()
        {
            Vector2 futurePos = Vector2.Zero;

            Vector2 playerPos = new Vector2(controller.player.position.X, controller.player.position.Y);
            Vector2 myPos = new Vector2(position.X, position.Y);
            Vector2 preferred = Vector2.Zero;



            double distance = (playerPos - myPos).Length();

            if (playerPos == myPos || (distance > controller.map.floorSize/8))
            {
                return;
            }

            futurePos = new Vector2(position.X, position.Y + 1);
            if ((playerPos - futurePos).Length() < distance && controller.map.IsWalkable(new Point((int)futurePos.X, (int)futurePos.Y)))
            {
                distance = (playerPos - futurePos).Length();
                preferred = futurePos;
            }

            futurePos = new Vector2(position.X, position.Y - 1);
            if ((playerPos - futurePos).Length() < distance && controller.map.IsWalkable(new Point((int)futurePos.X, (int)futurePos.Y)))
            {
                distance = (playerPos - futurePos).Length();
                preferred = futurePos;
            }
            futurePos = new Vector2(position.X - 1, position.Y);
            if ((playerPos - futurePos).Length() < distance && controller.map.IsWalkable(new Point((int)futurePos.X, (int)futurePos.Y)))
            {
                distance = (playerPos - futurePos).Length();
                preferred = futurePos;
            }
            futurePos = new Vector2(position.X + 1, position.Y);
            if ((playerPos - futurePos).Length() < distance && controller.map.IsWalkable(new Point((int)futurePos.X, (int)futurePos.Y)))
            {
                distance = (playerPos - futurePos).Length();
                preferred = futurePos;
            }

            if (preferred != Vector2.Zero && preferred != playerPos)
            {
                Point dir = new Point((int)preferred.X, (int)preferred.Y);
                if (controller.map.IsWalkable(dir))
                {
                    foreach (Enemy e in controller.map.enemyList)
                    {
                        if (e.position == dir)
                        {
                            return;
                        }
                    }
                    position = dir;
                }
            }
        }
    }
}