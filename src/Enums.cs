using System.Diagnostics.CodeAnalysis;

namespace NoirCatto;

public static class AbstractObjectType
{
    [AllowNull] public static AbstractPhysicalObject.AbstractObjectType CatSlash = new (nameof(CatSlash), true);

    public static void AbstractPhysicalObjectOnRealize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        orig(self);
        if (self.type == CatSlash)
        {
            var absSlash = (NoirCatto.AbstractCatSlash)self;
            self.realizedObject = new NoirCatto.CatSlash(self, self.world, absSlash.Owner, absSlash.HandUsed, absSlash.SlashType);
        }
    }
}