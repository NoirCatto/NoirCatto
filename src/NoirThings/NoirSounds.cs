using System;
using System.Linq;
using Noise;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NoirCatto;

public partial class NoirCatto
{
    public static SoundID SlashSND;
    public static SoundID MeowSND;
    public static SoundID Meow1SND;
    public static SoundID Meow2SND;
    public static SoundID Meow3SND;
    public static SoundID Meow4SND;
    public static SoundID Meow5SND;
    public static SoundID MeowFrustratedSND;
    public static SoundID PurrLoopSND;
    public static SoundID[] MeowSNDs;

    public static void LoadSounds()
    {
        SlashSND = new SoundID("NoirCatto_Slash", true);
        MeowSND = new SoundID("NoirCatto_Meow", true);
        Meow1SND = new SoundID("NoirCatto_Meow1", true);
        Meow2SND = new SoundID("NoirCatto_Meow2", true);
        Meow3SND = new SoundID("NoirCatto_Meow3", true);
        Meow4SND = new SoundID("NoirCatto_Meow4", true);
        Meow5SND = new SoundID("NoirCatto_Meow5", true);
        MeowFrustratedSND = new SoundID("NoirCatto_MeowFrustrated", true);
        PurrLoopSND = new SoundID("NoirCatto_PurrLoop", true);
        MeowSNDs = [Meow1SND, Meow2SND, Meow3SND, Meow4SND, Meow5SND];
    }

    #region Sleep-Screen Purr
    private static MenuMicrophone.MenuSoundLoop PurrLoop;

    public static void MenuOnUpdate(On.Menu.Menu.orig_Update orig, Menu.Menu self)
    {
        orig(self);
        if (self.manager.oldProcess is not RainWorldGame game) return;
        if (game.StoryCharacter != Const.NoirName) return;
        if (self is not Menu.SleepAndDeathScreen sleep) return;
        if (Math.Min(sleep.hud.owner.CurrentFood + sleep.hud.foodMeter.survivalLimit, sleep.hud.foodMeter.maxFood) < sleep.hud.foodMeter.maxFood) return;

        if (PurrLoop != null)
        {
            PurrLoop.loopVolume = Custom.LerpAndTick(PurrLoop.loopVolume, 1f, 0.02f, 0.05f);
        }

        if (self.soundLoop == null)
        {
            if (self.manager.menuMic != null)
            {
                var flag = self.manager.menuMic.soundObjects.Any(t => t is MenuMicrophone.MenuSoundLoop loop && loop.isBkgLoop && ((self.mySoundLoopID != SoundID.None && t.soundData.soundID == self.mySoundLoopID) || (self.mySoundLoopName != "" && t.soundData.soundName == self.mySoundLoopName)));
                if (!flag && (self.mySoundLoopID == SoundID.MENU_Sleep_Screen_LOOP || self.mySoundLoopID == MoreSlugcats.MoreSlugcatsEnums.MSCSoundID.Sleep_Blizzard_Loop))
                {
                    PurrLoop = self.PlayLoop(PurrLoopSND, 0f, 1f, 1f, true);
                }
            }
        }
    }

    public static void MenuOnCommunicateWithUpcomingProcess(On.Menu.Menu.orig_CommunicateWithUpcomingProcess orig, Menu.Menu self, MainLoopProcess nextprocess)
    {
        orig(self, nextprocess);

        if (nextprocess is not Menu.Menu menu || ((self.mySoundLoopID == SoundID.None || menu.mySoundLoopID != self.mySoundLoopID) && (self.mySoundLoopName == "" || menu.mySoundLoopName != self.mySoundLoopName)))
        {
            if (PurrLoop == null) return;
            PurrLoop.Destroy();
        }
    }
    #endregion

    public static void MeowUpdate(NoirData noirData)
    {
        //Meow!
        if (noirData.Cat.stun == 0 && !noirData.Cat.dead && noirData.Cat.controller is not NoirStartController)
        {
            if (Input.GetKeyDown(ModOptions.NoirMeowKey.Value))
            {
                noirData.BaseMeowPitch = Random.Range(0.95f, 1.05f) + (Mathf.Pow(noirData.MeowCounter, 2f) * 0.0005f);
                noirData.MeowCounter += 1;
                noirData.SinceLastMeowCounter = 40;
                if (noirData.MeowCounter > 100) MeowSplode(noirData);
                
                var sndId = (byte)Random.Range(0, MeowSNDs.Length);
                var sndIdAndAttrMeow = (byte)((byte)(sndId << 1) + Convert.ToByte(ModOptions.NoirAttractiveMeow.Value));
                
                DoMeow(noirData.Cat, noirData.MeowPitch, sndIdAndAttrMeow);
                if (MeadowThings.IsMeadowOnline) MeadowThings.RpcSend_DoMeow(noirData, sndIdAndAttrMeow);
            }
        }
    }
    
    public static void DoMeow(Creature meower, float meowPitch, byte sndIdAndAttrMeow)
    {
        var attractiveMeow = sndIdAndAttrMeow % 2;
        var sndId = sndIdAndAttrMeow >> 1;
        
        meower.room?.PlaySound(MeowSNDs[sndId], meower.firstChunk, false, 1f, meowPitch);
        if (attractiveMeow == 1) meower.room?.InGameNoise(new InGameNoise(meower.firstChunk.pos, 600f, meower, 1f));
    }

    public static void MeowSplode(NoirData noirData) //Whimsymaxxing
    {
        var room = noirData.Cat.room;
        if (room == null) return;

        var pos = noirData.Cat.firstChunk.pos;
        const int explosionLifeTime = 6;
        const float explosionRadius = 100f;
        const float explosionForce = 1.2f;
        const float explosionDamage = 0.5f;
        const float explosionMinStun = 50f;
        const float explosionStun = 100f;
        var color = Color.white;
        
        room.AddObject(new Explosion(room, null, pos, explosionLifeTime, explosionRadius, explosionForce, explosionDamage, explosionStun, 0.10f, noirData.Cat, 1f, explosionMinStun, 1f));
        room.AddObject(new Explosion.ExplosionLight(pos, 28f * explosionForce, 1f, explosionLifeTime + 1, color));
        room.AddObject(new Explosion.ExplosionLight(pos, 23f * explosionForce, 1f, (int)(explosionLifeTime * 0.5f), color));
        room.AddObject(new ExplosionSpikes(room, pos, (int)Random.Range(14f, 14 * explosionDamage), 30f, explosionLifeTime + 3, explosionForce - 2, 170f, color));
        room.AddObject(new ShockWave(pos, 430f, 0.045f, explosionLifeTime - 2, false));
        for (var j = 0; j < 20; j++)
        {
            var a = Custom.RNV();
            room.AddObject(new Spark(pos + a * Random.value * 40f, a * Mathf.Lerp(4f, 30f, Random.value), color, null, 4, 18));
        }

        room.ScreenMovement(pos, default, 0.7f);

        room.PlaySound(SoundID.Bomb_Explode, pos);
        room.InGameNoise(new InGameNoise(pos, 4000f * explosionDamage, noirData.Cat, 1f));
    }
}