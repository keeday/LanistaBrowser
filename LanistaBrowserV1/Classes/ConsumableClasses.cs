using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanistaBrowserV1.Classes
{
    public class Consumable
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("cooldown")]
        public int? Cooldown { get; set; }

        [JsonProperty("weight")]
        public double? Weight { get; set; }

        [JsonProperty("crit_rate")]
        public double? CritRate { get; set; }

        [JsonProperty("min_crit_rate")]
        public double? MinCritRate { get; set; }

        [JsonProperty("max_crit_rate")]
        public double? MaxCritRate { get; set; }

        [JsonProperty("increased_hit_rate")]
        public double? IncreasedHitRate { get; set; }

        [JsonProperty("sell_value")]
        public double? SellValue { get; set; }

        [JsonProperty("required_level")]
        public int? RequiredLevel { get; set; }

        [JsonProperty("required_ranking_points")]
        public int? RequiredRankingPoints { get; set; }

        [JsonProperty("required_ranking_points_type")]
        public string? RequiredRankingPointsType { get; set; }

        [JsonProperty("min_popularity")]
        public int? MinPopularity { get; set; }

        [JsonProperty("max_popularity")]
        public int? MaxPopularity { get; set; }

        [JsonProperty("max_level")]
        public int? MaxLevel { get; set; }

        [JsonProperty("duration")]
        public int? Duration { get; set; }

        [JsonProperty("soulbound")]
        public bool? Soulbound { get; set; }

        [JsonProperty("instant")]
        public bool? Instant { get; set; }

        [JsonProperty("is_hidden")]
        public bool? IsHidden { get; set; }

        [JsonProperty("hide_duration")]
        public bool? HideDuration { get; set; }

        [JsonProperty("asset")]
        public Asset? Asset { get; set; }

        [JsonProperty("for_live_battle")]
        public bool? ForLiveBattle { get; set; }

        [JsonProperty("active_rounds")]
        public int? ActiveRounds { get; set; }

        [JsonProperty("proc_type_name")]
        public string? ProcTypeName { get; set; }

        [JsonProperty("live_battle_debuff")]
        public bool? LiveBattleDebuff { get; set; }

        [JsonProperty("restore_hp")]
        public string? RestoreHp { get; set; }

        [JsonProperty("restore_hp_chance")]
        public double? RestoreHpChance { get; set; }

        [JsonProperty("damage")]
        public string? Damage { get; set; }

        [JsonProperty("damage_chance")]
        public double? DamageChance { get; set; }

        [JsonProperty("reduced_hit_rate_chance")]
        public double? ReducedHitRateChance { get; set; }

        [JsonProperty("reduced_hit_rate")]
        public double? ReducedHitRate { get; set; }

        [JsonProperty("requirements")]
        public List<Requirement>? Requirements { get; set; }

        [JsonProperty("bonuses")]
        public List<PotionBonus>? Bonuses { get; set; }

        [JsonProperty("instant_points")]
        public List<InstantPoints>? InstantPoints { get; set; }

        [JsonProperty("xp")]
        public int? Xp { get; set; }

        [JsonProperty("given_coins")]
        public int? GivenCoins { get; set; }

        [JsonProperty("popularity")]
        public int? Popularity { get; set; }

        [JsonProperty("rounds")]
        public int? Rounds { get; set; }

        [JsonProperty("undead")]
        public bool? Undead { get; set; }

        [JsonProperty("undead_chance")]
        public string? UndeadChance { get; set; }

        public bool IsFavorited { get; set; } = false;

        public string? RaceRestrictions
        {
            get
            {
                var raceRequirements = this.Requirements?.Where(r => r.Requirementable == "App\\Models\\Race");

                if (raceRequirements == null || !raceRequirements.Any())
                {
                    return null;
                }

                return string.Join(", ", raceRequirements.Select(item => item.RaceName));
            }
        }

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
    }

    public class PotionBonus
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("bonusable_name")]
        public string? BonusableName { get; set; }

        [JsonProperty("bonus_value_display")]
        public string? BonusValueDisplay { get; set; }

        [JsonProperty("bonus_text")]
        public string? BonusText { get; set; }
    }

    public class InstantPoints
    {
        [JsonProperty("value")]
        public string? Value { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("effects_text")]
        public string? EffectsText { get; set; }
    }

    public class FavoritedConsumable
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}