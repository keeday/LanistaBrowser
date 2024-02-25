using CommunityToolkit.Mvvm.ComponentModel;
using LanistaBrowserV1.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanistaBrowserV1.Classes
{
    public class Tactic : ObservableObject
    {
        public int Id { get; set; }
        public string TacticName { get; set; } = string.Empty;
        public int RaceID { get; set; }
        public int WeaponSkillID { get; set; }
        public string LoadedCharacterName { get; set; } = string.Empty;

        public List<TacticEquippedItem> EquippedItems { get; set; } = [];
        public List<TacticPlacedStat> PlacedStats { get; set; } = [];

        public List<TacticsLevels> Levels { get; set; } = [];

        public bool IsLoaded
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.LoadedCharacterName))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public string RaceName { get; set; } = string.Empty;
        public string WeaponName { get; set; } = string.Empty;

        public Dictionary<string, string> LevelsWithStats { get; set; } = [];
    }

    public class TacticEquippedItem
    {
        public int Id { get; set; }
        public int TacticId { get; set; }
        public string EquippedType { get; set; } = string.Empty;
        public int EquippedId { get; set; }
        public int EquippedLevel { get; set; }
    }

    public class TacticPlacedStat
    {
        public int Id { get; set; }
        public int TacticId { get; set; }
        public int Level { get; set; }
        public string StatType { get; set; } = string.Empty;
        public int StatId { get; set; }
        public int StatValue { get; set; }
    }

    public class TacticsLevels
    {
        public int Id { get; set; }
        public int TacticId { get; set; }
        public int Level { get; set; }
        public string Levelnotes { get; set; } = string.Empty;

        public string LevelWithStats { get; set; } = string.Empty;
    }
}