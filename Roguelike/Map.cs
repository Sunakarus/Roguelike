using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Roguelike
{
    internal class Map
    {
        public int[,] mapArray;
        private Controller controller;

        private enum MapEntity : int { Nothing = 0, Wall = 1, Player = 2 }

        public int floorSize;
        public List<Room> roomList = new List<Room>();

        public Map(Controller controller, int floorSize)
        {
            this.controller = controller;
            this.floorSize = floorSize;

            /*for (int i = 0; i < 5; i++ )
            {
                GenerateRandomRoom();
            }*/
            roomList.Add(new Room(controller, this, new Point(1, 1), new Point(2, 3)));
            roomList.Add(new Room(controller, this, new Point(0, 0), new Point(4, 4)));
        }

        public void GenerateFloor()
        {
            
            mapArray = new int[floorSize, floorSize];

            for (int ix = 0; ix < mapArray.GetLength(0); ix++)
            {
                for (int iy = 0; iy < mapArray.GetLength(1); iy++)
                {
                    mapArray[ix, iy] = 1;
                }
            }
        }

        public void GenerateRandomRoom()
        {
            //GENERATES ROOMS AT LEAST OF SIZE 2x1
            int a, b, c, d;
            do
            {
                a = controller.random.Next(floorSize - 1);
                b = controller.random.Next(floorSize - 1);
                c = controller.random.Next(a + 1, floorSize);
                d = controller.random.Next(b + 1, floorSize);
            }
            while (c - a <= 1 && d - b <= 1);
            {
                roomList.Add(new Room(controller, this, new Point(a, b), new Point(c, d)));
            }
        }

        public void GenerateRoom(Point corner1, Point corner2)
        {
            if (!OutOfBounds(corner1) && !OutOfBounds(corner2))
            {
                for (int ix = corner1.X; ix < corner2.X; ix++)
                {
                    for (int iy = corner1.Y; iy < corner2.Y; iy++)
                    {
                        mapArray[ix, iy] = 0;
                    }
                }
            }
        }

        public bool OutOfBounds(Point position)
        {
            if (mapArray.GetLength(0) - 1 < position.X || mapArray.GetLength(1) - 1 < position.Y || position.X < 0 || position.Y < 0)
            {
                return true;
            }
            return false;
        }

        public bool OutOfBounds(Room room)
        {
            if (room.cornerSE.X > floorSize || room.cornerNW.Y > floorSize || room.cornerSE.X < 0 || room.cornerNW.Y < 0)
            {
                return true;
            }
            return false;
        }

        public bool IsIsolated(Room room)
        {
            //DOESN'T WORK WITH ROOMS INSIDE OTHER ROOMS
            Room tempRoom = new Room(controller, this, new Point(room.cornerNW.X - 1, room.cornerNW.Y - 1), new Point(room.cornerSE.X, room.cornerSE.Y));
            Point tempPoint = new Point(0, 0);
            Point corner1 = new Point(tempRoom.cornerSE.X, tempRoom.cornerNW.Y);
            Point corner2 = new Point(tempRoom.cornerNW.X, tempRoom.cornerSE.Y);

            for (int ix = tempRoom.cornerNW.X; ix < tempRoom.cornerSE.X + 1; ix++)
            {
                for (int iy = tempRoom.cornerNW.Y; iy < tempRoom.cornerSE.Y + 1; iy++)
                {
                    tempPoint.X = ix;
                    tempPoint.Y = iy;

                    if (OutOfBounds(tempPoint))
                    {
                        continue;
                    }

                    if (mapArray[ix, iy] == 0 && !room.ContainsPoint(tempPoint))
                    {
                        if (tempPoint != tempRoom.cornerNW && tempPoint != tempRoom.cornerSE && tempPoint != corner1 && tempPoint != corner2)
                        {
                            return false;
                        }
                    }
                }
            }

                return true;
        }

        public void Update()
        {
            GenerateFloor();
            foreach (Room r in roomList)
            {
                r.Update();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                roomList.Clear();
                for (int i = 0; i < 5; i++)
                {
                    GenerateRandomRoom();
                }

            }

            mapArray[controller.player.position.X, controller.player.position.Y] = 2;
        }

        public void Draw(SpriteBatch spriteBatch, ContentManager contentManager)
        {
            for (int ix = 0; ix < mapArray.GetLength(0); ix++)
            {
                for (int iy = 0; iy < mapArray.GetLength(1); iy++)
                {
                    switch (mapArray[ix, iy])
                    {
                        case 1:
                            {
                                spriteBatch.Draw(contentManager.tWall, new Vector2(ix * contentManager.tWall.Width, iy * contentManager.tWall.Height), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
                                break;
                            }
                        case 2:
                            {
                                spriteBatch.Draw(contentManager.tPlayer, new Vector2(ix * contentManager.tWall.Width, iy * contentManager.tWall.Height), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
                                break;
                            }
                    }
                    spriteBatch.DrawString(contentManager.font, mapArray[ix, iy].ToString(), new Vector2((float)ix * 32, (float)iy * 32), Color.Orange);
                }
            }

            foreach (Room r in roomList)
            {
                spriteBatch.DrawString(contentManager.font, r.ToString(), new Vector2((float)r.cornerNW.X * 32, (float)r.cornerNW.Y * 32), Color.Green);
            }
        }
    }
}