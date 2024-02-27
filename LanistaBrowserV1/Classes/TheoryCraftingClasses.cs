using CommunityToolkit.Mvvm.ComponentModel;
using LanistaBrowserV1.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private ObservableCollection<TacticEquippedItem> _equippedItems = [];

        public ObservableCollection<TacticEquippedItem> EquippedItems
        {
            get => _equippedItems;
            set => SetProperty(ref _equippedItems, value);
        }

        private ObservableCollection<TacticPlacedStat> _placedStats = [];

        public ObservableCollection<TacticPlacedStat> PlacedStats
        {
            get => _placedStats;
            set => SetProperty(ref _placedStats, value);
        }

        public ObservableCollection<TacticsLevels> Levels { get; set; } = [];

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
    }

    public class TacticEquippedItem : ObservableObject
    {
        public int Id { get; set; }
        public int TacticId { get; set; }
        public string EquippedType { get; set; } = string.Empty;
        public int EquippedId { get; set; }
        public int EquippedLevel { get; set; }

        public string equippedSlot { get; set; } = string.Empty;
    }

    public class TacticPlacedStat : ObservableObject
    {
        public int Id { get; set; }
        public int TacticId { get; set; }
        public int Level { get; set; }
        public string StatType { get; set; } = string.Empty;
        public int StatId { get; set; }
        public int StatValue { get; set; }
    }

    public class TacticsLevels : ObservableObject
    {
        public int Id { get; set; }
        public int TacticId { get; set; }
        public int Level { get; set; }
        public string Levelnotes { get; set; } = string.Empty;

        private string _equippedItemOnLevel = string.Empty;

        public string EquippedItemOnLevel
        {
            get => _equippedItemOnLevel;
            set => SetProperty(ref _equippedItemOnLevel, value);
        }

        public string LevelAsString { get; set; } = string.Empty;

        private string _placedStatsString = string.Empty;

        public string PlacedStatsString
        {
            get => _placedStatsString;
            set => SetProperty(ref _placedStatsString, value);
        }
    }
}