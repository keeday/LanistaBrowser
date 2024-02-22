using Avalonia.Metadata;
using LanistaBrowserV1.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LanistaBrowserV1.Functions
{
    public class LanistaApiCall
    {
        public static async Task<List<Weapon>> GetWeapons()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync("https://beta.lanista.se/api/external/items/weapons/all");

            List<Weapon> weaponList = JsonConvert.DeserializeObject<List<Weapon>>(response)!;

            return weaponList;
        }

        public static async Task<List<Armor>> GetArmors()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync("https://beta.lanista.se/api/external/items/armors/all");

            List<Armor> armorList = JsonConvert.DeserializeObject<List<Armor>>(response)!;

            return armorList;
        }

        public static async Task<List<Consumable>> GetConsumables()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync("https://beta.lanista.se/api/external/items/armors/all");

            List<Consumable> consumableList = JsonConvert.DeserializeObject<List<Consumable>>(response)!;

            return consumableList;
        }

        public static async Task<ApiConfig> GetConfig()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync("https://beta.lanista.se/api/config");

            ApiConfig apiConfig = JsonConvert.DeserializeObject<ApiConfig>(response)!;

            return apiConfig;
        }
    }
}