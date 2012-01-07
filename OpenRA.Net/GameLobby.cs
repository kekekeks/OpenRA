using System;

namespace OpenRA.Net
{
	public class GameLobby:Server.ServerTrait, Server.IInterpretChat, Server.IClientJoined
	{
	
		public Server.Connection Admin;
		
		public bool InterpretChat (OpenRA.Server.Server server, OpenRA.Server.Connection conn, OpenRA.Network.Session.Client client, string message)
		{
			if(Admin==conn)
			{
				if(message=="/start")
					server.StartGame();
				return true;
			}
			return false;
		}

		public void ClientJoined (OpenRA.Server.Server server, OpenRA.Server.Connection conn)
		{
			if(Admin==null)
			{
				Admin=conn;
				server.SendChatTo(conn, "Available commands:\n" +
					"/start - start game");
			}
		}
	}
}

