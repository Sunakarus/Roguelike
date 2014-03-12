﻿using Microsoft.Xna.Framework;
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

        public enum Element : int { Nothing = 0, Wall = 1, Player = 2, Door = 3, Item = 4, DoorOpen = 5, Enemy = 6, Stairs = 7 }

        public int floorSize;

        public List<Room> roomList = new List<Room>();
        public List<Room> bigRoomList = new List<Room>();
        public List<Enemy> enemyList = new List<Enemy>();
        public List<Item> itemList = new List<Item>();

        public string mapString = "";
        public float scale = 0.8f;
        private float minimapSize = 150;
        private float minimapScale;
        private MouseState mouse, prevMouse;

        public Map(Controller controller, int floorSize)
        {
            this.controller = controller;
            this.floorSize = floorSize;

            GenerateEmptyFloor();
            mapString = ArrayToString(mapArray);

            mouse = Mouse.GetState();
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

        public void GenerateEmptyFloor()
        {
            mapArray = new int[floorSize, floorSize];

            for (int ix = 0; ix < mapArray.GetLength(0); ix++)
            {
                for (int iy = 0; iy < mapArray.GetLength(1); iy++)
                {
                    mapArray[ix, iy] = (int)Element.Wall;
                }
            }
        }

       /* private void GenerateRandomRoom(int minX, int minY)
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

        private void GenerateRandomRoom(int minX, int minY, int maxX, int maxY)
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
        */

        private void GenerateRandomIsolatedRoom(int minX, int minY, int maxX, int maxY)
        {
            int a, b, c, d;
            bool isIsolated;
            int timeOut = 10000;
            do
            {
                timeOut--;
                isIsolated = true;
                Room tempRoom;
                a = controller.random.Next(1, floorSize - 1);
                b = controller.random.Next(1, floorSize - 1);
                c = controller.random.Next(a, floorSize);
                d = controller.random.Next(b, floorSize);

                if (c - a >= minX && d - b >= minY && c - a <= maxX && d - b <= maxY)
                {
                    tempRoom = new Room(controller, this, new Point(a, b), new Point(c, d));
                    foreach (Room r in roomList)
                    {
                        if (r.Intersects(tempRoom))
                        {
                            isIsolated = false;
                        }
                    }
                }
                else
                {
                    isIsolated = false;
                }
            }
            while (!isIsolated && timeOut > 0);

            if (timeOut == 0)
            {
                Console.WriteLine("GenerateRandomIsolatedRoom() timed out.");
                return;
            }
            GenerateRoom(new Point(a, b), new Point(c, d));
            bigRoomList.Add(roomList[roomList.Count - 1]);
        }

        public bool IsElement(Point position, Element elem)
        {
            if (mapArray[position.X, position.Y] == (int)elem)
            {
                return true;
            }
            return false;
        }

        private void GenerateRoom(Point corner1, Point corner2)
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
                            mapArray[ix, iy] = (int)Element.Nothing;
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

        public void CheckForDoors(Room room)
        {
            Room bigRoom = new Room(controller, this, new Point(room.cornerNW.X - 1, room.cornerNW.Y - 1), room.cornerSE);
            Point tempPoint = new Point(0, 0);
            Point corner1 = new Point(bigRoom.cornerSE.X, bigRoom.cornerNW.Y);
            Point corner2 = new Point(bigRoom.cornerNW.X, bigRoom.cornerSE.Y);

            for (int ix = bigRoom.cornerNW.X; ix < bigRoom.cornerSE.X + 1; ix++)
            {
                for (int iy = bigRoom.cornerNW.Y; iy < bigRoom.cornerSE.Y + 1; iy++)
                {
                    tempPoint.X = ix;
                    tempPoint.Y = iy;

                    if (OutOfBounds(tempPoint))
                    {
                        continue;
                    }

                    if (mapArray[ix, iy] == (int)Element.Nothing && !room.ContainsPoint(tempPoint) && tempPoint != bigRoom.cornerNW && tempPoint != bigRoom.cornerSE && tempPoint != corner1 && tempPoint != corner2)
                    {
                        foreach (Room r in roomList)
                        {
                            if (r.ContainsPoint(tempPoint))
                            {
                                Point pointLeft, pointRight, pointUp, pointDown;
                                pointLeft = new Point(tempPoint.X - 1, tempPoint.Y);
                                pointRight = new Point(tempPoint.X + 1, tempPoint.Y);
                                pointUp = new Point(tempPoint.X, tempPoint.Y - 1);
                                pointDown = new Point(tempPoint.X, tempPoint.Y + 1);
                                //top || bottom
                                if ((bigRoom.cornerNW.Y == room.cornerNW.Y - 1 || bigRoom.cornerSE.Y == room.cornerSE.Y) && (r.width == 1))
                                {
                                    if (IsElement(pointLeft, Element.Wall) && IsElement(pointRight, Element.Wall) && mapArray[pointUp.X, pointUp.Y] != (int)Element.Door && mapArray[pointDown.X, pointDown.Y] != (int)Element.Door && mapArray[pointUp.X, pointUp.Y] != (int)Element.Wall && mapArray[pointDown.X, pointDown.Y] != (int)Element.Wall)
                                    {
                                        mapArray[ix, iy] = (int)Element.Door;
                                    }
                                }
                                //left || right
                                else if ((bigRoom.cornerNW.X == room.cornerNW.X - 1 || bigRoom.cornerSE.X == room.cornerSE.X) && r.height == 1)
                                {
                                    if (IsElement(pointUp, Element.Wall) && IsElement(pointDown, Element.Wall) && mapArray[pointLeft.X, pointLeft.Y] != (int)Element.Door && mapArray[pointRight.X, pointRight.Y] != (int)Element.Door && mapArray[pointLeft.X, pointLeft.Y] != (int)Element.Wall && mapArray[pointRight.X, pointRight.Y] != (int)Element.Wall)
                                    {
                                        //SPAWN DOOR
                                        mapArray[ix, iy] = (int)Element.Door;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public Point GenerateFreePos()
        {
            int x, y;
            //check if there is a free space at all
            bool breakAll = false;
            for (int ix = 0; ix < mapArray.GetLength(0); ix++)
            {
                if (breakAll) { break; }
                for (int iy = 0; iy < mapArray.GetLength(1); iy++)
                {
                    if (breakAll) { break; }
                    if (mapArray[ix, iy] == (int)Element.Nothing)
                    {
                        breakAll = true;
                    }
                }
            }
            if (!breakAll)
            {
                return Point.Zero;
            }
            do
            {
                x = controller.random.Next(mapArray.GetLength(0));
                y = controller.random.Next(mapArray.GetLength(1));
            }
            while (mapArray[x, y] != (int)Element.Nothing);
            return new Point(x, y);
        }

        public bool IsWalkable(Point position)
        {
            if (mapArray[position.X, position.Y] == (int)Element.DoorOpen || mapArray[position.X, position.Y] == (int)Element.Nothing || mapArray[position.X, position.Y] == (int)Element.Item || mapArray[position.X, position.Y] == (int)Element.Stairs)
            {
                return true;
            }
            return false;
        }

        public void CreateRandomLevel(int numberOfRooms, int roomSizeX, int roomSizeY, int maxRoomSizeX, int maxRoomSizeY)
        {
            GenerateEmptyFloor();
            roomList.Clear();
            bigRoomList.Clear();
            enemyList.Clear();
            itemList.Clear();
            //controller.player.health = controller.player.maxHealth;

            while (roomList.Count == 0 || !CheckIfAllConnected(roomList))
            {
                GenerateEmptyFloor();
                roomList.Clear();
                bigRoomList.Clear();
                enemyList.Clear();
                itemList.Clear();
                //CREATE BIG ROOMS
                for (int i = 0; i < numberOfRooms; i++)
                {
                    GenerateRandomIsolatedRoom(roomSizeX, roomSizeY, maxRoomSizeX, maxRoomSizeY);
                }
                //CORRIDORS
                for (int i = 0; i < roomList.Count; i++)
                {
                    Room tempRoom;
                    int x1, x2, y1, y2;

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
                //ELIMINATE ISOLATED ROOMS
                for (int i = roomList.Count - 1; i > -1; i--)
                {
                    if (IsIsolated(roomList[i]) && roomList.Count > 0)
                    {
                        for (int ix = roomList[i].cornerNW.X; ix < roomList[i].cornerSE.X; ix++)
                        {
                            for (int iy = roomList[i].cornerNW.Y; iy < roomList[i].cornerSE.Y; iy++)
                            {
                                mapArray[ix, iy] = (int)Element.Wall;
                            }
                        }
                        roomList.RemoveAt(i);
                    }
                }
            }

            //MAKE DOORS
            foreach (Room r in roomList)
            {
                CheckForDoors(r);
            }
            //STAIRS
            bool posInBig;
            Point stairs;
            do
            {
                posInBig = false;
                stairs = GenerateFreePos();

                foreach (Room r in bigRoomList)
                {
                    if (r.ContainsPoint(stairs))
                    {
                        posInBig = true;
                        break;
                    }
                }
            }
            while (!posInBig);
            mapArray[stairs.X, stairs.Y] = (int)Element.Stairs;

            //PLAYER
            do
            {
                posInBig = false;
                controller.player.position = GenerateFreePos();

                foreach (Room r in bigRoomList)
                {
                    if (r.ContainsPoint(controller.player.position))
                    {
                        posInBig = true;
                        //Console.WriteLine(r.ToString());
                        break;
                    }
                }
            }
            while (!posInBig);
            //GENERATING ITEMS
            /*
            bool hasDoor = false, canSpawn = true;
            List<Point> pointList = new List<Point>();

            Point tempPoint = new Point(0,0);
            for (int ix = 0; ix < mapArray.GetLength(0); ix++)
            {
                for (int iy = 0; iy < mapArray.GetLength(1); iy++)
                {
                    if (mapArray[ix, iy] == (int)Element.Nothing)
                    {
                        pointList.Clear();
                        canSpawn = true;
                        hasDoor = false;
                        Console.WriteLine(ix + " " + iy);

                        for (int a = -1; a <= 1; a++)
                        {
                            for (int b = -1; b <= 1; b++)
                            {
                                if (a == 0 && b == 0)
                                {
                                    continue;
                                }
                                tempPoint.X = ix + a;
                                tempPoint.Y = iy + b;
                                if (!OutOfBounds(tempPoint))
                                {
                                    pointList.Add(tempPoint);
                                }
                            }
                        }

                        foreach (Point p in pointList)
                        {
                            if (mapArray[p.X, p.Y] != (int)Element.Door || mapArray[p.X, p.Y] != (int)Element.Wall)
                            {
                                canSpawn = false;
                                break;
                            }

                            if (mapArray[p.X, p.Y] == (int)Element.Door)
                            {
                                if (!hasDoor)
                                {
                                    hasDoor = true;
                                    continue;
                                }
                                else
                                {
                                    canSpawn = false;
                                    break;
                                }
                            }
                        }
                        //check for surrounding tiles, spawn item
                        if (canSpawn)
                        {
                            mapArray[ix, iy] = (int)Element.Item;
                        }
                    }
                }
            }*/

            Point temp;
            /*for (int i = 0; i < numberOfRooms; i++)
            {
                temp = GenerateFreePos();
                if (temp != Point.Zero && temp != controller.player.position)
                {
                    mapArray[temp.X, temp.Y] = (int)Element.Item;
                    itemList.Add(new Item(controller, temp, Item.ItemType.Potion));
                }
            }*/

            bool overlap = false;
            for (int i = 0; i < numberOfRooms; i++)
            {
                overlap = false;
                temp = GenerateFreePos();
                foreach (Item it in itemList)
                {
                    if (temp == it.position)
                    {
                        overlap = true;
                    }
                }
                if (temp != Point.Zero && temp != controller.player.position && !overlap)
                {
                    Type t = typeof(Item.ItemType);
                    itemList.Add(new Item(controller, temp, (Item.ItemType)controller.random.Next(Enum.GetValues(t).Length)));
                    mapArray[temp.X, temp.Y] = (int)Element.Item;
                }
            }
            //////////////////
            //ENEMIES
            overlap = false;
            for (int i = 0; i < numberOfRooms; i++)
            {
                overlap = false;
                temp = GenerateFreePos();
                foreach (Enemy e in enemyList)
                {
                    if (temp == e.position)
                    {
                        overlap = true;
                    }
                }
                if (temp != Point.Zero && temp != controller.player.position && !overlap)
                {
                    Type t = typeof(Enemy.EnemyType);
                    enemyList.Add(new Enemy(controller, temp, (Enemy.EnemyType)controller.random.Next(Enum.GetValues(t).Length)));
                }
            }

            mapString = ArrayToString(mapArray);
            controller.camera.ResetPosition();
        }

        public bool CheckIfAllConnected(List<Room> list)
        {
            List<Room> tempList = new List<Room>();
            tempList.Add(list[0]);
            bool repeat;
            do
            {
                repeat = false;
                foreach (Room r in list)
                {
                    for (int i = 0; i < tempList.Count; i++)
                    {
                        if (tempList[i].Intersects(r) && !tempList.Contains(r))
                        {
                            tempList.Add(r);
                            repeat = true;
                        }
                    }
                }
            }
            while (repeat);

            return (tempList.Count == list.Count);
        }

        public void Update()
        {
            prevMouse = mouse;
            mouse = Mouse.GetState();
            mapArray = StringToArray(mapString);

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                controller.MapLevelUp();
                mapArray = StringToArray(mapString);
            }

            //zoooooooooooom
            if (prevMouse.ScrollWheelValue != mouse.ScrollWheelValue)
            {
                if (mouse.ScrollWheelValue - prevMouse.ScrollWheelValue > 0)
                {
                    if (scale + 0.05 < 5)
                        scale += 0.05f;
                }
                else if (mouse.ScrollWheelValue - prevMouse.ScrollWheelValue < 0)
                {
                    if (scale - 0.05 > 0)
                        scale -= 0.05f;
                }
            }

            if (IsElement(controller.player.position, Map.Element.Door))
            {
                mapArray[controller.player.position.X, controller.player.position.Y] = (int)Map.Element.DoorOpen;
                mapString = ArrayToString(mapArray);
            }

            if (controller.player.stepped)
            {
                for (int i = enemyList.Count - 1; i > -1; i--)
                {
                    if (enemyList[i].health <= 0)
                    {
                        enemyList.RemoveAt(i);
                        continue;
                    }
                    enemyList[i].Move();
                }
                if (controller.player.health <= 0)
                {
                    controller.player.health = 0;
                }
                else if (controller.player.health >= controller.player.maxHealth)
                {
                    controller.player.health = controller.player.maxHealth;
                }
                controller.player.stepped = false;
            }
            //ADD NON STATIC ELEMENTS
            foreach (Enemy e in enemyList)
            {
                mapArray[e.position.X, e.position.Y] = (int)Map.Element.Enemy;
            }

            mapArray[controller.player.position.X, controller.player.position.Y] = (int)Element.Player;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int tileSize = ContentManager.tWall.Width;
            minimapScale = minimapSize / (floorSize * tileSize);

            //DRAW BACKGROUND
            /*spriteBatch.Draw(ContentManager.tWall, new Rectangle((int)controller.camera.position.X * tileSize, (int)controller.camera.position.Y * tileSize, (int)(controller.graphics.PreferredBackBufferWidth / controller.camera.scale), (int)(controller.graphics.PreferredBackBufferHeight / controller.camera.scale)), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);*/

            for (int ix = 0; ix < mapArray.GetLength(0); ix++)
            {
                for (int iy = 0; iy < mapArray.GetLength(1); iy++)
                {
                    //minimap math
                    Vector2 minimapPos = new Vector2(controller.camera.position.X * tileSize + ix * tileSize * minimapScale / controller.camera.scale, controller.camera.position.Y * tileSize + iy * tileSize * minimapScale / controller.camera.scale);
                    minimapPos.X += (controller.graphics.PreferredBackBufferWidth - floorSize * tileSize * minimapScale) / controller.camera.scale;

                    spriteBatch.Draw(ContentManager.tFloor, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.01f);
                    //minimap
                    spriteBatch.Draw(ContentManager.tFloor, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 0.99f);
                    switch (mapArray[ix, iy])
                    {
                        case (int)Element.Wall:
                            {
                                spriteBatch.Draw(ContentManager.tWall, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                //minimap
                                spriteBatch.Draw(ContentManager.tWall, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 1);

                                break;
                            }
                        case (int)Element.Player:
                            {
                                spriteBatch.Draw(ContentManager.tHealthBar, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, new Vector2(tileSize * (controller.player.health / controller.player.maxHealth), tileSize / 10), SpriteEffects.None, 0.98f);
                                spriteBatch.Draw(ContentManager.tPlayer, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                //minimap
                                spriteBatch.Draw(ContentManager.tPlayer, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 1);
                                break;
                            }
                        case (int)Element.Door:
                            {
                                spriteBatch.Draw(ContentManager.tDoor, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                //minimap
                                spriteBatch.Draw(ContentManager.tDoor, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 1);
                                break;
                            }
                        case (int)Element.Item:
                            {
                                foreach (Item it in itemList)
                                {
                                    if (it.position == new Point(ix, iy))
                                    {
                                        spriteBatch.Draw(it.texture, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                        //minimap
                                        spriteBatch.Draw(it.texture, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 1);
                                    }
                                }
                                break;
                            }
                        case (int)Element.DoorOpen:
                            {
                                spriteBatch.Draw(ContentManager.tDoorOpen, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                //minimap
                                spriteBatch.Draw(ContentManager.tDoorOpen, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 1);
                                break;
                            }
                        case (int)Element.Stairs:
                            {
                                spriteBatch.Draw(ContentManager.tStairs, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                //minimap
                                spriteBatch.Draw(ContentManager.tStairs, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 1);
                                break;
                            }
                        case (int)Element.Enemy:
                            {
                                foreach (Enemy e in enemyList)
                                {
                                    if (e.position == new Point(ix, iy))
                                    {
                                        spriteBatch.Draw(ContentManager.tHealthBar, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, new Vector2(tileSize * (e.health / e.maxHealth), tileSize / 10), SpriteEffects.None, 0.98f);
                                        spriteBatch.Draw(e.texture, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                        //minimap
                                        spriteBatch.Draw(e.texture, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 1);
                                        break;
                                    }
                                }
                                break;
                            }
                    }
                }
            }

            /*foreach (Room r in roomList)
            {
                spriteBatch.DrawString(ContentManager.font, r.ToString(), new Vector2((float)r.cornerNW.X * tileSize, (float)r.cornerNW.Y * tileSize), Color.Green, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            }
            */
            spriteBatch.DrawString(ContentManager.font, "HEALTH: " + controller.player.health + "/" + controller.player.maxHealth + "\nLEVEL: " + controller.level + "\nENEMY COUNT: " + enemyList.Count + "\nE: Interact\nF: Use potion", new Vector2(controller.camera.position.X * tileSize, controller.camera.position.Y * tileSize), Color.Red, 0, Vector2.Zero, 1 / controller.camera.scale, SpriteEffects.None, 1);

            for (int i = 0; i < controller.inventory.Count; i++)
            {
                spriteBatch.DrawString(ContentManager.font, (i + 1) + ") " + controller.inventory[i].ToString(), new Vector2(controller.camera.position.X * tileSize, controller.camera.position.Y * tileSize + i * tileSize / 1.4f / controller.camera.scale + 130 / controller.camera.scale), Color.DarkGoldenrod, 0, Vector2.Zero, 1 / controller.camera.scale, SpriteEffects.None, 1);
            }
        }
    }
}