using Microsoft.Xna.Framework;
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

        public int GetElementAtPos(Point position)
        {
            if (!OutOfBounds(position))
                return mapArray[position.X, position.Y];
            else
                return -1;
        }

        public void CreateRandomLevel(int numberOfRooms, int roomSizeX, int roomSizeY, int maxRoomSizeX, int maxRoomSizeY)
        {
            this.player = controller.player;
            GenerateEmptyFloor();
            roomList.Clear();
            bigRoomList.Clear();
            enemyList.Clear();
            itemList.Clear();
            minimapSightArray = new int[floorSize, floorSize];
            GenerateSightArray(out minimapSightArray);

            while (roomList.Count == 0 || !CheckIfAllConnected(roomList))
            {
                GenerateEmptyFloor();
                roomList.Clear();
                bigRoomList.Clear();
                enemyList.Clear();
                itemList.Clear();
                minimapSightArray = new int[floorSize, floorSize];
                GenerateSightArray(out minimapSightArray);
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
                    Type t = typeof(Item.ItemType);
                    itemList.Add(new Item(controller, temp, (Item.ItemType)controller.random.Next(Enum.GetValues(t).Length), (Map.Element)mapArray[temp.X, temp.Y]));
                    mapArray[temp.X, temp.Y] = (int)Element.Item;
                }
            }
            //////////////////
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
            //sightArray[player.position.X, player.position.Y] = 0;
            List<Ray> rayList = new List<Ray>();
            for (float ix = -1f; ix <= 1; ix += 1f)
            {
                for (float iy = -1f; iy <= 1; iy += 1f)
                {
                    if (ix == 0 && iy == 0)
                    {
                        continue;
                    }
                    else
                    {
                        rayList.Add(new Ray(controller, player.position, new Vector2(ix, iy), player.viewDistance));
                        rayList.Add(new Ray(controller, new Point(player.position.X - 1, player.position.Y - 1), new Vector2(ix, iy), player.viewDistance));
                        rayList.Add(new Ray(controller, new Point(player.position.X, player.position.Y - 1), new Vector2(ix, iy), player.viewDistance));
                        rayList.Add(new Ray(controller, new Point(player.position.X + 1, player.position.Y - 1), new Vector2(ix, iy), player.viewDistance));
                        rayList.Add(new Ray(controller, new Point(player.position.X - 1, player.position.Y), new Vector2(ix, iy), player.viewDistance));
                        rayList.Add(new Ray(controller, new Point(player.position.X + 1, player.position.Y), new Vector2(ix, iy), player.viewDistance));
                        rayList.Add(new Ray(controller, new Point(player.position.X - 1, player.position.Y + 1), new Vector2(ix, iy), player.viewDistance));
                        rayList.Add(new Ray(controller, new Point(player.position.X, player.position.Y + 1), new Vector2(ix, iy), player.viewDistance));
                        rayList.Add(new Ray(controller, new Point(player.position.X + 1, player.position.Y + 1), new Vector2(ix, iy), player.viewDistance));
                    }
                }
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
                        player.experience += enemyList[i].expValue;
                        enemyList.RemoveAt(i);

                        continue;
                    }
                    else
                    {
                        enemyList[i].Update();
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
                if (player.experience >= player.maxExperience)
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
                        if (controller.inventory.Count >= Controller.INVENTORYSIZE)
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

            //DRAW BACKGROUND
            /*spriteBatch.Draw(ContentManager.tWall, new Rectangle((int)controller.camera.position.X * tileSize, (int)controller.camera.position.Y * tileSize, (int)(controller.graphics.PreferredBackBufferWidth / controller.camera.scale), (int)(controller.graphics.PreferredBackBufferHeight / controller.camera.scale)), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);*/

            for (int ix = 0; ix < mapArray.GetLength(0); ix++)
            {
                for (int iy = 0; iy < mapArray.GetLength(1); iy++)
                {
                    //minimap math
                    Vector2 minimapPos = new Vector2(controller.camera.position.X * tileSize + ix * tileSize * minimapScale / controller.camera.scale, controller.camera.position.Y * tileSize + iy * tileSize * minimapScale / controller.camera.scale);
                    minimapPos.X += (controller.graphics.PreferredBackBufferWidth - floorSize * tileSize * minimapScale) / controller.camera.scale;

                    //minimap
                    spriteBatch.Draw(ContentManager.tFloor, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 10);

                    spriteBatch.Draw(ContentManager.tFloor, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.01f);

                    if (minimapSightArray[ix, iy] == 1 && controller.showFog)
                    {
                        spriteBatch.Draw(ContentManager.tFog, minimapPos, null, Color.Black, 0, Vector2.Zero, minimapScale * tileSize / controller.camera.scale, SpriteEffects.None, 15f);
                    }
                    //////////

                    if (controller.showFog)
                    {
                        foreach (Enemy e in enemyList)
                        {
                            if (sightArray[e.position.X, e.position.Y] == 1 && minimapSightArray[e.position.X, e.position.Y] == 0)
                            {
                                mapArray[e.position.X, e.position.Y] = tempFloor[e.position.X, e.position.Y];
                                continue;
                            }
                        }
                    }

                    switch (mapArray[ix, iy])
                    {
                        case (int)Element.Wall:
                            {
                                spriteBatch.Draw(ContentManager.tWall, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                //minimap
                                spriteBatch.Draw(ContentManager.tWall, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 11);

                                break;
                            }
                        case (int)Element.Player:
                            {
                                int num = (int)Math.Round(player.health * ContentManager.tHealthBar.Width / player.maxHealth);

                                spriteBatch.Draw(ContentManager.tHealthBar, new Vector2(ix * tileSize, iy * tileSize), new Rectangle(num, 0, 1, 1), Color.White, 0, Vector2.Zero, new Vector2(tileSize * (player.health / player.maxHealth), tileSize / 10), SpriteEffects.None, 0.97f);

                                spriteBatch.Draw(ContentManager.tPlayer, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                //minimap
                                spriteBatch.Draw(ContentManager.tPlayer, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 11);
                                break;
                            }
                        case (int)Element.Door:
                            {
                                spriteBatch.Draw(ContentManager.tDoor, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                //minimap
                                spriteBatch.Draw(ContentManager.tDoor, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 11);
                                break;
                            }
                        case (int)Element.Item:
                            {
                                foreach (Item it in itemList)
                                {
                                    if (it.position == new Point(ix, iy) && (sightArray[ix, iy] == 0 || !controller.showFog))
                                    {
                                        spriteBatch.Draw(it.texture, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                        //minimap
                                        spriteBatch.Draw(it.texture, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 11);
                                    }
                                }
                                break;
                            }
                        case (int)Element.DoorOpen:
                            {
                                spriteBatch.Draw(ContentManager.tDoorOpen, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                //minimap
                                spriteBatch.Draw(ContentManager.tDoorOpen, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 11);
                                break;
                            }
                        case (int)Element.Stairs:
                            {
                                spriteBatch.Draw(ContentManager.tStairs, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                //minimap
                                spriteBatch.Draw(ContentManager.tStairs, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 11);
                                break;
                            }
                        case (int)Element.Enemy:
                            {
                                foreach (Enemy e in enemyList)
                                {
                                    if (e.position == new Point(ix, iy) && (sightArray[ix, iy] == 0 || !controller.showFog))
                                    {
                                            int num = (int)Math.Round(e.health * ContentManager.tHealthBar.Width / e.maxHealth);
                                            spriteBatch.Draw(ContentManager.tHealthBar, new Vector2(ix * tileSize, iy * tileSize), new Rectangle(num, 0, 1, 1), Color.White, 0, Vector2.Zero, new Vector2(tileSize * (e.health / e.maxHealth), tileSize / 10), SpriteEffects.None, 0.97f);
                                            spriteBatch.Draw(e.texture, new Vector2(ix * tileSize, iy * tileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f);
                                            //minimap
                                            spriteBatch.Draw(e.texture, minimapPos, null, Color.White, 0, Vector2.Zero, minimapScale / controller.camera.scale, SpriteEffects.None, 11);
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
            spriteBatch.DrawString(ContentManager.font, "Health: " + player.health + "/" + player.maxHealth + "\nFloor: " + controller.level + "\nEnemy count: " + enemyList.Count + "\nPlayer level: " + player.playerLevel + "\nEXP: " + player.experience + "/" + player.maxExperience + "\nPlayer damage: " + player.damage, new Vector2(controller.camera.position.X * tileSize, controller.camera.position.Y * tileSize), Color.Red, 0, Vector2.Zero, 1 / controller.camera.scale, SpriteEffects.None, 100);

            spriteBatch.DrawString(ContentManager.font, message, new Vector2(controller.camera.position.X * tileSize + 300 / controller.camera.scale, controller.camera.position.Y * tileSize), Color.Red, 0, Vector2.Zero, 1 / controller.camera.scale, SpriteEffects.None, 100);

            if (controller.showInv)
            {
                for (int i = 0; i < controller.inventory.Count; i++)
                {
                    int inventoryY = 190;
                    Color fontColor;
                    if (player.chosenItem != i)
                    {
                        fontColor = Color.DarkGoldenrod;
                    }
                    else
                    {
                        fontColor = Color.Yellow;
                    }

                    spriteBatch.DrawString(ContentManager.font, (i + 1) + ") " + controller.inventory[i].ToString(), new Vector2(controller.camera.position.X * tileSize, controller.camera.position.Y * tileSize + i * tileSize / 1.4f / controller.camera.scale + inventoryY / controller.camera.scale), fontColor, 0, Vector2.Zero, 1 / controller.camera.scale, SpriteEffects.None, 100);
                }
            }
        }
    }
}