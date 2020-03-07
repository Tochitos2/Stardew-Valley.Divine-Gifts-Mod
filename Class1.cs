using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;

namespace YobaGifts
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            // Load events from save data, and initiate or delete them as appropriate.
            helper.Events.GameLoop.DayStarted += LoadEvents;

            helper.Events.GameLoop.DayEnding += CleanUpEvents;
            // Add/remove input listener for shrine interaction on entering leaving pierre's shop.
            helper.Events.Player.Warped += HandleShopEvents;
        }

        /**
         * Add/remove input listener for shrine interaction on entering leaving pierre's shop.
         */
        private void HandleShopEvents(object sender, WarpedEventArgs e)
        {
            if (Game1.currentLocation.name == "SeedShop")
            {
                this.Helper.Events.Input.ButtonPressed += CheckSelectedTileIsShrine;
            }
            else
            {
                this.Helper.Events.Input.ButtonPressed -= CheckSelectedTileIsShrine;
            }
        }

        /**
         * Checks whether the player currently has the shrine selected when a button is pressed, and opens the menu.
         */
        private void CheckSelectedTileIsShrine(object sender, ButtonPressedEventArgs e)
        {
            // Return if the pressed button is not an interaction button.
            if(!e.Button.IsActionButton()) { return; }
            // Suppresses the button pressed to prevent the original dialogue for the shrine from appearing.
            this.Helper.Input.Suppress(e.Button);
            
            // Get the selected tile coordinates and checks if they match the shrine's; if they do open the menu.
            var selectedTile = Game1.player.getTileLocation();
            if (selectedTile.Y == 272 && selectedTile.X > 575 && selectedTile.X < 609)
            {
                ShowMenu();
            }
        }

        /**
         * Opens the UI menu for donating items to the shrine.
         */
        private void ShowMenu()
        {
            throw new NotImplementedException();
        }

        public void SetUpShrineTile()
        {
            var location = Game1.getLocationFromName("SeedShop");
            
        }

        /**
         * Handles an item passed via the donation UI.
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
        private void AddRandomBadEvent()
        {
            var eEvent = new Event();
            var random = new Random();
            switch (random.Next(1,5))
            {
                case 1:
                    eEvent.eventID = "luck";
                    eEvent.daysLeft = random.Next(1, 3);
                    eEvent.modifierValue = -0.03;
                    break;
                case 2: case 3:
                    eEvent.eventID = "maxhealth";
                    eEvent.daysLeft = random.Next(1, 3);
                    eEvent.modifierValue = -15;
                    break;
                case 4: case 5:
                    eEvent.eventID = "maxstamina";
                    eEvent.daysLeft = random.Next(1, 3);
                    eEvent.modifierValue = -20;
                    break;
            }
            var events = this.Helper.Data.ReadSaveData<List<Event>>("events") ?? new List<Event>();
            HandleEvent(eEvent, events);
            events.Add(eEvent);
            SaveEvents(events);
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
            eEvent.daysLeft -= 1;
            if (eEvent.daysLeft == 0)
            {
                events.Remove(eEvent);
            }
            switch (eEvent.eventID)
            {
                case "luck":
                    Game1.player.team.sharedDailyLuck.Value += eEvent.modifierValue;
                    break;
                case "maxhealth":
                    Game1.player.maxHealth += Convert.ToInt16(eEvent.modifierValue);
                    break;
                case "maxstamina":
                    Game1.player.MaxStamina += Convert.ToInt16(eEvent.modifierValue);     
                    break;
                case "ringofyoba":
                    GiveYobaRing();
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
        private void CleanUpEvents(object sender, DayEndingEventArgs e)
        {
            var events = this.Helper.Data.ReadSaveData<List<Event>>("events");
            if (events == null) return;

            foreach (var eEvent in events)
            {
                switch (eEvent.eventID)
                {
                    case "maxhealth":
                    Game1.player.maxHealth -= Convert.ToInt16(eEvent.modifierValue);
                    break;
                    case "maxstamina":
                    Game1.player.MaxStamina -= Convert.ToInt16(eEvent.modifierValue);     
                    break;
                }
            }
        }
        
        /**
         * Gives the player the ring of yoba.
         */
        private void GiveYobaRing()
        {
            Item ring = new Ring(524); // ID of ring of yoba
            Game1.player.addItemByMenuIfNecessary(ring);
        }
        
        /**
         * Data class for holding event data, to be read/written to save data.
         */
        class Event
        {
            public string eventID { get; set; } // luck, maxhealth, maxstamina, ringofyoba
            public double modifierValue { get; set; }
            public int daysLeft { get; set; }
        }
    }
}