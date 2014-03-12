using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Roguelike
{
    internal class Item
    {
        public Texture2D texture;
        public Point position;
        public Controller controller;

        public enum ItemType { Potion = 0 }

        public ItemType itemType;

        public Item(Controller controller, Point position, ItemType itemType)
        {
            this.controller = controller;
            this.position = position;
            this.itemType = itemType;

            switch (itemType)
            {
                case ItemType.Potion:
                    {
                        this.texture = ContentManager.tPotion;
                        break;
                    }
                default:
                    break;
            }
        }

        public override string ToString()
        {
            switch (itemType)
            {
                case ItemType.Potion:
                    return "A vial of potion";

                default:
                    break;
            }
            return base.ToString();
        }
    }
}