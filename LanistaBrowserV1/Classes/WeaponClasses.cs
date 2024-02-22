using LanistaBrowserV1.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanistaBrowserV1.Classes
{
    public class Weapon
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("type")]
        public int? Type { get; set; }

        [JsonProperty("type_name")]
        public string? TypeName { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("base_damage_min")]
        public double? BaseDamageMin { get; set; }

        [JsonProperty("base_damage_max")]
        public double? BaseDamageMax { get; set; }

        [JsonProperty("crit_damage_min")]
        public double? CritDamageMin { get; set; }

        [JsonProperty("crit_damage_max")]
        public double? CritDamageMax { get; set; }

        [JsonProperty("crit_damage_min_info")]
        public double? CritDamageMinInfo { get; set; }

        [JsonProperty("crit_damage_max_info")]
        public double? CritDamageMaxInfo { get; set; }

        [JsonProperty("crit_damage")]
        public string? CritDamage { get; set; }

        [JsonProperty("crit_damage_info")]
        public string? CritDamageInfo { get; set; }

        [JsonProperty("durability")]
        public double? Durability { get; set; }

        [JsonProperty("damage_roof")]
        public double? DamageRoof { get; set; }

        [JsonProperty("absorption")]
        public double? Absorption { get; set; }

        [JsonProperty("weight")]
        public double? Weight { get; set; }

        [JsonProperty("is_two_handed")]
        public bool? IsTwoHanded { get; set; }

        [JsonProperty("can_dual_wield")]
        public bool? CanDualWield { get; set; }

        [JsonProperty("required_level")]
        public double? RequiredLevel { get; set; }

        [JsonProperty("required_ranking_points")]
        public double? RequiredRankingPoints { get; set; }

        [JsonProperty("required_ranking_points_type")]
        public string? RequiredRankingPointsType { get; set; }

        [JsonProperty("min_popularity")]
        public double? MinPopularity { get; set; }

        [JsonProperty("max_popularity")]
        public double? MaxPopularity { get; set; }

        [JsonProperty("max_level")]
        public int? MaxLevel { get; set; }

        [JsonProperty("recommended_level")]
        public int? RecommendedLevel { get; set; }

        [JsonProperty("crit_rate")]
        public double? CritRate { get; set; }

        [JsonProperty("min_crit_rate")]
        public double? MinCritRate { get; set; }

        [JsonProperty("max_crit_rate")]
        public double? MaxCritRate { get; set; }

        [JsonProperty("is_shield")]
        public bool? IsShield { get; set; }

        [JsonProperty("is_ranged")]
        public bool? IsRanged { get; set; }

        [JsonProperty("is_weapon")]
        public bool? IsWeapon { get; set; }

        [JsonProperty("sell_value")]
        public double? SellValue { get; set; }

        [JsonProperty("default_price")]
        public double? DefaultPrice { get; set; }

        [JsonProperty("requires_legend")]
        public bool? RequiresLegend { get; set; }

        [JsonProperty("soulbound")]
        public bool? Soulbound { get; set; }

        [JsonProperty("asset")]
        public Asset? Asset { get; set; }

        [JsonProperty("requirements")]
        public List<Requirement>? Requirements { get; set; }

        [JsonProperty("bonuses")]
        public List<Bonus>? Bonuses { get; set; }

        [JsonProperty("actions")]
        public int? Actions { get; set; }

        [JsonProperty("defensive_actions")]
        public int? DefensiveActions { get; set; }

        [JsonProperty("stack_multiplier")]
        public double? StackMultiplier { get; set; }

        [JsonProperty("stack_chance")]
        public double? StackChance { get; set; }

        [JsonProperty("stack_max")]
        public int? StackMax { get; set; }

        public int? StrengthRequirementValue
        {
            get
            {
                var requirement = this.Requirements?.FirstOrDefault(r => r.RequirementableId == 2);
                return requirement?.RequirementValue;
            }
        }

        public int? SkillRequirementValue
        {
            get
            {
                var requirement = this.Requirements?.FirstOrDefault(r => r.Requirementable == "App\\Models\\WeaponSkill");
                return requirement?.RequirementValue;
            }
        }

        public string? GrabType
        {
            get
            {
                if (this.IsTwoHanded == true)
                {
                    return "2h";
                }
                else
                {
                    return "1h";
                }
            }
        }

        public bool IsFavorited { get; set; } = false;
    }

    public class Asset
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }
    }

    public class Requirement
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("requirement_text")]
        public string? RequirementText { get; set; }

        [JsonProperty("requirement_meta_text")]
        public string? RequirementMetaText { get; set; }

        [JsonProperty("requirement_value")]
        public int? RequirementValue { get; set; }

        [JsonProperty("requirement_type")]
        public int? RequirementType { get; set; }

        [JsonProperty("requirementable")]
        public string? Requirementable { get; set; }

        [JsonProperty("Requirementable_id")]
        public int? RequirementableId { get; set; }

        [JsonProperty("show_current_value")]
        public bool? ShowCurrentValue { get; set; }

        [JsonProperty("achievements")]
        public List<string>? Achievements { get; set; }

        //[JsonProperty("age")]
        //public int? Age { get; set; }
    }

    public class Bonus
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("additive")]
        public int? Additive { get; set; }
    }

    public class FavoritedWeapon
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}