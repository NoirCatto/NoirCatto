using RainMeadow;

namespace NoirCatto;

internal static partial class MeadowThings
{
    public static void RpcSend_DoMeow(NoirCatto.NoirData noirData, byte sndIdAndAttrMeow)
    {
        var onlineCreature = noirData.AbstractCat.GetOnlineCreature();
        onlineCreature?.BroadcastRPCInRoom(Rpc_DoMeow, onlineCreature, noirData.MeowPitch, sndIdAndAttrMeow);
    }

    [RPCMethod]
    public static void Rpc_DoMeow(RPCEvent rpc, OnlineCreature onlineCreature, float meowPitch, byte sndIdAndAttrMeow)
    {
        var meower = onlineCreature.realizedCreature;
        if (meower == null) return;
        NoirCatto.DoMeow(meower, meowPitch, sndIdAndAttrMeow);
    }


}