using Microsoft.VisualBasic;
using MidnightStardew.MidnightCharacters;
using MidnightStardew.MidnightWorld;
using Newtonsoft.Json;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
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
        public MidnightRequirements Requirements { get; set; }
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
        public MidnightConversation(MidnightRequirements reqs, 
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
        /// <returns>If the farmer meets the requirements for this conversation.</returns>
        public bool MeetsRequirements()
        {
            if (Speaker == null) throw new ApplicationException("Conversation speaker not set.");

            // Ensure the conversation hasn't already happened.
            if (!string.IsNullOrEmpty(key) && Speaker.ExperiencedConverastions.Contains(key)) return false;

            //Check if is extended conversation and player is already in an extended conversation
            if (NextConversation != null && Speaker.NextConversation != null) return false;
            
            return Requirements?.AreMet(Speaker) ?? true;
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

            // Emote for the Npc to do
            if (Effects.Emote != -1)
            {
                Speaker.StardewNpc.doEmote(Effects.Emote, false);
            }

            // Move Npc
            if (Move != null)
            {
                Move.Requirements.FixRelativeReqs();
                if (Move.Requirements?.AreMet(Speaker) ?? true)
                {
                    Speaker.MoveTo(Move);
                }
                else
                {
                    Speaker.PlannedMovements.Add(Move);
                }
            }

            // Give gift to farmer
            if (Effects.ItemId != null)
            {
                Item item = ItemRegistry.Create(Effects.ItemId);
                MidnightFarmer.LocalFarmer.addItemByMenuIfNecessary(item);
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
