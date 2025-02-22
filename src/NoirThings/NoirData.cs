using System.Runtime.CompilerServices;

namespace NoirCatto;

public static class NoirCWT
{
    private static readonly ConditionalWeakTable<AbstractCreature, NoirCatto.NoirData> NoirDeets = new ConditionalWeakTable<AbstractCreature, NoirCatto.NoirData>();

    public static bool TryGetNoirData(this Player player, out NoirCatto.NoirData noirData) => TryGetNoirData(player.abstractCreature, out noirData);
    public static bool TryGetNoirData(this AbstractCreature crit, out NoirCatto.NoirData noirData)
    {
        if (crit.creatureTemplate.type == CreatureTemplate.Type.Slugcat &&
            crit.state is PlayerState state && state.slugcatCharacter == Const.NoirName)
        {
            noirData = NoirDeets.GetValue(crit, _ => new NoirCatto.NoirData(crit));
            return true;
        }

        noirData = null;
        return false;
    }
}

public partial class NoirCatto
{
    public class NoirData
    {
        public NoirData(AbstractCreature abstractCat)
        {
            
        }
    }
}