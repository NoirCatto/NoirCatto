using RainMeadow;

namespace NoirCatto;

public partial class NoirCatto
{
    public partial class CatSlash
    {
        public void OnlineHitSomethingReceive(SharedPhysics.CollisionResult result, bool eu) //Ref: Meadow's Weapon_HitSomething hook
        {
            OnlinePhysicalObject.map.TryGetValue(this.abstractPhysicalObject, out var weaponOnline);
            OnlinePhysicalObject.map.TryGetValue(result.obj.abstractPhysicalObject, out var onlineHit);
            if (onlineHit == null)
            {
                RainMeadow.RainMeadow.Debug($"Object hit by weapon not found in online space. object: {onlineHit}, weapon: {weaponOnline}");
                return;
            }

            if (weaponOnline == null || weaponOnline is not OnlineCatSlash onlineCatSlash)
            {
                RainMeadow.RainMeadow.Debug($"weapon that hit object not found in online space. object: {onlineHit}, weapon: {weaponOnline}");
                return;
            }

            if (onlineCatSlash.HittingRemotely)
            {
                if (this.thrownBy != null && result.obj != null && result.obj is Creature critter)
                {
                    this.thrownClosestToCreature = null;
                    this.closestCritDist = float.MaxValue;
                    critter.SetKillTag(this.thrownBy.abstractCreature);
                }
            }
        }
        
        public void OnlineHitSomethingSend(SharedPhysics.CollisionResult result, bool eu) //Ref: Meadow's Weapon_HitSomething hook
        {
            OnlinePhysicalObject.map.TryGetValue(this.abstractPhysicalObject, out var weaponOnline);
            OnlinePhysicalObject.map.TryGetValue(result.obj.abstractPhysicalObject, out var onlineHit);
            if (onlineHit == null)
            {
                //RainMeadow.RainMeadow.Debug($"Object hit by weapon not found in online space. object: {onlineHit}, weapon: {WeaponOnline}");
                return;
            }

            if (weaponOnline == null || weaponOnline is not OnlineCatSlash onlineCatSlash)
            {
                //RainMeadow.RainMeadow.Debug($"weapon that hit object not found in online space. object: {onlineHit}, weapon: {WeaponOnline}");
                return;
            }

            if (!onlineCatSlash.HittingRemotely && (onlineCatSlash.isMine || onlineCatSlash.isPending))
            {
                var realizedState = new OnlineCatSlash.RealizedCatSlashState(onlineCatSlash);
                
                BodyChunkRef chunk = result.chunk is null ? null : new BodyChunkRef(onlineHit, result.chunk.index);
                AppendageRef appendageRef = result.onAppendagePos is null ? null : new AppendageRef(result.onAppendagePos);
                
                if (!onlineHit.owner.isMe)
                {
                    LogSource.LogInfo($"SendOnlineHit");
                    onlineHit.owner.InvokeRPC(onlineCatSlash.CatSlashHitSomething, realizedState, new OnlinePhysicalObject.OnlineCollisionResult(
                        onlineHit.id, chunk, appendageRef, result.hitSomething, result.collisionPoint
                    ));
                }
            }
        }
        
        public void OnlineSetAsBeingMoved()
        {
            if (abstractPhysicalObject.GetOnlineObject(out var oe) && !oe.isMine)
                oe.beingMoved = true;
        }
        
    }
}