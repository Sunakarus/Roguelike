using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike
{
    class Player
    {
        Controller controller;
        KeyboardState state, prevState;
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
            switch (movement)
            {
                case Movement.Down:
                    {
                        if (!controller.map.OutOfBounds(new Point(position.X, position.Y + 1)))
                        {
                            position.Y += 1;
                        }
                        break;
                    }
                case Movement.Up:
                    {
                        if (!controller.map.OutOfBounds(new Point(position.X, position.Y - 1)))
                        {
                            position.Y -= 1;
                        }
                        break;
                    }
                case Movement.Left:
                    {
                        if (!controller.map.OutOfBounds(new Point(position.X - 1, position.Y)))
                        {
                            position.X -= 1;
                        }
                        break;
                    }
                case Movement.Right:
                    {
                        if (!controller.map.OutOfBounds(new Point(position.X + 1, position.Y)))
                        {
                            position.X += 1;
                        }
                        break;
                    }
            }
        }
    }
}
