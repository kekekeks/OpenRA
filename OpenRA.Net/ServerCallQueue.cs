using System;
using System.Collections.Generic;
namespace OpenRA.Net
{
	public class ServerCallQueue:Server.ServerTrait, Server.ITick
	{
		int _tick;
		Queue<System.Action<Server.Server>> _queue=new Queue<System.Action<Server.Server>>();
		public ServerCallQueue (int tick)
		{
			_tick=tick;
		}

		public void Enqueue(System.Action<Server.Server> proc)
		{
			lock(_queue)
				_queue.Enqueue(proc);
		}

		public void Enqueue(System.Action proc)
		{
			Enqueue((server)=>{proc();});
		}


		public void Tick (OpenRA.Server.Server server)
		{
			lock(_queue)
				while(_queue.Count!=0)
					_queue.Dequeue()(server);
		}

		public int TickTimeout
		{
			get
			{
				return _tick;
			}
		}

	}
}