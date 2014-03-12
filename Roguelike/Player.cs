using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Roguelike
{
    internal class Player
    {
        private Controller controller;
        private KeyboardState state, prevState;
        public Point position;
        public bool stepped = false;
        public float health, maxHealth = 15, damage = 5;

        public enum Movement { Left, Up, Right, Down }

        public Player(Controller controller)
        {
            this.controller = controller;
            state = Keyboard.GetState();
            position = new Point(0, 0);
            health = maxHealth;
        }

        public void Update()
        {
            prevState = state;
            state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.A) && prevState.IsKeyUp(Keys.A) || state.IsKeyDown(Keys.Left) && prevState.IsKeyUp(Keys.Left))
            {
                Move(Movement.Left);
            }
            if (state.IsKeyDown(Keys.D) && prevState.IsKeyUp(Keys.D) || state.IsKeyDown(Keys.Right) && prevState.IsKeyUp(Keys.Right))
            {
                Move(Movement.Right);
            }
            if (state.IsKeyDown(Keys.W) && prevState.IsKeyUp(Keys.W) || state.IsKeyDown(Keys.Up) && prevState.IsKeyUp(Keys.Up))
            {
                Move(Movement.Up);
            }
            if (state.IsKeyDown(Keys.S) && prevState.IsKeyUp(Keys.S) || state.IsKeyDown(Keys.Down) && prevState.IsKeyUp(Keys.Down))
            {
                Move(Movement.Down);
            }
            if (state.IsKeyDown(Keys.E) && prevState.IsKeyUp(Keys.E))
            {
                if (!stepped)
                {
                    int[,] tempMapArray = controller.map.StringToArray(controller.map.mapString);
                    if (tempMapArray[position.X, position.Y] == (int)Map.Element.Stairs)
                    {
                        controller.MapLevelUp();
                        stepped = true;
                    }
                    else if (tempMapArray[position.X, position.Y] == (int)Map.Element.Item)
                    {
                        tempMapArray[controller.player.position.X, controller.player.position.Y] = (int)Map.Element.Nothing;
                        controller.map.mapString = controller.map.ArrayToString(tempMapArray);
                        health += (int)maxHealth / 2;
                        stepped = true;
                        //picking up items etc
                    }
                }

            }
            if (health <= 0)
            {
                health = 0;
            }
            else if (health >= maxHealth)
            {
                health = maxHealth;
            }
        }

        public void Move(Movement movement)
        {
            if (stepped)
            {
                return;
            }

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

            //if (!controller.map.OutOfBounds(futurePos) && !controller.map.IsElement(futurePos, Map.Element.Wall) &&
            //!controller.map.IsElement(futurePos, Map.Element.Enemy))
            if (!controller.map.OutOfBounds(futurePos) && controller.map.IsWalkable(futurePos) || controller.map.IsElement(futurePos, Map.Element.Door))
            {
                position = futurePos;
            }
            else if (controller.map.IsElement(futurePos, Map.Element.Enemy))
            {
                for (int i = controller.map.enemyList.Count - 1; i > -1; i--)
                {
                    if (controller.map.enemyList[i].position == futurePos)
                    {
                        controller.map.enemyList[i].health -= controller.player.damage;
                    }
                }
            }
            stepped = true;

            //TO DO
            //Picking up items, attacking enemies, punchin through walls etc
        }
    }
}