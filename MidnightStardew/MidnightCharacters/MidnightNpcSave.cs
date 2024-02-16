using MidnightStardew.MidnightInteractions;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightStardew.MidnightCharacters
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

        public List<MidnightMovement> PlannedMovements { get; set; }

        /// <summary>
        /// Indicates if the NPC has met the player.
        /// </summary>
        public Dictionary<string, bool> RelationshipConversations { get; set; }

        public MidnightNpcSave()
        {
            Stats ??= new();
            ExperiencedConverastions ??= new();
            RelationshipConversations ??= new();
            PlannedMovements ??= new();
        }

        public MidnightNpcSave(MidnightNpc npc)
        {
            Stats = npc.Stats;
            ExperiencedConverastions = npc.ExperiencedConverastions;
            RelationshipConversations = npc.RelationshipConversations;
            NextConversation = npc.NextConversation;
            PlannedMovements = npc.PlannedMovements;
        }
    }
}
