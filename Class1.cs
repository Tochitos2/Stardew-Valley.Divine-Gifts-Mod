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
                Helper.Events.Input.ButtonPressed += CheckSelectedTileIsShrine;
            }
            else
            {
                Helper.Events.Input.ButtonPressed -= CheckSelectedTileIsShrine;
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

            // Get the selected tile coordinates and checks if they match the shrine's; if they do open the menu.
            var selectedTile = Game1.player.getTileLocation();
            var coordinates =  selectedTile.X + "," + selectedTile.Y;
            this.Monitor.Log(coordinates + "  " + Game1.player.FacingDirection);

            // Checks the player is adjacent to the shrine and has used an action button,
            // then suppresses old message and opens menu.
            if (!e.Button.IsActionButton() || !(selectedTile.X > 34) || !(selectedTile.X < 40) ||
                !(selectedTile.Y > 16) || !(selectedTile.Y < 19)) return;
            this.Helper.Input.Suppress(e.Button);
            ShowMenu();
        }

        /**
         * Opens the UI menu for donating items to the shrine.
         */
        private void ShowMenu()
        {
            this.Monitor.Log("Menu opening.");
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
         * Causes a random good event to occur to the player who donated the item. The higher the offering 
         * value the higher the chance of better events occuring, and the longer the effect. 
         */
        private  void AddRandomGoodEvent(int value)
        {
            var ringChance = value * 0.02;
            var luckChance = value * 0.08;
            var staminaChance = value * 0.14;
            var result = new Random().Next(1,101);
            var events = this.Helper.Data.ReadSaveData<List<Event>>("events") ?? new List<Event>();
            var eEvent = new Event {DaysLeft = Convert.ToInt32(Math.Floor(Math.Sqrt(value) / 6))};

            if (ringChance > result)
            {
                eEvent.EventId = "ringofyoba";
                HandleEvent(eEvent, events);
                SaveEvents(events);
            }
            else if (luckChance > result)
            {
                eEvent.EventId = "luck";
                eEvent.ModifierValue = 0.03;
                HandleEvent(eEvent, events);
                SaveEvents(events);
            }
            else if (staminaChance > result)
            {
                eEvent.EventId = "maxstamina";
                eEvent.ModifierValue = 17;
                HandleEvent(eEvent, events);
                SaveEvents(events);
            }
            else
            {
                eEvent.EventId = "maxhealth";
                eEvent.ModifierValue = 20;
                HandleEvent(eEvent, events);
                SaveEvents(events);
            }
        }

        /**
         * Causes a random bad event to occur to the player who donated the item.
         */
        private void AddRandomBadEvent()
        {
            var eEvent = new Event();
            var random = new Random();
            switch (random.Next(1,6))
            {
                case 1:
                    eEvent.EventId = "luck";
                    eEvent.DaysLeft = random.Next(1, 4);
                    eEvent.ModifierValue = -0.03;
                    break;
                case 2: case 3:
                    eEvent.EventId = "maxhealth";
                    eEvent.DaysLeft = random.Next(1, 4);
                    eEvent.ModifierValue = -15;
                    break;
                case 4: case 5:
                    eEvent.EventId = "maxstamina";
                    eEvent.DaysLeft = random.Next(1, 4);
                    eEvent.ModifierValue = -20;
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
            eEvent.DaysLeft -= 1;
            if (eEvent.DaysLeft == 0)
            {
                events.Remove(eEvent);
            }
            switch (eEvent.EventId)
            {
                case "luck":
                    Game1.player.team.sharedDailyLuck.Value += eEvent.ModifierValue;
                    break;
                case "maxhealth":
                    Game1.player.maxHealth += Convert.ToInt16(eEvent.ModifierValue);
                    break;
                case "maxstamina":
                    Game1.player.MaxStamina += Convert.ToInt16(eEvent.ModifierValue);     
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
                switch (eEvent.EventId)
                {
                    case "maxhealth":
                    Game1.player.maxHealth -= Convert.ToInt16(eEvent.ModifierValue);
                    break;
                    case "maxstamina":
                    Game1.player.MaxStamina -= Convert.ToInt16(eEvent.ModifierValue);     
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
            public string EventId { get; set; } // luck, maxhealth, maxstamina, ringofyoba
            public double ModifierValue { get; set; }
            public int DaysLeft { get; set; }
        }
    }
}