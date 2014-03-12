using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Roguelike
{
    internal class Enemy
    {
        public Texture2D texture;
        public Point position;
        public Controller controller;

        public float health;
        public float maxHealth;
        public float damage;

        public enum EnemyType { Skeleton = 0, Bat = 1}
        public enum Movement { Left, Up, Right, Down }
        
        public Enemy(Controller controller, Point position, EnemyType enemyType)
        {
            this.position = position;
            this.controller = controller;

            switch (enemyType)
            {
                case EnemyType.Skeleton:
                    {
                        texture = ContentManager.tSkeleton;
                        damage = 2;
                        maxHealth = 10;
                        break;
                    }
                case EnemyType.Bat:
                    {
                        texture = ContentManager.tBat;
                        damage = 1;
                        maxHealth = 8;
                        break;
                    }
            }
            health = maxHealth;
        }

        public void Move()
        {
            Vector2 futurePos = Vector2.Zero;

            Vector2 playerPos = new Vector2(controller.player.position.X, controller.player.position.Y);
            Vector2 myPos = new Vector2(position.X, position.Y);
            Vector2 preferred = Vector2.Zero;
            double distance = (playerPos - myPos).Length();

            if (playerPos == myPos || (distance > controller.map.floorSize / 8) || Attack())
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

        public bool Attack()
        {
            Point playerPos = controller.player.position;
            Point pointLeft, pointRight, pointUp, pointDown;
            pointLeft = new Point(position.X - 1, position.Y);
            pointRight = new Point(position.X + 1, position.Y);
            pointUp = new Point(position.X, position.Y - 1);
            pointDown = new Point(position.X, position.Y + 1);

            if (!(playerPos == pointLeft || playerPos == pointRight || playerPos == pointUp || playerPos == pointDown))
            {
                return false;
            }

            controller.player.health -= damage;
            return true;
        }
    }
}