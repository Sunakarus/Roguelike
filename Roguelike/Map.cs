﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Roguelike
{
    internal class Map
    {
        public int[,] mapArray, sightArray, minimapSightArray;
        private Controller controller;

        public enum Element : int { Nothing, Wall, Player, Door, Item, DoorOpen, Enemy, Stairs }

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

        public string message = "";
        public static readonly float FOGINTENSITY = 0.1f;

        private Player player;

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

        public void GenerateSightArray(out int[,] array)
        {
            array = new int[floorSize, floorSize];

            for (int ix = 0; ix < array.GetLength(0); ix++)
            {
                for (int iy = 0; iy < array.GetLength(1); iy++)
                {
                    array[ix, iy] = 1;
                }
            }
        }

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

        public int GetElementAtPos(Point position)
        {
            if (!OutOfBounds(position))
                return mapArray[position.X, position.Y];
            else
                return -1;
        }

        public void ResetFloorVars()
        {
            GenerateEmptyFloor();
            roomList.Clear();
            bigRoomList.Clear();
            enemyList.Clear();
            itemList.Clear();
            minimapSightArray = new int[floorSize, floorSize];
            GenerateSightArray(out minimapSightArray);
        }

        public void CreateRandomLevel(int numberOfRooms, int roomSizeX, int roomSizeY, int maxRoomSizeX, int maxRoomSizeY)
        {
            this.player = controller.player;
            ResetFloorVars();

            while (roomList.Count == 0 || !CheckIfAllConnected(roomList))
            {
                ResetFloorVars();

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
                        for (int ib = bigRoomList.Count - 1; ib > -1; ib--)
                        {
                            if (bigRoomList[ib] == roomList[i])
                            {
                                bigRoomList.RemoveAt(ib);
                                break;
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
                player.position = GenerateFreePos();

                foreach (Room r in bigRoomList)
                {
                    if (r.ContainsPoint(player.position))
                    {
                        posInBig = true;
                        break;
                    }
                }
            }
            while (!posInBig);
            //GENERATING ITEMS
            Point temp;
            bool overlap = false;
            for (int i = 0; i < bigRoomList.Count; i++)
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
                if (temp != Point.Zero && temp != player.position && !overlap)
                {
                    itemList.Add(new Item(controller, temp, (Map.Element)mapArray[temp.X, temp.Y]));
                    mapArray[temp.X, temp.Y] = (int)Element.Item;
                }
            }

            //ENEMIES
            overlap = false;
            for (int i = 0; i < bigRoomList.Count + (int)controller.level / 10; i++)
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
                if (temp != Point.Zero && temp != player.position && !overlap)
                {
                    Type t = typeof(Enemy.EnemyType);
                    enemyList.Add(new Enemy(controller, temp, (Enemy.EnemyType)controller.random.Next(Enum.GetValues(t).Length)));
                }
            }

            mapString = ArrayToString(mapArray);
            controller.camera.ResetPosition();
            GenerateFog();
            GenerateMinimapFog();
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

        public bool GenerateMinimapFog()
        {
            if (sightArray != null)
            {
                for (int ix = 0; ix < sightArray.GetLength(0); ix++)
                {
                    for (int iy = 0; iy < sightArray.GetLength(0); iy++)
                    {
                        if (sightArray[ix, iy] == 0)
                        {
                            minimapSightArray[ix, iy] = 0;
                        }
                    }
                }
                return true;
            }
            else return false;
        }

        public void GenerateFog()
        {
            //FOG
            GenerateSightArray(out sightArray);
            List<Ray> rayList = new List<Ray>();
            List<Point> pointList = new List<Point>();
            Point tempPoint;
            for (int ix = player.position.X - player.viewDistance; ix <= player.position.X + player.viewDistance; ix++)
            {
                for (int iy = player.position.Y - player.viewDistance; iy <= player.position.Y + player.viewDistance; iy++)
                {
                    tempPoint = new Point(ix, iy);
                    //((x1-x)^2 + (y1-y)^2) < r^2
                    if (Math.Pow(tempPoint.X - player.position.X, 2) + Math.Pow(tempPoint.Y - player.position.Y, 2) < Math.Pow(player.viewDistance, 2))
                    {
                        pointList.Add(tempPoint);
                    }
                }
            }
            foreach (Point p in pointList)
            {
                rayList.Add(new Ray(controller, player.position, new Vector2(p.X - player.position.X, p.Y - player.position.Y), player.viewDistance));
            }

            foreach (Ray r in rayList)
            {
                r.CreateSightArray(ref sightArray);
            }
            foreach (Enemy e in enemyList)
            {
                e.isSeen = false;
                foreach (Ray r in rayList)
                {
                    if (r.CanSee(e.position))
                    {
                        e.isSeen = true;
                        break;
                    }
                }
            }
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

            if (IsElement(player.position, Map.Element.Door))
            {
                mapArray[player.position.X, player.position.Y] = (int)Map.Element.DoorOpen;
                mapString = ArrayToString(mapArray);
            }

            if (player.stepped)
            {
                for (int i = enemyList.Count - 1; i > -1; i--)
                {
                    if (enemyList[i].health <= 0)
                    {
                        enemyList[i].Death();
                        enemyList.RemoveAt(i);
                        continue;
                    }
                    else
                    {
                        float distance = new Vector2(enemyList[i].position.X - player.position.X, enemyList[i].position.Y - player.position.Y).Length();

                        if (distance < player.viewDistance * 2)
                        {
                            enemyList[i].Update();
                        }
                    }
                }
                if (player.health <= 0)
                {
                    player.health = 0;
                }
                else if (player.health >= player.maxHealth)
                {
                    player.health = player.maxHealth;
                }
                while (player.experience >= player.maxExperience)
                {
                    player.LevelUp();
                }
                GenerateFog();
                GenerateMinimapFog();

                switch (mapArray[player.position.X, player.position.Y])
                {
                    case (int)Element.Stairs:
                        message = "Press E to go to next level";
                        break;

                    case (int)Element.Item:
                        if (controller.inventory.Count >= Inventory.INVENTORYSIZE)
                        {
                            message = "Inventory full";
                        }
                        else
                        {
                            message = "Press E to pick up";
                        }
                        break;

                    default:
                        message = "";
                        break;
                }
                for (int i = controller.buffList.Count - 1; i > -1; i--)
                {
                    if (controller.buffList[i].IsDone())
                    {
                        controller.buffList[i].Debuff();
                        controller.buffList.RemoveAt(i);
                    }
                    else
                    {
                        controller.buffList[i].Update();
                    }
                }
                player.stepped = false;
            }
            //ADD NON STATIC ELEMENTS

            foreach (Item i in itemList)
            {
                mapArray[i.position.X, i.position.Y] = (int)Map.Element.Item;
            }
            foreach (Enemy e in enemyList)
            {
                mapArray[e.position.X, e.position.Y] = (int)Map.Element.Enemy;
            }

            mapArray[player.position.X, player.position.Y] = (int)Element.Player;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int tileSize = ContentManager.tWall.Width;
            minimapScale = minimapSize / (floorSize * tileSize);
            int[,] tempFloor = StringToArray(mapString);

            Vector2 cameraZero = new Vector2(controller.camera.position.X * tileSize, controller.camera.position.Y * tileSize);

            //DRAW BACKGROUND
            spriteBatch.Draw(ContentManager.tFog, new Rectangle((int)cameraZero.X, (int)cameraZero.Y, (int)(controller.graphics.PreferredBackBufferWidth / controller.camera.scale), (int)(controller.graphics.PreferredBackBufferHeight / controller.camera.scale)), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, -0.5f);

            for (int ix = 0; ix < mapArray.GetLength(0); ix++)
            {
                for (int iy = 0; iy < mapArray.GetLength(1); iy++)
                {
                    //minimap math
                    Vector2 minimapPos = new Vector2(cameraZero.X + ix * tileSize * minimapScale / controller.camera.scale, cameraZero.Y + iy * tileSize * minimapScale / controller.camera.scale);

                    minimapPos.X += (controller.graphics.PreferredBackBufferWidth - floorSize * tileSize * minimapScale) / controller.camera.scale;

                    //minimap
                    spriteBatch.Draw(ContentManager.tFloor, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 10);

                    spriteBatch.Draw(ContentManager.tFloor, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.01f);

                    //////////fog logic
                    if (minimapSightArray[ix, iy] == 1 && controller.showFog)
                    {
                        spriteBatch.Draw(ContentManager.tFog, minimapPos, null, Color.Black, 0, Vector2.Zero, minimapScale * tileSize / controller.camera.scale, SpriteEffects.None, 15f);
                    }

                    if (controller.showFog)
                    {
                        foreach (Enemy e in enemyList)
                        {
                            if (sightArray[e.position.X, e.position.Y] == 1)
                            {
                                mapArray[e.position.X, e.position.Y] = tempFloor[e.position.X, e.position.Y];
                                continue;
                            }
                        }
                    }

                    /////////
                    switch (mapArray[ix, iy])
                    {
                        case (int)Element.Wall:
                            {
                                spriteBatch.Draw(ContentManager.tWall, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                DrawMinimap(spriteBatch, ContentManager.tWall, minimapPos, Color.White);

                                break;
                            }
                        case (int)Element.Player:
                            {
                                int num = (int)Math.Round(player.health * ContentManager.tHealthBar.Width / player.maxHealth);

                                spriteBatch.Draw(ContentManager.tHealthBar, new Vector2(ix * tileSize, iy * tileSize), new Rectangle(num, 0, 1, 1), Color.White * 0.8f, 0, Vector2.Zero, new Vector2(tileSize * (player.health / player.maxHealth), tileSize / 10), SpriteEffects.None, 0.97f);

                                spriteBatch.Draw(ContentManager.tFog, new Vector2(ix * tileSize, iy * tileSize + tileSize / 10), null, Color.White * 0.8f, 0, Vector2.Zero, new Vector2(tileSize * (player.experience / player.maxExperience), tileSize / 10), SpriteEffects.None, 0.97f);

                                spriteBatch.Draw(ContentManager.tPlayer, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);

                                DrawMinimap(spriteBatch, ContentManager.tPlayer, minimapPos, Color.White);
                                break;
                            }
                        case (int)Element.Door:
                            {
                                spriteBatch.Draw(ContentManager.tDoor, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                DrawMinimap(spriteBatch, ContentManager.tDoor, minimapPos, Color.White);
                                break;
                            }
                        case (int)Element.Item:
                            {
                                Color color;
                                for (int i = itemList.Count - 1; i > -1; i--)
                                {
                                    color = Color.White;
                                    Item it = itemList[i];
                                    if (it.itemCat == Item.ItemCat.Armor || it.itemCat == Item.ItemCat.Weapon)
                                    {
                                        if (it.bonusStat > 0)
                                        {
                                            color = Color.SkyBlue;
                                        }
                                    }
                                    if (it.position == new Point(ix, iy) && (sightArray[ix, iy] == 0 || !controller.showFog))
                                    {
                                        spriteBatch.Draw(it.texture, new Vector2(ix * tileSize, iy * tileSize), null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                        DrawMinimap(spriteBatch, it.texture, minimapPos, color);
                                    }
                                }
                                break;
                            }
                        case (int)Element.DoorOpen:
                            {
                                spriteBatch.Draw(ContentManager.tDoorOpen, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                DrawMinimap(spriteBatch, ContentManager.tDoorOpen, minimapPos, Color.White);
                                break;
                            }
                        case (int)Element.Stairs:
                            {
                                spriteBatch.Draw(ContentManager.tStairs, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                DrawMinimap(spriteBatch, ContentManager.tStairs, minimapPos, Color.White);
                                break;
                            }
                        case (int)Element.Enemy:
                            {
                                foreach (Enemy e in enemyList)
                                {
                                    if (e.position == new Point(ix, iy) && (sightArray[ix, iy] == 0 || !controller.showFog))
                                    {
                                        int num = (int)Math.Round(e.health * ContentManager.tHealthBar.Width / e.maxHealth);

                                        spriteBatch.Draw(ContentManager.tHealthBar, new Vector2(ix * tileSize, iy * tileSize), new Rectangle(num, 0, 1, 1), Color.White * 0.8f, 0, Vector2.Zero, new Vector2(tileSize * (e.health / e.maxHealth), tileSize / 10), SpriteEffects.None, 0.97f);

                                        spriteBatch.Draw(e.texture, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);

                                        DrawMinimap(spriteBatch, e.texture, minimapPos, Color.White);
                                        break;
                                    }
                                }
                                break;
                            }
                    }
                }
            }

            //FOG
            if (controller.showFog)
            {
                for (int ix = 0; ix < sightArray.GetLength(0); ix++)
                {
                    for (int iy = 0; iy < sightArray.GetLength(1); iy++)
                    {
                        if (sightArray[ix, iy] == 1 && minimapSightArray[ix, iy] == 1)
                        {
                            spriteBatch.Draw(ContentManager.tFog, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, new Vector2(tileSize, tileSize), SpriteEffects.None, 0.99f);
                        }
                        else if (sightArray[ix, iy] == 0 && minimapSightArray[ix, iy] == 0)
                        {
                            float distance = (new Vector2(ix, iy) - new Vector2(player.position.X, player.position.Y)).Length();
                            float scale = FOGINTENSITY * distance;
                            if (scale > 1 - FOGINTENSITY)
                            {
                                scale = 1 - FOGINTENSITY;
                            }

                            spriteBatch.Draw(ContentManager.tFog, new Vector2(ix * tileSize, iy * tileSize), null, Color.White * scale, 0, Vector2.Zero, new Vector2(tileSize, tileSize), SpriteEffects.None, 0.99f);
                        }
                        else if (sightArray[ix, iy] == 1 && minimapSightArray[ix, iy] == 0)
                        {
                            float distance = (new Vector2(ix, iy) - new Vector2(player.position.X, player.position.Y)).Length();
                            spriteBatch.Draw(ContentManager.tFog, new Vector2(ix * tileSize, iy * tileSize), null, Color.White * (1 - FOGINTENSITY), 0, Vector2.Zero, new Vector2(tileSize, tileSize), SpriteEffects.None, 0.99f);
                        }
                    }
                }
            }
            spriteBatch.DrawString(ContentManager.font, "Floor: " + controller.level + "\nEnemy count: " + enemyList.Count + " (+" + (int)(controller.level / 3) + ")\n", cameraZero, Color.Orange, 0, Vector2.Zero, 1 / controller.camera.scale, SpriteEffects.None, 100);

            spriteBatch.DrawString(ContentManager.font, "\n\n" + player.ToString(), cameraZero, Color.Cyan, 0, Vector2.Zero, 1 / controller.camera.scale, SpriteEffects.None, 100);

            spriteBatch.DrawString(ContentManager.font, message, new Vector2(cameraZero.X + 300 / controller.camera.scale, cameraZero.Y), Color.Red, 0, Vector2.Zero, 1 / controller.camera.scale, SpriteEffects.None, 100);

            //buffs
            int buffX = 200;
            for (int i = 0; i < controller.buffList.Count; i++)
            {
                spriteBatch.Draw(controller.buffList[i].texture, new Vector2(cameraZero.X + (tileSize * i + buffX) / controller.camera.scale, cameraZero.Y), null, Color.White, 0, Vector2.Zero, 1 / controller.camera.scale, SpriteEffects.None, 120);
                spriteBatch.DrawString(ContentManager.font, controller.buffList[i].turns.ToString(),
                    new Vector2(cameraZero.X + (tileSize * i + buffX) / controller.camera.scale, cameraZero.Y), Color.Gold, 0, Vector2.Zero, 1 / controller.camera.scale, SpriteEffects.None, 125);
            }

            if (controller.showInv)
            {
                controller.inv.Draw(spriteBatch, tileSize);
            }
        }

        public void DrawMinimap(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Color color)
        {
            int depth = 11;
            spriteBatch.Draw(texture, position, null, color, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, depth);
        }
    }
}