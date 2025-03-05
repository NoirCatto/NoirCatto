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

        orig(self, malnourished); //Save after orig so malnourished is fetched properly
    }

    public static void StoryGameSessionOnctor(On.StoryGameSession.orig_ctor orig, StoryGameSession self, SlugcatStats.Name savestatenumber, RainWorldGame game)
    {
        orig(self, savestatenumber, game);
        //Load data here
    }
}