using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightStardew.MidnightItems
{
    public class MidnightItem : ObjectData
    {
        public static Dictionary<string, MidnightItem> Parse(string filePath)
        {
            string itemJson;
            try
            {
                itemJson = File.ReadAllText(filePath);
            }
            catch (FileNotFoundException)
            {
                throw new ApplicationException($"File {filePath} does exist.");
            }

            return JsonConvert.DeserializeObject<Dictionary<string, MidnightItem>>(itemJson) ?? new();
        }

        public static HashSet<string> TexturePaths { get; set; } = new();

        public new MidnightItemCategory Category 
        {
            get
            {
                return MidnightItemCategory.GetByValue[base.Category];
            }
            set
            {
                base.Category = value;
            } 
        }

        [JsonConstructor]
        public MidnightItem(string? name,
                            string? displayName,
                            string? description,
                            string? type,
                            int? category,
                            string? categoryName,
                            int? price,
                            string? texture,
                            int? spriteIndex,
                            int? edibility,
                            bool? isDrink,
                            ObjectBuffData? buff,
                            bool? geodeDropsDefaultItems,
                            List<ObjectGeodeDropData>? geodeDrops,
                            Dictionary<string, float>? artifactSpotChanges,
                            bool? excludeFromFishingCollection,
                            bool? excludeFromShippingCollection,
                            bool? excludeFromRandomSale,
                            List<string>? contextTags,
                            Dictionary<string, string>? customFields) 
        {
            Name = name ?? throw new ApplicationException($"Item doesn't have a name defined.");
            DisplayName = displayName ?? "<Display Name Not Defined>";
            Description = description ?? "<Description Not Defined>";
            Type = type ?? "";
            if (category != null)
            {
                Category = (MidnightItemCategory)category;
            } else if (categoryName != null)
            {
                Category = MidnightItemCategory.GetByName[categoryName.ToLower()];
            }
            Price = price ?? 0;
            Texture = texture ?? throw new ApplicationException($"Item: {Name} doesn't have a Texture defined.");
            TexturePaths.Add(Texture);
            SpriteIndex = spriteIndex ?? 0;
            Edibility = edibility ?? -300;
            IsDrink = isDrink ?? false;
            Buff = buff;
            GeodeDropsDefaultItems = geodeDropsDefaultItems ?? true;
            GeodeDrops = geodeDrops;
            ArtifactSpotChances = artifactSpotChanges;
            ExcludeFromFishingCollection = excludeFromFishingCollection ?? true;
            ExcludeFromShippingCollection = excludeFromShippingCollection ?? true;
            ExcludeFromRandomSale = excludeFromRandomSale ?? true;
            ContextTags = contextTags;
            CustomFields = customFields;
        }
    }
}
