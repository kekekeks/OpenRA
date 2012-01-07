using System;
using System.Net;
using System.Collections.Generic;
using OpenRA.Network;
using System.Linq;
namespace OpenRA.Net
{
	public class GameServer:OpenRA.Server.Server
	{
		public ServerCallQueue CallQueue {get; private set;}
		
		

		
		
		public void AdoptConnection(OpenRA.Server.Connection conn, Network.Session.Client client)
		{
			CallQueue.Enqueue(_ =>
			{
				conn.PlayerIndex=ChooseFreePlayerIndex();
				client.Index=conn.PlayerIndex;
				client.Slot=lobbyInfo.FirstEmptySlot();
				lobbyInfo.Clients.Add(client);
				conns.Add(conn);
				SyncLobbyInfo();
				
			});
		}
		public GameServer():this(new ServerCallQueue(1))
		{

		}
		
		
		GameServer(ServerCallQueue cq):base(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1000),new string[]{"ra"},
			Game.Settings.Server, Game.modData, 
			new Server.ServerTrait[]{cq, new Mods.RA.Server.LobbyCommands()} , true)
		{
			CallQueue=cq;
		}
	}
}

