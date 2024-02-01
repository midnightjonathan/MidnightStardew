using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightStardew.MidnightWorld
{
    public class MidnightLocation : GameLocation
    {
        public static void Replace(int i)
        {
            //Game1.locations[i] = new MidnightLocation(Game1.locations[i]);
        }

        private MidnightLocation(GameLocation stardewLocation) : base(stardewLocation.mapPath.Value, stardewLocation.Name) { }

        public override bool TryGetLocationEvents(out string assetName, out Dictionary<string, string> events)
        {
            assetName = "";
            events = new Dictionary<string, string>();
            return false;
        }

        public override void checkForEvents()
        {

        }

        public override void TransferDataFromSavedLocation(GameLocation l)
        {
            base.TransferDataFromSavedLocation(l);
        }
    }
}
