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

        private string mapString = "";

        public Map(Controller controller, int floorSize)
        {
            this.controller = controller;
            this.floorSize = floorSize;

            roomList.Add(new Room(controller, this, new Point(1, 1), new Point(2, 3)));
            roomList.Add(new Room(controller, this, new Point(0, 0), new Point(4, 4)));

            GenerateFloor();
            /*for (int i = 0; i < roomList.Count; i++)
            {
                GenerateRoom(roomList[i].cornerNW, roomList[i].cornerSE);
            }*/
            mapString = ArrayToString(mapArray);
        }

        public string ArrayToString(int[,] mArray)
        {
            //starts at [0,0], then does each column
            string returnString = "";
            for (int ix = 0; ix < mArray.GetLength(0); ix++)
            {
                for (int iy = 0; iy < mArray.GetLength(1); iy++)
                {
                    returnString += mArray[ix, iy];
                }
            }
            return returnString;
        }

        public int[,] StringToArray(string mString)
        {
            //starts at [0,0], then does each column
            int size = floorSize;
            int[,] returnArray = new int[size, size];

            for (int ix = 0; ix < returnArray.GetLength(0); ix++)
            {
                for (int iy = 0; iy < returnArray.GetLength(1); iy++)
                {
                    returnArray[ix, iy] = int.Parse(mString[ix * size + iy].ToString());
                }
            }
            return returnArray;
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

        public void GenerateRandomRoom(int minX, int minY)
        {
            int a, b, c, d;
            do
            {
                a = controller.random.Next(1, floorSize - 1);
                b = controller.random.Next(1, floorSize - 1);
                c = controller.random.Next(a, floorSize);
                d = controller.random.Next(b, floorSize);
            }
            while (c - a < minX || d - b < minY);
            GenerateRoom(new Point(a, b), new Point(c, d));
        }

        public void GenerateRandomRoom(int minX, int minY, int maxX, int maxY)
        {
            int a, b, c, d;
            do
            {
                a = controller.random.Next(1, floorSize - 1);
                b = controller.random.Next(1, floorSize - 1);
                c = controller.random.Next(a, floorSize);
                d = controller.random.Next(b, floorSize);
            }
            while (c - a < minX || d - b < minY || c - a > maxX || d - b > maxY);
            GenerateRoom(new Point(a, b), new Point(c, d));
        }

        public bool IsWall(Point position)
        {
            if (mapArray[position.X, position.Y] == 1)
            {
                return true;
            }
            return false;
        }

        public void GenerateRoom(Point corner1, Point corner2)
        {
            if (!OutOfBounds(corner1) && !OutOfBounds(corner2))
            {
                bool exists = false;

                foreach (Room r in roomList)
                {
                    if (r.cornerNW == corner1 && r.cornerSE == corner2)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    for (int ix = corner1.X; ix < corner2.X; ix++)
                    {
                        for (int iy = corner1.Y; iy < corner2.Y; iy++)
                        {
                            mapArray[ix, iy] = 0;
                        }
                    }

                    bool add = true;
                    for (int i = 0; i < roomList.Count; i++)
                    {
                        if (roomList[i].cornerNW == corner1 && roomList[i].cornerSE == corner2)
                        {
                            add = false;
                        }
                    }
                    if (add)
                        roomList.Add(new Room(controller, this, corner1, corner2));
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

        /*public bool IsIsolated(Room room)
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
        }*/

        public bool IsIsolated(Room room)
        {
            foreach (Room r in roomList)
            {
                if (room.Intersects(r) && r != room)
                {
                    return false;
                }
            }
            return true;
        }

        public Point GenerateFreePos()
        {
            int x, y;
            do
            {
                x = controller.random.Next(mapArray.GetLength(0));
                y = controller.random.Next(mapArray.GetLength(1));
            }
            while (mapArray[x, y] != 0);
            return new Point(x, y);
        }

        public void CreateRandomLevel(int numberOfRooms, int roomSizeX, int roomSizeY, int maxRoomSizeX, int maxRoomSizeY)
        {
            GenerateFloor();
            bool repeat = false;
            int counter = 0;

            do
            {
                GenerateFloor();
                roomList.Clear();
                repeat = false;

                for (int i = 0; i < numberOfRooms; i++)
                {
                    GenerateRandomRoom(roomSizeX, roomSizeY, maxRoomSizeX, maxRoomSizeY);
                }

                for (int i = 0; i < roomList.Count; i++)
                {
                    if (!IsIsolated(roomList[i]))
                    {
                        repeat = true;
                        counter++;
                        break;
                    }
                    for (int i2 = 0; i2 < roomList.Count; i2++)
                    {
                        if (roomList[i].cornerNW == roomList[i2].cornerNW && roomList[i].cornerSE == roomList[i2].cornerSE && roomList[i] != roomList[i2])
                        {
                            repeat = true;
                            counter++;
                            break;
                        }
                    }
                }
            }
            while (repeat);

            //foreach (Room r in roomList)
            for (int i = 0; i < roomList.Count; i++)
            {
                Room tempRoom;
                int x1, x2, y1, y2;
                //foreach (Room rm in roomList)
                for (int i2 = 0; i2 < roomList.Count; i2++)
                {
                    if (roomList[i].cornerNW.X <= roomList[i2].cornerNW.X)
                    {
                        x1 = roomList[i].cornerNW.X;
                        x2 = roomList[i2].cornerSE.X;
                    }
                    else
                    {
                        x1 = roomList[i2].cornerNW.X;
                        x2 = roomList[i].cornerSE.X;
                    }

                    tempRoom = new Room(controller, this, new Point(x1, roomList[i].cornerNW.Y), new Point(x2, roomList[i].cornerNW.Y + 1));
                    if (roomList[i2] != roomList[i])
                    {
                        if (tempRoom.Intersects(roomList[i2]))
                        {
                            GenerateRoom(tempRoom.cornerNW, tempRoom.cornerSE);
                            break;
                        }
                    }
                }

                for (int i2 = 0; i2 < roomList.Count; i2++)
                {
                    if (roomList[i].cornerNW.Y <= roomList[i2].cornerNW.Y)
                    {
                        y1 = roomList[i].cornerNW.Y;
                        y2 = roomList[i2].cornerSE.Y;
                    }
                    else
                    {
                        y1 = roomList[i2].cornerNW.Y;
                        y2 = roomList[i].cornerSE.Y;
                    }

                    tempRoom = new Room(controller, this, new Point(roomList[i].cornerNW.X, y1), new Point(roomList[i].cornerNW.X + 1, y2));
                    if (tempRoom.Intersects(roomList[i2]) && roomList[i2] != roomList[i] /*&& IsIsolated(rm)*/)
                    {
                        GenerateRoom(tempRoom.cornerNW, tempRoom.cornerSE);
                        break;
                    }
                }
            }
            for (int i = roomList.Count - 1; i > -1; i--)
            {
                if (IsIsolated(roomList[i]))
                {
                    for (int ix = roomList[i].cornerNW.X; ix < roomList[i].cornerSE.X; ix++)
                    {
                        for (int iy = roomList[i].cornerNW.Y; iy < roomList[i].cornerSE.Y; iy++)
                        {
                            mapArray[ix, iy] = 1;
                        }
                    }
                    roomList.RemoveAt(i);
                }
            }

            mapString = ArrayToString(mapArray);
            controller.player.position = GenerateFreePos();
            Console.WriteLine("# of tries: " + counter + " count: " + roomList.Count);
        }

        public void Update()
        {
            mapArray = StringToArray(mapString);

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                CreateRandomLevel(10, 2, 2, 5, 5);
                mapArray = StringToArray(mapString);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.G))
            {
                Console.WriteLine(ArrayToString(mapArray));
            }

            mapArray[controller.player.position.X, controller.player.position.Y] = 2;
        }

        public void Draw(SpriteBatch spriteBatch, ContentManager contentManager)
        {
            float scale = 0.6f;
            for (int ix = 0; ix < mapArray.GetLength(0); ix++)
            {
                for (int iy = 0; iy < mapArray.GetLength(1); iy++)
                {
                    switch (mapArray[ix, iy])
                    {
                        case 1:
                            {
                                spriteBatch.Draw(contentManager.tWall, new Vector2(ix * contentManager.tWall.Width * scale, iy * contentManager.tWall.Height * scale), null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 1);
                                break;
                            }
                        case 2:
                            {
                                spriteBatch.Draw(contentManager.tPlayer, new Vector2(ix * contentManager.tWall.Width * scale, iy * contentManager.tWall.Height * scale), null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 1);
                                break;
                            }
                    }
                    //spriteBatch.DrawString(contentManager.font, mapArray[ix, iy].ToString(), new Vector2((float)ix * 32, (float)iy * 32), Color.Orange);
                }
            }

            foreach (Room r in roomList)
            {
                spriteBatch.DrawString(contentManager.font, r.ToString(), new Vector2((float)r.cornerNW.X * 32, (float)r.cornerNW.Y * 32) * scale, Color.Green, 0, Vector2.Zero, scale, SpriteEffects.None, 1);
            }
        }
    }
}