﻿using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Liberate
{
    [PacketPath("/liberate/get")]
    public class GetLiberateData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetLiberateData>();
            var user = GetUser();

            var response = new ResGetLiberateData() { };

            // TODO

            await WriteDataAsync(response);
        }
    }
}
