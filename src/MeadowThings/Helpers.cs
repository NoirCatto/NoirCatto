using RainMeadow;
using RainMeadow.Arena.ArenaOnlineGameModes.TeamBattle;

namespace NoirCatto;

internal static partial class MeadowThings
{
    public static bool IsMeadowOnline => NoirCatto.ModRainMeadow && IsOnline;
    public static bool IsOnline => OnlineManager.lobby != null;
    public static bool IsLobbyMine => OnlineManager.lobby.isOwner;

    public static bool IsOnlineObjectMine(AbstractPhysicalObject abstractPhysicalObject) => abstractPhysicalObject.GetOnlineObject().isMine;
    
    public static bool IsStoryFriendlyFireDisabled => RainMeadow.RainMeadow.isStoryMode(out var storyMode) && !storyMode.friendlyFire;
    
    public static bool IsArenaHoldFire => RainMeadow.RainMeadow.isArenaMode(out var arenaMode) && arenaMode.countdownInitiatedHoldFire;
    
    public static bool IsArenaTeammate(Creature crit1, Creature crit2)
    {
        var oc1 = crit1.abstractCreature.GetOnlineCreature();
        var oc2 = crit2.abstractCreature.GetOnlineCreature();
        if (oc1 == null || oc2 == null)
            return false;
        
        return RainMeadow.RainMeadow.isArenaMode(out var arenaMode) && TeamBattleMode.isTeamBattleMode(arenaMode, out _) && ArenaHelpers.CheckSameTeam(oc1.owner, oc2.owner, crit1, crit2);
    }

    public static bool IsOnlineObjectNull(AbstractPhysicalObject abstractPhysicalObject) => abstractPhysicalObject.GetOnlineObject() == null;
}