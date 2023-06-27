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

            self.runspeedFac = 0.8f;
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
        orig(self, eu);
    }
    #endregion
    #endregion
    private void PlayerOnUpdateBodyMode(On.Player.orig_UpdateBodyMode orig, Player self)
    {
        orig(self);
        if (self.SlugCatClass != NoirName) return;
        //var noirData = NoirDeets.GetValue(self, NoirDataCtor);
        
        if (self.bodyMode == Player.BodyModeIndex.Crawl)
        {
            //Crawl boost
            self.dynamicRunSpeed[0] *= CrawlSpeedFac;
            self.dynamicRunSpeed[1] *= CrawlSpeedFac;
            
            #region slideCounter
            if (self.slideCounter > 0)
            {
                self.slideCounter++;
                if (self.slideCounter > 20 || self.input[0].x != -self.slideDirection)
                {
                    self.slideCounter = 0;
                }
                var num = -Mathf.Sin(self.slideCounter / 20f * 3.1415927f * 0.5f) + 0.5f;
                var mainBodyChunk2 = self.mainBodyChunk;
                mainBodyChunk2.vel.x = mainBodyChunk2.vel.x + (num * 3.5f * self.slideDirection - self.slideDirection * ((num < 0f) ? 0.8f : 0.5f) * (self.isSlugpup ? 0.25f : 1f));
                var bodyChunk21 = self.bodyChunks[1];
                bodyChunk21.vel.x = bodyChunk21.vel.x + (num * 3.5f * self.slideDirection + self.slideDirection * 0.5f);
                if ((self.slideCounter == 4 || self.slideCounter == 7 || self.slideCounter == 11) && Random.value < Mathf.InverseLerp(0f, 0.5f, self.room.roomSettings.CeilingDrips))
                {
                    self.room.AddObject(new WaterDrip(self.bodyChunks[1].pos + new Vector2(0f, -self.bodyChunks[1].rad + 1f), Custom.DegToVec(self.slideDirection * Mathf.Lerp(30f, 70f, Random.value)) * Mathf.Lerp(6f, 11f, Random.value), false));
                }
            }
            else if (self.input[0].x != 0)
            {
                if (self.input[0].x != self.slideDirection)
                {
                    if (self.initSlideCounter > 10 && self.mainBodyChunk.vel.x > 0f == self.slideDirection > 0 && Mathf.Abs(self.mainBodyChunk.vel.x) > 1f)
                    {
                        self.slideCounter = 1;
                        self.room.PlaySound(SoundID.Slugcat_Skid_On_Ground_Init, self.mainBodyChunk, false, 1f, 0.9f);
                    }
                    else
                    {
                        self.slideDirection = self.input[0].x;
                    }
                    self.initSlideCounter = 0;
                    return;
                }
                if (self.initSlideCounter < 30)
                {
                    self.initSlideCounter++;
                }
            }
            else if (self.initSlideCounter > 0)
            {
                self.initSlideCounter--;
            }
        }
        #endregion
    }
    
    private void PlayerILUpdateBodyMode(ILContext il) //Obsolete, using normal hook instead
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

    private void PlayerOnUpdateAnimation(On.Player.orig_UpdateAnimation orig, Player self)
    {
        orig(self);
        if (self.SlugCatClass != NoirName) return;
        var noirData = NoirDeets.GetValue(self, NoirDataCtor);

        if (self.animation == Player.AnimationIndex.StandOnBeam)
        {
            if (noirData.CanCrawlOnBeam())
            {
                //Boost while crawling on horizontal pole
                // self.dynamicRunSpeed[0] = (2.1f + self.slugcatStats.runspeedFac * 0.5f) * CrawlSpeedFac;
                // self.dynamicRunSpeed[1] = (2.1f + self.slugcatStats.runspeedFac * 0.5f) * CrawlSpeedFac;
                self.dynamicRunSpeed[0] = (self.dynamicRunSpeed[0] + 0.82f) * CrawlSpeedFac; //hardcoded for now, will definitely use runspeedfac here later, mhm
                self.dynamicRunSpeed[1] = (self.dynamicRunSpeed[1] + 0.82f) * CrawlSpeedFac;
            }
        }

        if (self.animation == Player.AnimationIndex.CrawlTurn)
        {
            self.dynamicRunSpeed[0] /= 0.75f; // Game multiplies it by 0.75f
            self.dynamicRunSpeed[1] /= 0.75f;
        }
    }
    
    private void PlayerILUpdateAnimation(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            c.GotoNext(
                i => i.MatchLdsfld<Player.AnimationIndex>("CrawlTurn"),
                i => i.MatchCall(out _),
                i => i.MatchBrfalse(out label)
            );
            c.GotoPrev(MoveType.Before, i => i.MatchLdarg(0));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(CustomCrawlTurn);
            c.Emit(OpCodes.Brtrue, label);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }
    public bool CustomCrawlTurn(Player self)
    {
        if (self.SlugCatClass != NoirName) return false;
        if (self.animation != Player.AnimationIndex.CrawlTurn) return false;
        var noirData = NoirDeets.GetValue(self, NoirDataCtor);

        //If we're jumping, don't proceed with other code, it breaks the jump
        if (self.input[0].jmp)
        {
            //Also reset the anims so they don't break, too
            self.bodyMode = Player.BodyModeIndex.Default;
            self.animation = Player.AnimationIndex.None;
            return true;
        }
        
        //If our input does not match the facing direction of the slug
        if (self.input[0].x != 0 && (self.input[0].x > 0) == (self.bodyChunks[0].pos.x < self.bodyChunks[1].pos.x))
        {
            //Debug.Log($"CAT NOT FACING CORRECT DIRECTION");
            noirData.CrawlTurnCounter++;
            noirData.AfterCrawlTurnCounter = 0;
            
            //Temporarily turning off player's bodychunk push/pull
            self.bodyChunkConnections[0].active = false;
            
            //The back legs drag behind, initiating the turn
            self.bodyChunks[1].vel.x -= noirData.CrawlTurnCounter * 0.5f * self.flipDirection;

            return true;
        }
        
        if (self.input[0].x != 0)
        {
            noirData.AfterCrawlTurnCounter++;
        }
        
        if (noirData.AfterCrawlTurnCounter >= 3 || Custom.Dist(self.bodyChunks[0].pos, self.bodyChunks[1].pos) > 15f)
        {
            //Letting orig run after this
            //Debug.Log($"Resetting cat's anim...");
            self.bodyChunkConnections[0].active = true;
            return false;
        }
        return true;
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
        
        #region Leap changes
        if (!self.standing && self.superLaunchJump >= 20)
        {
            if (self.input[0].y > 0)
            {
                self.input[0].y = 0;
                self.input[0].x = 0;
            }
        }
        
        //Pole leap
        if (!noirData.CanCrawlOnBeam() || !self.input[0].jmp)
        {
            if (noirData.SuperCrawlPounce > 0)
            {
                noirData.SuperCrawlPounce--;
            }
        }

        if (noirData.CanCrawlOnBeam() && self.input[0].x == 0 || (self.input[0].jmp && noirData.SuperCrawlPounce >= 20))
        {
            if (self.input[0].jmp)
            {
                self.input[0].jmp = false;
                self.input[0].x = 0;
                self.input[0].y = 0;
                noirData.Ycounter = 0;
                
                if (noirData.SuperCrawlPounce < 20)
                {
                    noirData.SuperCrawlPounce++;
                }
            }
        }
        
        if (noirData.CanCrawlOnBeam() && noirData.SuperCrawlPounce >= 19 && !noirData.UnchangedInput[0].jmp && noirData.UnchangedInput[1].jmp)
        {
            self.Jump();
        }
        #endregion
        
        #region Stand on beam if dropping from above (ORIG is here)
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
            && !flag && noirData.CanGrabBeam())
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
        
        #region CrawlTurn
        if (self.animation != Player.AnimationIndex.CrawlTurn)
        {
            noirData.CrawlTurnCounter = 0;
            noirData.AfterCrawlTurnCounter = 0;
        }
        
        if (noirData.LastAnimation == Player.AnimationIndex.CrawlTurn && self.animation != Player.AnimationIndex.CrawlTurn)
        {
            self.bodyChunkConnections[0].active = true; //Just to make sure we're not left in an unhinged state
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

    private bool IsStuckOrWedged(Player player)
    {
        return patch_Player.IsStuckOrWedged(player);
    }
    private void PlayerOnJump(On.Player.orig_Jump orig, Player self)
    {
        if (self.SlugCatClass != NoirName || RotundWorld && IsStuckOrWedged(self))
        {
            orig(self);
            return;
        }
        
        var noirData = NoirDeets.GetValue(self, NoirDataCtor);
        noirData.LastJumpFromHorizontalBeam = false;

        //Jump() constants
        var num1 = Mathf.Lerp(1f, 1.15f, self.Adrenaline);
        if (self.grasps[0] != null && self.HeavyCarry(self.grasps[0].grabbed) && !(self.grasps[0].grabbed is Cicada))
            num1 += Mathf.Min(Mathf.Max(0.0f, self.grasps[0].grabbed.TotalMass - 0.2f) * 1.5f, 1.3f);

        var flip = !self.standing && self.slideCounter > 0 && self.slideCounter < 10;
        var longJump = self.superLaunchJump;

        if (self.animation == Player.AnimationIndex.StandOnBeam)
        {
            noirData.LastJumpFromHorizontalBeam = true;
        }
        
        #region Pole pounce
        var forcePounce = false;
        if (noirData.CanCrawlOnBeam() && noirData.SuperCrawlPounce >= 19) //We're checking one input late, hence why it's 19 not 20
        {
            forcePounce = true;
            noirData.SuperCrawlPounce = 0;
            
            var num5 = 9f;
            var num4 = self.bodyChunks[0].pos.x > self.bodyChunks[1].pos.x ? 1 : -1;
            self.simulateHoldJumpButton = 6;
            
            self.bodyMode = Player.BodyModeIndex.Default;
            self.animation = Player.AnimationIndex.None;
            
            self.bodyChunks[0].pos.y += 6f;
            
            if (self.bodyChunks[0].ContactPoint.y == -1)
            {
                self.bodyChunks[0].vel.y += 3f * num1;
            }
            
            self.bodyChunks[1].vel.y += 4f * num1;
            self.jumpBoost = 6f;
            
            if ( self.bodyChunks[0].pos.x > self.bodyChunks[1].pos.x == num4 > 0)
            {
                self.bodyChunks[0].vel.x += num4 * num5 * num1;
                self.bodyChunks[1].vel.x +=num4 * num5 * num1;
                self.room.PlaySound(SoundID.Slugcat_Super_Jump, self.mainBodyChunk, false, 1f, 1f);
            }
            goto nope;
        }
        #endregion

        if ((!self.standing && self.bodyChunks[1].contactPoint.y == 0 && self.animation != Player.AnimationIndex.Roll) || //The run thingy fix
            self.bodyMode == Player.BodyModeIndex.Crawl && 
            (self.animation == Player.AnimationIndex.None || self.animation == Player.AnimationIndex.CrawlTurn) &&
            self.input[0].x != 0 && longJump < 20)
        {
            //Modifier constants
            const float xMod = 1f;
            var yMod = flip ? 10.5f : 9f;
            const float xModPos = 5f;
            const float yModPos = 5f;
            
            //self.simulateHoldJumpButton = 6;
            self.bodyChunks[1].pos += new Vector2(xModPos * self.flipDirection, yModPos);
            self.bodyChunks[0].pos = self.bodyChunks[1].pos + new Vector2(xModPos * self.flipDirection, yModPos);
            self.bodyChunks[1].vel += new Vector2(self.flipDirection * xMod, yMod) * num1;
            self.bodyChunks[0].vel += new Vector2(self.flipDirection * xMod, yMod) * num1;
            
            if (flip)
            {
                self.flipFromSlide = true;
                self.animation = Player.AnimationIndex.Flip;
                self.slideCounter = 0;
            }
            self.room.PlaySound(flip ? SoundID.Slugcat_Flip_Jump : SoundID.Slugcat_Normal_Jump, self.mainBodyChunk, false, 1f, 1f);
            
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
            noirData.Jumping = true;
            noirData.JumpingFromCrawl = true;
            return;
        }

        orig(self); // <-- ORIG here
        nope:
        
        //Pounce changes
        if ((!self.standing && self.animation == Player.AnimationIndex.None && longJump >= 20) || forcePounce)
        {
            if (noirData.UnchangedInput[0].y > 0)
            {
                self.bodyChunks[0].vel += new Vector2(0f, 10.0f);
                self.bodyChunks[1].vel += new Vector2(0f, 4.5f);
                
                if (noirData.UnchangedInput[0].x == 0)
                {
                    self.bodyChunks[0].vel.x *= 0.25f;
                    self.bodyChunks[1].vel.x *= 0.25f;
                }
                else
                {
                    self.bodyChunks[0].vel.x *= 0.75f;
                    self.bodyChunks[1].vel.x *= 0.75f;
                }
            }

            else if (noirData.UnchangedInput[0].x > 0)
            {
                var mod = 1.25f;
                self.bodyChunks[0].vel.x *= mod;
                self.bodyChunks[0].vel.x *= mod;
            }
        }
        

        if (!SetStandingOnUpdate) self.standing = false;
        noirData.Jumping = true;
    }
    

}