using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace YobaGifts
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            // Event handlers will be added here.
        }

        /**
         * Handles an item offering to Yoba.
         */
        public void ItemHandler(Item item)
        {
            if (item.getCategoryName() == "Trash")
            {
                AddRandomBadEvent();
                return;
            }

            var value = GetItemValue(item);
            AddRandomGoodEvent(value);

        }

        /**
         * Causes a random bad event to occur to the player who donated the item. The higher the offering 
         * value the higher the chance of better events occuring. 
         */
        private static void AddRandomGoodEvent(int value)
        {
            throw new NotImplementedException();
        }

        /**
         * Causes a random bad event to occur to the player who donated the item.
         */
        private static void AddRandomBadEvent()
        {
            throw new NotImplementedException();
        }
        
        /**
         * Calculates the value of an item based on its base value and its quality.
         */
        private static int GetItemValue(Item item)
        {
            int value;
            var sellPrice = item.salePrice();
            switch (item.getCategoryName())
            {
                case "Cooking":
                    value = sellPrice * 2; // Yoba likes a personal touch.
                    break;
                
                default:
                    value = sellPrice;
                    break;
            }

            return value;
        }
    }
}