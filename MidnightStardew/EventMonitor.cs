using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley.Monsters;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Objects;
using Newtonsoft.Json;
using MidnightStardew.MidnightItems;
using MidnightStardew.MidnightCharacters;

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
            // Override character dialogue
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

            // Load custom sprites
            if (e.NameWithoutLocale.StartsWith("Characters/"))
            {
                var split = e.NameWithoutLocale.ToString()?.Split('/');
                if (split != null)
                {
                    var characterName = split[^1];
                    if (MidnightNpc.Get.TryGetValue(characterName, out var character) && character.SpriteName != null)
                    {
                        var image = Path.Combine(ModHelper.DirectoryPath, "Data", "Characters", $"{characterName}{character.spriteName}.png");
                        if (File.Exists(image))
                        {
                            e.Edit(asset =>
                            {
                                Texture2D ribbon = ModHelper.ModContent.Load<Texture2D>(image);
                                asset.AsImage().PatchImage(source: ribbon, patchMode: PatchMode.Replace);
                            });
                        }
                    }
                }
            }

            // Load custom items
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                var itemsJson = Path.Combine(ModHelper.DirectoryPath, "Data", "Items", "Items.json");
                if (File.Exists(itemsJson))
                {
                    e.Edit((asset) =>
                    {
                        var editor = asset.AsDictionary<string, ObjectData>();
                        foreach (var item in MidnightItem.Parse(itemsJson))
                        {
                            editor.Data[item.Key] = item.Value;
                        }
                    });
                }
            }

            // Load custom item textures.
            if (MidnightItem.TexturePaths.Contains(e.NameWithoutLocale.BaseName))
            {
                e.LoadFromModFile<Texture2D>(e.NameWithoutLocale.BaseName, AssetLoadPriority.Medium);
            }
        }

        protected Dictionary<string, string> GetEmpty() => new();
    }
}
