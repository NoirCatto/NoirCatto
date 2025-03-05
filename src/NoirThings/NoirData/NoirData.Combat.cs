namespace NoirCatto;

public partial class NoirCatto
{
    public partial class NoirData
    {
        public readonly int[] SlashCooldown = new[] { 0, 0 };
        public int AirSlashCooldown;
        public int AutoSlashCooldown;

        public int ComboBonus;
        public int MovementBonus;
        public int RotundnessBonus;
        public int CombinedBonus => ComboBonus + MovementBonus + RotundnessBonus;
        public int ComboTimer;
        public int rjumpTimer;


        #region CanSlash
        public bool CanSlashInpt
        {
            get
            {
                if (Cat.animation == Player.AnimationIndex.Flip)
                {
                    if (Cat.input[0].x == 0 && Cat.input[0].y == 0) return true;
                }
                else
                {
                    if (Cat.input[0].x == 0) return true;
                }
                return false;
            }
        }
        public bool CanSlash
        {
            get
            {
                if (Cat.input[0].thrw && !Cat.input[1].thrw ||
                    Cat.input[0].thrw && ModOptions.NoirAutoSlash.Value && AutoSlashCooldown == 0)
                {
                    if (!ModOptions.NoirAltSlashConditions.Value)
                    {
                        if (GraspsAllNull ||
                            GraspsAnyNull && (CanSlashInpt || !Cat.IsObjectThrowable(Cat.grasps[0]?.grabbed) || !Cat.IsObjectThrowable(Cat.grasps[1]?.grabbed)))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (GraspsAllNull || GraspsFirstNull ||
                            GraspsAnyNull && !Cat.IsObjectThrowable(Cat.grasps[0]?.grabbed))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        #endregion

        public void ClawHit()
        {
            ComboTimer = 40 * 3; //multiplier is number of seconds
            ComboBonus += 1;
        }

        private void CombatUpdate()
        {
            SlashCooldown[0].Tick();
            SlashCooldown[1].Tick();
            AirSlashCooldown.Tick();
            AutoSlashCooldown.Tick();
            ComboTimer.Tick();
            if (ComboTimer == 0 && ComboBonus > 0)
            {
                ComboBonus = 0;
            }

            if (Cat.animation == Player.AnimationIndex.RocketJump) rjumpTimer++;
            else rjumpTimer = 0;
            if (Cat.animation == Player.AnimationIndex.BellySlide || rjumpTimer >= 15) MovementBonus = 2;
            else MovementBonus = 0;

            if (ModRotundWorld)
            {
                RotundnessBonus = (int)((Cat.bodyChunks[1].mass - DefaultFirstChunkMass) * 15f);
            }
        }
    }
}