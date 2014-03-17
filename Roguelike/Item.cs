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

        public enum ItemType : int { Potion, PotionRed, BigSword, Shield, PlateArmor, BrokenSword, HealingPotion } //ADDITEM

        public enum ItemCat { Consumable, Armor, Weapon }

        public ItemCat itemCat;
        public int damageStat = 0;
        public int defenseStat = 0;
        public int bonusStat;
        public static readonly int BONUSCHANCE = 3; // chance = 1/BONUSCHANCE

        //TODO: Rarity system
        public enum Rarity : int { Common, Uncommon, Rare }

        public ItemType[] commonArray = { ItemType.Potion, ItemType.HealingPotion }; //ADDITEM
        public ItemType[] uncommonArray = { ItemType.PotionRed, ItemType.Shield, ItemType.BrokenSword };
        public ItemType[] rareArray = { ItemType.BigSword, ItemType.PlateArmor };

        public ItemType itemType;

        public Item(Controller controller, Point position, Map.Element baseElem)
        {
            this.controller = controller;
            this.position = position;
            this.baseElem = baseElem;

            Rarity rarity = RollRarity();

            int random;
            switch (rarity)
            {
                case Rarity.Common:
                    random = controller.random.Next(commonArray.Length);
                    itemType = commonArray[random];
                    break;

                case Rarity.Uncommon:
                    random = controller.random.Next(uncommonArray.Length);
                    itemType = uncommonArray[random];
                    break;

                case Rarity.Rare:
                    random = controller.random.Next(rareArray.Length);
                    itemType = rareArray[random];
                    break;
            }

            switch (itemType)//ADDITEM
            {
                case ItemType.Potion:
                    {
                        texture = ContentManager.tPotion;
                        itemCat = ItemCat.Consumable;
                        break;
                    }
                case ItemType.HealingPotion:
                    {
                        texture = ContentManager.tHealingPotion;
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
                        itemCat = ItemCat.Weapon;

                        damageStat = 5;
                        break;
                    }
                case ItemType.Shield:
                    {
                        texture = ContentManager.tShield;
                        itemCat = ItemCat.Armor;

                        defenseStat = 1;
                        break;
                    }
                case ItemType.PlateArmor:
                    {
                        texture = ContentManager.tPlateArmor;
                        itemCat = ItemCat.Armor;

                        defenseStat = 4;
                        break;
                    }
                case ItemType.BrokenSword:
                    {
                        texture = ContentManager.tBrokenSword;
                        itemCat = ItemCat.Weapon;

                        damageStat = 2;
                        break;
                    }
            }

            if (itemCat == ItemCat.Armor || itemCat == ItemCat.Weapon)
            {
                if (controller.random.Next(BONUSCHANCE) == BONUSCHANCE - 1)
                {
                    if (damageStat > 0)
                    {
                        bonusStat = controller.random.Next(damageStat) + 1;
                        damageStat += bonusStat;
                    }
                    if (defenseStat > 0)
                    {
                        bonusStat = controller.random.Next(defenseStat) + 1;
                        defenseStat += bonusStat;
                    }
                }
            }
        }

        private Rarity RollRarity()
        {
            int diceRoll = controller.random.Next(1, 101);
            if (diceRoll > 90)
            {
                return Rarity.Rare;
            }
            else if (diceRoll > 70)
            {
                return Rarity.Uncommon;
            }
            else
            {
                return Item.Rarity.Common;
            }
        }

        //ADDITEM
        public void Use()
        {
            Buff buff;
            switch (itemType)
            {
                //CONSUMABLES ONLY
                case ItemType.Potion:
                    buff = new Buff(ContentManager.tPotion, controller, 50);
                    buff.AddPlayerAttributes(1, 1, 1);
                    controller.buffList.Add(buff);
                    //controller.player.health += 5;
                    break;

                case ItemType.PotionRed:
                    buff = new Buff(ContentManager.tPotionRed, controller, 40);
                    buff.AddPlayerAttributes(5, 0, 0);
                    controller.buffList.Add(buff);
                    //controller.player.damage += 1;
                    break;

                case ItemType.HealingPotion:
                    controller.player.health += 5;
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
            switch (itemType) //ADDITEM
            {
                case ItemType.Potion:
                    return "An elixir";

                case ItemType.PotionRed:
                    return "A vial of blood";

                case ItemType.BigSword:
                    return "A big fuckin sword";

                case ItemType.BrokenSword:
                    return "A shitty sword";

                case ItemType.Shield:
                    return "A kiteshield";

                case ItemType.PlateArmor:
                    return "Steel plate armor";

                case ItemType.HealingPotion:
                    return "A bottle of medicine";

                default:
                    break;
            }
            return base.ToString();
        }
    }
}