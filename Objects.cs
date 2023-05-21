using MoreSlugcats;
using UnityEngine;

namespace NoirCatto;

public partial class NoirCatto
{
    private void AbstractPhysicalObjectOnRealize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        orig(self);
        if (self.type == CatSlash) self.realizedObject = new CatSlash(self, self.world, ((AbstractCatSlash)self).Owner);
    }
    
    private void RoomOnAddObject(On.Room.orig_AddObject orig, Room self, UpdatableAndDeletable obj)
    {
        orig(self, obj);
        if (obj is Player pl && pl.SlugCatClass == NoirName)
        {
            self.AddObject(new CatFur(pl)); // Kitty adapts to the cold
        }
    }
    

    public static readonly AbstractPhysicalObject.AbstractObjectType CatSlash = new AbstractPhysicalObject.AbstractObjectType("CatSlash", true);
    public class AbstractCatSlash : AbstractPhysicalObject
    {
        public readonly Player Owner;
        public AbstractCatSlash(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, Player owner) : base(world, type, realizedObject, pos, ID)
        {
            Owner = owner;
        }
    }

    public class CatFur : UpdatableAndDeletable, IProvideWarmth
    {
        public readonly Player Owner;
        public CatFur(Player owner)
        {
            Owner = owner;
            room = owner.room;
        }
        
        public Vector2 Position()
        {
            return Owner.firstChunk.pos;
        }

        public Room loadedRoom => Owner.room;   
        public float warmth => RainWorldGame.DefaultHeatSourceWarmth;
        public float range => 50f;

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (Owner.room == null || Owner.room != room) this.Destroy();
        }
    }
    
}