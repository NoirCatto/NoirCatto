using System.Linq;
using RWCustom;
using UnityEngine;

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
    
    private void LoadSounds()
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
    }

    private static MenuMicrophone.MenuSoundLoop PurrLoop;
    private void MenuOnUpdate(On.Menu.Menu.orig_Update orig, Menu.Menu self)
    {
        orig(self);

        if (self.manager.oldProcess is not RainWorldGame game) return;
        if (game.StoryCharacter != NoirName) return;
        
        if (PurrLoop != null)
        {
            PurrLoop.loopVolume = Custom.LerpAndTick(PurrLoop.loopVolume, 1f, 0.02f, 0.05f);
        }

        if (self.soundLoop == null)
        {
            if (self.manager.menuMic != null)
            {
                var flag = self.manager.menuMic.soundObjects.Any(t => t is MenuMicrophone.MenuSoundLoop && (t as MenuMicrophone.MenuSoundLoop).isBkgLoop && ((self.mySoundLoopID != SoundID.None && t.soundData.soundID == self.mySoundLoopID) || (self.mySoundLoopName != "" && t.soundData.soundName == self.mySoundLoopName)));
                if (!flag && self.mySoundLoopID == SoundID.MENU_Sleep_Screen_LOOP)
                {
                    PurrLoop = self.PlayLoop(PurrLoopSND, 0f, 1f, 1f, true);
                }
            }
        }
    }

    private void MenuOnCommunicateWithUpcomingProcess(On.Menu.Menu.orig_CommunicateWithUpcomingProcess orig, Menu.Menu self, MainLoopProcess nextprocess)
    {
        orig(self, nextprocess);
        
        if (nextprocess is not Menu.Menu menu || ((self.mySoundLoopID == SoundID.None || menu.mySoundLoopID != self.mySoundLoopID) && (self.mySoundLoopName == "" || menu.mySoundLoopName != self.mySoundLoopName)))
        {
            if (PurrLoop == null)
                return;
            PurrLoop.Destroy();
        }
    }
}