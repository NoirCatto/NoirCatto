using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RWCustom;
using UnityEngine;

namespace NoirCatto;

public partial class NoirCatto
{
    #region PlayerData
    public static readonly ConditionalWeakTable<Player, NoirData> NoirDeets = new ConditionalWeakTable<Player, NoirData>();
    public static NoirData NoirDataCtor(Player cat)
    {
        return new NoirData(cat);
    }
    public class NoirData
    {
        public Player Cat;
        public Player.AnimationIndex LastAnimation;
        public Player.BodyModeIndex LastBodyMode;
        public Player.AnimationIndex SpearThrownAnimation;
        public TailSegment[] Ear;
        public TailSegment[] Ear2;
        public int EarFlip = 1; //-1 or 1
        public int Ear2Flip = 1; //-1 or 1
        public Vector2 LastHeadRotation;
        public bool Jumping;
        public bool JumpingFromCrawl;
        public int StandCounter;
        public int Ycounter;
        public bool LastJumpFromHorizontalBeam;
        public bool CallingAddToContainerFromOrigInitiateSprites;
        public Player.InputPackage[] UnchangedInput;
        public bool LastGraspsNull;
        public bool LastGraspsAnyNull;
        public bool LastFirstGraspNull;
        

        public int SlashCooldown;
        public int ComboBonus
        {
            get => comboBonus + movementBonus;
            set => comboBonus = value;
        }
        public int comboBonus;
        public int movementBonus;
        public int comboTimer;
        public int rjumpCounter;

        private Player.AnimationIndex lastAnimationInternal;
        private Player.BodyModeIndex lastBodyModeInternal;

        public NoirData(Player cat)
        {
            Cat = cat;
            UnchangedInput = new Player.InputPackage[cat.input.Length];
        }
        
        #region uh beam stuff I guess
        public bool OnVerticalBeam()
        {
            return Cat.bodyMode == Player.BodyModeIndex.ClimbingOnBeam;
        }
        public bool OnHorizontalBeam()
        {
            return Cat.animation == Player.AnimationIndex.HangFromBeam || Cat.animation == Player.AnimationIndex.StandOnBeam;
        }
        public bool OnAnyBeam()
        {
            return OnVerticalBeam() || OnHorizontalBeam();
        }
        public bool WasOnVerticalBeam()
        {
            return lastBodyModeInternal == Player.BodyModeIndex.ClimbingOnBeam;
        }
        public bool WasOnHorizontalBeam()
        {
            return lastAnimationInternal == Player.AnimationIndex.HangFromBeam || lastAnimationInternal == Player.AnimationIndex.StandOnBeam;
        }
        public bool WasOnAnyBeam()
        {
            return WasOnVerticalBeam() || WasOnHorizontalBeam();
        }
        #endregion

        public bool jumpInitiated;

        public bool CanCrawlOnBeam()
        {
            var graphics = (PlayerGraphics)Cat.graphicsModule;
            if (graphics == null || Cat.room == null) return false;

            var beamLengthL = 0;
            var beamLengthR = 0;
            while (Cat.room.GetTile(Cat.room.GetTilePosition(graphics.legs.pos + new Vector2(beamLengthR, 0f))).horizontalBeam)
            {
                beamLengthR++;
            }
            while (Cat.room.GetTile(Cat.room.GetTilePosition(graphics.legs.pos + new Vector2(-beamLengthL, 0f))).horizontalBeam)
            {
                beamLengthL++;
            }
        
            //Debug.Log($"BEAM LENGTH - LEFT: {beamLengthL}, RIGHT: {beamLengthR}, TOTAL: {beamLengthL + beamLengthR}");
        
            return (Cat.animation == Player.AnimationIndex.StandOnBeam && beamLengthL + beamLengthR > 100 && Ycounter < YcounterTreshold);
        }
        
        public void ClawHit()
        {
            comboTimer = 40 * 3; //multiplier is number of seconds
            ComboBonus = comboBonus + 1;
        }

        private void CombatUpdate()
        {
            if (SlashCooldown > 0) SlashCooldown--;
            if (comboTimer > 0)
            {
                comboTimer--;
            }
            else ComboBonus = 1;

            if (Cat.animation == Player.AnimationIndex.RocketJump) rjumpCounter++;
            else rjumpCounter = 0;
            if (Cat.animation == Player.AnimationIndex.BellySlide || rjumpCounter >= 15) movementBonus = 2;
            else movementBonus = 0;
            
            //Debug.Log($"ComboBonus: {ComboBonus}");
        }
        
        public void Update()
        {
            CombatUpdate();

            if (Jumping)
            {
                if (Cat.mainBodyChunk.pos.y < Cat.mainBodyChunk.lastPos.y && Cat.animation != Player.AnimationIndex.RocketJump &&
                    !jumpInitiated && !Cat.standing && 
                    Cat.bodyMode == Player.BodyModeIndex.Default && Cat.animation == Player.AnimationIndex.None)
                {
                    //Change anim to RocketJump at the peak of the jump
                    Cat.animation = Player.AnimationIndex.RocketJump;
                    jumpInitiated = true;
                }
                
                if (Cat.bodyChunks[1].lastContactPoint == new IntVector2(0, 0) && Cat.bodyChunks[1].contactPoint != new IntVector2(0, 0) || !WasOnAnyBeam() && OnAnyBeam())
                {
                    Jumping = false;
                    JumpingFromCrawl = false;
                    jumpInitiated = false;
                    if (Cat.animation == Player.AnimationIndex.RocketJump)
                    {
                        if (OnVerticalBeam()) Cat.animation = Player.AnimationIndex.ClimbOnBeam;
                        if (OnHorizontalBeam()) Cat.animation = Player.AnimationIndex.HangFromBeam;
                        Cat.animation = Player.AnimationIndex.None;
                    }

                    if (lastAnimationInternal == Player.AnimationIndex.Flip)
                    {
                        if (!SetStandingOnUpdate) Cat.standing = false;
                    }
                }
            }

            lastBodyModeInternal = Cat.bodyMode;
            lastAnimationInternal = Cat.animation;
        }

        public void Update60FPS()
        {
            //Meow!
            if (Cat.stun == 0 && !Cat.dead && Cat.controller is not NoirStartController)
            {
                if (Input.GetKeyDown(Options.MeowKey.Value))
                {
                    Cat.room?.PlaySound(MeowSND, Cat.firstChunk);
                }
            }
        }
    }
    #endregion

    private void PlayerOnctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractcreature, World world)
    {
        orig(self, abstractcreature, world);
        if (self.SlugCatClass != NoirName) return;
        NoirDataCtor(self);
    }

    private void PlayerOnUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        if (self.SlugCatClass != NoirName) return;
        NoirDeets.GetValue(self, NoirDataCtor).Update();
    }
    
    private void PlayerOncheckInput(On.Player.orig_checkInput orig, Player self)
    {
        orig(self);
        if (self.SlugCatClass != NoirName) return;
        
        var noirData = NoirDeets.GetValue(self, NoirDataCtor);

        self.input.CopyTo(noirData.UnchangedInput, 0);

        if (noirData.UnchangedInput[0].y > 0)
        {
            if (noirData.Ycounter < 40) noirData.Ycounter++;
        }
        else
        {
           noirData.Ycounter = 0;
        }
    }
}