using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Roguelike
{
    internal class Player
    {
        private Controller controller;
        private KeyboardState state, prevState;
        public Point position;

        public enum Movement { Left, Up, Right, Down }

        public Player(Controller controller)
        {
            this.controller = controller;
            state = Keyboard.GetState();
            position = new Point(0, 0);
        }

        public void Update()
        {
            prevState = state;
            state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.A) && prevState.IsKeyUp(Keys.A))
            {
                Move(Movement.Left);
            }
            if (state.IsKeyDown(Keys.D) && prevState.IsKeyUp(Keys.D))
            {
                Move(Movement.Right);
            }
            if (state.IsKeyDown(Keys.W) && prevState.IsKeyUp(Keys.W))
            {
                Move(Movement.Up);
            }
            if (state.IsKeyDown(Keys.S) && prevState.IsKeyUp(Keys.S))
            {
                Move(Movement.Down);
            }
        }

        public void Move(Movement movement)
        {
            Point futurePos = Point.Zero;
            switch (movement)
            {
                case Movement.Down:
                    {
                        futurePos = new Point(position.X, position.Y + 1);
                        break;
                    }
                case Movement.Up:
                    {
                        futurePos = new Point(position.X, position.Y - 1);
                        break;
                    }
                case Movement.Left:
                    {
                        futurePos = new Point(position.X - 1, position.Y);
                        break;
                    }
                case Movement.Right:
                    {
                        futurePos = new Point(position.X + 1, position.Y);
                        break;
                    }
            }
            if (!controller.map.OutOfBounds(futurePos) && !controller.map.IsWall(futurePos))
            {
                position = futurePos;
            }
        }
    }
}