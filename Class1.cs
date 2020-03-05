using System;
using System.Collections.Generic;
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
        private  void AddRandomGoodEvent(int value)
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
         * Adds a new event to the list of ongoing events stored in save data.
         */
        private void SaveEvent(Event newEvent)
        {
            // Reads in the list of events, initialises list if null, then adds the new event.
            var events = this.Helper.Data.ReadSaveData<List<Event>>("events") ?? new List<Event>();
            events.Add(newEvent);
            
            // Write the updated list of events to save data.
            this.Helper.Data.WriteGlobalData("events", events);
        }

        /**
         * Loads the list of events from save data and calls HandleEvent to alter the game appropriately.
         */
        private void LoadEvents()
        {
            var events = this.Helper.Data.ReadSaveData<List<Event>>("events");

            if (events == null) return;
            foreach (var eEvent in events)
            {
                HandleEvent(eEvent);        
            }
        }

        /**
         * Implements the effects of an event on the player / world.
         */
        private void HandleEvent(Event eEvent)
        {
            
        }
        
        /**
         * Calculates the value of an item based on its base value and its quality, with some preference.
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
                    
                case "Artisan Goods":
                    value = (int)Math.Floor(sellPrice * 1.5); // There's probably a nicer way to do this.
                    break;
                
                default:
                    value = sellPrice;
                    break;
            }

            return value;
        }
        
        /**
         * Data class for holding event data, to be read/written to save data.
         */
        class Event
        {
            public int eventID { get; set; }
            public int modifierValue { get; set; }
            public int daysLeft { get; set; }
        }
    }
}