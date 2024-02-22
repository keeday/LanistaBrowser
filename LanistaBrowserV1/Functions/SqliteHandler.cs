using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Dapper;
using DryIoc.FastExpressionCompiler.LightExpression;
using System.Data;
using LanistaBrowserV1.Classes;
using LanistaBrowserV1.ViewModels;
using DryIoc.ImTools;

namespace LanistaBrowserV1.Functions
{
    public class SqliteHandler
    {
        public static async Task CreateAndUpdateTables()
        {
            await using var connection = new SqliteConnection("Data Source=mydatabase.db");
            await connection.OpenAsync();

            // Create tables if they don't exist
            await connection.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS FavoritedWeapons (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL)");
            await connection.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS FavoritedArmors (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL)");
            await connection.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS FavoritedConsumables (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL)");
            await connection.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS Tactics (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Race TEXT NOT NULL)");
            await connection.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS TacticsLevels (Id INTEGER PRIMARY KEY AUTOINCREMENT, TacticsId INTEGER, Level INTEGER)");

            // List of tables and their expected columns
            var tablesAndColumns = new Dictionary<string, List<(string ColumnName, string ColumnType)>>
            {
        { "FavoritedWeapons", new List<(string, string)> { ("Id", "INTEGER"), ("Name", "TEXT") } },
        { "FavoritedArmors", new List<(string, string)> { ("Id", "INTEGER"), ("Name", "TEXT") } },
        { "FavoritedConsumables", new List<(string, string)> { ("Id", "INTEGER"), ("Name", "TEXT")} },
        { "Tactics", new List<(string, string)> { ("Id", "INTEGER"), ("Name", "TEXT"), ("Race", "TEXT")} },
        { "TacticsLevels", new List<(string, string)> { ("Id", "INTEGER"), ("TacticsId", "INTEGER"), ("Level", "INTEGER"),
                                                        ("STAMINA", "INTEGER"), ("STRENGTH", "INTEGER"), ("ENDURANCE", "INTEGER"),
                                                        ("INITIATIVE", "INTEGER"), ("LEARNING_CAPACITY", "INTEGER"), ("DODGE", "INTEGER"),
                                                        ("LUCK", "INTEGER"), ("DISCIPLINE", "INTEGER"), ("AXE", "INTEGER"),
                                                        ("SWORD", "INTEGER"), ("MACE", "INTEGER"), ("STAVE", "INTEGER"),
                                                        ("SHIELD", "INTEGER"), ("SPEAR", "INTEGER"), ("CHAIN", "INTEGER"), ("EquippedMainhaindID","INTEGER"), ("EquippedOffhandID","INTEGER") } }
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
        }

        public static List<FavoritedWeapon> FetchFavoritedWeapons()
        {
            using var connection = new SqliteConnection("Data Source=mydatabase.db");
            connection.Open();

            return connection.Query<FavoritedWeapon>("SELECT * FROM FavoritedWeapons").ToList();
        }

        public static List<FavoritedConsumable> FetchFavoritedConsumables()
        {
            using var connection = new SqliteConnection("Data Source=mydatabase.db");
            connection.Open();

            return connection.Query<FavoritedConsumable>("SELECT * FROM FavoritedConsumables").ToList();
        }

        public static List<FavoritedArmor> FetchFavoritedArmors()
        {
            using var connection = new SqliteConnection("Data Source=mydatabase.db");
            connection.Open();

            return connection.Query<FavoritedArmor>("SELECT * FROM FavoritedArmors").ToList();
        }

        public static void ToggleFavoritedItem(object x, MainViewModel viewModel)
        {
            using var connection = new SqliteConnection("Data Source=mydatabase.db");
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