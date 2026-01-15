using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NoirCatto.HuntThings;
using RainMeadow;

namespace NoirCatto;

internal partial class MeadowThings
{
    public static void RpcSend_HuntQuests(List<HuntQuestThings.HuntQuest> quests)
    {
        var rawQuests = JsonConvert.SerializeObject(quests);

        foreach (var onlinePlayer in OnlineManager.players.Where(onlinePlayer => !onlinePlayer.isMe)) 
            onlinePlayer.InvokeRPC(Rpc_HuntQuests, rawQuests);
    }
    [RPCMethod]
    private static void Rpc_HuntQuests(RPCEvent rpc, string rawQuests)
    {
        HuntQuestThings.Master ??= new HuntQuestThings.HuntQuestMaster();
        HuntQuestThings.Master.Quests.Clear();
        HuntQuestThings.Master.Completed = false;
        HuntQuestThings.Master.NextRewardPhase = HuntQuestThings.RewardPhase.Normal;

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
            foreach (var onlinePlayer in OnlineManager.players.Where(onlinePlayer => !onlinePlayer.isMe)) 
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