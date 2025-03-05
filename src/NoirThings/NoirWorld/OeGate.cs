namespace NoirCatto;

public partial class NoirCatto
{
    public static bool RegionGateOncustomOEGateRequirements(On.RegionGate.orig_customOEGateRequirements orig, RegionGate self)
    {
        var result = orig(self);
        if (!ModManager.MSC) return result;
        if (self.room.game.session is not StoryGameSession session) return result;
        return session.saveStateNumber == Const.NoirName || result;
    }
}