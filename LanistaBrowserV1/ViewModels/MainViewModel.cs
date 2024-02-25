using Avalonia.Platform;
using Avalonia;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using LanistaBrowserV1.Functions;
using System.Collections.ObjectModel;
using LanistaBrowserV1.Classes;
using System.Linq;
using System.Diagnostics;

namespace LanistaBrowserV1.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    //Main
    private bool _isLanistaVisible = true;

    private bool _isSearchItemsVisible = false;

    private bool _isTheoryCraftingMainVisible = false;

    private bool _isWikiPageVisible = false;

    private string _currentUrl = "https://www.wikipedia.org/";

    //Search
    private bool _isWeaponsVisible = true;

    private bool _isArmorVisible = false;
    private bool _isConsumablesVisible = false;

    private List<Weapon> _weaponList = [];

    private List<Armor> _armorList = [];

    private List<Consumable> _consumableList = [];

    private List<FavoritedWeapon> _favoritedWeapons = [];

    private List<FavoritedArmor> _favoritedArmors = [];

    private List<FavoritedConsumable> _favoritedConsumables = [];

    private ApiConfig _apiConfig = new();

    //TheoryCrafting
    private ObservableCollection<Tactic> _tactics = new();

    //Other

    public static bool IsWindows = true; // => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsAndroid => RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID"));

    //Main
    public bool IsLanistaVisible
    {
        get => _isLanistaVisible;
        set
        {
            if (SetProperty(ref _isLanistaVisible, value) && value)
            {
                IsSearchItemsVisible = false;
                IsTheoryCraftingMainVisible = false;
                IsWikiPageVisible = false;
            }
        }
    }

    public bool IsSearchItemsVisible
    {
        get => _isSearchItemsVisible;
        set
        {
            if (SetProperty(ref _isSearchItemsVisible, value) && value)
            {
                IsLanistaVisible = false;
                IsTheoryCraftingMainVisible = false;
                IsWikiPageVisible = false;
            }
        }
    }

    public bool IsTheoryCraftingMainVisible
    {
        get => _isTheoryCraftingMainVisible;
        set
        {
            if (SetProperty(ref _isTheoryCraftingMainVisible, value) && value)
            {
                IsLanistaVisible = false;
                IsWikiPageVisible = false;
                IsSearchItemsVisible = false;
            }
        }
    }

    public bool IsWikiPageVisible
    {
        get => _isWikiPageVisible;
        set
        {
            if (SetProperty(ref _isWikiPageVisible, value) && value)
            {
                IsLanistaVisible = false;
                IsTheoryCraftingMainVisible = false;
                IsSearchItemsVisible = false;
            }
        }
    }

    public string CurrentUrl
    {
        get => _currentUrl;
        set => SetProperty(ref _currentUrl, value);
    }

    //Search

    public bool IsWeaponsVisible
    {
        get => _isWeaponsVisible;
        set
        {
            if (SetProperty(ref _isWeaponsVisible, value) && value)
            {
                IsArmorVisible = false;
                IsConsumablesVisible = false;
            }
        }
    }

    public List<FavoritedWeapon> FavoritedWeapons
    {
        get => _favoritedWeapons;
        set => SetProperty(ref _favoritedWeapons, value);
    }

    public List<FavoritedConsumable> FavoritedConsumables
    {
        get => _favoritedConsumables;
        set => SetProperty(ref _favoritedConsumables, value);
    }

    public List<FavoritedArmor> FavoritedArmors
    {
        get => _favoritedArmors;
        set => SetProperty(ref _favoritedArmors, value);
    }

    public List<Weapon> WeaponList
    {
        get => _weaponList;
        set => SetProperty(ref _weaponList, value);
    }

    public List<Armor> ArmorList
    {
        get => _armorList;
        set => SetProperty(ref _armorList, value);
    }

    public List<Consumable> ConsumableList
    {
        get => _consumableList;
        set => SetProperty(ref _consumableList, value);
    }

    public ApiConfig ApiConfig
    {
        get => _apiConfig;
        set => SetProperty(ref _apiConfig, value);
    }

    public bool IsArmorVisible
    {
        get => _isArmorVisible;
        set
        {
            if (SetProperty(ref _isArmorVisible, value) && value)
            {
                IsWeaponsVisible = false;
                IsConsumablesVisible = false;
            }
        }
    }

    public bool IsConsumablesVisible
    {
        get => _isConsumablesVisible;
        set
        {
            if (SetProperty(ref _isConsumablesVisible, value) && value)
            {
                IsWeaponsVisible = false;
                IsArmorVisible = false;
            }
        }
    }

    //TheoryCrafting
    public ObservableCollection<Tactic> Tactics
    {
        get => _tactics;
        set
        {
            try
            {
                foreach (var tactic in value)
                {
                    var matchingRace = ApiConfig.Races!.FirstOrDefault(race => race.Type == tactic.RaceID);
                    if (matchingRace != null)
                    {
                        tactic.RaceName = matchingRace.Name!.ToLower();
                    }

                    var matchingWeaponSkill = ApiConfig.WeaponSkills!.FirstOrDefault(skill => skill.Type == tactic.WeaponSkillID);
                    if (matchingWeaponSkill != null)
                    {
                        tactic.WeaponName = matchingWeaponSkill.Name!.ToLower();
                    }

                    foreach (var level in tactic.Levels)
                    {
                        string levelAsName = "Level " + level.Level.ToString();
                        List<string> levelStatsList = new List<string>();

                        foreach (var stat in tactic.PlacedStats)
                        {
                            if (stat.Level == level.Level && stat.StatType == "stats")
                            {
                                string statString = ApiConfig.Stats!.First(stats => stats.Type == stat.StatId).Name!.ToLower() + " +" + stat.StatValue.ToString();
                                levelStatsList.Add(statString);
                            }
                        }
                        string levelStats = string.Join(", ", levelStatsList);
                        tactic.LevelsWithStats.Add(levelAsName, levelStats);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            SetProperty(ref _tactics, value);
        }
    }
}