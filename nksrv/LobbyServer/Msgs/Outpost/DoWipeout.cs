﻿using nksrv.Net;
using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Outpost
{
    [PacketPath("/outpost/obtainfastbattlereward")]
    public class DoWipeout : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<DoWipeOutRequest>();
            var response = new DoWipeOutResponse();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
