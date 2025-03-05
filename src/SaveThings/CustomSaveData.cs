using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace NoirCatto.SaveThings;

public static partial class CustomSaveData
{
    private static string ModSaveDataPath => Application.persistentDataPath + Path.DirectorySeparatorChar + Const.MOD_ID;
    private static readonly Dictionary<string, string> MalnourishedSaveData;

    static CustomSaveData()
    {
        MalnourishedSaveData = new Dictionary<string, string>();
        Directory.CreateDirectory(ModSaveDataPath);
    }

    public static bool SaveStorySpecific<T>(string key, T value, StoryGameSession storySession)
    {
        try
        {
            var rawValue = JsonConvert.SerializeObject(value);
            if (storySession.saveState.malnourished)
            {
                var name = storySession.saveStateNumber;
                NoirCatto.LogSource.LogInfo($"{name} starved! Saving {key} to memory...");
                MalnourishedSaveData[key + name] = rawValue;
                return true;
            }

            var savePath = GetStorySavePath(storySession);

            if (File.Exists(savePath))
            {
                var rawData = File.ReadAllText(savePath);
                var jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(rawData);
                jsonData[key] = rawValue;
                rawData = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                File.WriteAllText(savePath, rawData);
            }
            else
            {
                var jsonData = new Dictionary<string, string>();
                jsonData[key] = rawValue;
                var rawData = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                File.WriteAllText(savePath, rawData);
            }
        }
        catch (Exception ex)
        {
            NoirCatto.LogSource.LogError($"ERROR WHILE SAVING: {key}");
            NoirCatto.LogSource.LogError(ex);
            return false;
        }
        return true;
    }

    public static bool LoadStorySpecific<T>(string key, out T value, StoryGameSession storySession)
    {
        value = default;
        try
        {
            if (storySession.saveState.malnourished)
            {
                var name = storySession.saveStateNumber;
                NoirCatto.LogSource.LogInfo($"{name} starved! Trying to load {key} from memory...");
                if (MalnourishedSaveData.TryGetValue(key + name, out var rawValue))
                {
                    value = JsonConvert.DeserializeObject<T>(rawValue);
                    MalnourishedSaveData.Remove(key + name);
                    return true;
                }
                NoirCatto.LogSource.LogInfo("Failed to load from memory, attempting to load from disk instead");
            }

            var savePath = GetStorySavePath(storySession);

            if (File.Exists(savePath))
            {
                var rawData = File.ReadAllText(savePath);
                var jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(rawData);
                if (jsonData.TryGetValue(key, out var rawValue))
                {
                    value = JsonConvert.DeserializeObject<T>(rawValue);
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            NoirCatto.LogSource.LogError($"ERROR WHILE LOADING: {key}");
            NoirCatto.LogSource.LogError(ex);
            return false;
        }
    }

    private static string GetStorySavePath(StoryGameSession storySession)
    {
        return GetStorySavePath(storySession.saveStateNumber);
    }
    private static string GetStorySavePath(SlugcatStats.Name name)
    {
        var saveSlot = RWCustom.Custom.rainWorld.options.saveSlot.ToString();
        var savePath = Path.Combine(ModSaveDataPath, saveSlot);
        Directory.CreateDirectory(savePath);
        return Path.Combine(savePath, name + ".json");
    }

    //-----

    public static void PlayerProgressionOnClearOutSaveStateFromMemory(On.PlayerProgression.orig_ClearOutSaveStateFromMemory orig, PlayerProgression self)
    {
        orig(self);
        MalnourishedSaveData.Clear();
    }

    public static void PlayerProgressionOnWipeSaveState(On.PlayerProgression.orig_WipeSaveState orig, PlayerProgression self, SlugcatStats.Name savestatenumber)
    {
        orig(self, savestatenumber);
        var savePath = GetStorySavePath(savestatenumber);
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
    }
}