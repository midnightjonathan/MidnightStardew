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
        public List<string> Days { get; set; }
        public string Hearts { get; set; }
        public List<string> Keys { get; set; }
        public string Location { get; set; }
        public Dictionary<string, string> Stats { get; set; }
        public Dictionary<string, Dictionary<string, string>> OtherStats { get; set; }

        [JsonConstructor]
        public MidnightDialogueRequirements(Dictionary<string, string> stats, 
                                            Dictionary<string, Dictionary<string, string>> otherStats, 
                                            List<string> days, 
                                            List<string> keys, 
                                            string hearts, 
                                            string location)
        {
            Stats = stats ?? new();
            OtherStats = otherStats ?? new();
            Days = days;
            Keys = keys;
            Hearts = hearts;
            Location = location;
        }
    }
}
