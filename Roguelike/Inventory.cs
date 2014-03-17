using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Roguelike
{
    internal class Inventory
    {
        public List<Item> inventory = new List<Item>();
        public static readonly int INVENTORYSIZE = 8;
        public int chosenItem = 0;
        private Player player;

        private Controller controller;

        public Inventory(Controller controller)
        {
            this.controller = controller;
            player = controller.player;
        }

        public void DropItem()
        {
            int[,] tempArray = controller.map.StringToArray(controller.map.mapString);

            if (!player.stepped && controller.inventory.Count > 0 && tempArray[player.position.X, player.position.Y] != (int)Map.Element.Stairs)
            //can't drop on stairs to prevent getting stuck
            {
                Item tempItem = controller.inventory[chosenItem];
                if (player.equippedList.Contains(tempItem))
                {
                    tempItem.Unequip();
                    player.equippedList.Remove(tempItem);
                }
                controller.inventory.RemoveAt(chosenItem);
                if (chosenItem > 0)
                {
                    chosenItem--;
                }

                tempItem.position = player.position;
                tempItem.baseElem = (Map.Element)tempArray[player.position.X, player.position.Y];
                tempArray[player.position.X, player.position.Y] = (int)Map.Element.Item;
                controller.map.itemList.Add(tempItem);
                controller.map.mapString = controller.map.ArrayToString(tempArray);
            }
        }

        public void UseItem()
        {
            if (inventory.Count > 0)
            {
                if (inventory[chosenItem].itemCat == Item.ItemCat.Consumable)
                {
                    inventory[chosenItem].Use();
                    inventory.RemoveAt(chosenItem);
                    if (chosenItem != 0)
                    {
                        chosenItem--;
                    }
                }
                else if (inventory[chosenItem].itemCat == Item.ItemCat.Armor || inventory[chosenItem].itemCat == Item.ItemCat.Weapon)
                {
                    Item tempItem = inventory[chosenItem];
                    if (player.equippedList.Count == 0)
                    {
                        tempItem.Equip();
                        player.equippedList.Add(tempItem);
                        if (tempItem.itemCat == Item.ItemCat.Weapon)
                        {
                            player.weapon = tempItem;
                        }
                    }
                    else if (player.weapon == tempItem)
                    {
                        player.weapon.Unequip();
                        player.equippedList.Remove(player.weapon);
                        player.weapon = null;
                    }
                    else if (player.weapon == null && tempItem.itemCat == Item.ItemCat.Weapon)
                    {
                        tempItem.Equip();
                        player.equippedList.Add(tempItem);
                        player.weapon = tempItem;
                    }
                    else if (player.weapon != null && tempItem.itemCat == Item.ItemCat.Weapon)
                    {
                        player.weapon.Unequip();
                        player.equippedList.Remove(player.weapon);
                        tempItem.Equip();
                        player.equippedList.Add(tempItem);
                        player.weapon = tempItem;
                    }
                    else
                    {
                        bool tryAdd = true;
                        for (int i = player.equippedList.Count - 1; i > -1; i--)
                        {
                            if (player.equippedList[i] == tempItem)
                            {
                                player.equippedList[i].Unequip();
                                player.equippedList.RemoveAt(i);
                                tryAdd = false;
                                break;
                            }

                            else if (player.equippedList[i].itemType == tempItem.itemType)
                            {
                                player.equippedList[i].Unequip();
                                player.equippedList.RemoveAt(i);
                                tempItem.Equip();
                                player.equippedList.Add(tempItem);
                                break;
                            }
                        }
                        if (!player.equippedList.Contains(tempItem) && tryAdd)
                        {
                            tempItem.Equip();
                            player.equippedList.Add(tempItem);
                        }
                    }
                }
            }
        }

        public void PickUpItem()
        {
            int[,] tempMapArray = controller.map.StringToArray(controller.map.mapString);
            if (tempMapArray[player.position.X, player.position.Y] == (int)Map.Element.Stairs)
            {
                controller.MapLevelUp();
            }

            else if (tempMapArray[player.position.X, player.position.Y] == (int)Map.Element.Item && inventory.Count < INVENTORYSIZE)
            {
                for (int i = controller.map.itemList.Count - 1; i > -1; i--)
                {
                    if (controller.map.itemList[i].position == player.position)
                    {
                        controller.inventory.Add(controller.map.itemList[i]);
                        tempMapArray[player.position.X, player.position.Y] = (int)controller.map.itemList[i].baseElem;
                        controller.map.mapString = controller.map.ArrayToString(tempMapArray);
                        controller.map.itemList.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        public void ScrollUp()
        {
            if (chosenItem > 0)
            {
                chosenItem--;
            }
            else
            {
                chosenItem = controller.inventory.Count - 1;
            }
        }

        public void ScrollDown()
        {
            if (chosenItem < controller.inventory.Count - 1)
            {
                chosenItem++;
            }
            else
            {
                chosenItem = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch, int tileSize)
        {
            string equip = "";
            Vector2 cameraZero = new Vector2(controller.camera.position.X * tileSize, controller.camera.position.Y * tileSize);

            spriteBatch.DrawString(ContentManager.font, "\n\n\n\n\n\n\nZ,C - scroll\nF: Use\nR: Drop", cameraZero, Color.ForestGreen, 0, Vector2.Zero, 1 / controller.camera.scale, SpriteEffects.None, 100);
            string newLines = "\n\n\n\n\n\n\n\n\n";

            for (int i = 0; i < controller.inventory.Count; i++)
            {
                newLines += "\n";
                equip = "";
                Color fontColor = Color.DarkGoldenrod;

                if ((controller.inventory[i].itemCat == Item.ItemCat.Armor || controller.inventory[i].itemCat == Item.ItemCat.Weapon) && controller.inventory[i].bonusStat != 0)
                {
                    fontColor = Color.SkyBlue;
                }

                if (controller.inv.chosenItem == i)
                {
                    fontColor = Color.Yellow;
                }

                if ((controller.inventory[i].itemCat == Item.ItemCat.Armor || controller.inventory[i].itemCat == Item.ItemCat.Weapon) && controller.inventory[i].bonusStat != 0)
                {
                    equip += " (+" + controller.inventory[i].bonusStat + ")";
                }

                if (player.equippedList.Contains(controller.inventory[i]))
                {
                    equip += " - E";
                }
                else
                {
                    equip += "";
                }

                spriteBatch.DrawString(ContentManager.font, newLines + (i + 1) + ") " + controller.inventory[i].ToString() + equip, cameraZero, fontColor, 0, Vector2.Zero, 1 / controller.camera.scale, SpriteEffects.None, 100);
            }
        }
    }
}