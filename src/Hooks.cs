namespace NoirCatto;
using static NoirCatto;

public static class Hooks
{
    public static void Apply()
    {
        On.SlugcatStats.ctor += SlugcatStatsOnctor;
        On.Player.AllowGrabbingBatflys += PlayerOnAllowGrabbingBatflys;
    }
}