
namespace MidnightStardew.MidnightItems
{
    /// <summary>
    /// A way to easily translate to Stardew Valley item category from human readable descriptions.
    /// </summary>
    public class MidnightItemCategory
    {
        public static Dictionary<string, MidnightItemCategory> GetByName { get; } = new();
        public static Dictionary<int, MidnightItemCategory> GetByValue { get; } = new();

        #region Category definitions
        public static MidnightItemCategory Gem { get; } = new("Gem", StardewValley.Object.GemCategory, "category_gem");
        public static MidnightItemCategory Fish { get; } = new("Fish", StardewValley.Object.FishCategory, "category_fish");
        public static MidnightItemCategory Egg { get; } = new("Egg", StardewValley.Object.EggCategory, "category_egg");
        public static MidnightItemCategory Milk { get; } = new("Milk", StardewValley.Object.MilkCategory, "category_milk");
        public static MidnightItemCategory Cooking { get; } = new("Cooking", StardewValley.Object.CookingCategory, "category_cooking");
        public static MidnightItemCategory Crafting { get; } = new("Crafting", StardewValley.Object.CraftingCategory, "category_crafting");
        public static MidnightItemCategory BigCraftable { get; } = new("Big Craftable", StardewValley.Object.BigCraftableCategory, "category_big_craftable");
        public static MidnightItemCategory Mineral { get; } = new("Mineral", StardewValley.Object.mineralsCategory, "category_minerals");
        public static MidnightItemCategory Meat { get; } = new("Meat", StardewValley.Object.meatCategory, "category_meat");
        public static MidnightItemCategory Metal { get; } = new("Metal", StardewValley.Object.metalResources, "category_metal_resources");
        public static MidnightItemCategory BuildingResource { get; } = new("Building Resource", StardewValley.Object.buildingResources, "category_building_resources");
        public static MidnightItemCategory SellAtPierres { get; } = new("Sell at Pierres", StardewValley.Object.sellAtPierres, "category_sell_at_pierres");
        public static MidnightItemCategory SellAtPierresAndMarnies { get; } = new("Sell at Pierres and Marnies", StardewValley.Object.sellAtPierresAndMarnies, "category_sell_at_pierres_and_marnies");
        public static MidnightItemCategory Fertilizer { get; } = new("Fertilizer", StardewValley.Object.fertilizerCategory, "category_fertilizer");
        public static MidnightItemCategory Junk { get; } = new("Junk", StardewValley.Object.junkCategory, "category_junk");
        public static MidnightItemCategory Bait { get; } = new("Bait", StardewValley.Object.baitCategory, "category_bain");
        public static MidnightItemCategory Tackle { get; } = new("Tackle", StardewValley.Object.tackleCategory, "category_tackle");
        public static MidnightItemCategory SellAtFishShop { get; } = new("Sell at Fish Shop", StardewValley.Object.sellAtFishShopCategory, "category_sell_at_fish_shop");
        public static MidnightItemCategory Furniture { get; } = new("Furniture", StardewValley.Object.furnitureCategory, "category_furniture");
        public static MidnightItemCategory Ingredient { get; } = new("Ingredients", StardewValley.Object.ingredientsCategory, "category_ingredients");
        public static MidnightItemCategory ArtisanGood { get; } = new("Artisan Good", StardewValley.Object.artisanGoodsCategory, "category_artisan_goods");
        public static MidnightItemCategory Syrup { get; } = new("Syrup", StardewValley.Object.syrupCategory, "category_syrup");
        public static MidnightItemCategory MonsterLoot { get; } = new("Monster Loot", StardewValley.Object.monsterLootCategory, "category_monster_loot");
        public static MidnightItemCategory Equipment { get; } = new("Equipment", StardewValley.Object.equipmentCategory, "category_equipment");
        public static MidnightItemCategory Seed { get; } = new("Seed", StardewValley.Object.SeedsCategory, "category_seeds");
        public static MidnightItemCategory Vegetable { get; } = new("Vegetable", StardewValley.Object.VegetableCategory, "category_vegetable");
        public static MidnightItemCategory Fruit { get; } = new("Fruit", StardewValley.Object.FruitsCategory, "category_fruits");
        public static MidnightItemCategory Forage { get; } = new("Forage", StardewValley.Object.GreensCategory, "category_greens");
        public static MidnightItemCategory Hat { get; } = new("Hat", StardewValley.Object.hatCategory, "category_hat");
        public static MidnightItemCategory Ring { get; } = new("Ring", StardewValley.Object.ringCategory, "category_ring");
        public static MidnightItemCategory Weapon { get; } = new("Weapon", StardewValley.Object.weaponCategory, "category_weapon");
        public static MidnightItemCategory Tool { get; } = new("Tool", StardewValley.Object.toolCategory, "category_tool");
        #endregion

        /// <summary>
        /// Context tag that is added to the item automatically for having the given category.
        /// </summary>
        public string ContextTag { get; }
        /// <summary>
        /// The name of the category to use as a reference.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The integer used to represent the category in Stardew Valley.
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Creates a new Midnight Item Category object.
        /// </summary>
        /// <param name="name">The human readable name for the category.</param>
        /// <param name="value">The internal Stardew Vally integer used to represent the category.</param>
        /// <param name="contextTag">The tag added to the item for the category.</param>
        public MidnightItemCategory(string name, int value, string contextTag)
        {
            GetByName[name.ToLower()] = this;
            GetByValue[value] = this;
            Name = name;
            Value = value;
            ContextTag = contextTag;
        }

        public static implicit operator int(MidnightItemCategory category)
        {
            return category.Value;
        }
        public static explicit operator MidnightItemCategory(int categoryId)
        {
            return GetByValue[categoryId];
        }
    }
}
