﻿using EpinelPS.Database;
using EpinelPS.LobbyServer.Msgs.Stage;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Campaign
{
    [PacketPath("/campaign/obtain/item")]
    public class ObtainItem : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqObtainCampaignItem>();
            var user = GetUser();

            var response = new ResObtainCampaignItem();

            var chapter = GameData.Instance.GetNormalChapterNumberFromFieldName(req.MapId);
            var mod = req.MapId.Contains("hard") ? "Hard" : "Normal";
            var key = chapter + "_" + mod;
            var field = user.FieldInfoNew[key];


            foreach (var item in field.CompletedObjects)
            {
                if (item.PositionId == req.FieldObject.PositionId)
                {
                    Console.WriteLine("attempted to collect campaign field object twice!");
                    return;
                }
            }

            // Register and return reward

            if (!GameData.Instance.PositionReward.ContainsKey(req.FieldObject.PositionId)) throw new Exception("bad position id");
            var fieldReward = GameData.Instance.PositionReward[req.FieldObject.PositionId];
            var positionReward = GameData.Instance.FieldItems[fieldReward];
            var reward = GameData.Instance.GetRewardTableEntry(positionReward.type_value);
            if (reward == null) throw new Exception("failed to get reward");
            response.Reward = ClearStage.RegisterRewardsForUser(user, reward);

            // Hide it from the field
            field.CompletedObjects.Add(new NetFieldObject() { PositionId = req.FieldObject.PositionId, Type = req.FieldObject.Type});

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
