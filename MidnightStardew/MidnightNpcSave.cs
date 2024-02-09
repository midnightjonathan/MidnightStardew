using MidnightStardew.MidnightInteractions;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightStardew
{
    public class MidnightNpcSave
    {        /// <summary>
        /// Custom stats for your mod.
        /// </summary>
        public Dictionary<string, MidnightStats> Stats { get; set; }

        /// <summary>
        /// The set of all conversations the player has had with the NPC.
        /// </summary>
        public HashSet<string> ExperiencedConverastions { get; set; }

        public MidnightConversation? NextConversation { get; set; }

        /// <summary>
        /// Indicates if the NPC has met the player.
        /// </summary>
        public bool HasIntroduced { get; set; }

        public MidnightNpcSave() 
        {
            Stats ??= new();
            ExperiencedConverastions ??= new HashSet<string>();
        }

        public MidnightNpcSave(MidnightNpc npc)
        {
            Stats = npc.Stats;
            ExperiencedConverastions = npc.ExperiencedConverastions;
            HasIntroduced = npc.HasIntroduced;
            NextConversation = npc.NextConversation;
        }
    }
}
