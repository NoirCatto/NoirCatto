namespace NoirCatto;

public partial class NoirCatto
{
    private void AbstractCreatureOnRealize(On.AbstractCreature.orig_Realize orig, AbstractCreature self)
    {
        orig(self);
    }
}