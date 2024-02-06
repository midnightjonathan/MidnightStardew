using Microsoft.Xna.Framework;
using MidnightStardew.MidnightInteractions;
using Newtonsoft.Json;
using StardewHappyEndings;
using StardewModdingAPI;
using StardewValley;
using System.Diagnostics;

namespace MidnightStardew
{
    [DebuggerDisplay("MidnightNpc: {Name}")]
    public class MidnightNpc
    {
        /// <summary>
        /// A dictionary to get all of the MidnightNpcs in the game based on their name.
        /// </summary>
        [JsonIgnore]
        public static Dictionary<string, MidnightNpc> Get { get; } = new();

        protected Random dailyRandom = new();

        /// <summary>
        /// The underlying Stardew NPC that the MidnightNpc wraps.
        /// </summary>
        [JsonIgnore]
        public NPC StardewNpc { get; private set; }

        /// <summary>
        /// The friendship the local farmer has with the Stardew NPC.
        /// </summary>
        [JsonIgnore]
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
        [JsonIgnore]
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
        /// The descriptor of the location player's relationship with this NPC.
        /// Possible values: friendly, dating, engaged, married, divorced
        /// </summary>
        public string RelationshipStatus
        {
            get
            {
                MidnightFarmer.LocalFarmer.friendshipData.TryGetValue(Name, out Friendship friendship);
                return friendship?.Status.ToString().ToLower() ?? "stranger";
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
        public Dictionary<string, MidnightStats> Stats { get; protected set; } = new();

        /// <summary>
        /// List of all conversations the NPC has.
        /// </summary>
        [JsonIgnore]
        public List<MidnightConversation> Conversations { get; set; }

        /// <summary>
        /// Check if the player has had an extended conversation today.
        /// </summary>
        [JsonIgnore]
        public bool HadExtendedConversationToday { get; set; } = false;
        private MidnightConversation? nextConversation;
        /// <summary>
        /// If filled out indicates that the NPC is in an extended conversation with the farmer.
        /// </summary>
        [JsonIgnore]
        public MidnightConversation? NextConversation 
        {
            get => nextConversation;
            set
            {
                if (value != null)
                {
                    HadExtendedConversationToday = true;
                    nextConversation = value;
                }
            } 
        }

        /// <summary>
        /// List of locations the player has spoken to the NPC today.
        /// </summary>
        [JsonIgnore]
        public List<GameLocation> SpokenToLocations { get; } = new();

        /// <summary>
        /// The set of all conversations the player has had with the NPC.
        /// </summary>
        public HashSet<string> ExperiencedConverastions { get; protected set; } = new();

        private bool hasIntroduced = false;
        public bool HasIntroduced
        {
            get
            {
                if (!hasIntroduced)
                {
                    hasIntroduced = ExperiencedConverastions.Contains($"introduction");
                }
                return hasIntroduced;
            }
        }
        #endregion

        /// <summary>
        /// Creates a new MidnightNpc.
        /// </summary>
        /// <param name="filePath">The path to the character json.</param>
        public static void Create(string filePath)
        {
            var characterJson = File.ReadAllText(filePath);
            var character = JsonConvert.DeserializeObject<MidnightNpc>(characterJson)
                ?? throw new ApplicationException($"Failed to deserialize character json at {filePath}");
            if (Get.TryGetValue(character.Name, out MidnightNpc? npc))
            {
                npc.Conversations.AddRange(character.Conversations);
            }
            else
            {
                Get[character.Name] = character;
            }
        }

        /// <summary>
        /// Creates a new MidnightNpc of the provided child class.
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
            eventMonitor.ModHelper.Events.GameLoop.Saving += OnSaving;

            //Setup the speaker for all of this NPCs conversations.
            foreach (var conversation in Conversations )
            {
                conversation.SetSpeaker(this);
            }

            LoadNpc();
            StardewNpc.DefaultPosition = new Vector2(3, 4);
            foreach (var home in Game1.characterData[Name].Home)
            {
                if (home.Condition == null)
                {
                    home.Tile = new Point(3, 4);
                }
            }
        }

        /// <summary>
        /// Loads the MidnightNpc data if it exists. Only works for the main player currently.
        /// </summary>
        /// <exception cref="ApplicationException">Will throw if the EventMenitor is not properly setup.</exception>
        private void LoadNpc()
        {
            var eventMonitor = EventMonitor.Get ?? throw new ApplicationException("Event Monitor not set up.");

            var npcSave = eventMonitor.ModHelper.Data.ReadSaveData<MidnightNpcSave>($"{Name}.save");
            if ( npcSave != null )
            {
                Stats = npcSave.Stats;
                ExperiencedConverastions = npcSave.ExperiencedConverastions;
                hasIntroduced = npcSave.HasIntroduced;
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
            StardewNpc.faceGeneralDirection(MidnightFarmer.LocalFarmer.Position);
            //StardewNpc.controller = new StardewValley.Pathfinding.PathFindController(StardewNpc, StardewNpc.currentLocation, new Point(9, 6), 1);
            //StardewNpc.performTenMinuteUpdate(500, StardewNpc.currentLocation);
            //StardewNpc.controller.endBehaviorFunction = AfterPathing;
        }


        private MidnightConversation? AfterMoveConversation;
        /// <summary>
        /// Moves the NPC to the provided position.
        /// </summary>
        /// <param name="locationName">The name of the location the NPC should move to.</param>
        /// <param name="position">The point to move the NPC to within the location.</param>
        /// <param name="afterPathing">The function to call once the NPC reaches the destintation.</param>
        public void MoveTo(string locationName, Point position, MidnightConversation? afterMoveConveration = null)
        {
            AfterMoveConversation = afterMoveConveration;
            var location = Game1._locationLookup[locationName];
            StardewNpc.controller = new StardewValley.Pathfinding.PathFindController(StardewNpc, location, position, 1, StartAfterMoveConversation);
        }

        public void StartAfterMoveConversation(Character character, GameLocation gameLocation)
        {
            if (AfterMoveConversation != null)
            {
                new MidnightDialogueBox(this, AfterMoveConversation).Display();
            }
        }

        /// <summary>
        /// Choose a valid dialogue.
        /// </summary>
        /// <returns>A valid dialogue.</returns>
        public MidnightConversation ChooseDialogue()
        {
            if (!HasIntroduced && MidnightConversation.TryGetConversation(this, "introduction", out MidnightConversation? introConversation))
            {
                return introConversation ?? throw new ApplicationException("Null conversation returned on true MidnightConversation.TryGetConversation");
            }

            if (NextConversation != null && !HadExtendedConversationToday)
            {
                HadExtendedConversationToday = true;
                var returnConversation = NextConversation;
                NextConversation = null;
                return returnConversation;
            }

            List<MidnightConversation> available = new();

            foreach (var conversation in  Conversations)
            {
                if (conversation.MeetsRequirements())
                {
                    available.Add(conversation);
                }
            }

            if (available.Count > 0)
            {
                return available[dailyRandom.Next(available.Count)];
            }
            else
            {
                throw new ApplicationException("No available conversations.");
            }
            
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

        private void OnSaving(object? sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            var eventMonitor = EventMonitor.Get ?? throw new ApplicationException("Event Monitor not set up.");
            var saveData = new MidnightNpcSave(this);
            eventMonitor.ModHelper.Data.WriteSaveData($"{Name}.save", saveData);
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
