using RWCustom;

namespace NoirCatto;

public partial class NoirCatto
{
    public enum CustomStartMode
    {
        StoryAndExpedition = 0,
        Story = 1,
        Disabled = 2,
    }

    private const string StartingRoom = "SI_B12";

    public static void SaveStateOnsetDenPosition(On.SaveState.orig_setDenPosition orig, SaveState self)
    {
        orig(self);

        if (Custom.rainWorld.ExpeditionMode && ModOptions.NoirUseCustomStart.Value != CustomStartMode.StoryAndExpedition) return;
        if (self.saveStateNumber != Const.NoirName) return;
        if (self.cycleNumber == 0)
        {
            self.denPosition = StartingRoom;
        }
    }

    public static void RainWorldGameOnctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);
        if (!self.IsStorySession) return;
        if (self.StoryCharacter != Const.NoirName) return;
        if (Custom.rainWorld.ExpeditionMode && ModOptions.NoirUseCustomStart.Value != CustomStartMode.StoryAndExpedition) return;
        var session = self.GetStorySession;

        if (session.saveState.cycleNumber == 0)
        {
            Room startRoom = null;

            foreach (var player in self.Players)
            {
                if (player.Room.name != StartingRoom) break;
                var pState = (PlayerState)player.state;

                player.pos.Tile = new IntVector2(11 + pState.playerNumber, 54);

                startRoom ??= player.Room.realizedRoom;
            }
            if (ModOptions.NoirUseCustomStart.Value != CustomStartMode.Disabled && startRoom != null)
            {
                startRoom.AddObject(new NoirStart());
            }
        }
    }
}