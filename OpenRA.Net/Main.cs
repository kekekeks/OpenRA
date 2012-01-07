using System;
using OpenRA.FileFormats;
using System.Net;
using OpenRA.GameRules;

namespace OpenRA.Net
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			AppDomain.CurrentDomain.AssemblyResolve += FileSystem.ResolveAssembly;
			new OpenRA.Mods.RA.ExitInfo();
			Game.Settings = new Settings(Platform.SupportDir + "settings.yaml", new Arguments(args));
			FileSystem.Mount(".");
			Game.Settings.Graphics.Renderer="Null";
			Game.Settings.Server.AdvertiseOnline=false;
			OpenRA.Graphics.Renderer.Initialize( Game.Settings.Graphics.Mode );
			Game.Renderer=new OpenRA.Graphics.Renderer();
			Game.modData= new ModData("ra");
			Game.Renderer.InitializeFonts(Game.modData.Manifest);
			Game.modData.LoadInitialAssets();
			var settings=Game.Settings.Server;
			new Server.Server(new IPEndPoint(IPAddress.Any, settings.ListenPort),
				Game.Settings.Game.Mods, settings, Game.modData, new Server.ServerTrait[]
			                 {new CommonLobby(), new Mods.RA.Server.MasterServerPinger()}, false);
			
			System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
		}
	}
}
