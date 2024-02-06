
namespace MidnightStardew.MidnightWorld
{
    public class MidnightSpot
    {
        /// <summary>
        /// All spots in the game.
        /// </summary>
        public static Dictionary<string, MidnightSpot> Get { get; set; } = new();

        /// <summary>
        /// Add a set of additional spots to the game.
        /// </summary>
        /// <param name="additionalSpots">The named spots to add.</param>
        public static void AddSpots(Dictionary<string, MidnightSpot> additionalSpots)
        {
            foreach (var spot in  additionalSpots)
            {
                Get.Add(spot.Key, spot.Value);
            }
        }

        /// <summary>
        /// The name of the game map where the spot is located.
        /// </summary>
        public string LocationName { get; set; }
        /// <summary>
        /// The position and size of the spot on the game map.
        /// </summary>
        public List<Microsoft.Xna.Framework.Rectangle> Rects { get; set; }

        /// <summary>
        /// The middle of the primary rect.
        /// </summary>
        public Microsoft.Xna.Framework.Point Middle 
        {
            get
            {
                return new Microsoft.Xna.Framework.Point(Rects[0].X + (Rects[0].Width / 2), Rects[0].Y + (Rects[0].Height / 2));
            } 
        }

        /// <summary>
        /// Creates a new spot.
        /// </summary>
        /// <param name="locationName">The name of the map that the spot is located on.</param>
        /// <param name="rects">The position and size of the spot.</param>
        public MidnightSpot(string locationName, List<Microsoft.Xna.Framework.Rectangle> rects) 
        {
            LocationName = locationName;
            Rects = rects;
        }

        /// <summary>
        /// Checks if an NPC is in the spot.
        /// </summary>
        /// <param name="npc">NPC to check their position.</param>
        /// <returns>True if NPC is in the spot.</returns>
        public bool IsIn(MidnightNpc npc)
        {
            if (npc.StardewNpc.currentLocation.Name != LocationName)
            {
                return false;
            }

            foreach (var rect in Rects)
            {
                if (!rect.Contains(npc.StardewNpc.TilePoint))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
