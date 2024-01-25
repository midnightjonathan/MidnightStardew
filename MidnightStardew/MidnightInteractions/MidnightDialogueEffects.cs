using Newtonsoft.Json;
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
        public string Hearts;
        public Dictionary<string, string> Stats { get; set; }

        [JsonConstructor]
        public MidnightDialogueEffects(string hearts, Dictionary<string, string> stats)
        {
            Hearts = hearts;
            Stats = stats ?? new();
        }
    }
}
