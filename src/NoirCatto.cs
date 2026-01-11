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
[BepInDependency("henpemaz.rainmeadow", BepInDependency.DependencyFlags.SoftDependency)]
public partial class NoirCatto : BaseUnityPlugin
{
    public static NoirCattoOptions ModOptions;
    public static ManualLogSource LogSource;

    public static bool ModRotundWorld;
    public static bool ModRainMeadow;

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
        if (_isInit) return;
        try
        {
            _isInit = true;
            AbstractObjectType.InitStaticEnums();
            Hooks.Apply();
            LoadAtlases();
            LoadSounds();
            MachineConnector.SetRegisteredOI("NoirCatto.NoirCatto", ModOptions);
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
            _isPostInit = true;
            NoirNameFix.Apply();

            if (ModManager.ActiveMods.Any(x => x.id == "willowwisp.bellyplus"))
            {
                ModRotundWorld = true;
                Logger.LogInfo("Rotund World detected! Noir gonna be chonky...");
            }
            else
            {
                ModRotundWorld = false;
            }
            ModRainMeadow = ModManager.ActiveMods.Any(x => x.id == "henpemaz_rainmeadow");
            
            if (!_isPostInit)
            {
                //NoirBase = SlugBaseCharacter.Get(NoirName);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
