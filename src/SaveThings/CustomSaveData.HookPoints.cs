using NoirCatto.HuntThings;

namespace NoirCatto.SaveThings;

public partial class CustomSaveData
{
    public static void RainWorldGameOnWin(On.RainWorldGame.orig_Win orig, RainWorldGame self, bool malnourished)
    {
        if (self.manager.upcomingProcess != null)
        {
            orig(self, malnourished);
            return;
        }

        var flag = false;
        if (!self.rainWorld.ExpeditionMode && self.StoryCharacter == Const.NoirName && HuntQuestThings.Master != null)
        {
            if (HuntQuestThings.Master.NextRewardPhase == HuntQuestThings.RewardPhase.IncreaseKarmaCap)
                self.GetStorySession.saveState.deathPersistentSaveData.karmaCap++;
            flag = true;
        }
        orig(self, malnourished); //Save after orig so malnourished is fetched properly
        if (flag) HuntQuestThings.Master.SaveQuestProgress();
    }

    public static void StoryGameSessionOnctor(On.StoryGameSession.orig_ctor orig, StoryGameSession self, SlugcatStats.Name savestatenumber, RainWorldGame game)
    {
        orig(self, savestatenumber, game);

        if (!game.rainWorld.ExpeditionMode && savestatenumber == Const.NoirName)
        {
            HuntQuestThings.Master ??= new HuntQuestThings.HuntQuestMaster();
            HuntQuestThings.Master.StorySession = self;
            HuntQuestThings.Master.LoadOrCreateQuests();
        }
    }
}