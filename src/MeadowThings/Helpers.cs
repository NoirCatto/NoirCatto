using RainMeadow;

namespace NoirCatto;

public static partial class MeadowThings
{
    public static bool IsMeadowOnline => NoirCatto.ModRainMeadow && IsOnline;
    public static bool IsOnline => OnlineManager.lobby != null;
}