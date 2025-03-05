using RWCustom;
using UnityEngine;

namespace NoirCatto;

public partial class NoirCatto
{
    public static Color PlayerGraphicsOnJollyFaceColorMenu(On.PlayerGraphics.orig_JollyFaceColorMenu orig, SlugcatStats.Name slugname, SlugcatStats.Name reference, int playernumber)
    {
        if (Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.DEFAULT)
            return orig(slugname, reference, playernumber);

        return slugname == Const.NoirName ? NoirBlueEyesDefault : orig(slugname, reference, playernumber);
    }

    public static Color PlayerGraphicsOnJollyUniqueColorMenu(On.PlayerGraphics.orig_JollyUniqueColorMenu orig, SlugcatStats.Name slugname, SlugcatStats.Name reference, int playernumber)
    {
        if (Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.DEFAULT)
            return orig(slugname, reference, playernumber);

        return slugname == Const.NoirName ? NoirWhite : orig(slugname, reference, playernumber);
    }

    public static bool SymbolButtonTogglePupButtonOnHasUniqueSprite(On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.orig_HasUniqueSprite orig, JollyCoop.JollyMenu.SymbolButtonTogglePupButton self)
    {
        if (self.symbol.fileName.Contains("on"))
            return orig(self);

        return self.symbolNameOff.Contains("noir") || orig(self);
    }

    public static string JollyPlayerSelectorOnGetPupButtonOffName(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_GetPupButtonOffName orig, JollyCoop.JollyMenu.JollyPlayerSelector self)
    {
        var playerClass = self.JollyOptions(self.index).playerClass;
        if (playerClass == null || self.JollyOptions(self.index).isPup) return orig(self);
        return playerClass == Const.NoirName ? "noir_pup_off" : orig(self);
    }
}