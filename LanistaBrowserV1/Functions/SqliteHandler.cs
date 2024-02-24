﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DryIoc.FastExpressionCompiler.LightExpression;
using System.Data;
using LanistaBrowserV1.Classes;
using LanistaBrowserV1.ViewModels;
using DryIoc.ImTools;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace LanistaBrowserV1.Functions
{
    public class SqliteHandler
    {
        private static string DbPath()
        {
            string folderPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "LanistaBrowser");

            Directory.CreateDirectory(folderPath);

            string dbPath = Path.Combine(folderPath, "mydatabase.db");

            return dbPath;
        }

        public static async Task CreateAndUpdateTables()
        {
            string path = DbPath();
            await using var connection = new SqliteConnection($"Data Source={path}");
            await connection.OpenAsync();

            // Create tables if they don't exist
            await connection.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS FavoritedWeapons (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL)");
            await connection.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS FavoritedArmors (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL)");
            await connection.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS FavoritedConsumables (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL)");
            await connection.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS Tactics (Id INTEGER PRIMARY KEY AUTOINCREMENT, TacticName TEXT NOT NULL)");
            await connection.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS TacticsPlacedStats (Id INTEGER PRIMARY KEY AUTOINCREMENT, TacticId INTEGER NOT NULL, Level INTEGER NOT NULL, FOREIGN KEY(TacticId) REFERENCES Tactics(Id) ON DELETE CASCADE)");
            await connection.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS TacticsEquipped (Id INTEGER PRIMARY KEY AUTOINCREMENT, TacticId INTEGER NOT NULL, FOREIGN KEY(TacticId) REFERENCES Tactics(Id) ON DELETE CASCADE)");

            // List of tables and their expected columns
            var tablesAndColumns = new Dictionary<string, List<(string ColumnName, string ColumnType)>>
            {
        { "FavoritedWeapons", new List<(string, string)> { ("Id", "INTEGER"), ("Name", "TEXT") } },
        { "FavoritedArmors", new List<(string, string)> { ("Id", "INTEGER"), ("Name", "TEXT") } },
        { "FavoritedConsumables", new List<(string, string)> { ("Id", "INTEGER"), ("Name", "TEXT")} },
        { "Tactics", new List<(string, string)> { ("Id", "INTEGER"), ("TacticName", "TEXT"), ("RaceID", "INTEGER NOT NULL"), ("WeaponSkillID", "INTEGER NOT NULL"), ("LoadedCharacterName", "TEXT DEFAULT ''") } },
        { "TacticsPlacedStats", new List<(string, string)> { ("Id", "INTEGER"), ("TacticId", "INTEGER"), ("StatType", "TEXT NOT NULL"), ("StatID", "INTEGER NOT NULL"), ("StatValue", "INTEGER NOT NULL") } },
        { "TacticsEquipped", new List<(string, string)> { ("Id", "INTEGER"), ("TacticId", "INTEGER"), ("EquippedType", "TEXT NOT NULL"), ("EquippedID", "INTEGER NOT NULL"), ("EquippedLevel", "INTEGER NOT NULL") } },
            };

            // Check each table for missing columns
            foreach (var table in tablesAndColumns)
            {
                var columns = (await connection.QueryAsync<dynamic>($"PRAGMA table_info({table.Key})", commandType: CommandType.Text))
                    .Select(row => (string)row.name)
                    .ToList();
                var missingColumns = table.Value.Select(c => c.ColumnName).Except(columns);

                // Add missing columns
                foreach (var column in missingColumns)
                {
                    var columnType = table.Value.First(c => c.ColumnName == column).ColumnType;
                    await connection.ExecuteAsync($"ALTER TABLE {table.Key} ADD COLUMN {column} {columnType}");
                }
            }

            //Create Triggers
            await connection.ExecuteAsync(@"CREATE TRIGGER IF NOT EXISTS update_gladiator_name
                                                BEFORE UPDATE ON Tactics
                                                FOR EACH ROW
                                                BEGIN
                                                   UPDATE Tactics
                                                   SET LoadedCharacterName = ''
                                                   WHERE LoadedCharacterName = NEW.LoadedCharacterName;
                                                END;");
        }

        public static List<FavoritedWeapon> FetchFavoritedWeapons()
        {
            string path = DbPath();
            using var connection = new SqliteConnection($"Data Source={path}");
            connection.Open();

            return connection.Query<FavoritedWeapon>("SELECT * FROM FavoritedWeapons").ToList();
        }

        public static List<FavoritedConsumable> FetchFavoritedConsumables()
        {
            string path = DbPath();
            using var connection = new SqliteConnection($"Data Source={path}");
            connection.Open();

            return connection.Query<FavoritedConsumable>("SELECT * FROM FavoritedConsumables").ToList();
        }

        public static List<FavoritedArmor> FetchFavoritedArmors()
        {
            string path = DbPath();
            using var connection = new SqliteConnection($"Data Source={path}");
            connection.Open();

            return connection.Query<FavoritedArmor>("SELECT * FROM FavoritedArmors").ToList();
        }

        public static ObservableCollection<Tactic> FetchTactics()
        {
            string path = DbPath();
            using var connection = new SqliteConnection($"Data Source={path}");
            connection.Open();

            var tacticsList = connection.Query<Tactic>("SELECT * FROM Tactics").ToList();

            foreach (var t in tacticsList)
            {
                t.EquippedItems = FetchTacticsEquippedItems(t.Id);
                t.PlacedStats = FetchTacticsPlacedStat(t.Id);
            }

            return new ObservableCollection<Tactic>(tacticsList);
        }

        public static List<TacticEquippedItem> FetchTacticsEquippedItems(int tacticId)
        {
            string path = DbPath();
            using var connection = new SqliteConnection($"Data Source={path}");
            connection.Open();

            var equippedItems = connection.Query<TacticEquippedItem>("SELECT * FROM TacticsEquipped WHERE TacticId = @tacticId", new { tacticId }).ToList();

            return equippedItems;
        }

        public static List<TacticPlacedStat> FetchTacticsPlacedStat(int tacticId)
        {
            string path = DbPath();
            using var connection = new SqliteConnection($"Data Source={path}");
            connection.Open();

            var placedStats = connection.Query<TacticPlacedStat>("SELECT * FROM TacticsPlacedStats WHERE TacticId = @tacticId", new { tacticId }).ToList();

            return placedStats;
        }

        public static void CreateNewTactic(string name, int raceId, int weaponTypeId)
        {
            string path = DbPath();
            using var connection = new SqliteConnection($"Data Source={path}");
            connection.Open();

            connection.Execute("INSERT INTO Tactics (TacticName, RaceID, WeaponSkillID) VALUES (@TacticName, @RaceID, @WeaponSkillID)", new { TacticName = name, RaceID = raceId, WeaponSkillID = weaponTypeId });
        }

        public static void ToggleFavoritedItem(object x, MainViewModel viewModel)
        {
            string path = DbPath();
            using var connection = new SqliteConnection($"Data Source={path}");
            connection.Open();

            if (x is Weapon weapon)
            {
                var favoritedWeapon = viewModel.FavoritedWeapons.FirstOrDefault(w => w.Id == weapon.Id);
                if (favoritedWeapon != null)
                {
                    connection.Execute("DELETE FROM FavoritedWeapons WHERE Id = @Id", new { weapon.Id });
                    viewModel.FavoritedWeapons.Remove(favoritedWeapon);
                    viewModel.WeaponList.First(w => w.Id == weapon.Id).IsFavorited = false;
                }
                else
                {
                    connection.Execute("INSERT INTO FavoritedWeapons (Id, Name) VALUES (@Id, @Name)", new { weapon.Id, weapon.Name });
                    viewModel.FavoritedWeapons.Add(new FavoritedWeapon { Id = weapon.Id, Name = weapon.Name! });
                    viewModel.WeaponList.First(w => w.Id == weapon.Id).IsFavorited = true;
                }
            }

            if (x is Armor armor)
            {
                var favoritedArmor = viewModel.FavoritedArmors.FirstOrDefault(a => a.Id == armor.Id);
                if (favoritedArmor != null)
                {
                    connection.Execute("DELETE FROM FavoritedArmor WHERE Id = @Id", new { armor.Id });
                    viewModel.FavoritedArmors.Remove(favoritedArmor);
                    viewModel.ArmorList.First(a => a.Id == armor.Id).IsFavorited = false;
                }
                else
                {
                    connection.Execute("INSERT INTO FavoritedArmor (Id, Name) VALUES (@Id, @Name)", new { armor.Id, armor.Name });
                    viewModel.FavoritedArmors.Add(new FavoritedArmor { Id = armor.Id, Name = armor.Name! });
                    viewModel.ArmorList.First(a => a.Id == armor.Id).IsFavorited = true;
                }
            }

            if (x is Consumable consumable)
            {
                var favoritedConsumable = viewModel.FavoritedConsumables.FirstOrDefault(c => c.Id == consumable.Id);
                if (favoritedConsumable != null)
                {
                    connection.Execute("DELETE FROM FavoritedConsumables WHERE Id = @Id", new { consumable.Id });
                    viewModel.FavoritedConsumables.Remove(favoritedConsumable);
                    viewModel.ConsumableList.First(c => c.Id == consumable.Id).IsFavorited = false;
                }
                else
                {
                    connection.Execute("INSERT INTO FavoritedConsumables (Id, Name) VALUES (@Id, @Name)", new { consumable.Id, consumable.Name });
                    viewModel.FavoritedConsumables.Add(new FavoritedConsumable { Id = consumable.Id, Name = consumable.Name! });
                    viewModel.ConsumableList.First(c => c.Id == consumable.Id).IsFavorited = true;
                }
            }
        }
    }
}