﻿using Microsoft.VisualBasic;
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

        /// <summary>
        /// List of statements that the NPC will say to the player.I Thats 
        /// </summary>
        public List<string> Statement { get; set; }
        /// <summary>
        /// The requirements that need to be met to make this conversation happen.
        /// </summary>
        public MidnightDialogueRequirements Requirements { get; set; }
        /// <summary>
        /// The options that the player can choose at the end of the Statements.
        /// </summary>
        public Dictionary<string, MidnightConversation> Responses { get; set; }
        /// <summary>
        /// The effects to apply to the NPC as a result of this conversation.
        /// </summary>
        public MidnightDialogueEffects Effects { get; set; }
        /// <summary>
        /// If populated, this indicates that the next conversation should be the given key.
        /// </summary>
        public MidnightConversation NextConversation { get; set; }
        private string key;
        /// <summary>
        /// The identifier of the conversation.
        /// </summary>
        public string Key 
        {
            get
            {
                if (string.IsNullOrEmpty(key)) return Statement[0];
                return key;
            }
            set => key = value;
        }

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
                } else if (value == null)
                {
                    throw new ApplicationException("Speaker can not have null assigned to it.");
                }

                speaker = value;
                AddConversation(speaker, this);
            }
        }

        [JsonConstructor]
        public MidnightConversation(MidnightDialogueRequirements reqs, 
                                    List<string> statement, 
                                    Dictionary<string, MidnightConversation> responses, 
                                    MidnightDialogueEffects effects, 
                                    MidnightConversation nextConversation,
                                    string key)

        {
            Requirements = reqs;
            Statement = statement;
            Responses = responses;
            Effects = effects;
            this.key = key ?? "";
            NextConversation = nextConversation;
        }

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

            #region Check Days of the Week
            if (!(Requirements.Days?.Contains(SDate.Now().DayOfWeek.ToString()) ?? true))
            {
                return false;
            }
            #endregion

            #region Check NPC hearts
            if (!CheckInRange(Speaker.Hearts, Requirements.Hearts)) return false;
            #endregion

            #region Check NPC stats
            foreach (var stat in Requirements.Stats)
            {
                var id = MidnightFarmer.LocalFarmer.UniqueMultiplayerID.ToString();
                var npcStat = Speaker.GetStatLevel(id, stat.Key);

                if (!CheckInRange(npcStat, stat.Value)) return false;
            }
            #endregion

            #region Check Required key
            foreach (var reqKey in Requirements.Keys ?? new())
            {
                if (!Speaker.ExperiencedConverastions.Contains(reqKey))
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

            #region Check location
            if (Speaker.StardewNpc.currentLocation.Name != (Requirements.Location ?? ""))
            {
                return false;
            }
            #endregion

            return true;
        }

        /// <summary>
        /// Checks if a value is within a string range.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="range">The range to check with in the for of a single number or a range (e.g. "2", "2-4")</param>
        /// <returns>True if value is greater than or equal to the first number and less than or equal to the second number.</returns>
        private bool CheckInRange(int value, string range)
        {
            if (range == null) return true;

            var rangeArray = range.Split('-', StringSplitOptions.RemoveEmptyEntries);
            var min = int.Parse(rangeArray[0]);
            var max = rangeArray.Length > 1 ? int.Parse(rangeArray[1]) : int.MaxValue;

            return min <= value && value <= max;
        }

        /// <summary>
        /// Apply the effects of the conversation to the speaker and/or farmer.
        /// </summary>
        /// <param name="farmer">The farmer that the npc is speaking to.</param>
        public void ApplyEffects(Farmer farmer)
        {
            if (Speaker == null) throw new ApplicationException("Conversation speaker not set");

            //Set NextConversation for an extended conversation.
            if (NextConversation != null)
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
        }
    
        /// <summary>
        /// Sets the speaker for this conversation.
        /// </summary>
        /// <param name="npc">The npc is that is speaking.</param>
        public void SetSpeaker(MidnightNpc npc)
        {
            Speaker = npc;

            if (Responses == null) return;
            
            foreach (var conversation in Responses.Values)
            {
                conversation.SetSpeaker(Speaker);
            }
        }
    }
}
