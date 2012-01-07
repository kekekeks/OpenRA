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
			CallQueue.Enqueue(delegate(Server.Server server) 
			{
				client.Slot=lobbyInfo.FirstEmptySlot();
				conns.Add(conn);
				
				if (client.Slot != null)
					SyncClientToPlayerReference(client, Map.Players[client.Slot]);
				lobbyInfo.Clients.Add(client);
				foreach (var t in ServerTraits.WithInterface<Server.IClientJoined>())
					t.ClientJoined(this, conn);
				
				SyncLobbyInfo();
				SendChat(conn, "has joined the game.");
					
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

