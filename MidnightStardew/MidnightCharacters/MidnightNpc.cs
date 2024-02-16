using Microsoft.Xna.Framework;
using MidnightStardew.MidnightInteractions;
using MidnightStardew.MidnightWorld;
using Newtonsoft.Json;
using StardewHappyEndings;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;
using System.Diagnostics;

namespace MidnightStardew.MidnightCharacters
{
    [DebuggerDisplay("MidnightNpc: {Name}")]
    public class MidnightNpc
    {
        #region Static Members
        /// <summary>
        /// A dictionary to get all of the MidnightNpcs in the game based on their name.
        /// </summary>
        public static Dictionary<string, MidnightNpc> Get { get; } = new();
        #endregion

        protected Random dailyRandom = new();

        /// <summary>
        /// The name of the NPC.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A set of date, times that the NPC will move to a new location.
        /// </summary>
        public List<MidnightMovement> PlannedMovements { get; set; } = new();

        public string? spriteName;
        /// <summary>
        /// The name of the sprite the the character should use, without the characters leading name.
        /// </summary>
        public string? SpriteName 
        {
            get => spriteName;
            set
            {
                spriteName = value;
                EventMonitor.Get?.ModHelper.GameContent.InvalidateCache($"Characters/{Name}");
            }
        }

        /// <summary>
        /// The underlying Stardew NPC that the MidnightNpc wraps.
        /// </summary>
        public NPC StardewNpc { get; private set; }

        #region Relationship Fields
        /// <summary>
        /// List of all conversations the NPC has.
        /// </summary>
        public List<MidnightConversation> Conversations { get; set; }

        /// <summary>
        /// The set of all conversations the player has had with the NPC.
        /// </summary>
        public HashSet<string> ExperiencedConverastions { get; protected set; } = new();

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
        /// Check if the player has had an extended conversation today.
        /// </summary>
        public bool HadExtendedConversationToday { get; set; } = false;

        public Dictionary<string, bool> RelationshipConversations { get; set; } = new()
                                                                                  {
                                                                                        ["stranger"] = false,
                                                                                        ["friendly"] = false,
                                                                                        ["dating"] = false,
                                                                                        ["married"] = false,
                                                                                        ["divorced"] =  false
                                                                                  };

        /// <summary>
        /// Returns true if the player has spoken to the NPC at least once.
        /// </summary>
        public bool HasIntroduced
        {
            get
            {
                if (!RelationshipConversations["stranger"])
                {
                    RelationshipConversations["stranger"] = ExperiencedConverastions.Contains($"introduction");
                }
                return RelationshipConversations["stranger"];
            }
        }

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

        private MidnightConversation? nextConversation;
        /// <summary>
        /// If filled out indicates that the NPC is in an extended conversation with the farmer.
        /// </summary>
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
        /// List of locations the player has spoken to the NPC today.
        /// </summary>
        public List<GameLocation> SpokenToLocations { get; } = new();

        /// <summary>
        /// Custom stats for your mod.
        /// </summary>
        public Dictionary<string, MidnightStats> Stats { get; protected set; } = new();
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
            eventMonitor.ModHelper.Events.GameLoop.TimeChanged += OnTimeChange;

            //Setup the speaker for all of this NPCs conversations.
            foreach (var conversation in Conversations)
            {
                conversation.SetSpeaker(this);
            }

            LoadNpc();
        }

        /// <summary>
        /// Loads the MidnightNpc data if it exists. Only works for the main player currently.
        /// </summary>
        /// <exception cref="ApplicationException">Will throw if the EventMenitor is not properly setup.</exception>
        private void LoadNpc()
        {
            var eventMonitor = EventMonitor.Get ?? throw new ApplicationException("Event Monitor not set up.");

            if (!Context.IsMainPlayer) return;

            var npcSave = eventMonitor.ModHelper.Data.ReadSaveData<MidnightNpcSave>($"{Name}.save");
            if (npcSave != null)
            {
                Stats = npcSave.Stats;
                ExperiencedConverastions = npcSave.ExperiencedConverastions;
                RelationshipConversations = npcSave.RelationshipConversations;
                nextConversation = npcSave.NextConversation;
                PlannedMovements = npcSave.PlannedMovements;
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

        #region Conversations
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
        }

        #region Conversation Movement
        private MidnightConversation? AfterMoveConversation;
        /// <summary>
        /// Moves the NPC to the provided position.
        /// </summary>
        /// <param name="locationName">The name of the location the NPC should move to.</param>
        /// <param name="position">The point to move the NPC to within the location.</param>
        /// <param name="afterPathing">The function to call once the NPC reaches the destintation.</param>
        public void MoveTo(MidnightMovement Move)
        {
            AfterMoveConversation = Move.AfterMoveConversation;
            var location = Game1._locationLookup[Move.LocationName];
            StardewNpc.controller = new StardewValley.Pathfinding.PathFindController(StardewNpc, location, Move.Position, 1, StartAfterMoveConversation);
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

            if (!RelationshipConversations[RelationshipStatus] && MidnightConversation.TryGetConversation(this, RelationshipStatus, out MidnightConversation? relationshipConversation))
            {
                return relationshipConversation ?? throw new ApplicationException("Null conversation returned on true MidnightConversation.TryGetConversation");
            }

            if (NextConversation != null && !HadExtendedConversationToday)
            {
                HadExtendedConversationToday = true;
                var returnConversation = NextConversation;
                NextConversation = null;
                return returnConversation;
            }

            List<MidnightConversation> available = new();

            foreach (var conversation in Conversations)
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

        /// <summary>
        /// Checks if the Midnight NPC meets a set of requirements.
        /// </summary>
        /// <param name="reqs">The requirements to meet.</param>
        /// <returns>True if the Midnight NPC meets the requirements.</returns>
        public bool MeetsRequirements(MidnightRequirements? reqs)
        {
            if (MidnightRequirements.CheckOutRange(reqs?.Hearts, Hearts)) return false;
            foreach (var stat in reqs?.Stats ?? new())
            {
                var id = MidnightFarmer.LocalFarmer.UniqueMultiplayerID.ToString();
                var npcStat = GetStatLevel(id, stat.Key);

                if (MidnightRequirements.CheckOutRange(stat.Value, npcStat)) return false;
            }
            if (MidnightRequirements.CheckOutList(reqs?.RelationshipStatus, RelationshipStatus)) return false;

            foreach (var reqKey in reqs?.Keys ?? new())
            {
                if (!ExperiencedConverastions.Contains(reqKey))
                {
                    return false;
                }
            }

            foreach (var missingKey in reqs?.MissingKeys ?? new())
            {
                if (ExperiencedConverastions.Contains(missingKey))
                {
                    return false;
                }
            }

            if (reqs?.Spot != null &&
                !MidnightSpot.Get[reqs.Spot].IsIn(this))
            {
                return false;
            }

            return true;
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

        protected void OnButtonPressed(object? sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.MouseRight && IsMouseOver() && StardewNpc.withinPlayerThreshold(1) && CanTalk())
            {
                DisplayDialogue();
                SpokenToLocations.Add(StardewNpc.currentLocation);
            }
        }

        protected void OnDayStart(object? sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            Utility.CreateDaySaveRandom(StardewNpc.id);
            SpokenToLocations.Clear();
            HadExtendedConversationToday = false;
        }

        protected void OnLocationChange(object? sender, EventArgs e)
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

        protected void OnSaving(object? sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            if (!Context.IsMainPlayer) return;
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

        protected void OnTimeChange(object? sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            for (int i = 0; i < PlannedMovements.Count; i++)
            {
                if (PlannedMovements[i].Requirements.AreMet(this))
                {
                    MoveTo(PlannedMovements[i]);
                    PlannedMovements.RemoveAt(i);
                    break;
                }
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
