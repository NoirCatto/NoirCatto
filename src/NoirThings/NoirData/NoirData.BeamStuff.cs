using UnityEngine;

namespace NoirCatto;

public partial class NoirCatto
{
    public partial class NoirData
    {
        #region uh beam stuff I guess
        public bool OnVerticalBeam()
        {
            return Cat?.bodyMode == Player.BodyModeIndex.ClimbingOnBeam;
        }
        public bool OnHorizontalBeam()
        {
            return Cat?.animation == Player.AnimationIndex.HangFromBeam || Cat?.animation == Player.AnimationIndex.StandOnBeam;
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

        #region more beam stuff
        private const int BeamLengthCap = 40;
        public bool CanCrawlOnBeam()
        {
            var graphics = (PlayerGraphics)Cat?.graphicsModule;
            if (graphics == null || Cat?.room == null) return false;

            var beamLengthL = 0;
            var beamLengthR = 0;
            while (beamLengthR <= BeamLengthCap && Cat.room.GetTile(Cat.room.GetTilePosition(graphics.legs.pos + new Vector2(beamLengthR, 0f))).horizontalBeam)
            {
                beamLengthR++;
            }
            while (beamLengthL <= BeamLengthCap && Cat.room.GetTile(Cat.room.GetTilePosition(graphics.legs.pos + new Vector2(-beamLengthL, 0f))).horizontalBeam)
            {
                beamLengthL++;
            }

            return (Cat.animation == Player.AnimationIndex.StandOnBeam && beamLengthL + beamLengthR > BeamLengthCap && (!YinputForPole || YinputForPoleBlocker > 0));
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

            return beamLengthL + beamLengthR > BeamLengthCap;
        }
        #endregion
    }
}