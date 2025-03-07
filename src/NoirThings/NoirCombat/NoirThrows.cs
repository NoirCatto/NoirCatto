using MoreSlugcats;
using RWCustom;

namespace NoirCatto;

public partial class NoirCatto
{
    public static void PlayerOnThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
    {
        if (!self.TryGetNoirData(out var noirData))
        {
            orig(self, grasp, eu);
            return;
        }

        if (noirData.CanSlash) return;
        if (ModOptions.NoirAutoSlash.Value && noirData.AutoSlashCooldown == 0)
            noirData.AutoSlashCooldown += CatSlash.BaseAutoSlashCooldown;

        Weapon thrownWeapon = null;
        if (self.grasps[grasp].grabbed is Weapon w)
        {
            thrownWeapon = w; //Get held weapon before it's thrown
        }

        orig(self, grasp, eu);

        switch (thrownWeapon)
        {
            case Spear:
                break; //Handled in OnThrownSpear
            case ScavengerBomb:
                thrownWeapon.firstChunk.vel *= 0.5f;
                break;
            case SingularityBomb:
                thrownWeapon.firstChunk.vel *= 0.6f;
                break;
            case not null:
                thrownWeapon.exitThrownModeSpeed = 10f;
                thrownWeapon.firstChunk.vel *= 0.65f;
                break;
        }
    }

    public static void PlayerOnThrownSpear(On.Player.orig_ThrownSpear orig, Player self, Spear spear)
    {
        orig(self, spear);
        if (!self.TryGetNoirData(out var noirData)) return;

        noirData.SpearThrownAnimation = self.animation;
        if (spear.exitThrownModeSpeed > 10f)
            spear.exitThrownModeSpeed = 10f;
        spear.spearDamageBonus = 0.4f;

        if (spear.abstractSpear.explosive || spear.abstractSpear.electric)
        {
            spear.firstChunk.vel *= 0.38f;
        }
        else if (spear.bugSpear)
        {
            spear.firstChunk.vel *= 0.77f;
        }
        else
        {
            spear.firstChunk.vel *= 0.5f;
        }
    }

    public static void SpearOnUpdate(On.Spear.orig_Update orig, Spear self, bool eu)
    {
        if (self.thrownBy is Player pl && pl.TryGetNoirData(out var noirData))
        {
            if (Custom.DistLess(self.thrownPos, self.firstChunk.pos, 75f) ||  noirData.SpearThrownAnimation == Player.AnimationIndex.BellySlide)
            {
                self.alwaysStickInWalls = true;
            }
            else
            {
                self.alwaysStickInWalls = false;
            }
        }
        orig(self, eu);
    }
}