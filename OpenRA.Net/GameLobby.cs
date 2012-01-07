using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Network;
namespace OpenRA.Net
{
	public class GameLobby:OpenRA.Mods.RA.Server.LobbyCommands, Server.IInterpretChat, Server.IClientJoined
	{
	
		public Server.Connection Admin;
		private List<Map>  _maps=(from m in Game.modData.AvailableMaps.Values where m.Selectable select m).ToList();
		
		public bool InterpretChat (OpenRA.Server.Server server, OpenRA.Server.Connection conn, OpenRA.Network.Session.Client client, string message)
		{
			if(Admin==conn)
			{
				if(message=="/start")
				{
					server.StartGame();
					return true;
				}
				if(message=="/maps")
				{
					string list="Available maps:";
					for (int c=0; c<_maps.Count-1; c++)
						list+="\n"+(c+1)+". "+_maps[c].Title;
					server.SendChatTo(conn, list);
					return true;
				}
				if(message.StartsWith("/map "))
				{
					message=message.Substring(4);
					int num;
					if(int.TryParse(message, out num))
					{
						num--;
						if((num<0)||(num>=_maps.Count))
							return false;
						server.lobbyInfo.GlobalSettings.Map = _maps[num].Uid;
						var oldSlots = server.lobbyInfo.Slots.Keys.ToArray();
						LoadMap(server);

						// Reassign players into new slots based on their old slots:
						//  - Observers remain as observers
						//  - Players who now lack a slot are made observers
						//  - Bots who now lack a slot are dropped
						var slots = server.lobbyInfo.Slots.Keys.ToArray();
						int i = 0;
						foreach (var os in oldSlots)
						{
							var c = server.lobbyInfo.ClientInSlot(os);
							if (c == null)
								continue;

							c.SpawnPoint = 0;
							c.State = Session.ClientState.NotReady;
							c.Slot = i < slots.Length ? slots[i++] : null;
							if (c.Slot != null)
								OpenRA.Server.Server.SyncClientToPlayerReference(c, server.Map.Players[c.Slot]);
							else if (c.Bot != null)
								server.lobbyInfo.Clients.Remove(c);
						}

						server.SyncLobbyInfo();
						return true;
					}
					
				}
					
				
				
			}
			return false;
		}

		public void ClientJoined (OpenRA.Server.Server server, OpenRA.Server.Connection conn)
		{
			if(Admin==null)
			{
				Admin=conn;
				server.SendChatTo(conn, "Available commands:\n" +
					"/start - start game\n" +
					"/maps - list of maps\n" +
					"/map number - change map");
			}
		}
	}
}

