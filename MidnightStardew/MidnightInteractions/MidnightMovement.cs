using MidnightStardew.MidnightWorld;
using StardewValley;
using System.Text.Json.Serialization;

namespace MidnightStardew.MidnightInteractions
{
    public  class MidnightMovement
    {
        public string LocationName { get; set; }
        public Microsoft.Xna.Framework.Point Position { get; set; }
        public MidnightConversation AfterMoveConversation { get; set; }
        public MidnightRequirements Requirements { get; set; }

        [JsonConstructor]
        public MidnightMovement(string? spot, string? locationName, Microsoft.Xna.Framework.Point? position, MidnightConversation afterMove, MidnightRequirements reqs) 
        { 
            if (spot != null)
            {
                var spotObject = MidnightSpot.Get[spot];
                LocationName = spotObject.LocationName;
                Position = spotObject.Middle;
            }
            else
            {
                LocationName = locationName ?? throw new ApplicationException("If spot is not defined LocationName and Position must be defined.");
                Position = position ?? throw new ApplicationException("If spot is not defined LocationName and Position must be defined.");
            }

            AfterMoveConversation = afterMove;
            Requirements = reqs;
        }
    }
}
