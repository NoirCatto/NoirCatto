using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NoirCatto;

public partial class NoirCatto
{
    public static readonly AbstractPhysicalObject.AbstractObjectType CatSlash = new AbstractPhysicalObject.AbstractObjectType("CatSlash", true);
    
    private void AbstractPhysicalObjectOnRealize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        orig(self);
        if (self.type == CatSlash) self.realizedObject = new CatSlash(self, self.world, ((AbstractCatSlash)self).Owner);
    }
    
    private PhysicalObject PlayerOnPickupCandidate(On.Player.orig_PickupCandidate orig, Player self, float favorspears) //Favor grabbing stunned critters over spears
    {
        if (self.SlugCatClass != NoirName) return orig(self, favorspears);
        
        for (var i = 0; i < self.room.physicalObjects.Length; i++)
        {
            for (var j = 0; j < self.room.physicalObjects[i].Count; j++)
            {
                if (Custom.DistLess(self.bodyChunks[0].pos, self.room.physicalObjects[i][j].bodyChunks[0].pos, self.room.physicalObjects[i][j].bodyChunks[0].rad + 40f) 
                    && (Custom.DistLess(self.bodyChunks[0].pos, self.room.physicalObjects[i][j].bodyChunks[0].pos, self.room.physicalObjects[i][j].bodyChunks[0].rad + 20f) 
                        || self.room.VisualContact(self.bodyChunks[0].pos, self.room.physicalObjects[i][j].bodyChunks[0].pos)) && self.CanIPickThisUp(self.room.physicalObjects[i][j]))
                {
                    if (self.room.physicalObjects[i][j] is Creature crit)
                    {
                        if (crit.stun >= 20)
                        {
                            if (crit.State is HealthState healthState)
                            {
                                if (healthState.health <= 0) return crit;
                            }
                            else if (!crit.State.dead) return crit;
                        }
                    }
                }
            }
        }

        return orig(self, favorspears);
    }
    
    private void PlayerOnGrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu) //Maul bonus prototype
    {
        orig(self, eu);
        if (self.SlugCatClass != NoirName) return;
        
        var whichGrasp = 0;
        if (ModManager.MMF && (self.grasps[0]?.grabbed is not Creature) && self.grasps[1]?.grabbed is Creature)
        {
            whichGrasp = 1;
        }

        if (self.grasps[whichGrasp] != null && self.maulTimer % 20 == 0 && self.maulTimer % 40 != 0)
        {
            self.room.PlaySound(SoundID.Slugcat_Eat_Meat_B, self.mainBodyChunk);
            self.room.PlaySound(SoundID.Drop_Bug_Grab_Creature, self.mainBodyChunk, false, 1f, 0.76f);
            if (RainWorld.ShowLogs)
            {
                Debug.Log("Mauled target");
            }

            if (self.grasps[whichGrasp].grabbed is Creature crit && !crit.dead)
            {
                for (var num12 = Random.Range(8, 14); num12 >= 0; num12--)
                {
                    self.room.AddObject(new WaterDrip(Vector2.Lerp(self.grasps[whichGrasp].grabbedChunk.pos, self.mainBodyChunk.pos, Random.value) + self.grasps[whichGrasp].grabbedChunk.rad * Custom.RNV() * Random.value, Custom.RNV() * 6f * Random.value + Custom.DirVec(self.grasps[whichGrasp].grabbed.firstChunk.pos, (self.mainBodyChunk.pos + (self.graphicsModule as PlayerGraphics).head.pos) / 2f) * 7f * Random.value + Custom.DegToVec(Mathf.Lerp(-90f, 90f, Random.value)) * Random.value * self.EffectiveRoomGravity * 7f, false));
                }

                crit.SetKillTag(self.abstractCreature);
                float dmgToDeal = 1f;
                var critHealth = ((HealthState)crit.State).health;
                
                var dmg = critHealth * crit.Template.baseDamageResistance * 0.5f;
                if (dmg > 1f)
                {
                    dmgToDeal = dmg;
                }
                
                //Debug.Log(dmgToDeal);
                crit.Violence(self.bodyChunks[0], new Vector2?(new Vector2(0f, 0f)), self.grasps[whichGrasp].grabbedChunk, null, Creature.DamageType.Bite, dmgToDeal, 15f);
                crit.Stun(5);

                //Debug.Log($"Health: {((HealthState)crit.State).health}");

                if (critHealth <= 0f)
                {
                    Debug.Log("Creature health below zero, releasing...");
                    self.TossObject(whichGrasp, eu);
                    self.ReleaseGrasp(whichGrasp);
                }

            }
        }
    }

    private static bool CanSlash(Player self)
    {
        if (self.animation == Player.AnimationIndex.Flip)
        {
            if (self.input[0].x == 0 && self.input[0].y == 0) return true;
        }
        else
        {
            if (self.input[0].x == 0) return true;
        }
        return false;
    }
    
    private void CustomCombatUpdate(Player self, bool eu)
    {
        var noirData = NoirDeets.GetValue(self, NoirDataCtor);
        
        // //AutoSlash hack
        // if (noirData.SlashCooldown > 0) return;
        // for (var i = 0; i < self.room.physicalObjects.Length; i++)
        // {
        //     for (var j = 0; j < self.room.physicalObjects[i].Count; j++)
        //     {
        //         if (self.room.physicalObjects[i][j] is Spear spir && spir.mode == Weapon.Mode.Thrown && 
        //             Custom.DistLess(self.firstChunk.pos, spir.firstChunk.pos, spir.firstChunk.vel.magnitude * 3f + spir.firstChunk.rad + 30f) &&
        //             spir.thrownBy != self)
        //         {
        //             var slash = new AbstractCatSlash(self.room.world, CatSlash, null, self.abstractCreature.pos, self.room.game.GetNewID(), self);
        //             noirData.SlashCooldown = 40;
        //             slash.RealizeInRoom();
        //         }
        //     }
        // }
        
        if (noirData.SlashCooldown > 0) return;
        if (!Options.AlternativeSlashConditions.Value && (self.input[0].thrw && !self.input[1].thrw && (noirData.LastGraspsNull || noirData.LastGraspsAnyNull && 
        (CanSlash(self) || !self.IsObjectThrowable(self.grasps[0]?.grabbed) || !self.IsObjectThrowable(self.grasps[1]?.grabbed)))) 
            ||
        (Options.AlternativeSlashConditions.Value && (self.input[0].thrw && !self.input[1].thrw && (noirData.LastGraspsNull || noirData.LastFirstGraspNull
            || noirData.LastGraspsAnyNull && !self.IsObjectThrowable(self.grasps[0]?.grabbed)))))
        {
            var slash = new AbstractCatSlash(self.room.world, CatSlash, null, self.abstractCreature.pos, self.room.game.GetNewID(), self);
            noirData.SlashCooldown = 40;
            slash.RealizeInRoom();
        }
    }
}

public class AbstractCatSlash : AbstractPhysicalObject
{
    public readonly Player Owner;
    public AbstractCatSlash(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, Player owner) : base(world, type, realizedObject, pos, ID)
    {
        Owner = owner;
    }
}

public class CatSlash : Weapon
{
    public readonly Player Owner;
    private readonly NoirCatto.NoirData noirData;
    public readonly int Direction;
    private int MaxLifetime = 40;
    private int lifeTime;
    private float Radius = 30f;
    private float TraveledAngle;
    
    private readonly List<Creature> CreaturesHit = new List<Creature>();

    public override bool HeavyWeapon => true;

    public CatSlash(AbstractPhysicalObject abstractPhysicalObject, World world, Player owner) : base(abstractPhysicalObject, world)
    {
        Owner = owner;
        noirData = NoirCatto.NoirDeets.GetValue(owner, NoirCatto.NoirDataCtor);
        Direction = owner.flipDirection;
        
        bodyChunks = new[] { new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f) };
        bodyChunkConnections = Array.Empty<BodyChunkConnection>();
        airFriction = 0.999f;
        gravity = 0f;
        bounce = 0f;
        surfaceFriction = 0.999f;
        waterFriction = 0.999f;
        waterRetardationImmunity = 1f;
        buoyancy = 0f;
        exitThrownModeSpeed = float.MinValue;
        rotation = Vector2.zero;
        Direction = Owner.flipDirection;
        tailPos = firstChunk.pos;
        firstChunk.loudness = 9f;
        soundLoop = new ChunkDynamicSoundLoop(firstChunk);

        if (owner.animation == Player.AnimationIndex.BellySlide)
        {
            Radius += 10f;
        }

        firstChunk.pos = GetSpawnPosition();
        firstChunk.lastPos = firstChunk.pos;
        tailPos = firstChunk.pos;
        
        collisionLayer = 0; //default = 2;
        firstChunk.collideWithTerrain = false;
        firstChunk.collideWithObjects = true;
        
        mode = Mode.Thrown;
        thrownBy = owner;
        throwDir = new IntVector2(Direction, 0);
        firstChunk.vel.x = Direction * Radius * 0.6f;
    }

    private Vector2 GetSpawnPosition()
    {
        var spawnPosition = Owner.firstChunk.pos + Custom.RotateAroundOrigo(new Vector2(Radius * Direction, 0f), 90f * -Direction);
        return spawnPosition;
    }
    
    public override void PlaceInRoom(Room placeRoom)
    {
        placeRoom.AddObject(this);
        
        if (noirData.movementBonus > 0 || noirData.ComboBonus >= 5) placeRoom.PlaySound(NoirCatto.SlashSND, firstChunk);
        else placeRoom.PlaySound(SoundID.Slugcat_Throw_Rock, firstChunk, false, 0.5f, 1.2f);
    }

    public override void Update(bool eu)
    {
        lifeTime++;
        if (lifeTime >= MaxLifetime)
        {
            Destroy();
            return;
        }

        for (var i = 0; i < room?.physicalObjects.Length; i++)
        {
            for (var j = 0; j < room?.physicalObjects[i].Count; j++)
            {
                if (room?.physicalObjects[i][j] is Weapon wep && wep.mode == Mode.Thrown)
                {
                    if (wep is CatSlash slash && slash.Owner == Owner) break;
                    if (!Custom.DistLess(wep.firstChunk.pos, firstChunk.pos, wep.firstChunk.vel.magnitude + Radius)) break;

                    var wepDir = Custom.AimFromOneVectorToAnother(wep.firstChunk.lastPos, wep.firstChunk.pos);
                    var thisDir = Custom.AimFromOneVectorToAnother(Owner.firstChunk.pos, firstChunk.pos);

                    const int maxAngle = 90;
                    if (wepDir + thisDir <= maxAngle && wepDir + thisDir >= -maxAngle)
                    {
                        HitAnotherThrownWeapon(wep);
                    }
                }
            }
        }

        RotationUpdate(eu);
        base.Update(eu);
        var posAdjustment = Owner.firstChunk.pos - Owner.firstChunk.lastPos;
        firstChunk.pos = Vector2.Lerp(firstChunk.pos, firstChunk.pos + posAdjustment, 1f);

        soundLoop.sound = SoundID.None;
    }

    private void RotationUpdate(bool eu)
    {
        var ang = Mathf.Acos(firstChunk.vel.magnitude / (Radius));
        ang *= Mathf.Rad2Deg;
        ang = 90 - ang;

        // Debug.Log($"Vel Magnitude: {firstChunk.vel.magnitude}");
        // Debug.Log($"Angle : {ang}");
        // Debug.Log($"Rotation : {TraveledAngle}");

        if (mode == Mode.Free) return;
        
        firstChunk.vel = Custom.RotateAroundOrigo(firstChunk.vel, ang * Direction);
        TraveledAngle += ang;
        if (TraveledAngle >= 180f)
        {
            Destroy();
        }
    }
    
    
    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        var directionAndMomentum = firstChunk.vel * Custom.PerpendicularVector(firstChunk.vel) * firstChunk.mass;
        
        if (result.obj == null)
        {
            return false;
        }
        
        if (result.obj is Creature crit)
        {
            if (crit == Owner) return false;
            if (CreaturesHit.Contains(crit)) return false;
            if (((ModManager.CoopAvailable && !Custom.rainWorld.options.friendlyFire) || 
                 room.game.IsArenaSession && !room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.spearsHitPlayers ) && 
                crit is Player) return false;
            
            var smallCrit = NoirCatto.IsSmallerThanMe(Owner, crit);
            //
            //if (!smallCrit) ChangeMode(Mode.Free);

            var stunBonus = 20f * noirData.ComboBonus;
            if (smallCrit)
            {
                stunBonus += 30f;
            }

            var damage = 0.2f + 0.2f * noirData.ComboBonus;
            if (crit is Player) damage *= 2f;

            (result.obj as Creature).Violence(firstChunk, directionAndMomentum, result.chunk, result.onAppendagePos, Creature.DamageType.Stab, damage, stunBonus);
            CreaturesHit.Add(crit);
            noirData.ClawHit();
            if (!smallCrit) firstChunk.vel = firstChunk.vel * 0.5f + Custom.DegToVec(90f) * 0.1f * firstChunk.vel.magnitude;
            
            if (crit is Lizard liz && result.chunk.index == 0)
            {
                if (liz.HitHeadShield(directionAndMomentum))
                {
                    Owner.Stun(30);
                    noirData.SlashCooldown = 50;
                    room?.PlaySound(NoirCatto.MeowFrustratedSND, Owner.firstChunk);
                }
            }
        }
        
        else if (result.chunk != null)
        {
            //change direction to perpendicular?
            result.chunk.vel += firstChunk.vel * firstChunk.mass / result.chunk.mass;
        }
        else if (result.onAppendagePos != null)
        {
            //change direction to perpendicular?
            ((IHaveAppendages)result.obj).ApplyForceOnAppendage(result.onAppendagePos, firstChunk.vel * firstChunk.mass);
        }

        room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, firstChunk, false , 0.30f, 0.65f);
        if (result.chunk != null)
        {
            room.AddObject(new ExplosionSpikes(room, result.chunk.pos + Custom.DirVec(result.chunk.pos, result.collisionPoint) * result.chunk.rad, 5, 2f, 4f, 4.5f, 30f, new Color(1f, 1f, 1f, 0.5f)));
        }
        
        return true;
    }

    public override void HitSomethingWithoutStopping(PhysicalObject obj, BodyChunk chunk, Appendage appendage)
    {
        if (obj is Creature crit)
        {
            crit.SetKillTag(Owner.abstractCreature);
            if (NoirCatto.IsSmallerThanMe(Owner, crit)) crit.Die();
            else crit.Stun(80);
        }
        obj.HitByWeapon(this);
        noirData.ClawHit();
    }

    public override void WeaponDeflect(Vector2 inbetweenPos, Vector2 deflectDir, float bounceSpeed)
    {
        firstChunk.pos = Vector2.Lerp(firstChunk.pos, inbetweenPos, 0.5f);
        firstChunk.vel = deflectDir * bounceSpeed * 0.5f;
        ChangeMode(Mode.Free);
        noirData.ClawHit();
        room?.PlaySound(NoirCatto.MeowFrustratedSND, Owner.firstChunk);
    }

    public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
    }
    
    public override void PickedUp(Creature upPicker)
    {
    }
    
    public override void ChangeMode(Mode newMode)
    {
        if (newMode == mode)
            return;
        if (newMode == Mode.Thrown || newMode == Mode.StuckInWall)
            ChangeCollisionLayer(0);
        else
            ChangeCollisionLayer(DefaultCollLayer);
        if (newMode != Mode.Thrown)
        {
            throwModeFrames = -1;
            firstFrameTraceFromPos = new Vector2?();
        }

        if (newMode == Mode.Free)
        {
            var extraTime = 5;
            if (lifeTime < MaxLifetime - extraTime)
            {
                lifeTime = MaxLifetime - extraTime;
            }
        }
        
        firstChunk.goThroughFloors = true;
        thrownClosestToCreature = null;
        closestCritDist = float.MaxValue;
        mode = newMode;
    }

    #region Graphics
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];
        sLeaser.sprites[1] = new FSprite("Pebble" + Random.Range(1, 15).ToString());
        
        TriangleMesh.Triangle[] tris = {
            new TriangleMesh.Triangle(0, 1, 2),
            new TriangleMesh.Triangle(3, 4, 5)
        };
        var triangleMesh = new TriangleMesh("Futile_White", tris, false);
        sLeaser.sprites[0] = triangleMesh;
        AddToContainer(sLeaser, rCam, null);
    }
    
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[1].isVisible = true; //Debug flag
        var vector2_1 = Vector2.Lerp(this.firstChunk.lastPos, this.firstChunk.pos, timeStacker);
        if (this.vibrate > 0)
            vector2_1 += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
        sLeaser.sprites[1].x = vector2_1.x - camPos.x;
        sLeaser.sprites[1].y = vector2_1.y - camPos.y;
        var p2 = Vector3.Slerp((Vector3) this.lastRotation, (Vector3) this.rotation, timeStacker);
        sLeaser.sprites[1].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0.0f, 0.0f), (Vector2) p2);
        
        
        var firstChunkPosLerpd = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        var tailPosLerpd = Vector2.Lerp(tailPos, firstChunk.lastPos, timeStacker);

        var perpendicularVector = Custom.PerpendicularVector((firstChunkPosLerpd - tailPosLerpd).normalized);

        var vel = firstChunk.vel.magnitude * 0.25f;

        var direction = (tailPosLerpd - firstChunkPosLerpd).normalized;
        var newTailPos = tailPosLerpd + Custom.RotateAroundOrigo(new Vector2(0f, vel), Custom.VecToDeg(direction));
        var newLeadingPos = firstChunkPosLerpd + Custom.RotateAroundOrigo(new Vector2(0f, -vel), Custom.VecToDeg(direction));


        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(0, firstChunkPosLerpd + perpendicularVector * 2.5f - camPos);
        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(1, firstChunkPosLerpd - perpendicularVector * 2.5f - camPos);
        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(2, newTailPos - camPos);
        
        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(3, firstChunkPosLerpd + perpendicularVector * 2.5f - camPos);
        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(4, firstChunkPosLerpd - perpendicularVector * 2.5f - camPos);
        ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(5, newLeadingPos - camPos);

        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }
    
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        color = Color.white;
        sLeaser.sprites[1].color = Color.red;
        sLeaser.sprites[0].color = Color.Lerp(Color.white, Color.red, (noirData.ComboBonus - 1) / 5f);
    }
    #endregion
}