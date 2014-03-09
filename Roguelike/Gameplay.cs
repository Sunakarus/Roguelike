using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Roguelike
{
    internal class Gameplay
    {
        public Controller controller;
        public ContentManager contentManager;
        public GraphicsDeviceManager graphics;

        public Gameplay(ContentManager contentManager, GraphicsDeviceManager graphics)
        {
            this.contentManager = contentManager;
            this.graphics = graphics;
            controller = new Controller(graphics);
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