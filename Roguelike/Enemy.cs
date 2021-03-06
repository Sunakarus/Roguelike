﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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
        public int dropRate;
        public int expValue;

        public bool asleep = true;
        public int viewDistance;

        public bool isSeen = false;

        public Ray rayToPlayer;

        public enum EnemyType : int { Skeleton, Bat } //ADDENEMY

        public enum Movement { Left, Up, Right, Down }

        public Enemy(Controller controller, Point position, EnemyType enemyType)
        {
            this.position = position;
            this.controller = controller;

            switch (enemyType) //ADDENEMY
            {
                case EnemyType.Skeleton:
                    {
                        texture = ContentManager.tSkeleton;
                        damage = 2;
                        maxHealth = 10;
                        viewDistance = 4;
                        expValue = 5;
                        dropRate = 8;
                        break;
                    }
                case EnemyType.Bat:
                    {
                        texture = ContentManager.tBat;
                        damage = 1;
                        maxHealth = 8;
                        viewDistance = 6;
                        dropRate = 6;
                        expValue = 3;
                        break;
                    }
            }
            //DIFFICULTY SCALING
            int scale = (int)(controller.level / 3);
            maxHealth += scale;
            damage += scale;
            expValue += scale;

            health = maxHealth;
            if (CanSeePlayer())
            {
                asleep = false;
            }
        }

        public bool CanSeePlayer()
        {
            Vector2 playerPos = new Vector2(controller.player.position.X, controller.player.position.Y);
            Vector2 myPos = new Vector2(position.X, position.Y);

            rayToPlayer = new Ray(controller, position, playerPos - myPos, viewDistance);

            List<Ray> rayList = new List<Ray>();
            List<Point> pointList = new List<Point>();
            Point tempPoint;

            for (int ix = position.X - viewDistance; ix <= position.X + viewDistance; ix++)
            {
                for (int iy = position.Y - viewDistance; iy <= position.Y + viewDistance; iy++)
                {
                    tempPoint = new Point(ix, iy);
                    //((x1-x)^2 + (y1-y)^2) < r^2
                    if (Math.Pow(tempPoint.X - position.X, 2) + Math.Pow(tempPoint.Y - position.Y, 2) < Math.Pow(viewDistance, 2))
                    {
                        pointList.Add(tempPoint);
                    }
                }
            }
            foreach (Point p in pointList)
            {
                rayList.Add(new Ray(controller, position, new Vector2(p.X - position.X, p.Y - position.Y), viewDistance));
            }

            foreach (Ray r in rayList)
            {
                if (r.CanSee(controller.player.position))
                {
                    return true;
                }
            }

            return false;
        }

        public void Death()
        {
            controller.player.experience += expValue;
            DropLoot();
        }

        public void DropLoot()
        {
            if (controller.random.Next(dropRate) == dropRate - 1)
            {
                int[,] tempArray = controller.map.StringToArray(controller.map.mapString);

                if (tempArray[position.X, position.Y] == (int)Map.Element.Stairs)
                {
                    return;
                }

                controller.map.itemList.Add(new Item(controller, position, (Map.Element)tempArray[position.X, position.Y]));
                tempArray[position.X, position.Y] = (int)Map.Element.Item;
                controller.map.mapString = controller.map.ArrayToString(tempArray);
            }
        }

        public void Update()
        {
            if (!asleep)
            {
                Vector2 playerPos = new Vector2(controller.player.position.X, controller.player.position.Y);
                Vector2 myPos = new Vector2(position.X, position.Y);

                if ((playerPos - myPos).Length() <= viewDistance * 2)
                {
                    Move();
                }
                else
                {
                    asleep = true;
                }
            }
            else if (CanSeePlayer())
            {
                asleep = false;
            }
            else
            {
                Roam();
            }
        }

        public void Roam()
        {
            List<Point> possibleMoves = new List<Point>();
            possibleMoves.Add(new Point(position.X - 1, position.Y));
            possibleMoves.Add(new Point(position.X + 1, position.Y));
            possibleMoves.Add(new Point(position.X, position.Y - 1));
            possibleMoves.Add(new Point(position.X, position.Y + 1));

            for (int i = possibleMoves.Count - 1; i > -1; i--)
            {
                if (controller.map.OutOfBounds(possibleMoves[i]) || !controller.map.IsWalkable(possibleMoves[i]))
                {
                    possibleMoves.RemoveAt(i);
                    continue;
                }
                foreach (Enemy e in controller.map.enemyList)
                {
                    if (possibleMoves[i] == e.position)
                    {
                        possibleMoves.RemoveAt(i);
                        break;
                    }
                }
            }

            if (possibleMoves.Count == 0)
            {
                return;
            }

            int randMove = controller.random.Next(possibleMoves.Count);
            position = possibleMoves[randMove];
        }

        public void Move()
        {
            Vector2 futurePos = Vector2.Zero;

            Vector2 playerPos = new Vector2(controller.player.position.X, controller.player.position.Y);
            Vector2 myPos = new Vector2(position.X, position.Y);
            Vector2 preferred = Vector2.Zero;
            double distance = (playerPos - myPos).Length();

            if (playerPos == myPos || Attack())
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

            float dealtDamage = damage - controller.player.defense;
            if (dealtDamage > 0)
            {
                controller.player.health -= damage;
            }
            return true;
        }
    }
}