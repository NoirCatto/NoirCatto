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
        On.Player.ThrowObject += PlayerOnThrowObject;
        On.Player.ThrownSpear += PlayerOnThrownSpear;

        On.Player.PickupCandidate += PlayerOnPickupCandidate;
        On.Player.GrabUpdate += PlayerOnGrabUpdate;

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

        On.Menu.Menu.Update += MenuOnUpdate;
        On.Menu.Menu.CommunicateWithUpcomingProcess += MenuOnCommunicateWithUpcomingProcess;

        On.RainWorld.Update += RainWorldOnUpdate;
        On.SaveState.setDenPosition += SaveStateOnsetDenPosition;
        On.RainWorldGame.ctor += RainWorldGameOnctor;
        On.RegionGate.customOEGateRequirements += RegionGateOncustomOEGateRequirements;

        On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConversationOnAddEvents;
        On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += MoonConversationOnAddEvents;

        On.AbstractPhysicalObject.Realize += AbstractObjectType.AbstractPhysicalObjectOnRealize;
        On.AbstractPhysicalObject.Abstractize += AbstractPhysicalObjectOnAbstractize;
        On.SeedCob.PlaceInRoom += SeedCobOnPlaceInRoom;
        On.Spear.Update += SpearOnUpdate;

        ApplyIL();
    }

    private static void ApplyIL()
    {
        IL.Player.UpdateAnimation += PlayerILUpdateAnimation;
        IL.SeedCob.Update += SeedCobILUpdate;
        IL.Weapon.Update += WeaponILUpdate;
        IL.SharedPhysics.TraceProjectileAgainstBodyChunks += SharedPhysicsILTraceProjectileAgainstBodyChunks;
        IL.Spear.Update += Fixes.SpearILUpdate;

        IL.SSOracleBehavior.Update += SSOracleBehaviorOnUpdate;
        IL.SSOracleBehavior.SeePlayer += SSOracleBehaviorILSeePlayer;
    }

   
}