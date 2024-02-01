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
        public List<string>? Season { get; set; }
        public string? Hearts { get; set; }
        public List<string>? Keys { get; set; }
        public string? Location { get; set; }
        public List<string>? MissingKeys { get; set; }
        public List<string>? RelationshipStatus { get; set; }
        public Dictionary<string, string>? Stats { get; set; }
        public Dictionary<string, Dictionary<string, string>>? OtherStats { get; set; }
        public string? Weather { get; set; }
        public string? Time { get; set; }
        public string? Spot { get; set; }

        [JsonConstructor]
        public MidnightDialogueRequirements(Dictionary<string, string> stats, 
                                            Dictionary<string, Dictionary<string, string>> otherStats, 
                                            List<string> days, 
                                            List<string> keys, 
                                            List<string> missingKeys,
                                            List<string> relationshipStatus,
                                            List<string> season,
                                            string spot,
                                            string time,
                                            string weather,
                                            string year,
                                            string hearts, 
                                            string location)
        {
            Stats = stats;
            OtherStats = otherStats;
            
            Days = ListToLower(days);
            Keys = ListToLower(keys);
            MissingKeys = ListToLower(missingKeys);
            Season = ListToLower(season);
            RelationshipStatus = ListToLower(relationshipStatus);

            Spot = spot;
            Year = year;
            Time = time;
            Hearts = hearts;
            Location = location?.ToLower();
            Weather = weather?.ToLower();
        }

        private static List<string>? ListToLower(List<string> listToLower)
        {
            if (listToLower == null) return null;

            var list = new List<string>();
            foreach (var item in listToLower)
            {
                list.Add(item.ToLower());
            }
            return list;
        }
    }
}
