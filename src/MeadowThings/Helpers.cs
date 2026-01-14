using RainMeadow;

namespace NoirCatto;

internal static partial class MeadowThings
{
    public static bool IsMeadowOnline => NoirCatto.ModRainMeadow && IsOnline;
    public static bool IsOnline => OnlineManager.lobby != null;

    public static bool IsStoryMode(out StoryGameMode storyMode)
    {
        storyMode = null;
        return IsMeadowOnline && RainMeadow.RainMeadow.isStoryMode(out storyMode);
    }

    public static bool IsOnlineObjectMine(AbstractPhysicalObject abstractPhysicalObject) => abstractPhysicalObject.GetOnlineObject().isMine;
    
    public static bool IsFriendlyFire => IsStoryMode(out var storyMode) && storyMode.friendlyFire;
}