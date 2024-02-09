﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightStardew.MidnightInteractions
{
    [DebuggerDisplay("Midnight Dialogue Effects")]
    public class MidnightDialogueEffects
    {
        public int Emote { get; set; }  
        public string Hearts { get; set; }
        /// <summary>
        /// The item key of the item to give to the player.
        /// </summary>
        public string? ItemId { get; set; }
        public Dictionary<string, string> Stats { get; set; }

        [JsonConstructor]
        public MidnightDialogueEffects(string hearts, Dictionary<string, string> stats, int? emote, string itemId)
        {
            Hearts = hearts;
            Stats = stats ?? new();
            Emote = emote ?? -1;
            ItemId = itemId;
        }
    }
}
