using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Roguelike
{
    internal class Gameplay
    {
        public Controller controller;
        public GraphicsDeviceManager graphics;

        public Gameplay(GraphicsDeviceManager graphics)
        {
            this.graphics = graphics;
            controller = new Controller(graphics);
        }

        public void Update()
        {
            controller.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            controller.Draw(spriteBatch);
        }
    }
}