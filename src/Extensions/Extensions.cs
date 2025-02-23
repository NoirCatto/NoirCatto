using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;

namespace NoirCatto;

public static partial class Extensions
{
    public static SlugcatStats.Name SlugCatClass(this AbstractCreature crit) //Based off of Player.GetInitialSlugcatClass()
    {
        if (crit.realizedCreature is Player player)
            return player.SlugCatClass;

        if (crit.state is not PlayerState state)
        {
            NoirCatto.LogSource.LogError($"Not a Slugcat!: {crit.ID.ToString()}");
            return SlugcatStats.Name.White;
        }

        if (ModManager.MSC && crit.creatureTemplate.TopAncestor().type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
            return MoreSlugcatsEnums.SlugcatStatsName.Slugpup;

        if (ModManager.CoopAvailable && crit.Room.world.game.IsStorySession)
            return crit.world.game.rainWorld.options.jollyPlayerOptionsArray[state.playerNumber].playerClass ?? crit.world.game.GetStorySession.saveState.saveStateNumber;
        else if (!ModManager.MSC || crit.Room.world.game.IsStorySession)
            return state.slugcatCharacter;

        NoirCatto.LogSource.LogInfo($"Unable to determine SlugCatClass for: {crit.ID.ToString()}");
        return SlugcatStats.Name.White;
    }

    #region Built-in values extensions
    public static float Map(this float x, float in_min, float in_max, float out_min, float out_max, bool clamp = false)
    {
        if (clamp) x = Math.Max(in_min, Math.Min(x, in_max));
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
    
    public static void Tick(this ref int counter)
    {
        if (counter > 0) counter--;
    }
    
    public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> list)
    {
        foreach (var element in list)
            set.Add(element);
    }
    public static void RemoveRange<T>(this HashSet<T> set, IEnumerable<T> list)
    {
        foreach (var element in list)
            if (set.Contains(element)) set.Remove(element);
    }
    #endregion
    
    #region Reflection //Heavy operations involving the use of System.Reflection

    /// <summary>
    /// Returns the name of the getter method for the property
    /// </summary>
    /// <param name="T">The class the property originates from</param>
    /// <param name="propertyName">The name of the property</param>
    /// <example><code>GetGetMethodName&lt;MyClass&gt;(nameof(MyClass.MyProperty))</code></example>
    public static string GetGetMethodName<T>(string propertyName)
    {
        var result = typeof(T).GetProperty(propertyName)?.GetGetMethod().Name;
        if (result == null)
            throw new ArgumentNullException($"Property {propertyName} of {typeof(T).Name} returned null!");
        return result;
    }
    /// <summary>
    /// Returns the name of the getter method for the property
    /// </summary>
    /// <param name="T">The class the property originates from</param>
    /// <param name="propertyName">The name of the property</param>
    /// <example><code>GetSetMethodName&lt;MyClass&gt;(nameof(MyClass.MyProperty))</code></example>
    public static string GetSetMethodName<T>(string propertyName)
    {
        var result = typeof(T).GetProperty(propertyName)?.GetGetMethod().Name;
        if (result == null)
            throw new ArgumentNullException($"Property {propertyName} of {typeof(T).Name} returned null!");
        return result;
    }
    //For ILHooking
    public static bool MatchGetterCall<T>(this Instruction i, string methodName)
    {
        return i.MatchCallOrCallvirt<T>(GetGetMethodName<T>(methodName));
    }
    //For ILHooking
    public static bool MatchSetterCall<T>(this Instruction i, string methodName)
    {
        return i.MatchCallOrCallvirt<T>(GetSetMethodName<T>(methodName));
    }

    #endregion
}