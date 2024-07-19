﻿using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/user/getwallpaperinventory")]
    public class GetWallpaperInventory : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetWallpaperInventory>();

            var r = new ResGetWallpaperInventory();

            await WriteDataAsync(r);
        }
    }
}
