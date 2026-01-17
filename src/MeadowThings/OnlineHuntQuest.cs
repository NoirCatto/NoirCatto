using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NoirCatto.HuntThings;
using RainMeadow;

namespace NoirCatto;

internal partial class MeadowThings
{
    public static void RpcSend_HuntQuestsRequest()
    {
        OnlineManager.lobby.owner.InvokeRPC(Rpc_HuntQuestsRequest, OnlineManager.mePlayer);
    }
    [RPCMethod]
    private static void Rpc_HuntQuestsRequest(RPCEvent rpc, OnlinePlayer onlinePlayer)
    {
        if (HuntQuestThings.Master == null) return;
        RpcSend_HuntQuests(onlinePlayer, HuntQuestThings.Master.Quests.ToList());
    }
    
    public static void RpcSend_HuntQuests(OnlinePlayer onlinePlayer, List<HuntQuestThings.HuntQuest> quests)
    {
        var rawQuests = JsonConvert.SerializeObject(quests);
        onlinePlayer.InvokeRPC(Rpc_HuntQuests, rawQuests);
    }
    [RPCMethod]
    private static void Rpc_HuntQuests(RPCEvent rpc, string rawQuests)
    {
        var quests = JsonConvert.DeserializeObject<List<HuntQuestThings.HuntQuest>>(rawQuests);
        foreach (var quest in quests)
            HuntQuestThings.Master.Quests.Add(quest);
    }

    public static void RpcSend_TargetHuntedNotifyOwner(AbstractCreature prey)
    {
        var onlinePrey = prey.GetOnlineCreature();
        if (onlinePrey == null)
            NoirCatto.LogSource.LogError($"Creature {prey.ID.ToString()} does not exist in online space!");
        else
            OnlineManager.lobby.owner.InvokeRPC(Rpc_TargetHuntedNotifyOwner, onlinePrey);
    }
    [RPCMethod]
    private static void Rpc_TargetHuntedNotifyOwner(RPCEvent rpc, OnlineCreature onlinePrey)
    {
        if (HuntQuestThings.Master == null) return;
        if (!HuntQuestThings.Master.AlreadyHunted(onlinePrey.abstractCreature))
        {
            HuntQuestThings.Master.TargetHunted(onlinePrey.abstractCreature);
            RpcSend_TargetHunted(onlinePrey.abstractCreature);
        }
    }
    
    public static void RpcSend_TargetHunted(AbstractCreature prey)
    {
        var onlinePrey = prey.GetOnlineCreature();
        if (onlinePrey == null)
            NoirCatto.LogSource.LogError($"Creature {prey.ID.ToString()} does not exist in online space!");
        else
            foreach (var onlinePlayer in OnlineManager.lobby.participants.Where(onlinePlayer => !onlinePlayer.isMe)) 
                onlinePlayer.InvokeRPC(Rpc_TargetHunted, onlinePrey);
    }
    [RPCMethod]
    private static void Rpc_TargetHunted(RPCEvent rpc, OnlineCreature onlinePrey)
    {
        if (HuntQuestThings.Master == null) return;
        if (!HuntQuestThings.Master.AlreadyHunted(onlinePrey.abstractCreature))
            HuntQuestThings.Master.TargetHunted(onlinePrey.abstractCreature);
    }
}