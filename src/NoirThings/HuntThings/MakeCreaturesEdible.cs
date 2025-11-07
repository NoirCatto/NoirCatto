using System.Linq;

namespace NoirCatto.HuntThings;

public static class MakeCreaturesEdible
{
    public static void StaticWorldOnInitStaticWorld(On.StaticWorld.orig_InitStaticWorld orig)
    {
        orig();
        StaticWorld.creatureTemplates.First(x => x.type == CreatureTemplate.Type.BigEel).meatPoints = 20;
        StaticWorld.creatureTemplates.First(x => x.type == CreatureTemplate.Type.Deer).meatPoints = 20;
    }
}