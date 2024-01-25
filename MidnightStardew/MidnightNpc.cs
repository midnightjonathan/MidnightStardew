using Force.DeepCloner;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MidnightStardew.MidnightInteractions;
using Newtonsoft.Json;
using StardewHappyEndings;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MidnightStardew
{
    [DebuggerDisplay("MidnightNpc: {Name}")]
    public class MidnightNpc
    {
        /// <summary>
        /// A dictionary to get all of the MidnightNpcs in the game based on their name.
        /// </summary>
        public static Dictionary<string, MidnightNpc> Get { get; } = new();

        protected Random dailyRandom = new();

        /// <summary>
        /// The underlying Stardew NPC that the MidnightNpc wraps.
        /// </summary>
        public NPC StardewNpc { get; private set; }

        /// <summary>
        /// The friendship the local farmer has with the Stardew NPC.
        /// </summary>
        public int Hearts
        {
            get
            {
                return MidnightFarmer.LocalFarmer.getFriendshipHeartLevelForNPC(Name);
            }
        }

        /// <summary>
        /// The friendship points that the Stardew NPC has with the local farmer.
        /// </summary>
        public int FriendshipPoints
        {
            get
            {
                return MidnightFarmer.LocalFarmer.getFriendshipLevelForNPC(Name);
            }
            set
            {
                // Need to remove currentFriendship from the amount of the change so that we are only adding or subtracting the amount expected.
                var currentFriendship = MidnightFarmer.LocalFarmer.getFriendshipLevelForNPC(Name);
                MidnightFarmer.LocalFarmer.changeFriendship(value - currentFriendship, StardewNpc);
            }
        }

        /// <summary>
        /// The name of the NPC.
        /// </summary>
        public string Name { get; }

        #region Relationship Fields
        /// <summary>
        /// Custom stats for your mod.
        /// </summary>
        public Dictionary<string, MidnightStats> Stats { get; } = new();

        /// <summary>
        /// List of all conversations the NPC has.
        /// </summary>
        public List<MidnightConversation> Conversations { get; set; }

        /// <summary>
        /// Check if the player has had an extended conversation today.
        /// </summary>
        public bool HadExtendedConversationToday { get; set; } = false;
        private string? nextConversation;
        /// <summary>
        /// If filled out indicates that the NPC is in an extended conversation with the farmer.
        /// </summary>
        public string? NextConversation 
        {
            get => nextConversation;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    HadExtendedConversationToday = true;
                    nextConversation = value;
                }
            } 
        }

        /// <summary>
        /// List of locations the player has spoken to the NPC today.
        /// </summary>
        public List<GameLocation> SpokenToLocations { get; } = new();

        /// <summary>
        /// The set of all conversations the player has had with the NPC.
        /// </summary>
        public HashSet<string> ExperiencedConverastions { get; } = new();
        #endregion

        /// <summary>
        /// Creates a new MidnightNpc.
        /// </summary>
        /// <typeparam name="T">A child class of MidnightNpc to create.</typeparam>
        /// <param name="filePath">The path to the character json.</param>
        public static void Create<T>(string filePath) where T : MidnightNpc
        {
            var characterJson = File.ReadAllText(filePath);
            var character = JsonConvert.DeserializeObject<T>(characterJson)
                ?? throw new ApplicationException($"Failed to deserialize character json at {filePath}");
            Get[character.Name] = character;
        }

        #region Constructor
        [JsonConstructor]
        public MidnightNpc(string name, List<MidnightConversation> conversations)
        {
            Name = name;
            Conversations = conversations;

            SetStardewNpc();
            if (StardewNpc == null) throw new ApplicationException($"Unable to load NPC {name}. Please ensure that the character exists.");

            var eventMonitor = EventMonitor.Get ?? throw new ApplicationException("Event Monitor not set up.");
            eventMonitor.ModHelper.Events.World.NpcListChanged += OnLocationChange;
            eventMonitor.ModHelper.Events.Player.Warped += OnLocationChange;
            eventMonitor.ModHelper.Events.GameLoop.DayStarted += OnDayStart;

            //Setup the speaker for all of this NPCs conversations.
            foreach (var conversation in Conversations )
            {
                conversation.SetSpeaker(this);
            }
        }

        /// <summary>
        /// Links the MidnightNpc to the StardewValley.NPC
        /// </summary>
        private void SetStardewNpc()
        {
            foreach (var npc in Utility.getAllCharacters())
            {
                if (npc.Name == Name)
                {
                    StardewNpc = npc;
                    break;
                }
            }
        }
        #endregion

        #region Dialog
        /// <summary>
        /// Checks if the NPC can talk to the Farmer.
        /// </summary>
        /// <returns>True if the NPC can talk.</returns>
        public bool CanTalk()
        {
            return !SpokenToLocations.Contains(StardewNpc.currentLocation);
        }

        /// <summary>
        /// Displays a dialogue window for the farmer.
        /// </summary>
        public void DisplayDialogue()
        {
            new MidnightDialogueBox(this, ChooseDialogue()).Display();
        }

        /// <summary>
        /// Choose a valid dialogue.
        /// </summary>
        /// <returns>A valid dialogue.</returns>
        public MidnightConversation ChooseDialogue()
        {
            if (NextConversation != null && !HadExtendedConversationToday)
            {
                HadExtendedConversationToday = true;
                return MidnightConversation.Get[NextConversation];
            }

            List<MidnightConversation> available = new();

            foreach (var conversation in  Conversations)
            {
                if (conversation.MeetsRequirements(MidnightFarmer.LocalFarmer))
                {
                    available.Add(conversation);
                }
            }

            return available[dailyRandom.Next(available.Count)];
        }
        #endregion

        #region Relationship
        /// <summary>
        /// Gets the levels of the stats for the player.
        /// </summary>
        /// <param name="targetId">The id of the player or NPC related to the relationship stat.</param>
        /// <param name="statName">The name of the stat to get.</param>
        /// <returns>The level of the stat.</returns>
        public int GetStatLevel(string targetId, string statName)
        {
            return Stats.ContainsKey(targetId) && Stats[targetId].ContainsKey(statName) ? Stats[targetId][statName] / 1000 : 0;
        }
        #endregion

        #region EventHandling
        private void OnDayStart(object? sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            Utility.CreateDaySaveRandom(StardewNpc.id);
            SpokenToLocations.Clear();
            HadExtendedConversationToday = false;
        }

        private void OnLocationChange(object? sender, EventArgs e)
        {
            var eventMonitor = EventMonitor.Get ?? throw new ApplicationException("Event Monitor not set up.");

            if (Game1.currentLocation == StardewNpc.currentLocation)
            {
                eventMonitor.ModHelper.Events.GameLoop.UpdateTicked += OnTick;
                eventMonitor.ModHelper.Events.Input.ButtonPressed += OnButtonPressed;
            }
            else
            {
                eventMonitor.ModHelper.Events.GameLoop.UpdateTicked -= OnTick;
                eventMonitor.ModHelper.Events.Input.ButtonPressed -= OnButtonPressed;
            }
        }

        protected virtual bool IsMouseOver()
        {
            if (Game1.currentLocation != StardewNpc.currentLocation || !CanTalk()) return false;

            var eventMonitor = EventMonitor.Get ?? throw new ApplicationException("EventMonitor not set up.");
            
            var pos = eventMonitor.ModHelper.Input.GetCursorPosition().GetScaledAbsolutePixels();
            
            var spriteArea = new Rectangle((int)StardewNpc.position.X,
                                           (int)StardewNpc.position.Y - StardewNpc.Sprite.SpriteWidth * 4,
                                           StardewNpc.Sprite.SpriteWidth * 4,
                                           StardewNpc.Sprite.SpriteHeight * 4);

            return spriteArea.Contains(pos.X, pos.Y);
        }

        private void OnButtonPressed(object? sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.MouseRight && IsMouseOver() && StardewNpc.withinPlayerThreshold(1) && CanTalk())
            {
                DisplayDialogue();
                SpokenToLocations.Add(StardewNpc.currentLocation);
            }
        }

        protected virtual void OnTick(object? sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (IsMouseOver() && CanTalk())
            {
                Game1.isActionAtCurrentCursorTile = true;
                Game1.isSpeechAtCurrentCursorTile = true;
                Game1.mouseCursorTransparency = StardewNpc.withinPlayerThreshold(1) ? 1f : 0.5f;
            }
        }
        #endregion

        /// <summary>
        /// Allows a MidnightNpc to be used as an NPC.
        /// </summary>
        /// <param name="npc">The MidnightNpc being converted.</param>
        public static implicit operator NPC(MidnightNpc npc)
        {
            return npc.StardewNpc;
        }

    }
}
