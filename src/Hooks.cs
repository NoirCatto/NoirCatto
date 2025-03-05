namespace NoirCatto;
using static NoirCatto;

public static class Hooks
{
    public static void Apply()
    {
        On.SlugcatStats.ctor += SlugcatStatsOnctor;
        On.Player.AllowGrabbingBatflys += PlayerOnAllowGrabbingBatflys;

        On.Player.Update += PlayerOnUpdate;
        On.Player.checkInput += PlayerOncheckInput;
        On.Player.UpdateBodyMode += PlayerOnUpdateBodyMode;
        On.Player.UpdateAnimation += PlayerOnUpdateAnimation;
        On.Player.MovementUpdate += PlayerOnMovementUpdate;
        On.Player.Jump += PlayerOnJump;

        On.PlayerGraphics.ctor += PlayerGraphicsOnctor;
        On.PlayerGraphics.InitiateSprites += PlayerGraphicsOnInitiateSprites;
        On.PlayerGraphics.AddToContainer += PlayerGraphicsOnAddToContainer;
        On.PlayerGraphics.DrawSprites += PlayerGraphicsOnDrawSprites;
        On.PlayerGraphics.ApplyPalette += PlayerGraphicsOnApplyPalette;
        On.PlayerGraphics.Reset += PlayerGraphicsOnReset;

        On.PlayerGraphics.Update += PlayerGraphicsOnUpdate;
        On.SlugcatHand.EngageInMovement += SlugcatHandOnEngageInMovement;
        On.Player.GraphicsModuleUpdated += PlayerOnGraphicsModuleUpdated;

        //Jolly menu
        On.PlayerGraphics.JollyFaceColorMenu += PlayerGraphicsOnJollyFaceColorMenu;
        On.PlayerGraphics.JollyUniqueColorMenu += PlayerGraphicsOnJollyUniqueColorMenu;
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.HasUniqueSprite += SymbolButtonTogglePupButtonOnHasUniqueSprite;
        On.JollyCoop.JollyMenu.JollyPlayerSelector.GetPupButtonOffName += JollyPlayerSelectorOnGetPupButtonOffName;

        ApplyIL();
    }

    private static void ApplyIL()
    {
        IL.Player.UpdateAnimation += PlayerILUpdateAnimation;
    }

   
}