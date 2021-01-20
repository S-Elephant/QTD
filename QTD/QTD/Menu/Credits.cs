using XNALib;
using Microsoft.Xna.Framework;

namespace QTD
{
    public class Credits : CreditsMenu
    {
        public Credits() :
            base(null, Engine.Instance, "Menu/spaceBG", "Font01_20", "Font02_28")
        {
            AddCreditTitle("QTD");
            AddCredit("Released <Not yet>");
            AddCredit("Released under the CC-BY-SA 3.0 license");
            AddCredit("Be aware that the Audio & Graphics might have their own licenses!");
            AddCreditTitle("");
            AddCreditTitle("");

            AddCreditTitle("Programming and Design");
            AddCredit("Napoleon");
            AddCredit("Napoleons XNA Library");
            AddCreditTitle("");
            AddCreditTitle("");

            AddCreditTitle("Graphics");
            AddCredit("OpenClipArt.org - Sell icon");
            AddCredit("Reiner Protekin - Animated Flags");
            AddCredit("Reiner Protekin - All runners and defenders");
            AddCredit("Reiner Protekin - Windmill, Palace");
            AddCreditTitle("");
            AddCredit("Below is all from Open Game Art (www.opengameart.org)");
            AddCredit("qubodup - Mouse cursor");
            AddCredit("sunburn - Gold coins icon");
            AddCredit("Clint Bellanger - Food icon");
            AddCredit("Bart - Gold pile");
            AddCredit("Scrittl - Wave Progress Bar");
            AddCredit("Nikke (mod. by Napoleon) - Menu Button");
            AddCredit("LokiF - Heart GUI texture");
            AddCredit("Saphy. Len Pabin - Mana Forest Tileset");
            AddCreditTitle("");
            AddCreditTitle("");

            AddCreditTitle("Audio FX (opengameart)");
            AddCredit("artisticdude - Buy/Sell sound");
            AddCredit("Scribe - RTS Commands");
            AddCredit("Lokif - GUI sounds");
            AddCredit("Michel Baradari apollo-music - Explosions");
            AddCreditTitle("");
            AddCreditTitle("");

            AddCreditTitle("Audio Music (www.opengameart.org)");
            AddCreditTitle("");
            AddCreditTitle("");

            AddCreditTitle("Testing");
            AddCredit("Napoleon");
            AddCreditTitle("");
            AddCreditTitle("");

            AddCreditTitle("Special Thanks & Misc");
            AddCredit("AudaCity");
            AddCredit("Microsoft");
            AddCredit("The Gimp");
            AddCredit("OpenGameArt.org");
            AddCredit("Iron Star Media LTD. (Fancy Bitmap Generator)");
            AddCreditTitle("");
            AddCreditTitle("");

            SetLocations();
        }
    }
}