using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Roguelike
{
    internal class Controller
    {
        public Map map;
        public Player player;
        public Random random = new Random();
        public Camera camera;
        public GraphicsDeviceManager graphics;

        public Controller(GraphicsDeviceManager graphics)
        {
            this.graphics = graphics;
            map = new Map(this, 50);
            player = new Player(this);
            camera = new Camera(this, player.position, 32);

            map.CreateRandomLevel(15, 2, 1, 5, 4);
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