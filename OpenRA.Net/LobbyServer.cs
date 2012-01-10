using System;
using OpenRA.Server;
using System.Collections.Generic;
using System.Linq;
namespace OpenRA.Net
{
	public class LobbyServer:ExtendedServer
	{
		
		
		public LobbyServer()
			:base(new System.Net.IPEndPoint(System.Net.IPAddress.Any, Game.Settings.Server.ListenPort)
			, new Server.ServerTrait[]{new CommonLobby()})
		{
			
		}
		
		protected override void InterpretServerOrder (Connection conn, ServerOrder so)
		{
			if((so.Name=="Chat")||(so.Name=="TeamChat"))
			{
				var fromClient = GetClient(conn);
				foreach (var t in ServerTraits.WithInterface<IInterpretChat>())
					if (t.InterpretChat(this, conn, fromClient, so.Data))
						return;
			}
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
		
		public class CommonLobby:ServerTrait, ITick,IClientJoined, IInterpretChat
		{
			Dictionary<string, GameServer> _servers=new Dictionary<string, GameServer>();
	
			public bool InterpretChat (OpenRA.Server.Server server, Connection conn, OpenRA.Network.Session.Client client, string message)
			{
				if(message=="/list")
				{
					server.SendChatTo(conn, "Available games:\n"+string.Join("\n", _servers.Keys.ToArray()));
					return true;
				}	
				var pair=message.Split(new char[]{' '},2);
				if(pair.Length!=2)
					return false;
				if((pair[0]=="/create")||pair[0]=="/join")
				{
					GameServer gs;
					if(_servers.ContainsKey(pair[1]))
					{
						if(pair[0]=="/create")
						{	
							server.SendChatTo(conn, "Game with the same name already exists");
							return true;
						}
						gs=_servers[pair[1]];
					}
					else
					{
						if(pair[0]=="/join")
						{
							server.SendChatTo(conn, "Not found");
							return true;
						}
						Game.Settings.Server.Name=pair[1];
						gs=new GameServer();
						_servers.Add(pair[1], gs);
	
					}
	
					server.conns.Remove(conn);
					server.lobbyInfo.Clients.Remove(client);
					gs.AdoptConnection(conn, client);
					server.SyncLobbyInfo();
					return true;
				}
				return false;
			}
	
			#region ITick implementation
			public void Tick (Server.Server server)
			{
				var lst=(from s in _servers where s.Value.GameStarted select s.Key).ToList();
				foreach(var n in lst)
					_servers.Remove(n);
				System.Threading.Thread.Sleep(10);
			}
	
			public int TickTimeout
			{
				get
				{
					return 0;
				}
			}
			#endregion
	
			#region IClientJoined implementation
			public void ClientJoined (Server.Server server, Connection conn)
			{
				server.lobbyInfo.ClientWithIndex(conn.PlayerIndex).IsAdmin = false;
				server.SendChatTo(conn, "Welcome to OpenRA.Net\n" +
					"Available commands:\n" +
					"/list - list of games waiting for players\n" +
					"/join name - join game\n" +
					"/create name - create game");
			}
			#endregion
			
		}

	}
}

