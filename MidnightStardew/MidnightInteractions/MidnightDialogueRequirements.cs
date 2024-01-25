using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightStardew.MidnightInteractions
{
    [DebuggerDisplay("Midnight Dialogue Requirements")]
    public class MidnightDialogueRequirements
    {
        public Dictionary<string, string> Stats { get; set; }
        public List<string> Days { get; set; }
        public string Extends { get; set; }
        public List<string> Keys { get; set; }
        public string Hearts { get; set; }

        [JsonConstructor]
        public MidnightDialogueRequirements(Dictionary<string, string> stats, string extends, List<string> days, List<string> keys, string hearts)
        {
            Stats = stats ?? new();
            Days = days;
            Keys = keys;
            Extends = extends;
            Hearts = hearts;
        }
    }
}
