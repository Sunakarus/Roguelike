using Microsoft.Xna.Framework.Graphics;
using System;

namespace Roguelike
{
    internal class Controller
    {
        public Map map;
        public Player player;
        public Random random = new Random();

        public Controller()
        {
            map = new Map(this, 30);
            map.GenerateEmptyFloor();
            player = new Player(this);
        }

        public void Update()
        {
            player.Update();
            map.Update();
        }

        public void Draw(SpriteBatch spriteBatch, ContentManager contentManager)
        {
            map.Draw(spriteBatch, contentManager);
        }
    }
}