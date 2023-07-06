using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;

namespace NoirCatto;

public partial class NoirCatto
{
    private void GhostWorldPresenceILSpawnGhost(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            var skip = il.DefineLabel();
            c.GotoNext(MoveType.After,
                i => i.MatchLdarg(2),
                i => i.MatchLdcI4(4),
                i => i.MatchBeq(out _)
            );

            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldarg_2);
            c.EmitDelegate((int karma, int karmaCap) =>
            {
                if (Custom.rainWorld.progression.currentSaveState.saveStateNumber == NoirName)
                {
                    if (karma >= karmaCap)
                    {
                        return true;
                    }
                }
                return false;
            });
            c.Emit(OpCodes.Brfalse, skip);
            c.Emit(OpCodes.Ldc_I4_1);
            c.Emit(OpCodes.Ret);
            c.MarkLabel(skip);

        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }
    
    private void KarmaLadderScreenILGetDataFromGame(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            c.GotoNext(MoveType.After,
                i => i.MatchBr(out label),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<Menu.KarmaLadderScreen>("saveState"),
                i => i.MatchLdfld<SaveState>("saveStateNumber"),
                i => i.MatchLdsfld<MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName>("Saint"),
                i => i.MatchCallOrCallvirt(out _)
            );
            c.GotoPrev(MoveType.After, i => i.MatchLdarg(0));
            c.EmitDelegate((Menu.KarmaLadderScreen self) =>
            {
                if (self.saveState.saveStateNumber == NoirName) return true;
                return false;
            });
            c.Emit(OpCodes.Brtrue, label);
            c.Emit(OpCodes.Ldarg_0);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }
}