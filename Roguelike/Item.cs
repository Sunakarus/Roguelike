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

        public enum ItemType : int { Potion, PotionRed } //ADDITEM

        //TODO
        /*public enum Rarity : int { Common, Uncommon, Rare }
         public enum CommonItems :int { Potion }
         public enum UncommonItems : int { PotionRed }*/

        public ItemType itemType;

        public Item(Controller controller, Point position, ItemType itemType, Map.Element baseElem)
        {
            this.controller = controller;
            this.position = position;
            this.itemType = itemType;
            this.baseElem = baseElem;

            switch (itemType)//ADDITEM
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

        //ADDITEM
        public void Use()
        {
            switch (itemType) 
            {
                case ItemType.Potion:
                    controller.player.health += (int)(controller.player.maxHealth / 2);
                    break;

                case ItemType.PotionRed:
                    controller.player.damage += 1;
                    break;

                default:
                    break;
            }
        }

        public override string ToString()
        {
            switch (itemType)//ADDITEM
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