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

        public enum ItemType : int { Potion, PotionRed, BigSword } //ADDITEM

        public enum ItemCat { Consumable, Equipment }

        public ItemCat itemCat;
        public int damageStat;

        //TODO: Rarity system
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
                        this.itemCat = ItemCat.Consumable;
                        break;
                    }
                case ItemType.PotionRed:
                    {
                        this.texture = ContentManager.tPotionRed;
                        this.itemCat = ItemCat.Consumable;
                        break;
                    }
                case ItemType.BigSword:
                    {
                        this.texture = ContentManager.tBigSword;
                        this.itemCat = ItemCat.Equipment;
                        this.damageStat = 5;
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
                //CONSUMABLES ONLY
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

        public void Equip()
        {
            switch (itemType)
            {
                case ItemType.BigSword:
                    controller.player.damage += damageStat;
                    break;

                default:
                    break;
            }
        }

        public void Unequip()
        {
            switch (itemType)
            {
                case ItemType.BigSword:
                    controller.player.damage -= damageStat;
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

                case ItemType.BigSword:
                    return "A big fuckin sword";

                default:
                    break;
            }
            return base.ToString();
        }
    }
}