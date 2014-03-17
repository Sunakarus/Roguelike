using Microsoft.Xna.Framework.Graphics;

namespace Roguelike
{
    internal class Buff
    {
        public int turns, maxTurns;
        private Controller controller;
        public int buffDamage, buffMaxHealth, buffDefense;
        public Texture2D texture;

        public Buff(Texture2D texture, Controller controller, int maxTurns)
        {
            this.texture = texture;
            this.maxTurns = maxTurns;
            this.controller = controller;
            turns = this.maxTurns;
        }

        public void Update()
        {
            if (turns > 0)
            {
                turns--;
            }
        }

        public void AddPlayerAttributes(int damage, int defense, int maxHealth)
        {
            buffDamage = damage;
            buffDefense = defense;
            buffMaxHealth = maxHealth;
            controller.player.damage += buffDamage;
            controller.player.defense += buffDefense;
            controller.player.maxHealth += buffMaxHealth;
        }

        public bool IsDone()
        {
            return turns <= 1;
        }

        public void Debuff()
        {
            controller.player.damage -= buffDamage;
            controller.player.defense -= buffDefense;
            controller.player.maxHealth -= buffMaxHealth;
        }
    }
}