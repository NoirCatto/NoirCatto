using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using RainMeadow;

namespace NoirCatto;

public class MeadowHooks
{
    public static void Apply()
    {
        try
        {
            _ = new Hook(
                typeof(OnlinePhysicalObject).GetMethod(nameof(OnlinePhysicalObject.NewFromApo), BindingFlags.Static | BindingFlags.Public),
                typeof(NoirCatto).GetMethod(nameof(OnNewFromApo), BindingFlags.Static | BindingFlags.Public)
            );
        }
        catch (Exception ex)
        {
            NoirCatto.LogSource.LogError(ex);
        }
    }

    public static object OnNewFromApo(Func<AbstractPhysicalObject, object> orig, AbstractPhysicalObject apo) //Using generic 'object' in place of 'OnlinePhysicalObject' as otherwise it prevents the game from loading without Meadow on
    {
        if (apo is NoirCatto.AbstractCatSlash slash)
        {
            OnlineEntity.EntityId entityId = new OnlineEntity.EntityId(OnlineManager.mePlayer.inLobbyId, OnlineEntity.EntityId.IdType.apo, slash.ID.number);
            if (OnlineManager.recentEntities.ContainsKey(entityId))
            {
                RainMeadow.RainMeadow.Error($"entity with repeated ID: {entityId}");
                var origid = slash.ID;
                var newid = slash.world.game.GetNewID();
                newid.spawner = origid.spawner;
                newid.altSeed = origid.RandomSeed;
                slash.ID = newid;
                entityId = new OnlineEntity.EntityId(OnlineManager.mePlayer.inLobbyId, OnlineEntity.EntityId.IdType.apo, slash.ID.number);
                RainMeadow.RainMeadow.Error($"set as: {entityId}");
            }
            
            return new OnlineCatSlash(slash, entityId, OnlineManager.mePlayer, false);
        }

        return orig(apo);
    }
}