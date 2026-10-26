using System;
using System.Collections.Generic;
using UnityEngine;
using VertigoDemo.Data;

namespace VertigoDemo.Game
{
    // JsonUtility cannot write a Dictionary, so the inventory is saved as a list of entries keyed by asset name.
    public static class InventorySave
    {
        [Serializable]
        private class SaveData
        {
            public List<Entry> items = new List<Entry>();
        }

        [Serializable]
        private class Entry
        {
            public string id;
            public int amount;
        }

        #region Public API

        public static string Write(IReadOnlyDictionary<RewardData, int> items)
        {
            var data = new SaveData();
            foreach (KeyValuePair<RewardData, int> pair in items)
            {
                data.items.Add(new Entry { id = pair.Key.name, amount = pair.Value });
            }
            return JsonUtility.ToJson(data, true);
        }

        public static Dictionary<RewardData, int> Read(string json, IReadOnlyList<RewardData> catalog)
        {
            var items = new Dictionary<RewardData, int>();
            if (string.IsNullOrEmpty(json))
            {
                return items;
            }

            SaveData data;
            try
            {
                data = JsonUtility.FromJson<SaveData>(json);
            }
            catch (ArgumentException exception)
            {
                Debug.LogWarning($"The save could not be read and was ignored: {exception.Message}");
                return items;
            }

            var byId = new Dictionary<string, RewardData>();
            foreach (RewardData reward in catalog)
            {
                byId[reward.name] = reward;
            }

            foreach (Entry entry in data.items)
            {
                RewardData reward;
                if (entry.amount <= 0 || !byId.TryGetValue(entry.id, out reward))
                {
                    Debug.LogWarning($"Skipped unknown or empty save entry '{entry.id}'.");
                    continue;
                }
                items[reward] = entry.amount;
            }
            return items;
        }

        #endregion
    }
}
