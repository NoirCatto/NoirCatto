using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using RWCustom;
using BepInEx;
using BepInEx.Logging;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using SlugBase;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using Debug = UnityEngine.Debug;
#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace NoirCatto;

[BepInPlugin("NoirCatto.NoirCatto", "NoirCatto", "1.0.0")]
public partial class NoirCatto : BaseUnityPlugin
{
    public static NoirCattoOptions Options;
    public RainWorld RwInstance;

    public NoirCatto()
    {
        try
        {
            Options = new NoirCattoOptions(this, Logger);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }
    
    private void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
    }

    private bool IsInit;
    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            if (IsInit) return;

            RwInstance = self;
            LoadAtlases();
            LoadSounds();

            On.RegionGate.customOEGateRequirements += RegionGateOncustomOEGateRequirements;
            On.SaveState.setDenPosition += SaveStateOnsetDenPosition;
            On.RainWorldGame.ctor += RainWorldGameOnctor;
            On.RainWorld.Update += RainWorldOnUpdate;
            
            IL.GhostWorldPresence.SpawnGhost += GhostWorldPresenceILSpawnGhost; //Fix echoes not spawning for slugs with karma cap below 4
            IL.Menu.KarmaLadderScreen.GetDataFromGame += KarmaLadderScreenILGetDataFromGame; //Fix MSC breaking echoes upgrade screen for slugs with karma cap below 4

            On.Menu.Menu.Update += MenuOnUpdate;
            On.Menu.Menu.CommunicateWithUpcomingProcess += MenuOnCommunicateWithUpcomingProcess;
            
            On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConversationOnAddEvents;
            IL.SSOracleBehavior.Update += SSOracleBehaviorOnUpdate;
            IL.SSOracleBehavior.SeePlayer += SSOracleBehaviorILSeePlayer;
            
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += MoonConversationOnAddEvents;
            
            On.Player.ctor += PlayerOnctor;
            On.Player.Update += PlayerOnUpdate;
            On.Player.GrabUpdate += PlayerOnGrabUpdate;
            On.Player.AllowGrabbingBatflys += PlayerOnAllowGrabbingBatflys;
            On.Player.PickupCandidate += PlayerOnPickupCandidate;
            On.SlugcatStats.ctor += SlugcatStatsOnctor;
            //IL.Player.UpdateBodyMode += PlayerILUpdateBodyMode;
            On.Player.UpdateBodyMode += PlayerOnUpdateBodyMode;
            //IL.Player.UpdateAnimation += PlayerILUpdateAnimation;
            On.Player.UpdateAnimation += PlayerOnUpdateAnimation;
            On.Player.MovementUpdate += PlayerOnMovementUpdate;
            //IL.Player.MovementUpdate += PlayerILMovementUpdate; //Breaks grabbing vertical poles, using a hacky normal hook instead
            On.Player.Jump += PlayerOnJump;
            On.Player.checkInput += PlayerOncheckInput;
            On.PlayerGraphics.ctor += PlayerGraphicsOnctor;
            On.PlayerGraphics.InitiateSprites += PlayerGraphicsOnInitiateSprites;
            On.PlayerGraphics.Update += PlayerGraphicsOnUpdate;
            IL.PlayerGraphics.Update += PlayerGraphicsILUpdate;
            On.PlayerGraphics.DrawSprites += PlayerGraphicsOnDrawSprites2;
            On.PlayerGraphics.DrawSprites += PlayerGraphicsOnDrawSprites;
            On.PlayerGraphics.AddToContainer += PlayerGraphicsOnAddToContainer;
            On.PlayerGraphics.ApplyPalette += PlayerGraphicsOnApplyPalette;
            On.PlayerGraphics.Reset += PlayerGraphicsOnReset;
            On.SlugcatHand.EngageInMovement += SlugcatHandOnEngageInMovement;
            On.Player.ThrownSpear += PlayerOnThrownSpear;
            On.Player.ThrowObject += PlayerOnThrowObject;
            IL.Spear.Update += SpearILUpdate;
            On.Spear.Update += SpearOnUpdate;
            On.Spear.DrawSprites += SpearOnDrawSprites;

            On.AbstractPhysicalObject.Realize += AbstractPhysicalObjectOnRealize;

            On.RainWorldGame.ShutDownProcess += RainWorldGameOnShutDownProcess;
            On.GameSession.ctor += GameSessionOnctor;
            
            MachineConnector.SetRegisteredOI("NoirCatto.NoirCatto", Options);
            IsInit = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }

    private void RainWorldOnUpdate(On.RainWorld.orig_Update orig, RainWorld self)
    {
        orig(self);

        if (self.processManager.currentMainLoop is not RainWorldGame game) return;
        if (game.GamePaused) return;
        foreach (var absPlayer in game.Players)
        {
            var player = (Player)absPlayer.realizedCreature;
            if (player == null) continue;
            if (player.SlugCatClass != NoirName) continue;
            var noirData = NoirDeets.GetValue(player, NoirDataCtor);
            noirData.Update60FPS();
        }
    }

    private void RainWorldGameOnShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
    {
        orig(self);
        ClearMemory();
    }
    private void GameSessionOnctor(On.GameSession.orig_ctor orig, GameSession self, RainWorldGame game)
    {
        orig(self, game);
        ClearMemory();
    }

    #region Helpers
    private void ClearMemory()
    {
        
    }
    #endregion
}
