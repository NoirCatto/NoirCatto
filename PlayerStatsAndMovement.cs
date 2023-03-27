using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NoirCatto;

public partial class NoirCatto
{
    #region Stats
    private void SlugcatStatsOnctor(On.SlugcatStats.orig_ctor orig, SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
    {
        orig(self, slugcat, malnourished);

        if (slugcat == NoirName)
        {
            self.generalVisibilityBonus = -0.1f;
            self.visualStealthInSneakMode = 0.6f;
            self.loudnessFac = 0.75f;

            self.bodyWeightFac = 0.90f;
            self.throwingSkill = 1;

            self.runspeedFac = 0.8f; //1.3
            self.poleClimbSpeedFac = 1.35f;
            self.corridorClimbSpeedFac = 1.4f;

            self.foodToHibernate = 5;
            self.maxFood = 7;
        }
    }
    private const float CrawlSpeedFac = 2.5f;
    private bool PlayerOnAllowGrabbingBatflys(On.Player.orig_AllowGrabbingBatflys orig, Player self)
    {
        if (self.SlugCatClass == NoirName) return false;
        return orig(self);
    }

    #region Throwing objects
    private void PlayerOnThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
    {
        if (self.SlugCatClass != NoirName)
        {
            orig(self, grasp, eu);
            return;
        }

        var noirData = NoirDeets.GetValue(self, NoirDataCtor);
        
        if ((!Options.AlternativeSlashConditions.Value && (CanSlash(self) && noirData.LastGraspsAnyNull)|| Options.AlternativeSlashConditions.Value && noirData.LastFirstGraspNull) 
            && self.grasps[grasp].grabbed is not Creature)
        {
            return;
        }
        
        Weapon wep = null;
        if (self.grasps[grasp].grabbed is Weapon w)
        {
            wep = w;
        }
        
        orig(self, grasp, eu);

        switch (wep)
        {
            case Spear:
                break;
            case ScavengerBomb:
                wep.firstChunk.vel *= 0.5f;
                break;
            case SingularityBomb:
                wep.firstChunk.vel *= 0.6f;
                break;
            case not null:
                wep.exitThrownModeSpeed = 10f;
                wep.firstChunk.vel *= 0.65f;
                break;
        }
    }
    
    private void PlayerOnThrownSpear(On.Player.orig_ThrownSpear orig, Player self, Spear spear)
    {
        orig(self, spear);
        if (self.SlugCatClass != NoirName) return;
        var noirData = NoirDeets.GetValue(self, NoirDataCtor);

        noirData.SpearThrownAnimation = self.animation;

        spear.exitThrownModeSpeed = 10f;
        spear.spearDamageBonus = 0.4f;
        if (spear.bugSpear)
        {
            spear.firstChunk.vel *= 0.77f;
        }
        else
        {
            spear.firstChunk.vel *= 0.5f;
        }
    }
    
    private void SpearOnUpdate(On.Spear.orig_Update orig, Spear self, bool eu)
    {
        orig(self, eu);
        if (self.thrownBy is Player pl && pl.SlugCatClass == NoirName)
        {
            if (Custom.DistLess(self.thrownPos, self.firstChunk.pos, 75f) ||  NoirDeets.GetValue(pl, NoirDataCtor).SpearThrownAnimation == Player.AnimationIndex.BellySlide)
            {
                self.alwaysStickInWalls = true;
            }
            else
            {
                self.alwaysStickInWalls = false;
            }
        }
    }
    #endregion
    #endregion
    private void PlayerILUpdateBodyMode(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<Player>("bodyMode"),
                i => i.MatchLdsfld<Player.BodyModeIndex>("Crawl")
            );
            c.GotoNext(MoveType.Before, i => i.MatchLdcR4(out _));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Player self) =>
            {
                if (self.SlugCatClass == NoirName)
                    return 2.5 * CrawlSpeedFac; //When crawling
                return 2.5;
            });
            c.Remove();

            c.GotoNext(MoveType.Before, i => i.MatchLdcR4(out _));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Player self) => //Todo: convert to NOT using return.
            {
                if (self.SlugCatClass == NoirName)
                    return 1.5 * CrawlSpeedFac; //When standing up from crawl(?)
                return 1;
            });
            c.Remove();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }

    private void PlayerILUpdateAnimation(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            c.GotoNext(MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchCallOrCallvirt<Player>("get_input"),
                i => i.MatchLdcI4(1),
                i => i.MatchLdelema<Player.InputPackage>(),
                i => i.MatchLdfld<Player.InputPackage>("y"),
                i => i.MatchBrtrue(out _),
                i => i.MatchLdarg(0),
                i => i.MatchLdsfld<Player.AnimationIndex>("None"),
                i => i.MatchStfld<Player>("animation"),
                i => i.MatchBr(out _),
                i => i.MatchLdarg(0),
                i => i.MatchLdsfld<Player.AnimationIndex>("None"),
                i => i.MatchStfld<Player>("animation"),
                i => i.MatchLdarg(0),
                i => i.MatchCallOrCallvirt<Player>("get_input"),
                i => i.MatchLdcI4(0),
                i => i.MatchLdelema<Player.InputPackage>(),
                i => i.MatchLdfld<Player.InputPackage>("x"),
                i => i.MatchBrfalse(out _)
            );
            c.GotoPrev(MoveType.After, i => i.MatchLdarg(0));
            
            c.EmitDelegate((Player self) =>
            {
                if (self.SlugCatClass == NoirName && NoirDeets.GetValue(self, NoirDataCtor).Ycounter < YcounterTreshold)
                {
                    //Boost while standing on horizontal pole
                    self.dynamicRunSpeed[0] = (2.1f + 0.4f) * CrawlSpeedFac + 0f;
                    self.dynamicRunSpeed[1] = (2.1f + 0.4f) * CrawlSpeedFac + 0f;
                }
            });
            c.Emit(OpCodes.Ldarg_0);

            
            //Scug don't hang down from poles #1
            c.GotoNext(MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchCallOrCallvirt<Player>("get_input"),
                i => i.MatchLdcI4(0),
                i => i.MatchLdelema<Player.InputPackage>(),
                i => i.MatchLdfld<Player.InputPackage>("y"),
                i => i.MatchLdcI4(1),
                i => i.MatchBneUn(out label)
            );
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Player self) =>
            {
                if (self.SlugCatClass == NoirName)
                {
                    return true;
                }
                return false;
            });

            c.Emit(OpCodes.Brtrue, label);

        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }

    }
    
    private void PlayerOnMovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu)
    {
        if (self.SlugCatClass != NoirName)
        {
            orig(self, eu);
            return;
        }

        var noirData = NoirDeets.GetValue(self, NoirDataCtor);

        var graphics = (PlayerGraphics)self.graphicsModule;
        
        #region Stand on beam if dropping from above
        //Slugcat will attempt snapping on top of a horizontal beam if dropping from above
        //This is a very rough prototype and could use some ironing out, but works good enough for now
        var anyPartOnVertBeam = (self.room.GetTile(self.room.GetTilePosition(self.bodyChunks[0].pos)).verticalBeam || self.room.GetTile(self.room.GetTilePosition(self.bodyChunks[1].pos)).verticalBeam ||
                             self.room.GetTile(self.room.GetTilePosition(graphics.hands[0].pos)).verticalBeam || self.room.GetTile(self.room.GetTilePosition(graphics.hands[1].pos)).verticalBeam);
        
        var anyPartOnHoriBeam = (self.room.GetTile(self.room.GetTilePosition(self.bodyChunks[0].pos)).horizontalBeam || self.room.GetTile(self.room.GetTilePosition(self.bodyChunks[1].pos)).horizontalBeam ||
                             self.room.GetTile(self.room.GetTilePosition(graphics.hands[0].pos)).horizontalBeam || self.room.GetTile(self.room.GetTilePosition(graphics.hands[1].pos)).horizontalBeam);
        
        if (!anyPartOnVertBeam && 
            (self.animation == Player.AnimationIndex.StandOnBeam || noirData.LastAnimation == Player.AnimationIndex.StandOnBeam || 
             self.animation == Player.AnimationIndex.GetUpOnBeam || noirData.LastAnimation == Player.AnimationIndex.GetUpOnBeam))
        {
            if (self.input[0].y > 0)
            {
                self.input[0].y = 0;
            }
        }

        var flag = false;
        if (!anyPartOnVertBeam && noirData.LastJumpFromHorizontalBeam && 
            self.mainBodyChunk.lastPos.y <= self.mainBodyChunk.pos.y &&
            (anyPartOnHoriBeam || self.animation == Player.AnimationIndex.StandOnBeam || noirData.LastAnimation == Player.AnimationIndex.StandOnBeam) )
        {
            
            for (var i = 0; i < self.input.Length; i++)
            {
                if (self.input[0].jmp || (self.input[i].jmp && !self.input[0].jmp))
                {
                    self.input[0].y = 0;
                    flag = true;
                }
            }
        }

        orig(self, eu);

        if (self.animation == Player.AnimationIndex.HangFromBeam && noirData.LastAnimation != Player.AnimationIndex.HangFromBeam &&
            noirData.LastAnimation != Player.AnimationIndex.StandOnBeam && 
            self.input[0].y > 0 && self.mainBodyChunk.lastPos.y >= self.mainBodyChunk.pos.y
            && !flag)
        {
            // Debug.Log($"tile: {self.room.MiddleOfTile(self.bodyChunks[1].pos).y}");
            // Debug.Log($"pos: {self.bodyChunks[1].pos.y}");
            
            if (self.bodyChunks[1].pos.y < self.bodyChunks[0].pos.y)
            {
                self.bodyChunks[1].pos.y += 7f;
            }
            else
            {
                self.bodyChunks[1].pos.y -= 14f;
                self.bodyChunks[0].pos.y += 7f;
            }
           
            self.bodyChunks[0].vel.y = 0.0f;
            self.bodyChunks[1].vel.y = 0.0f;
            self.animation = Player.AnimationIndex.StandOnBeam;
        }
        #endregion

        #region Standing
        //Set standing to false after crawling a ledge
        if (noirData.LastAnimation == Player.AnimationIndex.LedgeCrawl && self.animation != Player.AnimationIndex.LedgeCrawl)
        {
            if (!SetStandingOnUpdate) self.standing = false;
        }

        if (self.input[0].y > 0 || !noirData.Jumping && self.input[0].y == 0 && noirData.StandCounter is >= 1 and <= 10)
        {
            if (SetStandingOnUpdate) self.standing = true;
        }
        else
        {
            if (SetStandingOnUpdate) self.standing = false;
        }

        if (self.standing)
        {
            if (self.input[0].y == 0 && self.input[0].x != 0) noirData.StandCounter++;
            else noirData.StandCounter = 1;
        }
        else if (!self.standing)
        {
            noirData.StandCounter = 0;
        }
        #endregion
        
        CustomCombatUpdate(self, eu);
        noirData.LastAnimation = self.animation;
        noirData.LastBodyMode = self.bodyMode;
        noirData.LastGraspsNull = self.grasps.All(x => x is null);
        noirData.LastGraspsAnyNull = self.grasps.Any(x => x is null);
        noirData.LastFirstGraspNull = self.grasps[0] == null;
    }
    
    private void PlayerILMovementUpdate(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            c.GotoNext(MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<Player>("wantToGrab"),
                i => i.MatchLdcI4(0),
                i => i.MatchBle(out label),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<Player>("noGrabCounter"),
                i => i.MatchBrtrue(out _),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<Player>("bodyMode"),
                i => i.MatchLdsfld<Player.BodyModeIndex>("Default"),
                _ => true,
                i => i.MatchBrtrue(out _)
            );

            //Scug don't hang down from poles #2
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Player self) =>
            {
                if (self.SlugCatClass == NoirName && self.animation == Player.AnimationIndex.StandOnBeam)
                {
                    return true;
                }
                return false;
            });
            c.Emit(OpCodes.Brtrue, label);

        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }
    
    private void PlayerOnJump(On.Player.orig_Jump orig, Player self)
    {
        if (self.SlugCatClass != NoirName)
        {
            orig(self);
            return;
        }
        
        var noirData = NoirDeets.GetValue(self, NoirDataCtor);
        
        noirData.LastJumpFromHorizontalBeam = false;

        if (self.bodyMode == Player.BodyModeIndex.Crawl && self.animation == Player.AnimationIndex.None &&
            self.input[0].x != 0 && self.superLaunchJump < 20)
        {
            //Jump() constants
            var num1 = Mathf.Lerp(1f, 1.15f, self.Adrenaline);
            if (self.grasps[0] != null && self.HeavyCarry(self.grasps[0].grabbed) && !(self.grasps[0].grabbed is Cicada))
                num1 += Mathf.Min(Mathf.Max(0.0f, self.grasps[0].grabbed.TotalMass - 0.2f) * 1.5f, 1.3f);
            
            //Modifier constants
            const float xMod = 1f;
            const float yMod = 8.5f;
            const float xModPos = 5f;
            const float yModPos = 5f;
            
            //self.simulateHoldJumpButton = 6;
            //self.jumpBoost = 6f;
            self.bodyChunks[1].pos += new Vector2(xModPos * self.flipDirection, yModPos);
            self.bodyChunks[0].pos = self.bodyChunks[1].pos + new Vector2(xModPos * self.flipDirection, yModPos);
            self.bodyChunks[1].vel += new Vector2(self.flipDirection * xMod, yMod) * num1;
            self.bodyChunks[0].vel += new Vector2(self.flipDirection * xMod, yMod) * num1;
            self.room.PlaySound(SoundID.Slugcat_Normal_Jump, self.mainBodyChunk, false, 1f, 1f);
            
            //--
            // if (self.bodyChunks[1].onSlope == 0)
            //     return;
            // if (num4 == -self.bodyChunks[1].onSlope)
            // {
            //     self.bodyChunks[1].vel.x += (float) self.bodyChunks[1].onSlope * 8f * num1;
            // }
            // else
            // {
            //     self.bodyChunks[0].vel.x += (float) self.bodyChunks[1].onSlope * 1.8f * num1;
            //     self.bodyChunks[1].vel.x += (float) self.bodyChunks[1].onSlope * 1.2f * num1;
            // }

            
            //self.standing = false;
            //self.jumpBoost = 8f;
            noirData.Jumping = true;
            noirData.JumpingFromCrawl = true;
            return;
        }

        if (self.animation == Player.AnimationIndex.StandOnBeam)
        {
            noirData.LastJumpFromHorizontalBeam = true;
        }
        
        orig(self);
        if (!SetStandingOnUpdate) self.standing = false;
        noirData.Jumping = true;
    }
    

}