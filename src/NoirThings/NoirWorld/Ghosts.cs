using MoreSlugcats;

namespace NoirCatto;

public partial class NoirCatto
{
    public static void SaveStateOnGhostEncounter(On.SaveState.orig_GhostEncounter orig, SaveState self, GhostWorldPresence.GhostID ghost, RainWorld rainWorld)
    {
        if (self.saveStateNumber != Const.NoirName)
        {
            orig(self, ghost, rainWorld);
            return;
        }

        self.deathPersistentSaveData.ghostsTalkedTo[ghost] = 2;
        
        if (self.deathPersistentSaveData.karmaCap < 9)
            self.deathPersistentSaveData.karmaCap++;
        self.deathPersistentSaveData.karma = self.deathPersistentSaveData.karmaCap;
        
        if (ModManager.MSC)
        {
            self.deathPersistentSaveData.winState.UpdateGhostTracker(self, self.deathPersistentSaveData.winState.GetTracker(MoreSlugcatsEnums.EndgameID.Pilgrim, true) as WinState.BoolArrayTracker);
        }
        rainWorld.progression.SaveProgressionAndDeathPersistentDataOfCurrentState(false, false);
    }
}