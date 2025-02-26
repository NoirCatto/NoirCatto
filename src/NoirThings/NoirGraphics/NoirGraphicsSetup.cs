using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace NoirCatto;

public partial class NoirCatto
{
    public static void LoadAtlases()
    {
        var headAtlas = Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirHead");
        var faceAtlas = Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirFace");
        var bodyAtlas = Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirBodyA");
        var hipsAtlas = Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirHipsA");
        var leftHipsAtlas = Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirLeftHipsA");
        var rightHipsAtlas = Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirRightHipsA");
        var legsAtlas = Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirLegs");
        var playerArmAtlas = Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirPlayerArm");
        var tailAtlas = Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirTail");
        var earsAtlas = Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirEars");
        HeadTexture = (Texture2D)headAtlas.texture;
        FaceTexture = (Texture2D)faceAtlas.texture;
        BodyTexture = (Texture2D)bodyAtlas.texture;
        HipsTexture = (Texture2D)hipsAtlas.texture;
        LeftHipsTexture = (Texture2D)leftHipsAtlas.texture;
        RightHipsTexture = (Texture2D)rightHipsAtlas.texture;
        LegsTexture = (Texture2D)legsAtlas.texture;
        PlayerArmTexture = (Texture2D)playerArmAtlas.texture;
        TailTexture = (Texture2D)tailAtlas.texture;
        EarTexture = (Texture2D)earsAtlas.texture;
        //NoirBlueEyes = EyeTexture.GetPixels32().Where(c => c.a == 255).Distinct().Select(color32 => (Color)color32).ToArray();
    }
    
    #region Consts
    private static readonly string[] ValidSpriteNames = ["Head", "Face", "PFace", "BodyA", "HipsA", "PlayerArm", "OnTopOfTerrainHand", "Legs", "Tail", "Futile_White"];

    private const string Head = "Head";
    private const string Face = "Face";
    private const string BodyA = "BodyA";
    private const string PlayerArm = "PlayerArm";
    private const string HipsA = "HipsA";
    private const string Legs = "Legs";

    private const string NoirHead = "NoirHead";
    private const string NoirEars = "NoirEars";
    private const string NoirFace = "NoirFace";
    private const string NoirBodyA = "NoirBodyA";
    private const string NoirPlayerArm = "NoirPlayerArm";
    private const string NoirOnTopOfTerrainHand = "NoirOnTopOfTerrainHand";
    private const string NoirHipsA = "NoirHipsA";
    private const string NoirLeftHipsA = "NoirLeftHipsA";
    private const string NoirRightHipsA = "NoirRightHipsA";
    private const string NoirLegs = "NoirLegs";
    private const string NoirTail = "NoirTail";
    private const string Noir = "Noir"; //Prefix for sprite replacement
    
    private const int BodySpr = 0; //Midground
    private const int HipsSpr = 1;
    private const int TailSpr = 2;
    private const int HeadSpr = 3;
    private const int LegsSpr = 4;
    private const int ArmSpr = 5;
    private const int ArmSpr2 = 6; //Midground
    private const int OTOTArmSpr = 7; //Foreground
    private const int OTOTArmSpr2 = 8; //Foreground
    private const int FaceSpr = 9; //Midground
    
    private const int TailLength = 7;
    
    public static int CustomColorBody = 0; //Slugbase custom colors
    public static int CustomColorEyes = 1;
    public static int CustomColorFluff = 2;
    public static int CustomColorNose = 3;
    public static int CustomColorPaws = 4;
    
    public static readonly Color NoirWhite = Extensions.ColorFromHEX("e6e1e5");
    public static readonly Color NoirBlack = Extensions.ColorFromHEX("2f2e34");
    public static readonly Color NoirBlackPaws = Extensions.ColorFromHEX("2e2d33");
    public static readonly Color NoirPurple = Extensions.ColorFromHEX("6f5569");
    public static readonly Color NoirBlueEyesDefault = Extensions.ColorFromHEX("6b8de5");
    public static Color[] NoirBlueEyes =
    [
        Extensions.ColorFromHEX("bbe1f3"), //Lightest blue
        Extensions.ColorFromHEX("96c5ec"),
        Extensions.ColorFromHEX("7ba2e8"),
        Extensions.ColorFromHEX("6b8de5") //Darkest blue
    ];

    public static Texture2D HeadTexture;
    public static Texture2D FaceTexture;
    public static Texture2D BodyTexture;
    public static Texture2D HipsTexture;
    public static Texture2D LeftHipsTexture;
    public static Texture2D RightHipsTexture;
    public static Texture2D LegsTexture;
    public static Texture2D PlayerArmTexture;
    public static Texture2D TailTexture;
    public static Texture2D EarTexture;
    #endregion

    private static List<int> SprToReplace =
    [
        HeadSpr, FaceSpr, BodySpr, ArmSpr, ArmSpr2, OTOTArmSpr, OTOTArmSpr2, HipsSpr, LegsSpr //TailSpr
    ];

    //Helper methods
    private static void ReplaceSprites(RoomCamera.SpriteLeaser sleaser, PlayerGraphics self, NoirData noirData)
    {
        foreach (var num in SprToReplace)
        {
            var spr = sleaser.sprites[num].element;

            if (!spr.name.StartsWith(Noir))
            {
                if (!ValidSpriteNames.Any(spr.name.StartsWith))
                {
                    continue; //For DMS compatibility
                }

                if (num == HeadSpr) //Pup Fix
                {
                    if (!sleaser.sprites[num].element.name.Contains("HeadA"))
                    {
                        sleaser.sprites[num].element.name = spr.name.Replace("HeadB", "HeadA");
                        sleaser.sprites[num].element.name = spr.name.Replace("HeadC", "HeadA");
                        sleaser.sprites[num].element.name = spr.name.Replace("HeadD", "HeadA");
                    }
                }

                if (num == FaceSpr) //Pup Fix
                {
                    if (sleaser.sprites[num].element.name.Contains("PFace"))
                        sleaser.sprites[num].element.name = spr.name.Replace("PFace", "Face");
                }

                sleaser.sprites[num].element = Futile.atlasManager.GetElementWithName(Noir + spr.name);
            }
        }
    }

    private static void ReplaceHips(RoomCamera.SpriteLeaser sleaser, PlayerGraphics self, NoirData noirData, out string hipsName)
    {
        hipsName = NoirHipsA;
        if (sleaser.sprites[HipsSpr].element.name.StartsWith(Noir))
        {
            if (!self.player.standing && (self.player.animation == Player.AnimationIndex.None || self.player.animation == Player.AnimationIndex.CrawlTurn) ||
                self.player.animation == Player.AnimationIndex.StandOnBeam && noirData.CanCrawlOnBeam())
            {
                var angle = Custom.AimFromOneVectorToAnother(self.player.bodyChunks[0].pos, self.player.bodyChunks[1].pos);

                if (angle is > 0 and < 120)
                    hipsName = NoirLeftHipsA;
                else if (angle is < 0 and > -120)
                    hipsName = NoirRightHipsA;
                else
                    hipsName = NoirHipsA;

                if (!sleaser.sprites[HipsSpr].element.name.StartsWith(hipsName))
                    sleaser.sprites[HipsSpr].element = Futile.atlasManager.GetElementWithName(hipsName);
            }
            else if (!sleaser.sprites[HipsSpr].element.name.StartsWith(hipsName))
                sleaser.sprites[HipsSpr].element = Futile.atlasManager.GetElementWithName(hipsName);
        }
    }

    private static Vector2 EarAttachPos(NoirData noirData, int earNum, float timestacker)
    {
        var graphics = (PlayerGraphics)noirData.Cat.graphicsModule;
        var numXMod = earNum == 0 ? -4 : 4;
        return Vector2.Lerp(graphics.head.lastPos + new Vector2(numXMod, 1.5f), graphics.head.pos + new Vector2(numXMod, 1.5f), timestacker) + Vector3.Slerp(noirData.LastHeadRotation, graphics.head.connection.Rotation, timestacker).ToVector2InPoints() * 15f;
    }

    public static bool TryGetCustomJollyColor(int playerNumber, int bodyPartIndex, out Color color)
    {
        if (ModManager.CoopAvailable && Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.CUSTOM)
        {
            var col = PlayerGraphics.jollyColors;
            if (col.GetLength(0) > playerNumber)
            {
                if (col[playerNumber].Length > bodyPartIndex)
                {
                    if (col[playerNumber][bodyPartIndex].HasValue)
                    {
                        color = col[playerNumber][bodyPartIndex].Value;
                        return true;
                    }
                }
            }
        }
        color = default;
        return false;
    }
}