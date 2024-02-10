using Newtonsoft.Json;
using StardewModdingAPI.Utilities;
using StardewValley.Monsters;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightStardew.MidnightInteractions
{
    [DebuggerDisplay("Midnight Dialogue Requirements")]
    public class MidnightRequirements
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
        public Dictionary<string, MidnightRequirements> Others { get; set; }
        public string? Weather { get; set; }
        public string? Time { get; set; }
        public string? Spot { get; set; }

        public int? InDays { get; set; }
        public int? OnDay { get; set; }

        [JsonConstructor]
        public MidnightRequirements(Dictionary<string, string> stats,
                                    Dictionary<string, MidnightRequirements> others, 
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
                                    string location,
                                    int? inDays)
        {
            Stats = stats;
            Others = others;
            
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
            InDays = inDays;
        }

        public bool AreMet(MidnightNpc npc)
        {
            #region Check location information
            var location = npc.StardewNpc.currentLocation;
            if (CheckStringNoMatch(Location, location.Name) || //Location itself
                CheckStringNoMatch(Weather, location.GetWeather().Weather)) //Location weather
            {
                return false;
            }
            #endregion

            #region Check calendar reqs
            if (CheckOutRange(Time, Game1.timeOfDay) ||
                CheckOutList(Days, SDate.Now().DayOfWeek.ToString()) ||
                CheckOutList(Season, Game1.currentSeason) ||
                CheckOutRange(Year, Game1.year))
            {
                return false;
            }
            if (OnDay != null && OnDay != SDate.Now().DaysSinceStart)
            {
                return false;
            }
            #endregion

            #region Check NPC Relationships
            if (!npc.MeetsRequirements(this))
            {
                return false;
            }
            foreach ((var otherName, var otherReqs) in Others ?? new())
            {
                if (!MidnightNpc.Get[otherName].MeetsRequirements(otherReqs))
                {
                    return false;
                }
            }
            #endregion

            return true;
        }

        /// <summary>
        /// Used to set relative requirements
        /// </summary>
        public void FixRelativeReqs()
        {
            OnDay = InDays != null ? SDate.Now().AddDays((int)InDays).DaysSinceStart : null;
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

        /// <summary>
        /// Checks if the state is in the requirement list.
        /// </summary>
        /// <param name="reqList">The requirement list.</param>
        /// <param name="stateString">The state of the world to check.</param>
        /// <returns>True if the requirements is null or the state is in the list.</returns>
        public static bool CheckInList(IEnumerable<string>? reqList, string stateString)
        {
            return reqList == null || !reqList.Any() || reqList.Contains(stateString.ToLower());
        }

        /// <summary>
        /// Checks if the state is not in the requirement list.
        /// </summary>
        /// <param name="reqList">The requirement list.</param>
        /// <param name="stateString">The state of the world to check.</param>
        /// <returns>True if the requirements is not null and the state is not in the list.</returns>
        public static bool CheckOutList(IEnumerable<string>? reqList, string stateString)
        {
            return !CheckInList(reqList, stateString);
        }

        /// <summary>
        /// Check if the requirement string doesn't exist or matches the state string.
        /// </summary>
        /// <param name="requirementString">The requirement to be met.</param>
        /// <param name="stateString">The string that represents game state.</param>
        /// <returns>True if the requirement string is null or matches the state string.</returns>
        public static bool CheckStringMatch(string? requirementString, string stateString)
        {
            return requirementString == null || requirementString.ToLower() == stateString.ToLower();
        }

        /// <summary>
        /// Check if the requirement string doesn't exist or matches the state string.
        /// </summary>
        /// <param name="requirementString">The requirement to be met.</param>
        /// <param name="stateString">The string that represents game state.</param>
        /// <returns>True if the requirement string is not null and doesn't matches the state string.</returns>
        public static bool CheckStringNoMatch(string? requirementString, string stateString)
        {
            return !(CheckStringMatch(requirementString, stateString));
        }

        /// <summary>
        /// Checks if a value is within a string range.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="reqRange">The range to check with in the for of a single number or a range (e.g. "2", "2-4")</param>
        /// <returns>True if value is greater than or equal to the first number and less than or equal to the second number.</returns>
        public static bool CheckInRange(string? reqRange, int value)
        {
            if (reqRange == null) return true;

            var rangeArray = reqRange.Split('-', StringSplitOptions.RemoveEmptyEntries);
            var min = int.Parse(rangeArray[0]);
            var max = rangeArray.Length > 1 ? int.Parse(rangeArray[1]) : int.MaxValue;

            return min <= value && value <= max;
        }

        /// <summary>
        /// Checks if a value is outside of a string range.
        /// </summary>
        /// <param name="reqRange">The range to check with in the for of a single number or a range (e.g. "2", "2-4")</param>
        /// <param name="value">The value to check.</param>
        /// <returns>True if value is less than the first number and greater than the second number.</returns>
        public static bool CheckOutRange(string? reqRange, int value)
        {
            return !CheckInRange(reqRange, value);
        }
    }
}
