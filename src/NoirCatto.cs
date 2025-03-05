using System;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using SlugBase;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace NoirCatto;

[BepInPlugin(Const.MOD_ID, "NoirCatto", Const.MOD_VERSION)]
[BepInDependency("slime-cubed.slugbase")]
public partial class NoirCatto : BaseUnityPlugin
{
    public static NoirCattoOptions ModOptions;
    public static ManualLogSource LogSource;

    public static bool ModRotundWorld;

    public NoirCatto()
    {
        try
        {
            LogSource = Logger;
            ModOptions = new NoirCattoOptions();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
    
    private void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
        On.RainWorld.PostModsInit += RainWorldOnPostModsInit;
    }

    private bool _isInit;
    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            if (_isInit) return;

            Hooks.Apply();
            LoadAtlases();
            LoadSounds();
            
            MachineConnector.SetRegisteredOI("NoirCatto.NoirCatto", ModOptions);
            _isInit = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private bool _isPostInit;
    private void RainWorldOnPostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            if (ModManager.ActiveMods.Any(x => x.id == "willowwisp.bellyplus"))
            {
                ModRotundWorld = true;
                Logger.LogInfo("Rotund World detected! Noir gonna be chonky...");
            }
            else
            {
                ModRotundWorld = false;
            }
            
            if (!_isPostInit)
            {
                //NoirBase = SlugBaseCharacter.Get(NoirName);
            }

            _isPostInit = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
