using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewModdingAPI;

namespace YobaGifts
{
    internal class AltarMenu : MenuWithInventory
    {
        public static Texture2D MenuTexture;
        private readonly ModEntry modEntry;
        private string descriptionText;
        private ClickableTextureComponent donationSlot;
        private ClickableTextureComponent donateButton;

        public AltarMenu(ModEntry mod)
        {
            this.modEntry = mod;
            this.descriptionText = "Offer up an item to Yoba in exchange for his favour.";
        }
    }
}