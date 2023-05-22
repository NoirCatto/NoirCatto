using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace NoirCatto;

public partial class NoirCatto
{
    public static readonly SlugcatStats.Name NoirName = new SlugcatStats.Name("NoirCatto", false);
    //These names are set by MSC after the OptionInterface is already initialized, so OI will nullref if I try to use MSC values...
    public static readonly SlugcatStats.Name GourmandName = new SlugcatStats.Name("Gourmand", false);
    public static readonly SlugcatStats.Name RivuletName = new SlugcatStats.Name("Rivulet", false);
    public static readonly SlugcatStats.Name ArtificerName = new SlugcatStats.Name("Artificer", false);
    public static readonly SlugcatStats.Name SpearName = new SlugcatStats.Name("Spearmaster", false);
    public static readonly SlugcatStats.Name SaintName = new SlugcatStats.Name("Saint", false);
    public static readonly SlugcatStats.Name InvName = new SlugcatStats.Name("Inv", false);
    public static readonly SlugcatStats.Name[] MSCNames = new[] { GourmandName, RivuletName, ArtificerName, SpearName, SaintName, InvName };
    public static readonly bool SetStandingOnUpdate = true;
    public static bool RotundWorld;
    public const float DefaultFirstChunkMass = 0.315f;
    private const int YcounterTreshold = 10;
    

    public static bool IsSmallerThanMe(Creature self, Creature crit)
    {
        return crit.Template.smallCreature || self.TotalMass > crit.TotalMass;
    }

    private void SpearILUpdate(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            c.GotoNext(MoveType.Before,
                i => i.MatchLdsfld<ModManager>("MMF"),
                i => i.MatchBrfalse(out _),
                i => i.MatchLdarg(0),
                i => i.MatchCallOrCallvirt<PhysicalObject>("get_firstChunk"),
                i => i.MatchLdflda<BodyChunk>("vel"),
                i => i.MatchCallOrCallvirt<UnityEngine.Vector2>("get_magnitude"),
                i => i.MatchLdcR4(out _)
            );
            c.GotoNext(MoveType.After, i => i.MatchBrfalse(out label));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Spear self) =>
            {
                if (self.thrownBy is Player pl && pl.SlugCatClass == NoirName && self.alwaysStickInWalls) //MMF? more like SMH
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
}