using Microsoft.VisualBasic;
using MidnightStardew.MidnightWorld;
using Newtonsoft.Json;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightStardew.MidnightInteractions
{
    [DebuggerDisplay("MidnightConversation: {Key}")]
    public class MidnightConversation
    {
        #region Static
        /// <summary>
        /// All keyed Conversations.
        /// </summary>
        public static Dictionary<string, MidnightConversation> Get { get; } = new();
        /// <summary>
        /// Tries to get a conversation with the given key.
        /// </summary>
        /// <param name="speaker">The MidnightNpc who is speaking.</param>
        /// <param name="key">The key of the conversation to look for.</param>
        /// <param name="conversation">The conversation with the given key.</param>
        /// <returns>True if the conversation exists.</returns>
        public static bool TryGetConversation(MidnightNpc speaker, string key, out MidnightConversation? conversation)
        {
            return Get.TryGetValue($"{speaker.Name}_{key}", out conversation);
        }
        /// <summary>
        /// Adds a conversation to the keyed conversations.
        /// </summary>
        /// <param name="speaker">The Midnight NPC who is speaking.</param>
        /// <param name="conversation">The conversation to add.</param>
        private static void AddConversation(MidnightNpc speaker, MidnightConversation conversation)
        {
            if (!string.IsNullOrEmpty(conversation.key))
            {
                Get[$"{speaker.Name}_{conversation.key}"] = conversation;
            }
        }
        #endregion

        /// <summary>
        /// The effects to apply to the NPC as a result of this conversation.
        /// </summary>
        public MidnightDialogueEffects Effects { get; set; }
        private string key;
        /// <summary>
        /// The identifier of the conversation.
        /// </summary>
        public string Key
        {
            get
            {
                if (string.IsNullOrEmpty(key)) return Statement?[0] ?? "<Null Statement>";
                return key;
            }
            set => key = value;
        }
        /// <summary>
        /// Moves the character after the conversation.
        /// </summary>
        public MidnightMovement Move { get; set; }
        /// <summary>
        /// If populated, this indicates that the next conversation should be the given key.
        /// </summary>
        public MidnightConversation NextConversation { get; set; }
        /// <summary>
        /// The requirements that need to be met to make this conversation happen.
        /// </summary>
        public MidnightDialogueRequirements Requirements { get; set; }
        /// <summary>
        /// The options that the player can choose at the end of the Statements.
        /// </summary>
        public Dictionary<string, MidnightConversation> Responses { get; set; }
        private MidnightNpc? speaker;
        /// <summary>
        /// The default Midnight NPC that is speaking during the conversation.
        /// </summary>
        public MidnightNpc? Speaker
        {
            get => speaker;
            set
            {
                if (speaker != null)
                {
                    throw new ApplicationException("Speaker has already been set.");
                }
                else if (value == null)
                {
                    throw new ApplicationException("Speaker can not have null assigned to it.");
                }

                speaker = value;
                AddConversation(speaker, this);
            }
        }
        /// <summary>
        /// List of statements that the NPC will say to the player.I Thats 
        /// </summary>
        public List<string> Statement { get; set; }

        [JsonConstructor]
        public MidnightConversation(MidnightDialogueRequirements reqs, 
                                    List<string> statement, 
                                    Dictionary<string, MidnightConversation> responses, 
                                    MidnightDialogueEffects effects, 
                                    MidnightConversation nextConversation,
                                    MidnightMovement move,
                                    string key)

        {
            Requirements = reqs;
            Statement = statement;
            Responses = responses;
            Effects = effects;
            this.key = key?.ToLower() ?? "";
            NextConversation = nextConversation;
            Move = move;
        }

        #region Meets Requirements
        /// <summary>
        /// Determines if the this conversation can be displayed.
        /// </summary>
        /// <param name="farmer">The farmer for the npc to talk to.</param>
        /// <returns>If the farmer meets the requirements for this conversation.</returns>
        public bool MeetsRequirements()
        {
            if (Speaker == null) throw new ApplicationException("Conversation speaker not set.");

            #region Check not experianced
            if (!string.IsNullOrEmpty(key) && Speaker.ExperiencedConverastions.Contains(key))
            {
                return false;
            }
            #endregion

            if (Requirements == null) return true;

            #region Check location information
            var location = Speaker.StardewNpc.currentLocation;
            if (CheckStringNoMatch(Requirements?.Location, location.Name) || //Location itself
                CheckStringNoMatch(Requirements?.Weather, location.GetWeather().Weather)) //Location weather
            {
                return false;
            }

            if (Requirements?.Spot != null &&
                !MidnightSpot.Get[Requirements.Spot].IsIn(Speaker))
            {
                return false;
            }
            #endregion

            #region Check calendar reqs
            if (CheckOutRange(Requirements?.Time, Game1.timeOfDay) ||
                CheckOutList(Requirements?.Days, SDate.Now().DayOfWeek.ToString()) ||
                CheckOutList(Requirements?.Season, Game1.currentSeason) ||
                CheckOutRange(Requirements?.Year, Game1.year))
            {
                return false;
            }
            #endregion

            #region Check NPC Relationship
            if (CheckOutRange(Requirements?.Hearts, Speaker.Hearts)) return false;
            foreach (var stat in Requirements?.Stats ?? new())
            {
                var id = MidnightFarmer.LocalFarmer.UniqueMultiplayerID.ToString();
                var npcStat = Speaker.GetStatLevel(id, stat.Key);

                if (CheckOutRange(stat.Value, npcStat)) return false;
            }
            if (CheckOutList(Requirements?.RelationshipStatus, Speaker.RelationshipStatus)) return false;
            #endregion

            #region Check other NPC Relationship
            foreach (var otherStat in Requirements?.OtherStats ?? new())
            {
                var npc = MidnightNpc.Get[otherStat.Key];
                foreach (var stat in otherStat.Value)
                {
                    var id = MidnightFarmer.LocalFarmer.UniqueMultiplayerID.ToString();
                    var npcStat = npc.GetStatLevel(id, stat.Key);

                    if (CheckOutRange(stat.Value, npcStat)) return false;
                }
            }
            #endregion

            #region Check keys
            foreach (var reqKey in Requirements?.Keys ?? new())
            {
                if (!Speaker.ExperiencedConverastions.Contains(reqKey))
                {
                    return false;
                }
            }

            foreach (var missingKey in Requirements?.MissingKeys ?? new())
            {
                if (Speaker.ExperiencedConverastions.Contains(missingKey))
                {
                    return false;
                }
            }
            #endregion

            #region Check if is extended conversation and player is already in an extended conversation
            if (NextConversation != null && Speaker.NextConversation != null)
            {
                return false;
            }
            #endregion
            
            return true;
        }

        /// <summary>
        /// Checks if the state is in the requirement list.
        /// </summary>
        /// <param name="reqList">The requirement list.</param>
        /// <param name="stateString">The state of the world to check.</param>
        /// <returns>True if the requirements is null or the state is in the list.</returns>
        private static bool CheckInList(IEnumerable<string>? reqList, string stateString)
        {
            return reqList == null || !reqList.Any() || reqList.Contains(stateString.ToLower());
        }

        /// <summary>
        /// Checks if the state is not in the requirement list.
        /// </summary>
        /// <param name="reqList">The requirement list.</param>
        /// <param name="stateString">The state of the world to check.</param>
        /// <returns>True if the requirements is not null and the state is not in the list.</returns>
        private static bool CheckOutList(IEnumerable<string>? reqList, string stateString)
        {
            return !CheckInList(reqList, stateString);
        }

        /// <summary>
        /// Check if the requirement string doesn't exist or matches the state string.
        /// </summary>
        /// <param name="requirementString">The requirement to be met.</param>
        /// <param name="stateString">The string that represents game state.</param>
        /// <returns>True if the requirement string is null or matches the state string.</returns>
        private static bool CheckStringMatch(string? requirementString, string stateString)
        {
            return requirementString == null || requirementString.ToLower() == stateString.ToLower();
        }

        /// <summary>
        /// Check if the requirement string doesn't exist or matches the state string.
        /// </summary>
        /// <param name="requirementString">The requirement to be met.</param>
        /// <param name="stateString">The string that represents game state.</param>
        /// <returns>True if the requirement string is not null and doesn't matches the state string.</returns>
        private static bool CheckStringNoMatch(string? requirementString, string stateString)
        {
            return !(CheckStringMatch(requirementString, stateString));
        }

        /// <summary>
        /// Checks if a value is within a string range.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="reqRange">The range to check with in the for of a single number or a range (e.g. "2", "2-4")</param>
        /// <returns>True if value is greater than or equal to the first number and less than or equal to the second number.</returns>
        private static bool CheckInRange(string? reqRange, int value)
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
        private static bool CheckOutRange(string? reqRange, int value)
        {
            return !CheckInRange(reqRange, value);
        }
        #endregion

        /// <summary>
        /// Apply the effects of the conversation to the speaker and/or farmer.
        /// </summary>
        /// <param name="farmer">The farmer that the npc is speaking to.</param>
        public void ApplyEffects(Farmer farmer)
        {
            if (Speaker == null) throw new ApplicationException("Conversation speaker not set");

            //Set NextConversation for an extended conversation.
            if (NextConversation != null && NextConversation.MeetsRequirements())
            {
                Speaker.NextConversation = NextConversation;
            }

            //Track that the conversation has been experienced if it has a key.
            if (!string.IsNullOrEmpty(key))
            {
                Speaker.ExperiencedConverastions.Add(key);
            }

            if (Effects == null) return;

            //Update the stardew friendship points
            if (Effects.Hearts != null)
            {
                Speaker.FriendshipPoints += int.Parse(Effects.Hearts);
            }

            //Updates the stats the NPC has with the farmer.
            foreach (var stat in Effects.Stats)
            {
                if (!Speaker.Stats.ContainsKey(farmer.UniqueMultiplayerID.ToString())) 
                {
                    Speaker.Stats[farmer.UniqueMultiplayerID.ToString()] = new MidnightStats();
                }
                Speaker.Stats[farmer.UniqueMultiplayerID.ToString()][stat.Key] += int.Parse(stat.Value);
            }

            if (Effects.Emote != -1)
            {
                Speaker.StardewNpc.doEmote(Effects.Emote, false);
            }

            if (Move != null)
            {
                Speaker.MoveTo(Move.LocationName, Move.Position, Move.AfterMoveConversation);
            }
        }
    
        /// <summary>
        /// Sets the speaker for this conversation.
        /// </summary>
        /// <param name="npc">The npc is that is speaking.</param>
        public void SetSpeaker(MidnightNpc npc)
        {
            Speaker = npc;

            if (Responses != null)
            {
                foreach (var conversation in Responses.Values)
                {
                    conversation.SetSpeaker(Speaker);
                }
            }

            NextConversation?.SetSpeaker(Speaker);
            Move?.AfterMoveConversation?.SetSpeaker(Speaker);
        }
    }
}
