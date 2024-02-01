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
    public abstract class MidnightMod : Mod
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
        protected abstract void Start();

        /// <summary>
        /// Loads the Npcs when the game is loaded from a save.
        /// </summary>
        protected abstract void LoadNpcs();

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
                LoadNpcs();
            }
        }
    }
}
