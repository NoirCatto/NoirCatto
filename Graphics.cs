using System.Linq;
using MonoMod.Cil;
using RWCustom;
using UnityEngine;

namespace NoirCatto;

public partial class NoirCatto
{
    private void PlayerGraphicsOnUpdate(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);
        if (self.player.SlugCatClass != NoirName) return;
        var noirData = NoirDeets.GetValue(self.player, NoirDataCtor);

        #region Quad crawl on horizontal poles
        var angle = Custom.AimFromOneVectorToAnother(self.player.bodyChunks[0].pos, self.player.bodyChunks[1].pos);
        var lastAngle = Custom.AimFromOneVectorToAnother(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[1].lastPos);
        var a2 = Custom.PerpendicularVector(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos);
        var a3 = Custom.PerpendicularVector(self.player.bodyChunks[0].pos, self.player.bodyChunks[1].pos);
        var flpDirNeg = self.player.flipDirection * -1;
        var dirVec = (self.player.bodyChunks[0].pos - self.player.bodyChunks[1].pos).normalized;
        
        //Adjusting draw positions slightly
        if (noirData.CanCrawlOnBeam())
        {
            if (self.player.input[0].x != 0)
            {
                self.drawPositions[0, 0].y = Mathf.Lerp(self.drawPositions[0, 1].y + 5, self.drawPositions[0, 0].y + 5, 1f);
                self.drawPositions[1, 0].y = Mathf.Lerp(self.drawPositions[1, 1].y + 4, self.drawPositions[1, 0].y + 4, 1f);
                self.drawPositions[1, 0].x = Mathf.Lerp(self.drawPositions[1, 1].x + 2 * flpDirNeg, self.drawPositions[1, 0].x + 2 * flpDirNeg, 1f);
                
                self.head.pos.y = Mathf.Lerp(self.head.lastPos.y + 5f, self.head.pos.y + 5f, 1f);
                self.head.pos.x = Mathf.Lerp(self.head.lastPos.x + 1 * flpDirNeg, self.head.pos.x + 1 * flpDirNeg, 1f);
            }
            else
            {
                self.drawPositions[0, 0].y = Mathf.Lerp(self.drawPositions[0, 1].y + 2, self.drawPositions[0, 0].y + 2, 1f);
                self.drawPositions[1, 0].y = Mathf.Lerp(self.drawPositions[1, 1].y + 3, self.drawPositions[1, 0].y + 3, 1f);
                self.drawPositions[1, 0].x = Mathf.Lerp(self.drawPositions[1, 1].x + 1 * flpDirNeg, self.drawPositions[1, 0].x + 1 * flpDirNeg, 1f);
                
                self.head.pos.y = Mathf.Lerp(self.head.lastPos.y + 2.5f, self.head.pos.y + 2.5f, 1f);
                self.head.pos.x = Mathf.Lerp(self.head.lastPos.x + 1 * flpDirNeg, self.head.pos.x + 1 * flpDirNeg, 1f);
            }
            
            //Forcing bodychunks to rotate so the player is aligned horizontally
            
            switch (angle)
            {
                case > 0 and < 90: //Left Down->Middle
                    if (self.player.flipDirection != -1) break;
                    self.player.bodyChunks[0].vel += a2 * 1;
                    self.player.bodyChunks[1].vel += a3 * 1;
                    break;
                case > 90 and < 180: //Left Up->Middle
                    if (self.player.flipDirection != -1) break;
                    if (self.player.input[0].x != 0)
                    {
                        self.player.bodyChunks[0].vel -= a2 * self.player.bodyChunks[0].vel.y;
                        self.player.bodyChunks[1].vel -= a3 * self.player.bodyChunks[0].vel.y;
                    }
                    else
                    {
                        if (angle > 130)
                        {
                            self.player.bodyChunks[0].vel -= a2 * self.player.bodyChunks[0].vel.y * 0.75f;
                            self.player.bodyChunks[1].vel -= a3 * self.player.bodyChunks[0].vel.y * 0.75f;
                        }
                        else
                        {
                            self.player.bodyChunks[0].vel -= a2;
                            self.player.bodyChunks[1].vel -= a3;
                        }
                    }
                    break;

                case < 0 and > -90: //Right Down->Middle
                    if (self.player.flipDirection != 1) break;
                    self.player.bodyChunks[0].vel -= a2 * 1;
                    self.player.bodyChunks[1].vel -= a3 * 1;
                    break;
                case < -90 and > -180: //Right Up->Middle
                    if (self.player.flipDirection != 1) break;
                    if (self.player.input[0].x != 0)
                    {
                        self.player.bodyChunks[0].vel += a2 * self.player.bodyChunks[0].vel.y;
                        self.player.bodyChunks[1].vel += a3 * self.player.bodyChunks[0].vel.y;
                    }
                    else
                    {
                        if (angle < -130)
                        {
                            self.player.bodyChunks[0].vel += a2 * self.player.bodyChunks[0].vel.y * 0.75f;
                            self.player.bodyChunks[1].vel += a3 * self.player.bodyChunks[0].vel.y * 0.75f;
                        }
                        else
                        {
                            self.player.bodyChunks[0].vel += a2;
                            self.player.bodyChunks[1].vel += a3;
                        }
                    }
                    break;
            }
            
        }
        #endregion
        
        #region Pointing body in right direction after rocket jump init
        if (noirData.jumpInitiated)
        {
            //Debug.Log($"You can be my devil or my {angle}");
            var angelMod = 180 - angle * flpDirNeg;
            var diff = (lastAngle - angle) * flpDirNeg * 0.1f;
            if (diff < 0f) diff = 0f;
            var mod = 0.01f;

            switch (angle)
            {
                case > 0 and < 180: //Left Down->Middle
                    if (self.player.flipDirection != -1) break;
                    self.player.bodyChunks[0].vel += a2 * angelMod * mod * diff;
                    self.player.bodyChunks[1].vel += a3 * angelMod * mod * diff;
                    break;
                case < 0 and > -180: //Right Down->Middle
                    if (self.player.flipDirection != 1) break;
                    self.player.bodyChunks[0].vel -= a2  * angelMod * mod * diff;
                    self.player.bodyChunks[1].vel -= a3  * angelMod * mod * diff;
                    break;
            }
        }
        #endregion
        
        #region Adjusting draw positions for crawling
        if (self.player.bodyMode == Player.BodyModeIndex.Crawl && self.player.animation == Player.AnimationIndex.None)
        {
            if (self.player.input[0].x != 0)
            {
                self.drawPositions[0, 0].y = Mathf.Lerp(self.drawPositions[0, 1].y + 0.5f, self.drawPositions[0, 0].y + 0.5f, 1f); //Closer to head
                self.drawPositions[1, 0].y = Mathf.Lerp(self.drawPositions[1, 1].y + 2.5f, self.drawPositions[1, 0].y + 2.5f, 1f); //Closer to tail

                var diff = self.drawPositions[1, 0].y - self.head.pos.y;
                var lastDiff = self.drawPositions[1, 1].y - self.head.lastPos.y;
                
                self.head.pos.y = Mathf.Lerp(self.head.lastPos.y + 1.5f + lastDiff, self.head.pos.y + 1.5f + diff, 1f);

                for (var i = 0; i < self.tail.Length; i++)
                {
                    self.tail[i].pos.y = Mathf.Lerp(self.tail[i].lastPos.y + 0.75f, self.tail[i].pos.y + 0.75f, 1f);
                    
                    if (self.tail[i].pos.y <= self.head.pos.y + ((i < self.tail.Length / 2) ? 3f : 6f))
                        self.tail[i].vel.y += i * ((i < self.tail.Length / 2) ? (0.72f / self.tail.Length) : (0.8f / self.tail.Length)) * self.player.room.gravity;
                }
               
            }
            else
            {
                self.drawPositions[0, 0].y = Mathf.Lerp(self.drawPositions[0, 1].y + 0, self.drawPositions[0, 0].y + 0, 1f);
                self.drawPositions[1, 0].y = Mathf.Lerp(self.drawPositions[1, 1].y + 1, self.drawPositions[1, 0].y + 1, 1f);

                self.head.pos.y = Mathf.Lerp(self.head.lastPos.y + 0f, self.head.pos.y + 0f, 1f);
                
                for (var i = 0; i < self.tail.Length; i++)
                {
                    self.tail[i].pos.y = Mathf.Lerp(self.tail[i].lastPos.y + 0.25f, self.tail[i].pos.y + 0.25f, 1f);
                    self.tail[i].vel.x *= 0.5f;

                    if (self.tail[i].pos.y <= self.head.pos.y + ((i < self.tail.Length / 2) ? 5f : 8f))
                        self.tail[i].vel.y += i * ((i < self.tail.Length / 2) ? (0.4f / self.tail.Length) : (0.8f / self.tail.Length)) * self.player.room.gravity;
                }
           
            }
        }

        #endregion

        #region Tailtip!
        else if (!self.player.State.dead && self.player.bodyMode != Player.BodyModeIndex.Stunned &&
                 (self.player.bodyChunks[1].contactPoint != new IntVector2(0, 0) || self.player.bodyMode == Player.BodyModeIndex.ClimbingOnBeam) &&
                 self.player.animation != Player.AnimationIndex.Roll && self.player.animation != Player.AnimationIndex.BellySlide &&
                 self.player.bodyMode != Player.BodyModeIndex.Swimming)
        {
            for (var i = 0; i < self.tail.Length; i++)
            {
                self.tail[i].pos.y = Mathf.Lerp(self.tail[i].lastPos.y + 0.25f, self.tail[i].pos.y + 0.25f, 1f);
                self.tail[i].vel.x *= 0.5f;
                
                if (self.tail[i].pos.y <= self.drawPositions[1, 0].y + ((i < self.tail.Length / 2) ? 5f : 10f))
                    self.tail[i].vel.y += i * ((i < self.tail.Length / 2) ? (0.4f / self.tail.Length) : (0.8f / self.tail.Length)) * self.player.room.gravity
                        * (noirData.OnVerticalBeam() && !noirData.OnHorizontalBeam() ? 0.75f : 1f);
            }
        }
        #endregion

        #region Ears
        // var pos = EarAttachPos(noirData, 1f);
        // var earAttachPos = EarAttachPos(noirData, 1f);
        // var num9 = 28f;
        noirData.Ear[0].connectedPoint = EarAttachPos(noirData, 1f);
        for (var index = 0; index < noirData.Ear.Length; ++index)
        {
            noirData.Ear[index].Update();
            //var tailSegment6 = noirData.Ear[index];
            //tailSegment6.vel = tailSegment6.vel + Custom.DirVec(earAttachPos, noirData.Ear[index].pos) * num9 / Vector2.Distance(earAttachPos, noirData.Ear[index].pos);
            //num9 *= 0.5f;
            //earAttachPos = pos;
            //pos = noirData.Ear[index].pos;
        }
        noirData.Ear[0].vel.x *= 0.5f;
        noirData.Ear[0].vel.y += self.player.EffectiveRoomGravity * 0.5f;
        noirData.Ear[1].vel.x *= 0.3f;
        noirData.Ear[1].vel.y += self.player.EffectiveRoomGravity * 0.3f;
        
        //Ear2
        noirData.Ear2[0].connectedPoint = Ear2AttachPos(noirData, 1f);
        for (var index = 0; index < noirData.Ear2.Length; ++index)
        {
            noirData.Ear2[index].Update();
        }
        noirData.Ear2[0].vel.x *= 0.5f;
        noirData.Ear2[0].vel.y += self.player.EffectiveRoomGravity * 0.5f;
        noirData.Ear2[1].vel.x *= 0.3f;
        noirData.Ear2[1].vel.y += self.player.EffectiveRoomGravity * 0.3f;

        if ((self.player.animation == Player.AnimationIndex.None && self.player.input[0].x != 0) || 
            (self.player.animation == Player.AnimationIndex.StandOnBeam && self.player.input[0].x != 0) ||
            self.player.bodyMode == Player.BodyModeIndex.Crawl || 
            self.player.animation != Player.AnimationIndex.None && self.player.animation != Player.AnimationIndex.Flip && !noirData.OnAnyBeam())
        {
            if (self.player.flipDirection == 1)
            {
                noirData.EarFlip = 1;
                noirData.Ear2Flip = -1;
            }
            else
            {
                noirData.EarFlip = -1;
                noirData.Ear2Flip = 1;
            }

            if (self.player.bodyMode == Player.BodyModeIndex.Crawl && self.player.input[0].x == 0)
            {
                if (self.player.flipDirection == 1)
                {
                    noirData.Ear[0].vel.x += 0.45f * flpDirNeg;
                    noirData.Ear[1].vel.x += 0.45f * flpDirNeg;
                    noirData.Ear2[0].vel.x += 0.35f * flpDirNeg;
                    noirData.Ear2[1].vel.x += 0.35f * flpDirNeg;

                    if (self.player.superLaunchJump >= 20)
                    {
                        noirData.Ear[0].vel.x += 0.5f * flpDirNeg;
                        noirData.Ear[1].vel.x += 0.5f * flpDirNeg;
                        noirData.Ear2[0].vel.x += 0.5f * flpDirNeg;
                        noirData.Ear2[1].vel.x += 0.5f * flpDirNeg;
                    }
                }
                else
                {
                    noirData.Ear[0].vel.x += 0.35f * flpDirNeg;
                    noirData.Ear[1].vel.x += 0.35f * flpDirNeg;
                    noirData.Ear2[0].vel.x += 0.45f * flpDirNeg;
                    noirData.Ear2[1].vel.x += 0.45f * flpDirNeg;

                    if (self.player.superLaunchJump >= 20)
                    {
                        noirData.Ear[0].vel.x += 0.5f * flpDirNeg;
                        noirData.Ear[1].vel.x += 0.5f * flpDirNeg;
                        noirData.Ear2[0].vel.x += 0.5f * flpDirNeg;
                        noirData.Ear2[1].vel.x += 0.5f * flpDirNeg;
                    }
                }
            }

        }
        else
        {
            noirData.EarFlip = 1;
            noirData.Ear2Flip = 1;
            
            noirData.Ear[1].vel.x -= 0.5f;
            noirData.Ear2[1].vel.x += 0.5f;
        }
        #endregion
        
        noirData.LastHeadRotation = self.head.connection.Rotation;
    }

    private Vector2 EarAttachPos(NoirData noirData, float timestacker)
    {
        var graphics = (PlayerGraphics)noirData.Cat.graphicsModule;
        return Vector2.Lerp(graphics.head.lastPos + new Vector2(-4f, 1.5f), graphics.head.pos + new Vector2(-4f, 1.5f), timestacker) + Vector3.Slerp(noirData.LastHeadRotation, graphics.head.connection.Rotation, timestacker).ToVector2InPoints() * 15f;
    }
    private Vector2 Ear2AttachPos(NoirData noirData, float timestacker)
    {
        var graphics = (PlayerGraphics)noirData.Cat.graphicsModule;
        return Vector2.Lerp(graphics.head.lastPos + new Vector2(4f, 1.5f), graphics.head.pos + new Vector2(4f, 1.5f), timestacker) + Vector3.Slerp(noirData.LastHeadRotation, graphics.head.connection.Rotation, timestacker).ToVector2InPoints() * 15f;
    }
    
    private void PlayerGraphicsOnDrawSprites2(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        if (self.player.SlugCatClass != NoirName) return;

        var headSpr = sleaser.sprites[3];
        
        if (self.player.bodyMode == Player.BodyModeIndex.Crawl && self.player.animation == Player.AnimationIndex.None)
        {
            if (self.player.input[0].x != 0)
            {
                headSpr.rotation += -10f * self.player.flipDirection;
            }
        }
    }
    
    private bool SlugcatHandOnEngageInMovement(On.SlugcatHand.orig_EngageInMovement orig, SlugcatHand self)
    {
        var player = (Player)self.owner.owner;
        if (player.SlugCatClass != NoirName) return orig(self);
        var noirData = NoirDeets.GetValue(player, NoirDataCtor);
        
        if (noirData.CanCrawlOnBeam()) 
        {
            //Crawl anim code while on beams!
            self.mode = Limb.Mode.HuntAbsolutePosition;
            self.huntSpeed = 12f;
            self.quickness = 0.7f;
            if ((self.limbNumber == 0 || (Mathf.Abs((self.owner as PlayerGraphics).hands[0].pos.x - self.owner.owner.bodyChunks[0].pos.x) < 10f && (self.owner as PlayerGraphics).hands[0].reachedSnapPosition)) && !Custom.DistLess(self.owner.owner.bodyChunks[0].pos, self.absoluteHuntPos, 29f))
            {
                Vector2 absoluteHuntPos = self.absoluteHuntPos;
                self.FindGrip(self.owner.owner.room, self.connection.pos + new Vector2((float)(self.owner.owner as Player).flipDirection * 20f, 0f), self.connection.pos + new Vector2((float)(self.owner.owner as Player).flipDirection * 20f, 0f), 100f, new Vector2(self.owner.owner.bodyChunks[0].pos.x + (float)(self.owner.owner as Player).flipDirection * 28f, self.owner.owner.room.MiddleOfTile(self.owner.owner.bodyChunks[0].pos).y - 10f), 2, 1, false);
                if (self.absoluteHuntPos != absoluteHuntPos)
                {
                }
            }

            return false;
        }
        return orig(self);
    }
    
    private void SpearOnDrawSprites(On.Spear.orig_DrawSprites orig, Spear self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        
        if (self.mode == Weapon.Mode.Carried)
        {
            var pla = self.grabbedBy.Select(x => x.grabber).FirstOrDefault(x => x is Player);
            if (pla == null) return;
            var cat = (Player)pla;
            if (cat.SlugCatClass != NoirName) return;
            var noirData = NoirDeets.GetValue(cat, NoirDataCtor);
            if (!noirData.CanCrawlOnBeam()) return;

            //if (self.abstractSpear.electric) return; //ToDo: Problem child... I'll deal with you later
            for (var i = 0; i < sleaser.sprites.Length; i++)
            {
                sleaser.sprites[i].rotation = 90f * cat.flipDirection;
            }
        }
    }
}