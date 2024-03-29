﻿using MidnightStardew.MidnightCharacters;
using MidnightStardew.MidnightWorld;
using Newtonsoft.Json;
using StardewHappyEndings;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewValley;
using StardewValley.Internal;
using StardewValley.Locations;
using System;

namespace MidnightStardew
{
    public class MidnightMod : Mod
    {
        public delegate void GameLoadedDelegate(object sender, EventArgs e);
        public event GameLoadedDelegate? GameLoaded;

        public static MidnightMod? Get { get; private set; }

        public MidnightMod() : base()
        {
            Get = this;
        }

        public override sealed void Entry(IModHelper helper)
        {
            Helper.Events.Specialized.LoadStageChanged += Game_LoadStageChanged;
            EventMonitor.Create(Helper, Monitor);

            Start();
        }

        /// <summary>
        /// Called when the mod is loaded.
        /// </summary>
        protected virtual void Start() { }

        /// <summary>
        /// Loads the Npcs when the game is loaded from a save.
        /// </summary>
        protected virtual void LoadNpcs() 
        {
            var characterDir = Path.Combine(Helper.DirectoryPath, "Data", "Characters");

            if (!Directory.Exists(characterDir)) return;

            foreach (var characterFile in Directory.EnumerateFiles(characterDir))
            {
                if (Path.GetExtension(characterFile) != ".json") continue;
                Helper.GameContent.InvalidateCache($"Characters/Dialogue/{Path.GetFileNameWithoutExtension(characterFile)}");
                MidnightNpc.Create<MidnightNpc>(characterFile);
            }
        }

        /// <summary>
        /// Loads named spots within locations that can be used to check requirements.
        /// </summary>
        protected virtual void LoadSpots() 
        {
            var spotFile = Path.Combine(Helper.DirectoryPath, "Data", "Spots.json");

            if (!File.Exists(spotFile)) return;

            var spotJson = File.ReadAllText(spotFile);
            var spots = JsonConvert.DeserializeObject<Dictionary<string, MidnightSpot>>(spotJson) ?? throw new ApplicationException("No spots loaded.");
            MidnightSpot.Get = spots;
        }

        /// <summary>
        /// Called when the stage of game loading changes as a game is loaded.
        /// </summary>
        /// <param name="sender">The originating object of the event.</param>
        /// <param name="e">Always null</param>
        private void Game_LoadStageChanged(object? sender, StardewModdingAPI.Events.LoadStageChangedEventArgs e)
        {
            if (e.NewStage == LoadStage.Ready)
            { 
                GameLoaded?.Invoke(this, e);

                LoadSpots();
                LoadNpcs();
            }
        }
    }
}
