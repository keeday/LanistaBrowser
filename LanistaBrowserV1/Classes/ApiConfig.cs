using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanistaBrowserV1.Classes
{
    public class ApiConfig
    {
        [JsonProperty("armor_types")]
        public Dictionary<string, int>? ArmorTypes { get; set; }

        [JsonProperty("weapon_types")]
        public Dictionary<string, int>? WeaponTypes { get; set; }

        [JsonProperty("stats")]
        public List<Stats>? Stats { get; set; }

        [JsonProperty("weapon_skills")]
        public List<WeaponSkills>? WeaponSkills { get; set; }

        [JsonProperty("races")]
        public List<Races>? Races { get; set; }

        [JsonProperty("encant_tags")]
        public List<EncantTags>? EncantTags { get; set; }
    }

    public class Stats
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }
    }

    public class WeaponSkills
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }
    }

    public class Races
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("bonuses")]
        public Bonuses? Bonuses { get; set; }
    }

    public class Bonuses
    {
        [JsonProperty("stats")]
        public List<RaceStats>? Stats { get; set; }

        [JsonProperty("weapon_skills")]
        public List<WeaponSkills>? WeaponSkills { get; set; }
    }

    public class RaceStats
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }
    }

    public class EncantTags
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }
    }
}