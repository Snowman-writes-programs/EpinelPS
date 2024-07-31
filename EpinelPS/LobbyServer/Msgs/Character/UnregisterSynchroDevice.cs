﻿using EpinelPS.Database;
using EpinelPS.Utils;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Msgs.Character
{
    [PacketPath("/character/SynchroDevice/Unregist")]
    public class UnregisterSynchroDevice : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSynchroUnregist>();
            var user = GetUser();

            var response = new ResSynchroUnregist();

            foreach (var item in user.SynchroSlots)
            {
                if (item.Slot == req.Slot)
                {
                    if (item.CharacterSerialNumber == 0)
                    {
                        Logger.Warn("must add character from synchrodevice first");
                    }
                    else
                    {
                        var oldCSN = item.CharacterSerialNumber;
                        item.CharacterSerialNumber = 0;
                        var data = user.GetCharacterBySerialNumber(oldCSN);
                        if (data == null) throw new Exception("failed to lookup character"); 

                        response.Character = new NetUserCharacterDefaultData()
                        {
                            Csn = data.Csn,
                            CostumeId = data.CostumeId,
                            Grade = data.Grade,
                            Level = data.Level,
                            Skill1Lv = data.Skill1Lvl,
                            Skill2Lv = data.Skill2Lvl,
                            Tid = data.Tid,
                            UltiSkillLv = data.UltimateLevel
                        };
                        response.Slot = new NetSynchroSlot() { AvailableRegisterAt = item.AvailableAt, Csn = item.CharacterSerialNumber, Slot = item.Slot };

                        response.IsSynchro = false;
                        var highestLevelCharacters = user.Characters.OrderByDescending(x => x.Level).Take(5).ToList();


                        foreach (var item2 in highestLevelCharacters)
                        {
                            response.SynchroStandardCharacters.Add(item2.Csn);
                        }

                        response.SynchroLv = user.GetSynchroLevel();
                    }
                }
            }

            JsonDb.Save();


            await WriteDataAsync(response);
        }
    }
}
