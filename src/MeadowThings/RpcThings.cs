using RainMeadow;

namespace NoirCatto;

public static partial class MeadowThings
{
    public static void RpcSend_DoMeow(NoirCatto.NoirData noirData, bool attractiveMeow)
    {
        var onlineCreature = noirData.AbstractCat.GetOnlineCreature();
        onlineCreature?.BroadcastRPCInRoom(Rpc_DoMeow, onlineCreature, noirData.MeowPitch, attractiveMeow);
    }

    [RPCMethod]
    public static void Rpc_DoMeow(RPCEvent rpc, OnlineCreature onlineCreature, float meowPitch, bool attractiveMeow)
    {
        var meower = onlineCreature.realizedCreature;
        NoirCatto.DoMeow(meower, meowPitch, attractiveMeow);
    }


}