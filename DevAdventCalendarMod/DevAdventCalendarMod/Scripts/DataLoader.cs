using System.IO;
using UnityEngine;
using System.Collections.Generic;
using DevAdventCalendarMod.Models;

namespace DevAdventCalendarMod.Scripts
{
    public class DataLoader
    {
        public static Data currentData;
        public static string path = Application.persistentDataPath + "\\AdventCalendarData.txt";

        public static void LoadData()
        {
            if (File.Exists(path)) currentData = JsonUtility.FromJson<Data>(File.ReadAllText(path));

            if (currentData == null)
            {
                currentData = new Data();
                currentData.ChocolatesPickedUp = new List<int>();
                currentData.DoorsOpened = new List<int>();
                currentData.GiftOpened = false;
                currentData.DevChocolatePickedUp = false;
                File.WriteAllText(path, JsonUtility.ToJson(currentData));
            }

            Logger.LogMessage("Data logged", 0);
        }

        public static void SaveData()
        {
            File.WriteAllText(path, JsonUtility.ToJson(currentData));
            Logger.LogMessage("Data saved", 0);
        }
    }
}
