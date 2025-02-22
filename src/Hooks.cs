using UnityEngine;

namespace NoirCatto;
using static NoirCatto;

public static class Hooks
{
    public static void Apply()
    {
        On.SlugcatStats.ctor += SlugcatStatsOnctor;
        On.Player.AllowGrabbingBatflys += PlayerOnAllowGrabbingBatflys;

        On.Player.Update += PlayerOnUpdate;

        On.PlayerGraphics.ctor += PlayerGraphicsOnctor;
        On.PlayerGraphics.InitiateSprites += PlayerGraphicsOnInitiateSprites;
        On.PlayerGraphics.AddToContainer += PlayerGraphicsOnAddToContainer;
        On.PlayerGraphics.DrawSprites += PlayerGraphicsOnDrawSprites;
        On.PlayerGraphics.ApplyPalette += PlayerGraphicsOnApplyPalette;
        On.PlayerGraphics.Reset += PlayerGraphicsOnReset;

        On.PlayerGraphics.Update += PlayerGraphicsOnUpdate;
        On.SlugcatHand.EngageInMovement += SlugcatHandOnEngageInMovement;
        On.Player.GraphicsModuleUpdated += PlayerOnGraphicsModuleUpdated;
    }
}