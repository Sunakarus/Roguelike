using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike
{
    class Controller
    {
        
        public Map map;
        public Player player;
        public Random random = new Random();

        public Controller()
        {
            map = new Map(this, 12);
            map.GenerateFloor();
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
