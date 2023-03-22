using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using UnityEngine;

namespace NoirCatto;

public partial class NoirCatto
{
    private void LoadAtlases()
    {
        Futile.atlasManager.LoadAtlas("atlases/NoirHead");
        Futile.atlasManager.LoadAtlas("atlases/NoirFace");
        Futile.atlasManager.LoadAtlas("atlases/NoirEars");
        Futile.atlasManager.LoadAtlas("atlases/NoirBodyA");
        Futile.atlasManager.LoadAtlas("atlases/NoirHipsA");
        Futile.atlasManager.LoadAtlas("atlases/NoirLegs");
        Futile.atlasManager.LoadAtlas("atlases/NoirPlayerArm");
        Futile.atlasManager.LoadAtlas("atlases/NoirTail");
    }

    #region Consts
    private const string NoirHead = "NoirHead";
    private const string NoirEars = "NoirEars";
    private const string NoirFace = "NoirFace";
    private const string NoirBodyA = "NoirBodyA";
    private const string NoirPlayerArm = "NoirPlayerArm";
    private const string NoirHipsA = "NoirHipsA";
    private const string NoirLegs = "NoirLegs";
    private const string NoirTail = "NoirTail";
    private const string Noir = "Noir"; //Prefix for sprite replacement

    private const int HeadSpr = 3;
    private const int FaceSpr = 9;
    private const int BodySpr = 0;
    private const int ArmSpr = 5;
    private const int ArmSpr2 = 6;
    private const int OTOTArmSpr = 7;
    private const int OTOTArmSpr2 = 8;
    private const int HipsSpr = 1;
    private const int LegsSpr = 4;
    private const int TailSpr = 2;
    
    private const int TailLength = 6;
    private readonly Color NoirWhite = new Color(0.695f, 0.695f, 0.695f);
    #endregion

    private List<int> SprToReplace = new List<int>()
    {
        HeadSpr, FaceSpr, BodySpr, ArmSpr, ArmSpr2, HipsSpr, LegsSpr
    };

    private const int NewSprites = 2;
    private static int TotalSprites;
    private static int EarSpr;
    private static int EarSpr2;

    private void ReplaceSprites(RoomCamera.SpriteLeaser sleaser)
    {
        foreach (var num in SprToReplace)
        {
            if (!sleaser.sprites[num].element.name.StartsWith(Noir))
            {
                sleaser.sprites[num].element = Futile.atlasManager.GetElementWithName(Noir + sleaser.sprites[num].element.name);
            }
        }
    }
    
    private void PlayerGraphicsOnctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (self.player.SlugCatClass != NoirName) return;
        var noirData = NoirDeets.GetValue(self.player, NoirDataCtor);

        noirData.Ear = new TailSegment[2];
        noirData.Ear[0] = new TailSegment(self, 4.5f, 4f,  null, 0.85f, 1f, 1f, true);
        noirData.Ear[1] = new TailSegment(self, 1.5f, 7f, noirData.Ear[0], 0.85f, 1f, 0.5f, true);
        noirData.Ear2 = new TailSegment[2];
        noirData.Ear2[0] = new TailSegment(self, 4.5f, 4f,  null, 0.85f, 1f, 1f, true);
        noirData.Ear2[1] = new TailSegment(self, 1.5f, 7f, noirData.Ear2[0], 0.85f, 1f, 0.5f, true);
        
        #region Tail
        var tailThickness = 1f;
        var tailRoundness = 0.25f;
        self.tail = new TailSegment[TailLength];
        for (var i = 0; i < self.tail.Length; i++)
        {
            self.tail[i] = new TailSegment(self, Mathf.Lerp(6f, 1f, Mathf.Pow((i + 1) / (float)TailLength, tailThickness)) * (1f + Mathf.Sin(i / (float)TailLength * 3.1415927f) * tailRoundness) *
                                            1.25f /*BodyThickness*/, ((i == 0) ? 4 : 7) * (self.player.playerState.isPup ? 0.5f : 1f),
                (i > 0) ? self.tail[i - 1] : null, 0.85f, 1f, (i == 0) ? 1f : 0.5f, true);
        }

        var origBodyParts = self.bodyParts.Where(x => x is not TailSegment).ToList();
        var partsToAdd = self.tail.Cast<BodyPart>().ToList();
        #endregion
        
        partsToAdd.AddRange(origBodyParts);
        partsToAdd.AddRange(noirData.Ear);
        partsToAdd.AddRange(noirData.Ear2);
        self.bodyParts = partsToAdd.ToArray();
    }
    
    private void PlayerGraphicsOnInitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam)
    {
        if (self.player.SlugCatClass != NoirName)
        {
            orig(self, sleaser, rcam);
            return;
        }
        var noirData = NoirDeets.GetValue(self.player, NoirDataCtor);
        

        if (!self.owner.room.game.DEBUGMODE)
        {
            noirData.CallingAddToContainerFromOrigInitiateSprites = true;
        }
        
        orig(self, sleaser, rcam);

        if (!self.owner.room.game.DEBUGMODE)
        {
            TotalSprites = sleaser.sprites.Length;
            EarSpr = TotalSprites;
            EarSpr2 = TotalSprites + 1;
            Array.Resize(ref sleaser.sprites, TotalSprites + NewSprites);
            
            var array = new TriangleMesh.Triangle[(self.tail.Length - 1) * 4 + 1];
            for (int i = 0; i < self.tail.Length - 1; i++)
            {
                int num = i * 4;
                for (int j = 0; j < 4; j++)
                {
                    array[num + j] = new TriangleMesh.Triangle(num + j, num + j + 1, num + j + 2);
                }
            }
            array[(self.tail.Length - 1) * 4] = new TriangleMesh.Triangle((self.tail.Length - 1) * 4, (self.tail.Length - 1) * 4 + 1, (self.tail.Length - 1) * 4 + 2);
            sleaser.sprites[TailSpr] = new TriangleMesh("Futile_White", array, false, false);
        
            
            var earArray = new TriangleMesh.Triangle[(noirData.Ear.Length - 1) * 4 + 1];
            for (var i = 0; i < noirData.Ear.Length - 1; i++)
            {
                var indexTimesFour = i * 4;
                for (var j = 0; j <= 3; j++)
                {
                    earArray[indexTimesFour + j] = new TriangleMesh.Triangle(indexTimesFour + j, indexTimesFour + j + 1, indexTimesFour + j + 2);
                }
            }
            earArray[(noirData.Ear.Length - 1) * 4] = new TriangleMesh.Triangle((noirData.Ear.Length - 1) * 4, (noirData.Ear.Length - 1) * 4 + 1, (noirData.Ear.Length - 1) * 4 + 2);
            sleaser.sprites[EarSpr] = new TriangleMesh("Futile_White", earArray, false, false);
            var triMesh = new TriangleMesh("Futile_White", earArray, false, false);
            
            sleaser.sprites[EarSpr2] = triMesh;

            ReplaceSprites(sleaser);

            noirData.CallingAddToContainerFromOrigInitiateSprites = false;
            self.AddToContainer(sleaser, rcam, null);
        }
    }
    
    private void PlayerGraphicsOnAddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, FContainer newcontatiner)
    {
        if (self.player.SlugCatClass != NoirName)
        {
            orig(self, sleaser, rcam, newcontatiner);
            return;
        }
        var noirData = NoirDeets.GetValue(self.player, NoirDataCtor);
        
        if (noirData.CallingAddToContainerFromOrigInitiateSprites) return;

        orig(self, sleaser, rcam, newcontatiner);

        if (!self.owner.room.game.DEBUGMODE && self.player.SlugCatClass == NoirName)
        {
            var container = rcam.ReturnFContainer("Midground");
            container.AddChild(sleaser.sprites[EarSpr]);
            container.AddChild(sleaser.sprites[EarSpr2]);
            
            sleaser.sprites[ArmSpr].MoveBehindOtherNode(sleaser.sprites[HeadSpr]);
            sleaser.sprites[ArmSpr2].MoveBehindOtherNode(sleaser.sprites[HeadSpr]);
            
            sleaser.sprites[EarSpr].MoveInFrontOfOtherNode(sleaser.sprites[HeadSpr]);
            sleaser.sprites[EarSpr2].MoveInFrontOfOtherNode(sleaser.sprites[HeadSpr]);
        }
    }
    
    private void PlayerGraphicsOnDrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);

        if (!rcam.room.game.DEBUGMODE && self.player.SlugCatClass == NoirName)
        {
            var noirData = NoirDeets.GetValue(self.player, NoirDataCtor);
            
            ReplaceSprites(sleaser);
            sleaser.sprites[FaceSpr].color = Color.white;
            
            var drawPosChunk0 = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timestacker);
            var drawPosChunk1 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timestacker);
            
            #region Re-Moving the tail accordingly with new length
            var tailAttachPos = (drawPosChunk1 * 3f + drawPosChunk0) / 4f;
            var malnourishedMod = (float)(1.0 - 0.20000000298023224 * self.malnourished);
            var rad = self.tail[0].rad;
            for (var index = 0; index < self.tail.Length; ++index)
            {
                var tailPos = Vector2.Lerp(self.tail[index].lastPos, self.tail[index].pos, timestacker);
                var normalized = (tailPos - tailAttachPos).normalized;
                var vector2_3 = Custom.PerpendicularVector(normalized);
                var distance = Vector2.Distance(tailPos, tailAttachPos) / 5f;
                if (index == 0)
                    distance = 0.0f;
                (sleaser.sprites[TailSpr] as TriangleMesh).MoveVertice(index * 4, tailAttachPos - vector2_3 * rad * malnourishedMod + normalized * distance - campos);
                (sleaser.sprites[TailSpr] as TriangleMesh).MoveVertice(index * 4 + 1, tailAttachPos + vector2_3 * rad * malnourishedMod + normalized * distance - campos);
                if (index < self.tail.Length - 1)
                {
                    (sleaser.sprites[TailSpr] as TriangleMesh).MoveVertice(index * 4 + 2, tailPos - vector2_3 * self.tail[index].StretchedRad * malnourishedMod - normalized * distance - campos);
                    (sleaser.sprites[TailSpr] as TriangleMesh).MoveVertice(index * 4 + 3, tailPos + vector2_3 * self.tail[index].StretchedRad * malnourishedMod - normalized * distance - campos);
                }
                else
                    (sleaser.sprites[TailSpr] as TriangleMesh).MoveVertice(index * 4 + 2, tailPos - campos);
                rad = self.tail[index].StretchedRad;
                tailAttachPos = tailPos;
            }
            #endregion
            
            #region Moving Ears
            var earAttachPos = EarAttachPos(noirData, timestacker);
            var earRad = noirData.Ear[0].rad;
            for (var index = 0; index < noirData.Ear.Length; ++index)
            {
                var earMesh = (TriangleMesh)sleaser.sprites[EarSpr];
                
                var earPos = Vector2.Lerp(noirData.Ear[index].lastPos, noirData.Ear[index].pos, timestacker);
                var normalized = (earPos - earAttachPos).normalized;
                var vector2_3 = Custom.PerpendicularVector(normalized);
                var distance = Vector2.Distance(earPos, earAttachPos) / 5f;
                if (index == 0) distance = 0.0f;
                earMesh.MoveVertice(index * 4, earAttachPos - noirData.EarFlip * vector2_3 * earRad + normalized * distance - campos);
                earMesh.MoveVertice(index * 4 + 1, earAttachPos + noirData.EarFlip * vector2_3 * earRad + normalized * distance - campos);
                if (index < noirData.Ear.Length - 1)
                {
                    earMesh.MoveVertice(index * 4 + 2, earPos - noirData.EarFlip * vector2_3 * noirData.Ear[index].StretchedRad - normalized * distance - campos);
                    earMesh.MoveVertice(index * 4 + 3, earPos + noirData.EarFlip * vector2_3 * noirData.Ear[index].StretchedRad - normalized * distance - campos);
                }
                else
                    earMesh.MoveVertice(index * 4 + 2, earPos - campos);
                earRad = noirData.Ear[index].StretchedRad;
                earAttachPos = earPos;
            }

            var ear2Mesh = (TriangleMesh)sleaser.sprites[EarSpr2];
            var ear2AttachPos = Ear2AttachPos(noirData, timestacker);
            var ear2Rad = noirData.Ear2[0].rad;
            for (var index = 0; index < noirData.Ear2.Length; ++index)
            {
                
                
                var ear2Pos = Vector2.Lerp(noirData.Ear2[index].lastPos, noirData.Ear2[index].pos, timestacker);
                var normalized = (ear2Pos - ear2AttachPos).normalized;
                var vector2_3 = Custom.PerpendicularVector(normalized);
                var distance = Vector2.Distance(ear2Pos, ear2AttachPos) / 5f;
                if (index == 0) distance = 0.0f;
                ear2Mesh.MoveVertice(index * 4, ear2AttachPos + noirData.Ear2Flip * vector2_3 * ear2Rad + normalized * distance - campos);
                ear2Mesh.MoveVertice(index * 4 + 1, ear2AttachPos - noirData.Ear2Flip * vector2_3 * ear2Rad + normalized * distance - campos);
                if (index < noirData.Ear2.Length - 1)
                {
                    ear2Mesh.MoveVertice(index * 4 + 2, ear2Pos + noirData.Ear2Flip * vector2_3 * noirData.Ear2[index].StretchedRad - normalized * distance - campos);
                    ear2Mesh.MoveVertice(index * 4 + 3, ear2Pos - noirData.Ear2Flip * vector2_3 * noirData.Ear2[index].StretchedRad - normalized * distance - campos);
                }
                else
                    ear2Mesh.MoveVertice(index * 4 + 2, ear2Pos - campos);
                ear2Rad = noirData.Ear2[index].StretchedRad;
                ear2AttachPos = ear2Pos;
            }
            #endregion
            
            #region Moving Sprites to front/back
            if (self.player.flipDirection == 1)
            {
                sleaser.sprites[ArmSpr].MoveBehindOtherNode(sleaser.sprites[HeadSpr]);
                sleaser.sprites[ArmSpr2].MoveBehindOtherNode(sleaser.sprites[BodySpr]);
                
                sleaser.sprites[EarSpr].MoveInFrontOfOtherNode(sleaser.sprites[HeadSpr]);
                sleaser.sprites[EarSpr2].MoveBehindOtherNode(sleaser.sprites[BodySpr]);
            }
            else
            {
                sleaser.sprites[ArmSpr].MoveBehindOtherNode(sleaser.sprites[BodySpr]);
                sleaser.sprites[ArmSpr2].MoveBehindOtherNode(sleaser.sprites[HeadSpr]);
                
                sleaser.sprites[EarSpr].MoveBehindOtherNode(sleaser.sprites[BodySpr]);
                sleaser.sprites[EarSpr2].MoveInFrontOfOtherNode(sleaser.sprites[HeadSpr]);
            }
            ((TriangleMesh)sleaser.sprites[EarSpr]).Refresh();
            ((TriangleMesh)sleaser.sprites[EarSpr2]).Refresh();
            #endregion
            
        }
    }

    private Color ReColor(Color color, bool useNoirWhite)
    {
        color = Color.Lerp(color, Color.white, 0.5f);
        if (useNoirWhite)
        {
            color = Color.Lerp(color, NoirWhite, 0.5f);
        }
        return color;
    }
    private void PlayerGraphicsOnApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, RoomPalette palette)
    {
        orig(self, sleaser, rcam, palette);
        if (self.player.SlugCatClass != NoirName) return;

        for (var index = 0; index < sleaser.sprites.Length; ++index)
        {
            switch (index)
            {
                case OTOTArmSpr:
                case OTOTArmSpr2:
                    sleaser.sprites[index].color = ReColor(sleaser.sprites[index].color, true);
                    break;
                case FaceSpr:
                    break; //why do you change this in drawsprites Rain World...
                default:
                    sleaser.sprites[index].color = ReColor(sleaser.sprites[index].color, false);;
                    break;
            }
        }
        
        //Thank you CustomTails ;-;
        if (sleaser.sprites[TailSpr] is TriangleMesh tailMesh)
        {
            tailMesh.element = Futile.atlasManager.GetElementWithName(NoirTail);
            if (tailMesh.verticeColors == null || tailMesh.verticeColors.Length != tailMesh.vertices.Length)
            {
                tailMesh.verticeColors = new Color[tailMesh.vertices.Length];
            }
            tailMesh.customColor = true;
            
            //var color2 = color1; //Base color
            //var color3 = color1; //Tip color
            
            for (int j = tailMesh.verticeColors.Length - 1; j >= 0; j--)
            {
                float num = (j / 2f) / (tailMesh.verticeColors.Length / 2f);
                tailMesh.verticeColors[j] = sleaser.sprites[0].color; //Color.Lerp(color2, color3, num);
                Vector2 vector;
                if (j % 2 == 0)
                {
                    vector = new Vector2(num, 0f);
                }
                else if (j < tailMesh.verticeColors.Length - 1)
                {
                    vector = new Vector2(num, 1f);
                }
                else
                {
                    vector = new Vector2(1f, 0f);
                }
                vector.x = Mathf.Lerp(tailMesh.element.uvBottomLeft.x, tailMesh.element.uvTopRight.x, vector.x);
                vector.y = Mathf.Lerp(tailMesh.element.uvBottomLeft.y, tailMesh.element.uvTopRight.y, vector.y);
                tailMesh.UVvertices[j] = vector;
            }
            tailMesh.Refresh();
        }
        
        //Ears!
        if (sleaser.sprites[EarSpr] is TriangleMesh earMesh)
        {
            earMesh.element = Futile.atlasManager.GetElementWithName(NoirEars);
            if (earMesh.verticeColors == null || earMesh.verticeColors.Length != earMesh.vertices.Length)
            {
                earMesh.verticeColors = new Color[earMesh.vertices.Length];
            }
            earMesh.customColor = true;
            
            for (var j = earMesh.verticeColors.Length - 1; j >= 0; j--)
            {
                var num = (j / 2f) / (earMesh.verticeColors.Length / 2f);
                earMesh.verticeColors[j] = sleaser.sprites[0].color; //Color.Lerp(color2, color3, num);
                Vector2 vector;
                if (j % 2 == 0)
                {
                    vector = new Vector2(num, 0f);
                }
                else if (j < earMesh.verticeColors.Length - 1)
                {
                    vector = new Vector2(num, 1f);
                }
                else
                {
                    vector = new Vector2(1f, 0f);
                }
                vector.x = Mathf.Lerp(earMesh.element.uvBottomLeft.x, earMesh.element.uvTopRight.x, vector.x);
                vector.y = Mathf.Lerp(earMesh.element.uvBottomLeft.y, earMesh.element.uvTopRight.y, vector.y);
                earMesh.UVvertices[j] = vector;
            }
            earMesh.Refresh();
        }
        
        if (sleaser.sprites[EarSpr2] is TriangleMesh ear2Mesh)
        {
            ear2Mesh.element = Futile.atlasManager.GetElementWithName(NoirEars);
            if (ear2Mesh.verticeColors == null || ear2Mesh.verticeColors.Length != ear2Mesh.vertices.Length)
            {
                ear2Mesh.verticeColors = new Color[ear2Mesh.vertices.Length];
            }
            ear2Mesh.customColor = true;
            
            for (var j = ear2Mesh.verticeColors.Length - 1; j >= 0; j--)
            {
                var num = (j / 2f) / (ear2Mesh.verticeColors.Length / 2f);
                ear2Mesh.verticeColors[j] = sleaser.sprites[0].color; //Color.Lerp(color2, color3, num);
                Vector2 vector;
                if (j % 2 == 0)
                {
                    vector = new Vector2(num, 0f);
                }
                else if (j < ear2Mesh.verticeColors.Length - 1)
                {
                    vector = new Vector2(num, 1f);
                }
                else
                {
                    vector = new Vector2(1f, 0f);
                }
                vector.x = Mathf.Lerp(ear2Mesh.element.uvBottomLeft.x, ear2Mesh.element.uvTopRight.x, vector.x);
                vector.y = Mathf.Lerp(ear2Mesh.element.uvBottomLeft.y, ear2Mesh.element.uvTopRight.y, vector.y);
                ear2Mesh.UVvertices[j] = vector;
            }
            ear2Mesh.Refresh();
        }
    }
    
    private void PlayerGraphicsOnReset(On.PlayerGraphics.orig_Reset orig, PlayerGraphics self)
    {
        orig(self);
        if (self.player.SlugCatClass != NoirName) return;
        var noirData = NoirDeets.GetValue(self.player, NoirDataCtor);
        
        noirData.Ear[0].Reset(EarAttachPos(noirData, 1f));
        noirData.Ear[1].Reset(EarAttachPos(noirData, 1f));
        noirData.Ear2[0].Reset(Ear2AttachPos(noirData, 1f));
        noirData.Ear2[1].Reset(Ear2AttachPos(noirData, 1f));
    }
    
    private void PlayerGraphicsILUpdate(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            c.GotoNext(MoveType.Before,
                i => i.MatchLdcR4(1),
                i => i.MatchLdarg(0),
                i => i.MatchCallOrCallvirt<GraphicsModule>("get_owner"),
                i => i.MatchCallOrCallvirt<PhysicalObject>("get_bodyChunks"),
                i => i.MatchLdcI4(1),
                _ => true,
                _ => true,
                _ => true,
                _ => true,
                _ => true,
                _ => true,
                _ => true,
                _ => true,
                _ => true,
                _ => true,
                i => i.MatchCallOrCallvirt<UnityEngine.Mathf>("Clamp"),
                _ => true,
                _ => true,
                i => i.MatchLdloc(2),
                i => i.MatchBrfalse(out label)
            );

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((PlayerGraphics self) =>
            {
                return self.player.SlugCatClass == NoirName;
            });
            c.Emit(OpCodes.Brtrue, label);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }
}