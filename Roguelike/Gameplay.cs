using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike
{
    class Gameplay
    {
        public Controller controller;
        public ContentManager contentManager;

        public Gameplay(ContentManager contentManager)
        {
            this.contentManager = contentManager;
            controller = new Controller();

        }

        public void Update()
        {
            controller.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            controller.Draw(spriteBatch, contentManager);
        }
    }
}
