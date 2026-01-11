using System;
using JetBrains.Annotations;
using RainMeadow;

namespace NoirCatto;

public class OnlineCatSlash : OnlinePhysicalObject
{
    public int Direction; //Only set once
    
    public class OnlineCatSlashDefinition : OnlinePhysicalObjectDefinition
    {
        [OnlineField]
        public EntityId OwnerId;
        [OnlineField]
        public byte SlashType;
        [OnlineField]
        public bool HandUsed;
        [OnlineField]
        public byte ComboBonus;
        [OnlineField] 
        public bool Direction;
        
        [CanBeNull] public PhysicalObject Owner => (OwnerId?.FindEntity() as OnlinePhysicalObject)?.apo.realizedObject;
        
        public OnlineCatSlashDefinition() {}

        public OnlineCatSlashDefinition(OnlineCatSlash onlineCatSlash, OnlineResource inResource) : base(onlineCatSlash, inResource)
        {
            this.OwnerId = onlineCatSlash.AbstractCatSlash.Owner.abstractPhysicalObject.GetOnlineObject().id;
            this.SlashType = (byte)onlineCatSlash.AbstractCatSlash.SlashType;
            this.HandUsed = Convert.ToBoolean(onlineCatSlash.AbstractCatSlash.HandUsed);
            onlineCatSlash.Direction = onlineCatSlash.AbstractCatSlash.Owner.flipDirection;
            this.Direction = onlineCatSlash.Direction == 1;
            if (onlineCatSlash.AbstractCatSlash.Owner.TryGetNoirData(out var noirData))
            {
                var comboBonus = noirData.ComboBonus;
                if (comboBonus > byte.MaxValue)
                    comboBonus = byte.MaxValue;
                this.ComboBonus = (byte)comboBonus;
            }
        }

        public override OnlineEntity MakeEntity(OnlineResource inResource, EntityState initialState)
        {
            return new OnlineCatSlash(this, inResource, (OnlineCatSlashState)initialState);
        }
    }
    
    public OnlineCatSlash(OnlineCatSlashDefinition entityDefinition, OnlineResource inResource, OnlineCatSlashState initialState) : base(entityDefinition, inResource, initialState)
    {
    }
    
    public OnlineCatSlash(NoirCatto.AbstractCatSlash abstractCatSlash, EntityId id, OnlinePlayer owner, bool isTransferable) : base(abstractCatSlash, id, owner, isTransferable)
    {
    }
    
    public NoirCatto.AbstractCatSlash AbstractCatSlash => apo as NoirCatto.AbstractCatSlash;

    protected override AbstractPhysicalObject ApoFromDef(OnlinePhysicalObjectDefinition newObjectEvent, OnlineResource inResource, AbstractPhysicalObjectState initialState)
    {
        var entityDefinition = (OnlineCatSlashDefinition)newObjectEvent;
        var apo = base.ApoFromDef(newObjectEvent, inResource, initialState);
        Direction = entityDefinition.Direction == true ? 1 : -1;
        
        var slashOwner = entityDefinition.Owner as Player;
        if (slashOwner.TryGetNoirData(out var noirData)) 
            noirData.ComboBonus = entityDefinition.ComboBonus;
        
        var abstractCatSlash = new NoirCatto.AbstractCatSlash(apo.world, AbstractObjectType.CatSlash, null, slashOwner.abstractCreature.pos, apo.ID, slashOwner, Convert.ToInt32(entityDefinition.HandUsed), (NoirCatto.SlashType)entityDefinition.SlashType);

        return abstractCatSlash;
    }

    public override EntityDefinition MakeDefinition(OnlineResource onlineResource)
    {
        return new OnlineCatSlashDefinition(this, onlineResource);
    }

    protected override EntityState MakeState(uint tick, OnlineResource inResource)
    {
        return new OnlineCatSlashState(this, inResource, tick);
    }

    public class OnlineCatSlashState : AbstractPhysicalObjectState
    {
        //[OnlineField]
        //public type variableName;
        
        public OnlineCatSlashState() {}

        public OnlineCatSlashState(OnlineCatSlash onlineEntity, OnlineResource inResource, uint ts) : base(onlineEntity, inResource, ts)
        {
            //set state here
        }

        protected override RealizedPhysicalObjectState GetRealizedState(OnlinePhysicalObject onlineObject)
        {
            if (onlineObject.apo.realizedObject is NoirCatto.CatSlash) return new RealizedCatSlashState(onlineObject);
            return base.GetRealizedState(onlineObject);
        }

        public override void ReadTo(OnlineEntity onlineEntity)
        {
            base.ReadTo(onlineEntity);
            if ((onlineEntity as OnlineCatSlash).apo is NoirCatto.AbstractCatSlash abstractCatSlash)
            {
                //set abstract state here
            }
        }
    }

    public class RealizedCatSlashState : RealizedPhysicalObjectState
    {
        public RealizedCatSlashState() {}

        public RealizedCatSlashState(OnlinePhysicalObject onlineEntity) : base(onlineEntity)
        {
            
        }

        public override bool ShouldSyncChunks(PhysicalObject po) => false;

        public override bool ShouldPosBeLenient(PhysicalObject po) => true;
    }
}