using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Roguelike
{
    internal class Player
    {
        private Controller controller;
        private KeyboardState state, prevState;
        public Point position;
        public bool stepped = false;
        public int playerLevel = 1;

        public float health, maxHealth = 15;
        public float damage = 2;
        public float defense = 0;
        public float experience = 0, maxExperience = 15;

        public int viewDistance = 6;

        public Item weapon;

        public List<Item> equippedList = new List<Item>();

        private float delay;
        private float maxDelay = 30;

        public enum Movement { Left, Up, Right, Down }

        private Map map;

        public Player(Controller controller)
        {
            this.controller = controller;
            state = Keyboard.GetState();
            position = new Point(0, 0);
            health = maxHealth;
            delay = maxDelay;

            map = controller.map;
        }

        public void LevelUp()
        {
            playerLevel++;
            int addHealth = controller.random.Next(3) + 1;

            experience -= maxExperience;
            maxExperience += playerLevel * 2;

            maxHealth += addHealth;
            health += addHealth;

            damage += controller.random.Next(2) + 1;
            defense += controller.random.Next(2);
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
                if (delay > 0)
                {
                    delay--;
                }
            }
            else
            {
                delay = maxDelay;
            }

            foreach (Enemy e in map.enemyList)
            {
                if (e.isSeen)
                {
                    delay = maxDelay;
                    break;
                }
            }

            if (state.IsKeyDown(Keys.A) && prevState.IsKeyUp(Keys.A) || state.IsKeyDown(Keys.Left) && prevState.IsKeyUp(Keys.Left) || ((state.IsKeyDown(Keys.A) || state.IsKeyDown(Keys.Left)) && delay <= 0))
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
                    controller.inv.PickUpItem();
                    stepped = true;
                }
            }

            if (controller.showInv)
            {
                if (state.IsKeyDown(Keys.Z) && prevState.IsKeyUp(Keys.Z))
                {
                    controller.inv.ScrollUp();
                }
                if (state.IsKeyDown(Keys.C) && prevState.IsKeyUp(Keys.C))
                {
                    controller.inv.ScrollDown();
                }
                if (state.IsKeyDown(Keys.F) && prevState.IsKeyUp(Keys.F) && !stepped)
                {
                    controller.inv.UseItem();
                }
                if (state.IsKeyDown(Keys.R) && prevState.IsKeyUp(Keys.R))
                {
                    controller.inv.DropItem();
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

            if (!map.OutOfBounds(futurePos) && map.IsWalkable(futurePos) || map.IsElement(futurePos, Map.Element.Door))
            {
                position = futurePos;
            }
            else if (map.IsElement(futurePos, Map.Element.Enemy))
            {
                for (int i = map.enemyList.Count - 1; i > -1; i--)
                {
                    if (map.enemyList[i].position == futurePos)
                    {
                        map.enemyList[i].health -= controller.player.damage;
                    }
                }
            }
            stepped = true;
        }

        public override string ToString()
        {
            int buffDamage = 0, buffDefense = 0, buffMaxHealth = 0;
            foreach (Buff b in controller.buffList)
            {
                buffDamage += b.buffDamage;
                buffDefense += b.buffDefense;
                buffMaxHealth += b.buffMaxHealth;
            }
            string buffDam, buffDef, buffMaxHeal;
            buffDam = buffDamage == 0 ? "" : " (+" + buffDamage + ")";
            buffDef = buffDefense == 0 ? "" : " (+" + buffDefense + ")";
            buffMaxHeal = buffMaxHealth == 0 ? "" : " (+" + buffMaxHealth + ")";

            return String.Format("Health: {0}/{1}{7} \nPlayer level: {2}\nEXP: {3}/{4}\nDamage: {5}{8}\nDefense: {6}{9}", health, maxHealth, playerLevel, experience, maxExperience, damage, defense, buffMaxHeal, buffDam, buffDef);
        }
    }
}