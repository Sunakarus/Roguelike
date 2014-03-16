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

        public enum ItemType : int { Potion, PotionRed, BigSword, Shield } //ADDITEM

        public enum ItemCat { Consumable, Equipment }

        public ItemCat itemCat;
        public int damageStat = 0;
        public int defenseStat = 0;
        public int bonusStat;

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
                        texture = ContentManager.tPotion;
                        itemCat = ItemCat.Consumable;
                        break;
                    }
                case ItemType.PotionRed:
                    {
                        texture = ContentManager.tPotionRed;
                        itemCat = ItemCat.Consumable;
                        break;
                    }
                case ItemType.BigSword:
                    {
                        texture = ContentManager.tBigSword;
                        itemCat = ItemCat.Equipment;

                        damageStat = 5;
                        break;
                    }
                case ItemType.Shield:
                    {
                        texture = ContentManager.tShield;
                        itemCat = ItemCat.Equipment;

                        defenseStat = 3;
                        break;
                    }
            }

            if (itemCat == ItemCat.Equipment)
            {
                if (damageStat > 0)
                {
                    bonusStat = controller.random.Next(damageStat + 1);
                    damageStat += bonusStat;
                }
                if (defenseStat > 0)
                {
                    bonusStat = controller.random.Next(defenseStat + 1);
                    defenseStat += bonusStat;
                }
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
            controller.player.damage += damageStat;
            controller.player.defense += defenseStat;
        }

        public void Unequip()
        {
            controller.player.damage -= damageStat;
            controller.player.defense -= defenseStat;
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

                case ItemType.Shield:
                    return "A kiteshield";

                default:
                    break;
            }
            return base.ToString();
        }
    }
}