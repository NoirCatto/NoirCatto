using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MoreSlugcats;
using Noise;
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
        public int CrawlTurnCounter;
        public int AfterCrawlTurnCounter;
        public int SuperCrawlPounce;
        public int Ycounter;
        public bool LastJumpFromHorizontalBeam;
        public bool CallingAddToContainerFromOrigInitiateSprites;
        public Player.InputPackage[] UnchangedInput;
        public bool LastGraspsNull;
        public bool LastGraspsAnyNull;
        public bool LastFirstGraspNull;
        public float MeowPitch = 1f;

        public int FlipDirection
        {
            get
            {
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

        public int SlashCooldown;
        public int ComboBonus
        {
            get => _comboBonus + movementBonus + rotundnessBonus;
            set => _comboBonus = value;
        }
        public int _comboBonus;
        public int movementBonus;
        public int rotundnessBonus; // RotundWorld integration!
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
        
            return (Cat.animation == Player.AnimationIndex.StandOnBeam && beamLengthL + beamLengthR > 40 && Ycounter < YcounterTreshold);
        }
        
        public bool CanGrabBeam()
        {
            var graphics = (PlayerGraphics)Cat.graphicsModule;
            if (graphics == null || Cat.room == null) return false;

            Vector2 pos;
            if (Cat.room.GetTile(Cat.room.GetTilePosition(Cat.bodyChunks[0].pos)).horizontalBeam) pos = Cat.bodyChunks[0].pos;
            else if (Cat.room.GetTile(Cat.room.GetTilePosition(Cat.bodyChunks[1].pos)).horizontalBeam) pos = Cat.bodyChunks[1].pos;
            else if (Cat.room.GetTile(Cat.room.GetTilePosition(graphics.hands[0].pos)).horizontalBeam) pos = graphics.hands[0].pos;
            else if (Cat.room.GetTile(Cat.room.GetTilePosition(graphics.hands[1].pos)).horizontalBeam) pos = graphics.hands[1].pos;
            else return false;
            
            // Debug.Log("Found a beam tile!");

            var beamLengthL = 0;
            var beamLengthR = 0;
            while (Cat.room.GetTile(Cat.room.GetTilePosition(pos + new Vector2(beamLengthR, 0f))).horizontalBeam)
            {
                beamLengthR++;
            }
            while (Cat.room.GetTile(Cat.room.GetTilePosition(pos + new Vector2(-beamLengthL, 0f))).horizontalBeam)
            {
                beamLengthL++;
            }
        
            // Debug.Log($"BEAM LENGTH - LEFT: {beamLengthL}, RIGHT: {beamLengthR}, TOTAL: {beamLengthL + beamLengthR}");
        
            return beamLengthL + beamLengthR > 40;
        }
        
        public void ClawHit()
        {
            comboTimer = 40 * 3; //multiplier is number of seconds
            ComboBonus = _comboBonus + 1;
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
            
            if (RotundWorld)
            {
                rotundnessBonus = (int)((Cat.bodyChunks[1].mass - DefaultFirstChunkMass) * 15f);
            }
            // Debug.Log($"Noir's rotundness: {Cat.bodyChunks[1].mass}");
            // Debug.Log($"Rotundness bonus: {(Cat.bodyChunks[1].mass - 0.315f) * 15f}"); //0.315f is default Noir's firstchunk mass
            // Debug.Log($"Rotundness bonus INT: {rotundnessBonus}");
            // Debug.Log($"Combo bonus: {ComboBonus}");
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
            
            if (RotundWorld)
            {
                MeowPitch = 1f - (Cat.bodyChunks[1].mass - DefaultFirstChunkMass) * 0.65f;
                if (MeowPitch < 0.15f) MeowPitch = 0.15f;
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
                    Cat.room?.PlaySound(MeowSND, Cat.firstChunk, false, 1f, MeowPitch);
                    if (Options.AttractiveMeow.Value) Cat.room?.InGameNoise(new InGameNoise(Cat.firstChunk.pos, 300f, Cat, 1f));
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

        //Moving all inputs one slot up
        for (var i = noirData.UnchangedInput.Length - 1; i > 0; i--)
        {
            noirData.UnchangedInput[i] = noirData.UnchangedInput[i - 1];
        }
        //Copying original unmodified input
        noirData.UnchangedInput[0] = self.input[0];
        

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