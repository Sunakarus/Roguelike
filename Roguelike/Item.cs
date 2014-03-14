using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Roguelike
{
    internal class Item
    {
        public Texture2D texture;
        public Point position;
        public Controller controller;
        public Map.Element baseElem; //what is on the ground under the item

        public enum ItemType { Potion = 0, PotionRed = 1 }

        public ItemType itemType;

        public Item(Controller controller, Point position, ItemType itemType, Map.Element baseElem)
        {
            this.controller = controller;
            this.position = position;
            this.itemType = itemType;
            this.baseElem = baseElem;

            switch (itemType)
            {
                case ItemType.Potion:
                    {
                        this.texture = ContentManager.tPotion;
                        break;
                    }
                case ItemType.PotionRed:
                    {
                        this.texture = ContentManager.tPotionRed;
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

                case ItemType.PotionRed:
                    return "A vial of blood";

                default:
                    break;
            }
            return base.ToString();
        }
    }
}