using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanistaBrowserV1.Classes
{
    public class Armor
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

        [JsonProperty("base_block")]
        public int? BaseBlock { get; set; }

        [JsonProperty("min_block")]
        public int? MinBlock { get; set; }

        [JsonProperty("percentage_block")]
        public int? PercentageBlock { get; set; }

        [JsonProperty("weight")]
        public double? Weight { get; set; }

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

        [JsonProperty("crit_rate")]
        public double? CritRate { get; set; }

        [JsonProperty("min_crit_rate")]
        public double? MinCritRate { get; set; }

        [JsonProperty("max_crit_rate")]
        public double? MaxCritRate { get; set; }

        [JsonProperty("increased_hit_rate")]
        public double? IncreasedHitRate { get; set; }

        [JsonProperty("is_trinket")]
        public bool? IsTrinket { get; set; }

        [JsonProperty("sell_value")]
        public double? SellValue { get; set; }

        [JsonProperty("soulbound")]
        public bool? Soulbound { get; set; }

        [JsonProperty("asset")]
        public Asset? Asset { get; set; }

        [JsonProperty("requires_legend")]
        public bool? RequiresLegend { get; set; }

        [JsonProperty("requirements")]
        public List<Requirement>? Requirements { get; set; }

        [JsonProperty("bonuses")]
        public List<Bonus>? Bonuses { get; set; }

        //[JsonProperty("augmented_races")]
        //public List<object>? AugmentedRaces { get; set; }

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

    public class FavoritedArmor
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}