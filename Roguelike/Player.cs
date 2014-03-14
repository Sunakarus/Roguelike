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

        public int viewDistance = 8;

        float delay;
        float maxDelay = 30;

        public enum Movement { Left, Up, Right, Down }

        public Player(Controller controller)
        {
            this.controller = controller;
            state = Keyboard.GetState();
            position = new Point(0, 0);
            health = maxHealth;
            delay = maxDelay;
        }

        public void Update()
        {
            prevState = state;
            state = Keyboard.GetState();

            if ((state.IsKeyDown(Keys.A) && prevState.IsKeyDown(Keys.A)) ||
                (state.IsKeyDown(Keys.W) && prevState.IsKeyDown(Keys.W)) ||
                (state.IsKeyDown(Keys.D) && prevState.IsKeyDown(Keys.D)) ||
                (state.IsKeyDown(Keys.S) && prevState.IsKeyDown(Keys.S)) ||
                (state.IsKeyDown(Keys.Left) && prevState.IsKeyDown(Keys.Left)) ||
                (state.IsKeyDown(Keys.Right) && prevState.IsKeyDown(Keys.Right)) ||
                (state.IsKeyDown(Keys.Up) && prevState.IsKeyDown(Keys.Up)) ||
                (state.IsKeyDown(Keys.Down) && prevState.IsKeyDown(Keys.Down)))
            {
                delay--;
            }
            else
            {
                delay = maxDelay;
            }
            foreach(Enemy e in controller.map.enemyList)
            {
                if (!e.asleep)
                {
                    delay = maxDelay;
                    break;
                }
            }

            if (state.IsKeyDown(Keys.A) && prevState.IsKeyUp(Keys.A) || state.IsKeyDown(Keys.Left) && prevState.IsKeyUp(Keys.Left) || ((state.IsKeyDown(Keys.A) || state.IsKeyDown(Keys.Left)) && delay <=0))
            {
                Move(Movement.Left);
            }
            if (state.IsKeyDown(Keys.D) && prevState.IsKeyUp(Keys.D) || state.IsKeyDown(Keys.Right) && prevState.IsKeyUp(Keys.Right) || ((state.IsKeyDown(Keys.D) || state.IsKeyDown(Keys.Right)) && delay <= 0))
            {
                Move(Movement.Right);
            }
            if (state.IsKeyDown(Keys.W) && prevState.IsKeyUp(Keys.W) || state.IsKeyDown(Keys.Up) && prevState.IsKeyUp(Keys.Up) || ((state.IsKeyDown(Keys.W) || state.IsKeyDown(Keys.Up)) && delay <= 0))
            {
                Move(Movement.Up);
            }
            if (state.IsKeyDown(Keys.S) && prevState.IsKeyUp(Keys.S) || state.IsKeyDown(Keys.Down) && prevState.IsKeyUp(Keys.Down) || ((state.IsKeyDown(Keys.S) || state.IsKeyDown(Keys.Down)) && delay <= 0))
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
                    else if (tempMapArray[position.X, position.Y] == (int)Map.Element.Item && controller.inventory.Count < Controller.INVENTORYSIZE)
                    {
                        tempMapArray[controller.player.position.X, controller.player.position.Y] = (int)Map.Element.Nothing;
                        stepped = true;

                        for (int i = controller.map.itemList.Count - 1; i > -1; i--)
                        {
                            if (controller.map.itemList[i].position == position)
                            {
                                controller.inventory.Add(controller.map.itemList[i]);
                                tempMapArray[position.X, position.Y] = (int)controller.map.itemList[i].baseElem;
                                controller.map.mapString = controller.map.ArrayToString(tempMapArray);
                                controller.map.itemList.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }
            //healing
            if (state.IsKeyDown(Keys.F) && prevState.IsKeyUp(Keys.F) && !stepped)
            {
                for (int i = 0; i < controller.inventory.Count; i++)
                {
                    if (controller.inventory[i].itemType == Item.ItemType.Potion)
                    {
                        controller.inventory.RemoveAt(i);
                        health += (int)maxHealth / 2;
                        stepped = true;
                        break;
                    }
                }
            }
            if (state.IsKeyDown(Keys.I) && prevState.IsKeyUp(Keys.I))
            {
                controller.showInv = !controller.showInv;
            }

            if (state.IsKeyDown(Keys.Q) && prevState.IsKeyUp(Keys.Q))
            {
                controller.showFog = !controller.showFog;
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