using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Roguelike
{
    internal class Controller
    {
        public Map map;
        public Player player;
        public Random random = new Random();
        public Camera camera;
        public GraphicsDeviceManager graphics;
        public int level = 1;

        public bool showInv = true;
        public bool showFog = true;

        public static readonly int LEVELCAP = 30;
        public static readonly int STARTMAPSIZE = 15;
        public static readonly int INVENTORYSIZE = 8;

        public List<Item> inventory = new List<Item>();

        public Controller(GraphicsDeviceManager graphics)
        {
            this.graphics = graphics;
            map = new Map(this, STARTMAPSIZE + level * 2);
            player = new Player(this);
            camera = new Camera(this, player.position, 32);
            player.health = player.maxHealth;
            map.CreateRandomLevel(3 + (int)level / 2, 2 + (int)level / 10, 2 + (int)level / 10, 3 + (int)level / 10, 3 + (int)level / 10);
        }

        public void MapLevelUp()
        {
            level++;

            if (level < LEVELCAP)
            {
                map.floorSize = 15 + level * 2;
                map.CreateRandomLevel(3 + (int)level / 2, 2 + (int)level / 10, 2 + (int)level / 10, 3 + (int)level / 10, 3 + (int)level / 10);
            }
            else
            {
                map.floorSize = 15 + LEVELCAP * 2;
                map.CreateRandomLevel(3 + (int)LEVELCAP / 2, 2 + (int)LEVELCAP / 10, 2 + (int)LEVELCAP / 10, 3 + (int)LEVELCAP / 10, 3 + (int)LEVELCAP / 10);
            }
            map.mapArray = map.StringToArray(map.mapString);
        }

        public void Update()
        {
            player.Update();
            map.Update();
            camera.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            map.Draw(spriteBatch);
        }
    }
}