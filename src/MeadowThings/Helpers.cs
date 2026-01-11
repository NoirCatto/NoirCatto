using RainMeadow;

namespace NoirCatto;

public static partial class MeadowThings
{
    public static bool IsMeadowOnline => NoirCatto.ModRainMeadow && IsOnline;
    public static bool IsOnline => OnlineManager.lobby != null;

    public static bool IsOnlineObjectMine(AbstractPhysicalObject abstractPhysicalObject) => abstractPhysicalObject.GetOnlineObject().isMine;
}