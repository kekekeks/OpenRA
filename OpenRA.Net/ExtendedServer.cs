using System;
using System.Net;
using System.Collections.Generic;
using OpenRA.Network;
using System.Linq;
namespace OpenRA.Net
{
	public class ExtendedServer:OpenRA.Server.Server
	{
		public interface IInterpretChat
		{
			bool InterpretChat (OpenRA.Server.Server server, OpenRA.Server.Connection conn, OpenRA.Network.Session.Client client, string message);
		}
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
		
		public ExtendedServer(IPEndPoint endpoint, IEnumerable<OpenRA.Server.ServerTrait> traits)
			:base(
				(endpoint == null) ? new IPEndPoint(0x7f000001, 1234) : endpoint,
				Game.Settings.Game.Mods, Game.Settings.Server, Game.modData, (endpoint == null),
				traits.Append(new Func<ServerCallQueue>( () => CallQueue = new ServerCallQueue(1))()))
		{
			var wtf=CallQueue;
			wtf.Tick(this);
		}
	}
}