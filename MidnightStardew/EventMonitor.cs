using StardewModdingAPI.Events;
using StardewModdingAPI;
using MidnightStardew;
using StardewValley.Monsters;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewHappyEndings
{
    public class EventMonitor
    {
        public static EventMonitor? Get { get; private set; }

        public IModHelper ModHelper { get; }
        public IMonitor ModMonitor { get; }

        public static void Create(IModHelper modHelper, IMonitor modMonitor)
        {
            Get = new EventMonitor(modHelper, modMonitor);
        }

        private EventMonitor(IModHelper helper, IMonitor monitor)
        {
            ModHelper = helper;
            ModMonitor = monitor;

            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        }

        private void GameLoop_UpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
        }

        private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
        {

        }

        private void Content_AssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.Name.StartsWith("Characters/Dialogue/"))
            {
                var split = e.Name.ToString()?.Split('/');
                if (split != null)
                {
                    var characterName = split[^1];
                    if (MidnightNpc.Get.TryGetValue(characterName, out var character))
                    {
                        e.LoadFrom(GetEmpty, AssetLoadPriority.Medium);
                    }
                }
            }
        }

        private Dictionary<string, string> GetEmpty() => new();
    }
}
