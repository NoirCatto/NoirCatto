using System.Linq;
using System.Runtime.CompilerServices;
using RWCustom;
using UnityEngine;

namespace NoirCatto;

public static class NoirCWT
{
    private static readonly ConditionalWeakTable<AbstractCreature, NoirCatto.NoirData> NoirDeets = new ConditionalWeakTable<AbstractCreature, NoirCatto.NoirData>();

    public static bool TryGetNoirData(this Player player, out NoirCatto.NoirData noirData) => TryGetNoirData(player.abstractCreature, out noirData);
    public static bool TryGetNoirData(this AbstractCreature crit, out NoirCatto.NoirData noirData)
    {
        if ((crit.creatureTemplate.type == CreatureTemplate.Type.Slugcat || crit.state is PlayerState) && crit.SlugCatClass() == Const.NoirName)
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
    public partial class NoirData
    {
        public readonly AbstractCreature AbstractCat;
        public Player Cat => AbstractCat.realizedCreature as Player;
        public Player.AnimationIndex LastAnimation;
        public Player.AnimationIndex SpearThrownAnimation;

        public int StandCounter;
        public int CrawlTurnCounter;
        public int AfterCrawlTurnCounter;
        public int SuperCrawlPounce;
        public int ClimbCounter;
        public int ClimbCooldown;
        public bool Jumping;
        public bool JumpInitiated;
        public bool LastJumpFromHorizontalBeam;
        public bool FrontCrawlFlip;

        public bool GraspsAllNull;
        public bool GraspsAnyNull;
        public bool GraspsFirstNull;

        public int MeowCounter = 0;
        public int SinceLastMeowCounter = 0;
        public float BaseMeowPitch = 1f;
        public float RotundMeowPitch = 0f;
        public float MeowPitch
        {
            get
            {
                var pitch = BaseMeowPitch - RotundMeowPitch;
                if (pitch < 0.15f) pitch = 0.15f;
                return pitch;
            }
        }
        
        public const float DefaultFirstChunkMass = 0.315f;

        public int FlipDirection
        {
            get
            {
                if (Cat == null) return 1;
                
                if (Mathf.Abs(Cat.bodyChunks[0].pos.x - Cat.bodyChunks[1].pos.x) < 2f)
                {
                    return Cat.flipDirection;
                }
                else
                {
                    return Cat.bodyChunks[0].pos.x > Cat.bodyChunks[1].pos.x ? 1 : -1;
                }
            }
        }

        private Player.AnimationIndex lastAnimationInternal;
        private Player.BodyModeIndex lastBodyModeInternal;
        
        //ctor and methods
        public NoirData(AbstractCreature abstractCat)
        {
            AbstractCat = abstractCat;
        }

        public void Update()
        {
            CombatUpdate();

            if (Jumping)
            {
                if (Cat.mainBodyChunk.pos.y < Cat.mainBodyChunk.lastPos.y && Cat.animation != Player.AnimationIndex.RocketJump &&
                    !JumpInitiated && !Cat.standing &&
                    Cat.bodyMode == Player.BodyModeIndex.Default && Cat.animation == Player.AnimationIndex.None)
                {
                    //Change anim to RocketJump at the peak of the jump
                    Cat.animation = Player.AnimationIndex.RocketJump;
                    JumpInitiated = true;
                }

                if (Cat.bodyChunks[1].lastContactPoint == new IntVector2(0, 0) && Cat.bodyChunks[1].contactPoint != new IntVector2(0, 0) || !WasOnAnyBeam() && OnAnyBeam())
                {
                    Jumping = false;
                    JumpInitiated = false;
                    if (Cat.animation == Player.AnimationIndex.RocketJump)
                    {
                        if (OnVerticalBeam()) Cat.animation = Player.AnimationIndex.ClimbOnBeam;
                        if (OnHorizontalBeam()) Cat.animation = Player.AnimationIndex.HangFromBeam;
                        Cat.animation = Player.AnimationIndex.None;
                    }
                }
            }

            if (ModRotundWorld)
            {
                RotundMeowPitch = (Cat.bodyChunks[1].mass - DefaultFirstChunkMass) * 0.65f;
            }
            
            YinputForPoleBlocker.Tick();
            SinceLastMeowCounter.Tick();
            if (SinceLastMeowCounter == 0) MeowCounter = 0;
            lastBodyModeInternal = Cat.bodyMode;
            lastAnimationInternal = Cat.animation;
            GraspsAllNull = Cat.grasps.All(x => x is null);
            GraspsAnyNull = Cat.grasps.Any(x => x is null);
            GraspsFirstNull = Cat.grasps[0] == null;
        }

        public void UpdateRealTime()
        {
            MeowUpdate(this);
        }
    }

    //Hooks
    public static void PlayerOnUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (!self.TryGetNoirData(out var noirData)) return;
        noirData.Update();
    }

    public static void RainWorldOnUpdate(On.RainWorld.orig_Update orig, RainWorld self)
    {
        orig(self);
        if (self.processManager?.currentMainLoop is not RainWorldGame game) return;
        if (game.GamePaused) return;
        foreach (var absPlayer in game.Players)
        {
            var player = (Player)absPlayer.realizedCreature;
            if (player == null) continue;

            if (!player.TryGetNoirData(out var noirData)) continue;
            noirData.UpdateRealTime();
        }
    }
}