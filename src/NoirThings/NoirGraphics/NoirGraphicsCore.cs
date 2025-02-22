using System;
using System.Linq;
using RWCustom;
using SlugBase.DataTypes;
using UnityEngine;

namespace NoirCatto;

public partial class NoirCatto //Sprite replacement and layer management is here
{
    public static void PlayerGraphicsOnctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (!self.player.TryGetNoirData(out var noirData)) return;
        
        foreach (var ear in noirData.Ears)
        {
            ear[0] = new TailSegment(self, 4.5f, 4f, null, 0.85f, 1f, 1f, true);
            ear[1] = new TailSegment(self, 1.5f, 7f, ear[0], 0.85f, 1f, 0.5f, true);
        }
        
        var origBodyParts = self.bodyParts.Except(self.tail).ToList();
        #region Tail
        const float tailThickness = 2.50f;
        const float tailRoundness = 0.05f;
        self.tail = new TailSegment[TailLength];
        for (var i = 0; i < self.tail.Length; i++)
        {
            self.tail[i] = new TailSegment(self,
                Mathf.Lerp(6f, 1f, Mathf.Pow((i + 1) / (float)TailLength, tailThickness)) * (1f + Mathf.Sin(i / (float)TailLength * 3.1415927f) * tailRoundness) * 1.25f,
                ((i == 0) ? 4 : 7) * (self.player.playerState.isPup ? 0.5f : 1f),
                (i > 0) ? self.tail[i - 1] : null,
                0.85f,
                1f,
                (i == 0) ? 1f : 0.5f,
                true);
        }
        #endregion
        var partsToAdd = self.tail.Cast<BodyPart>().ToList();
        partsToAdd.AddRange(origBodyParts);
        partsToAdd.AddRange(noirData.Ears[0]);
        partsToAdd.AddRange(noirData.Ears[1]);
        self.bodyParts = partsToAdd.ToArray();
    }

    public static void PlayerGraphicsOnInitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam)
    {
        if (!self.player.TryGetNoirData(out var noirData))
        {
            orig(self, sleaser, rcam);
            return;
        }

        if (!rcam.game.DEBUGMODE)
        {
            noirData.CallingAddToContainerFromOrigInitiateSprites = true;
        }

        orig(self, sleaser, rcam);

        if (!rcam.game.DEBUGMODE)
        {
            noirData.TotalSprites = sleaser.sprites.Length;
            noirData.EarSpr[0] = noirData.TotalSprites;
            noirData.EarSpr[1] = noirData.TotalSprites + 1;
            Array.Resize(ref sleaser.sprites, noirData.TotalSprites + NoirData.NewSprites);
            
            #region Init Tail + Ear arrays
            var tailArray = new TriangleMesh.Triangle[(self.tail.Length - 1) * 4 + 1];
            for (var i = 0; i < self.tail.Length - 1; i++)
            {
                var indexTimesFour = i * 4;
                for (var j = 0; j <= 3; j++)
                {
                    tailArray[indexTimesFour + j] = new TriangleMesh.Triangle(indexTimesFour + j, indexTimesFour + j + 1, indexTimesFour + j + 2);
                }
            }
            tailArray[(self.tail.Length - 1) * 4] = new TriangleMesh.Triangle((self.tail.Length - 1) * 4, (self.tail.Length - 1) * 4 + 1, (self.tail.Length - 1) * 4 + 2);
            sleaser.sprites[TailSpr] = new TriangleMesh(NoirTail, tailArray, false, false);

            var earArray = new TriangleMesh.Triangle[(noirData.Ears[0].Length - 1) * 4 + 1];
            for (var i = 0; i < noirData.Ears[0].Length - 1; i++)
            {
                var indexTimesFour = i * 4;
                for (var j = 0; j <= 3; j++)
                {
                    earArray[indexTimesFour + j] = new TriangleMesh.Triangle(indexTimesFour + j, indexTimesFour + j + 1, indexTimesFour + j + 2);
                }
            }
            earArray[(noirData.Ears[0].Length - 1) * 4] = new TriangleMesh.Triangle((noirData.Ears[0].Length - 1) * 4, (noirData.Ears[0].Length - 1) * 4 + 1, (noirData.Ears[0].Length - 1) * 4 + 2);
            foreach (var sprNum in noirData.EarSpr)
            {
                sleaser.sprites[sprNum] = new TriangleMesh(NoirEars, earArray, false, false);
            }
            #endregion

            if (ModOptions.NoirHideEars.Value) //For.. DMS?
            {
                foreach (var sprNum in noirData.EarSpr)
                {
                    sleaser.sprites[sprNum].isVisible = false;
                }
            }

            ReplaceSprites(sleaser, self, noirData);

            //todo Recoloring Sprites

            noirData.CallingAddToContainerFromOrigInitiateSprites = false;
            self.AddToContainer(sleaser, rcam, null);
        }
    }

    public static void PlayerGraphicsOnAddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, FContainer newcontatiner)
    {
        if (!self.player.TryGetNoirData(out var noirData))
        {
            orig(self, sleaser, rcam, newcontatiner);
            return;
        }

        if (noirData.CallingAddToContainerFromOrigInitiateSprites) return;

        orig(self, sleaser, rcam, newcontatiner);

        if (!rcam.game.DEBUGMODE)
        {
            var container = rcam.ReturnFContainer("Midground");
            container.AddChild(sleaser.sprites[noirData.EarSpr[0]]);
            container.AddChild(sleaser.sprites[noirData.EarSpr[1]]);

            sleaser.sprites[ArmSpr].MoveBehindOtherNode(sleaser.sprites[HeadSpr]);
            sleaser.sprites[ArmSpr2].MoveBehindOtherNode(sleaser.sprites[HeadSpr]);
            sleaser.sprites[noirData.EarSpr[0]].MoveInFrontOfOtherNode(sleaser.sprites[HeadSpr]);
            sleaser.sprites[noirData.EarSpr[1]].MoveInFrontOfOtherNode(sleaser.sprites[HeadSpr]);
            sleaser.sprites[TailSpr].MoveBehindOtherNode(sleaser.sprites[BodySpr]);
            sleaser.sprites[LegsSpr].MoveBehindOtherNode(sleaser.sprites[BodySpr]);
        }
    }

    public static void PlayerGraphicsOnDrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        if (!self.player.TryGetNoirData(out var noirData)) return;
        if (rcam.room.game.DEBUGMODE) return;

        ReplaceSprites(sleaser, self, noirData);
        MoveMeshes(noirData, sleaser, timestacker, campos);

        #region Moving Sprites to front/back
        if (noirData.FlipDirection == 1)
        {
            sleaser.sprites[ArmSpr].MoveBehindOtherNode(sleaser.sprites[HeadSpr]);
            sleaser.sprites[ArmSpr2].MoveBehindOtherNode(sleaser.sprites[BodySpr]);
            sleaser.sprites[noirData.EarSpr[0]].MoveInFrontOfOtherNode(sleaser.sprites[HeadSpr]);
            sleaser.sprites[noirData.EarSpr[1]].MoveBehindOtherNode(sleaser.sprites[BodySpr]);
        }
        else
        {
            sleaser.sprites[ArmSpr].MoveBehindOtherNode(sleaser.sprites[BodySpr]);
            sleaser.sprites[ArmSpr2].MoveBehindOtherNode(sleaser.sprites[HeadSpr]);
            sleaser.sprites[noirData.EarSpr[0]].MoveBehindOtherNode(sleaser.sprites[BodySpr]);
            sleaser.sprites[noirData.EarSpr[1]].MoveInFrontOfOtherNode(sleaser.sprites[HeadSpr]);
        }


        if ((self.player.animation == Player.AnimationIndex.None && (self.player.input[0].x != 0 && self.player.input[4].x != 0)) ||
            self.player.bodyMode == Player.BodyModeIndex.Crawl ||
            self.player.animation != Player.AnimationIndex.None && self.player.animation != Player.AnimationIndex.Flip && !noirData.OnAnyBeam())
        {
            sleaser.sprites[LegsSpr].MoveInFrontOfOtherNode(sleaser.sprites[HipsSpr]);
        }
        else
        {
            sleaser.sprites[LegsSpr].MoveBehindOtherNode(sleaser.sprites[BodySpr]);
        }
        #endregion

        if (self.player.animation == Player.AnimationIndex.ClimbOnBeam)
        {
            if (sleaser.sprites[LegsSpr].element.name.Contains(Noir))
                sleaser.sprites[LegsSpr].y -= 6f; //Adjustment for the limited space in leg sprite
        }

        if (sleaser.sprites[FaceSpr].element.name.StartsWith(Noir))
        {
            sleaser.sprites[FaceSpr].color = Color.white; //Joar, why do you set this in drawsprites...
        }
        if (sleaser.sprites[TailSpr].element.name.StartsWith(Noir))
        {
            // if ((bodyColor != null && bodyColor.Value != NoirBlack) || (fluffColor != null && fluffColor.Value != NoirWhite))
            //     sleaser.sprites[TailSpr].element = Futile.atlasManager.GetElementWithName(NoirTail + "_" + playerNum);
            ApplyMeshTexture(sleaser.sprites[TailSpr] as TriangleMesh);
        }
        for (var i = 0; i < noirData.EarSpr.Length; i++)
        {
            var EarSprite = noirData.EarSpr[i];
            //var name = NoirEars + "_" + i;
            if (sleaser.sprites[EarSprite].element.name.StartsWith(Noir))
            {
                // if ((bodyColor != null && bodyColor.Value != NoirBlack) || (fluffColor != null && fluffColor.Value != NoirWhite))
                //     sleaser.sprites[EarSprite].element = Futile.atlasManager.GetElementWithName(name + "_" + playerNum);
                ApplyMeshTexture(sleaser.sprites[EarSprite] as TriangleMesh);
            }
        }
        //todo Recoloring sprites
    }

    public static void PlayerGraphicsOnApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, RoomPalette palette)
    {
        orig(self, sleaser, rcam, palette);
        if (!self.player.TryGetNoirData(out var noirData)) return;

        for (var i = 0; i < sleaser.sprites.Length; i++) //todo Check if this is even needed
            sleaser.sprites[i].color = Color.white; //We'll handle jollycoop coloring elsewhere
    }

    public static void PlayerGraphicsOnReset(On.PlayerGraphics.orig_Reset orig, PlayerGraphics self)
    {
        orig(self);
        if (!self.player.TryGetNoirData(out var noirData)) return;

        for (var i = 0; i < 2; i++)
        {
            noirData.Ears[i][0].Reset(EarAttachPos(noirData, i, 1f));
            noirData.Ears[i][1].Reset(EarAttachPos(noirData, i, 1f));
        }
    }

    //Helpers
    private static void EarsUpdate(NoirData noirData)
    {
        for (var i = 0; i < 2; i++)
        {
            noirData.Ears[i][0].connectedPoint = EarAttachPos(noirData, i, 1f);
            noirData.Ears[i][0].Update();
            noirData.Ears[i][1].Update();
        }
    }

    private static void MoveMeshes(NoirData noirData, RoomCamera.SpriteLeaser sleaser, float timestacker, Vector2 campos)
    {
        var self = (PlayerGraphics)noirData.Cat.graphicsModule;

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
            if (index == 0) distance = 0.0f;
            ((TriangleMesh)sleaser.sprites[TailSpr]).MoveVertice(index * 4, tailAttachPos - vector2_3 * rad * malnourishedMod + normalized * distance - campos);
            ((TriangleMesh)sleaser.sprites[TailSpr]).MoveVertice(index * 4 + 1, tailAttachPos + vector2_3 * rad * malnourishedMod + normalized * distance - campos);
            if (index < self.tail.Length - 1)
            {
                ((TriangleMesh)sleaser.sprites[TailSpr]).MoveVertice(index * 4 + 2, tailPos - vector2_3 * self.tail[index].StretchedRad * malnourishedMod - normalized * distance - campos);
                ((TriangleMesh)sleaser.sprites[TailSpr]).MoveVertice(index * 4 + 3, tailPos + vector2_3 * self.tail[index].StretchedRad * malnourishedMod - normalized * distance - campos);
            }
            else
                ((TriangleMesh)sleaser.sprites[TailSpr]).MoveVertice(index * 4 + 2, tailPos - campos);
            rad = self.tail[index].StretchedRad;
            tailAttachPos = tailPos;
        }
        #endregion

        #region Moving Ears
        //I swear, I TRIED optimizing this...
        var earAttachPos = EarAttachPos(noirData, 0, timestacker);
        var earRad = noirData.Ears[0][0].rad;
        for (var index = 0; index < noirData.Ears[0].Length; ++index)
        {
            var earMesh = (TriangleMesh)sleaser.sprites[noirData.EarSpr[0]];
            var earPos = Vector2.Lerp(noirData.Ears[0][index].lastPos, noirData.Ears[0][index].pos, timestacker);
            var normalized = (earPos - earAttachPos).normalized;
            var vector2_3 = Custom.PerpendicularVector(normalized);
            var distance = Vector2.Distance(earPos, earAttachPos) / 5f;
            if (index == 0) distance = 0.0f;
            earMesh.MoveVertice(index * 4, earAttachPos - noirData.EarsFlip[0] * vector2_3 * earRad + normalized * distance - campos);
            earMesh.MoveVertice(index * 4 + 1, earAttachPos + noirData.EarsFlip[0] * vector2_3 * earRad + normalized * distance - campos);
            if (index < noirData.Ears[0].Length - 1)
            {
                earMesh.MoveVertice(index * 4 + 2, earPos - noirData.EarsFlip[0] * vector2_3 * noirData.Ears[0][index].StretchedRad - normalized * distance - campos);
                earMesh.MoveVertice(index * 4 + 3, earPos + noirData.EarsFlip[0] * vector2_3 * noirData.Ears[0][index].StretchedRad - normalized * distance - campos);
            }
            else
                earMesh.MoveVertice(index * 4 + 2, earPos - campos);
            earRad = noirData.Ears[0][index].StretchedRad;
            earAttachPos = earPos;
        }


        var ear2AttachPos = EarAttachPos(noirData, 1, timestacker);
        var ear2Rad = noirData.Ears[1][0].rad;
        for (var index = 0; index < noirData.Ears[1].Length; ++index)
        {

            var ear2Mesh = (TriangleMesh)sleaser.sprites[noirData.EarSpr[1]];
            var ear2Pos = Vector2.Lerp(noirData.Ears[1][index].lastPos, noirData.Ears[1][index].pos, timestacker);
            var normalized = (ear2Pos - ear2AttachPos).normalized;
            var vector2_3 = Custom.PerpendicularVector(normalized);
            var distance = Vector2.Distance(ear2Pos, ear2AttachPos) / 5f;
            if (index == 0) distance = 0.0f;
            ear2Mesh.MoveVertice(index * 4, ear2AttachPos + noirData.EarsFlip[1] * vector2_3 * ear2Rad + normalized * distance - campos);
            ear2Mesh.MoveVertice(index * 4 + 1, ear2AttachPos - noirData.EarsFlip[1] * vector2_3 * ear2Rad + normalized * distance - campos);
            if (index < noirData.Ears[1].Length - 1)
            {
                ear2Mesh.MoveVertice(index * 4 + 2, ear2Pos + noirData.EarsFlip[1] * vector2_3 * noirData.Ears[1][index].StretchedRad - normalized * distance - campos);
                ear2Mesh.MoveVertice(index * 4 + 3, ear2Pos - noirData.EarsFlip[1] * vector2_3 * noirData.Ears[1][index].StretchedRad - normalized * distance - campos);
            }
            else
                ear2Mesh.MoveVertice(index * 4 + 2, ear2Pos - campos);
            ear2Rad = noirData.Ears[1][index].StretchedRad;
            ear2AttachPos = ear2Pos;
        }
        #endregion
    }

    private static void ApplyMeshTexture(TriangleMesh triMesh) //Code adapted from SlimeCubed's CustomTails
    {
        if (triMesh == null) return;

        if (triMesh.verticeColors == null || triMesh.verticeColors.Length != triMesh.vertices.Length)
        {
            triMesh.verticeColors = new Color[triMesh.vertices.Length];
        }
        triMesh.color = Color.white;
        triMesh.customColor = true;

        for (var j = triMesh.verticeColors.Length - 1; j >= 0; j--)
        {
            var num = (j / 2f) / (triMesh.verticeColors.Length / 2f);
            triMesh.verticeColors[j] = triMesh.color;
            Vector2 vector;
            if (j % 2 == 0)
            {
                vector = new Vector2(num, 0f);
            }
            else if (j < triMesh.verticeColors.Length - 1)
            {
                vector = new Vector2(num, 1f);
            }
            else
            {
                vector = new Vector2(1f, 0f);
            }
            vector.x = Mathf.Lerp(triMesh.element.uvBottomLeft.x, triMesh.element.uvTopRight.x, vector.x);
            vector.y = Mathf.Lerp(triMesh.element.uvBottomLeft.y, triMesh.element.uvTopRight.y, vector.y);
            triMesh.UVvertices[j] = vector;
        }
        triMesh.Refresh();
    }

    private static Texture2D GetTextureFromFAtlasElement(FAtlasElement element, Texture2D texture = null)
    {
        texture ??= (Texture2D)element.atlas.texture;
        var pos = Vector2Int.RoundToInt(element.uvRect.position * element.atlas.textureSize);
        var size = Vector2Int.RoundToInt(element.sourceSize);
        var spriteTexture = new Texture2D(size.x, size.y, texture.format, false);
        spriteTexture.filterMode = FilterMode.Point;

        var pixels = texture.GetPixels(pos.x, pos.y, size.x, size.y);
        spriteTexture.SetPixels(pixels);
        spriteTexture.Apply();
        return spriteTexture;
    }
}