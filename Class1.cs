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
            helper.Events.GameLoop.DayStarted += LoadEvents;
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
         * Updates the list of ongoing events stored in save data.
         */
        private void SaveEvents(List<Event> events)
        {
            // Write the updated list of events to save data.
            this.Helper.Data.WriteGlobalData("events", events);
        }

        /**
         * Loads the list of events from save data and calls HandleEvent to alter the game appropriately.
         */
        private void LoadEvents(object sender, DayStartedEventArgs e)
        {
            var events = this.Helper.Data.ReadSaveData<List<Event>>("events");

            if (events == null) return;
            foreach (var eEvent in events)
            {
                HandleEvent(eEvent, events);        
            }
            
            // Update the event list in memory, as events may have expired this day.
            SaveEvents(events);
        }

        /**
         * Implements the effects of an event on the player / world.
         */
        private void HandleEvent(Event eEvent, List<Event> events)
        {
            if (eEvent.daysLeft == 0)
            {
                CleanUpEvent(eEvent);
                events.Remove(eEvent);
            }
            switch (eEvent.eventID)
            {
                case "luck":
                    Game1.player.team.sharedDailyLuck.Value += 0.03;
                    break;
                case "maxhealth":
                    Game1.player.maxHealth += 20;
                    break;
                case "maxenergy":
                    Game1.player.MaxStamina += 20;     
                    break;
            }
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
         * Reverts changes to values caused by events.
         */
        private void CleanUpEvent(Event eEvent)
        {
            
            switch (eEvent.eventID)
                        {
                            case "maxhealth":
                                Game1.player.maxHealth -= 20;
                                break;
                            case "maxenergy":
                                Game1.player.MaxStamina -= 20;     
                                break;
                        }
        }
        
        /**
         * Data class for holding event data, to be read/written to save data.
         */
        class Event
        {
            public string eventID { get; set; } // luck, maxhealth, maxenergy,
            public int modifierValue { get; set; }
            public int daysLeft { get; set; }
        }
    }
}