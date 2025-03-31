using System.Linq;

namespace NoirCatto.HuntThings;

public partial class HuntQuestThings
{
    public static void HUDOnInitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
    {
        orig(self, cam);
        if (cam.room.game.IsStorySession && !cam.room.game.rainWorld.ExpeditionMode &&
            self.owner is Player && cam.room.game.StoryCharacter == Const.NoirName)
        {
            if (Master != null)
                self.AddPart(new HuntQuestHUD(self));
        }
    }

    public static void CreatureOnDie(On.Creature.orig_Die orig, Creature self)
    {
        if (self.killTag == null)
        {
            if (self.grabbedBy.FirstOrDefault(x => x.grabber is Player)?.grabber is Player player)
                self.SetKillTag(player.abstractCreature);
        }

        orig(self);
    }

    public static void PlayerSessionRecordOnAddEat(On.PlayerSessionRecord.orig_AddEat orig, PlayerSessionRecord self, PhysicalObject eatenobject)
    {
        Master?.AddEat(eatenobject);
        orig(self, eatenobject);
    }
}