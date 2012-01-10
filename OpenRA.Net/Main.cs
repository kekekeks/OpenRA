using System;
using OpenRA.FileFormats;
using OpenRA.GameRules;

namespace OpenRA.Net
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			EngineInit(args);
			new LobbyServer();
			System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
		}
		
		static void EngineInit(string[] args)
		{
			AppDomain.CurrentDomain.AssemblyResolve += FileSystem.ResolveAssembly;
			new OpenRA.Mods.RA.ExitInfo();
			Game.Settings = new Settings(Platform.SupportDir + "settings.yaml", new Arguments(args));
			FileSystem.Mount(".");
			Game.Settings.Graphics.Renderer="Null";
			Game.Settings.Server.AdvertiseOnline=false;
			OpenRA.Graphics.Renderer.Initialize( Game.Settings.Graphics.Mode );
			Game.Renderer=new OpenRA.Graphics.Renderer();
			
			Game.modData= new ModData(Game.Settings.Game.Mods);
			Game.Renderer.InitializeFonts(Game.modData.Manifest);
			Game.modData.LoadInitialAssets();
		}
	}
}
