using System;
using OpenRA.Server;
namespace OpenRA.Net
{
	public class GameServer:ExtendedServer
	{
		public GameServer ():base(null, new ServerTrait[]
		{
			new GameServerExtensions(),
			new OpenRA.Mods.RA.Server.LobbyCommands()
		})
		{
			
		}
		
		protected override void InterpretServerOrder (Connection conn, ServerOrder so)
		{
			try
			{
				base.InterpretServerOrder (conn, so);
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception while handling server order {0}:{1} from {2}"
					.F(so.Name, so.Data, ((System.Net.IPEndPoint) conn.socket.RemoteEndPoint).Address.ToString()));
				this.Shutdown();
			}
			
		}
		public class GameServerExtensions:ServerTrait, IClientJoined
		{
			#region IClientJoined implementation
			public void ClientJoined (Server.Server server, Connection conn)
			{
				if(server.lobbyInfo.Clients.Count==1)
					server.lobbyInfo.ClientWithIndex(conn.PlayerIndex).IsAdmin = true;
				
			}
			#endregion
			
		}
		
	}
}

