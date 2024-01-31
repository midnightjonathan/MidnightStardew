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
        public List<string>? Days { get; set; }
        public string? Year { get; set; }
        public string? Season { get; set; }
        public string? Hearts { get; set; }
        public List<string>? Keys { get; set; }
        public string? Location { get; set; }
        public List<string>? MissingKeys { get; set; }
        public Dictionary<string, string>? Stats { get; set; }
        public Dictionary<string, Dictionary<string, string>>? OtherStats { get; set; }

        [JsonConstructor]
        public MidnightDialogueRequirements(Dictionary<string, string> stats, 
                                            Dictionary<string, Dictionary<string, string>> otherStats, 
                                            List<string> days, 
                                            List<string> keys, 
                                            List<string> missingKeys,
                                            string year,
                                            string season,
                                            string hearts, 
                                            string location)
        {
            Stats = stats;
            OtherStats = otherStats;
            Days = days;
            Keys = keys;
            MissingKeys = missingKeys;
            Year = year;
            Season = season?.ToLower();
            Hearts = hearts;
            Location = location?.ToLower();
        }
    }
}
