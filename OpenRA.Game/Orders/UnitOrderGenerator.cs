#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Orders
{
	public class BaseUnitOrderGenerator : IOrderGenerator
	{
		Func<IOrderTargeter, bool> acceptTargeter;
		public BaseUnitOrderGenerator( Func<IOrderTargeter, bool> acceptTargeter )
		{
			this.acceptTargeter = acceptTargeter;
		}

        public virtual IEnumerable<Order> Order(World world, int2 xy, MouseInput mi)
        {
            var underCursor = world.FindUnitsAtMouse(mi.Location)
                .Where(a => a.HasTrait<ITargetable>())
                .OrderByDescending(
                    a =>
                    a.Info.Traits.Contains<SelectableInfo>()
                        ? a.Info.Traits.Get<SelectableInfo>().Priority
                        : int.MinValue)
                .FirstOrDefault();

            var orders = world.Selection.Actors
                .Select(a => OrderForUnit(a, xy, mi, underCursor, acceptTargeter))
                .Where(o => o != null)
                .ToArray();

            var actorsInvolved = orders.Select(o => o.self).Distinct();
			if (actorsInvolved.Any())
				yield return new Order("CreateGroup", actorsInvolved.First().Owner.PlayerActor, false)
				{
					TargetString = string.Join(",", actorsInvolved.Select(a => a.ActorID.ToString()).ToArray())
				};
                   

            foreach (var o in orders)
                yield return CheckSameOrder(o.iot, o.trait.IssueOrder(o.self, o.iot, o.target, mi.Modifiers.HasModifier(Modifiers.Shift)));
		}

		public void Tick( World world ) { }
		public void RenderBeforeWorld( WorldRenderer wr, World world ) { }
		public void RenderAfterWorld( WorldRenderer wr, World world ) { }

		public string GetCursor( World world, int2 xy, MouseInput mi )
		{
			bool useSelect = false;

			var underCursor = world.FindUnitsAtMouse(mi.Location)
				.Where(a => a.HasTrait<ITargetable>())
				.OrderByDescending(a => a.Info.Traits.Contains<SelectableInfo>() ? a.Info.Traits.Get<SelectableInfo>().Priority : int.MinValue)
				.FirstOrDefault();


			if (mi.Modifiers.HasModifier(Modifiers.Shift) || !world.Selection.Actors.Any())
				if (underCursor != null)
					useSelect = true;

			var orders = world.Selection.Actors
				.Select(a => OrderForUnit(a, xy, mi, underCursor, acceptTargeter))
				.Where(o => o != null)
				.ToArray();

			if( orders.Length == 0 ) return (useSelect) ? "select" : "default";

			return orders[0].cursor ?? ((useSelect) ? "select" : "default");
		}

		static UnitOrderResult OrderForUnit( Actor self, int2 xy, MouseInput mi, Actor underCursor, Func<IOrderTargeter,bool> acceptTargeter )
		{
			if (self.Owner != self.World.LocalPlayer)
				return null;

			if (self.Destroyed)
				return null;

			if( mi.Button == MouseButton.Right )
			{
				foreach( var o in self.TraitsImplementing<IIssueOrder>()
					.SelectMany( trait => trait.Orders
						.Select( x => new { Trait = trait, Order = x } ) )
					.OrderByDescending( x => x.Order.OrderPriority ) )
				{
					var actorsAt = self.World.ActorMap.GetUnitsAt( xy ).ToList();				
					var forceAttack = mi.Modifiers.HasModifier(Modifiers.Ctrl);
					var forceMove = mi.Modifiers.HasModifier(Modifiers.Alt);
					var forceQueue = mi.Modifiers.HasModifier(Modifiers.Shift);
					
					if (acceptTargeter(o.Order))
					{
						string cursor = null;
						if( underCursor != null && o.Order.CanTargetActor(self, underCursor, forceAttack, forceMove, forceQueue, ref cursor))
							return new UnitOrderResult( self, o.Order, o.Trait, cursor, Target.FromActor( underCursor ) );
						if (o.Order.CanTargetLocation(self, xy, actorsAt, forceAttack, forceMove, forceQueue, ref cursor))
							return new UnitOrderResult( self, o.Order, o.Trait, cursor, Target.FromCell( xy ) );
					}
				}
			}

			return null;
		}

		static Order CheckSameOrder( IOrderTargeter iot, Order order )
		{
			if( order == null && iot.OrderID != null )
				Game.Debug( "BUG: in order targeter - decided on {0} but then didn't order", iot.OrderID );
			else if( iot.OrderID != order.OrderString )
				Game.Debug( "BUG: in order targeter - decided on {0} but ordered {1}", iot.OrderID, order.OrderString );
			return order;
		}

		class UnitOrderResult
		{
			public readonly Actor self;
			public readonly IOrderTargeter iot;
			public readonly IIssueOrder trait;
			public readonly string cursor;
			public readonly Target target;

			public UnitOrderResult( Actor self, IOrderTargeter iot, IIssueOrder trait, string cursor, Target target )
			{
				this.self = self;
				this.iot = iot;
				this.trait = trait;
				this.cursor = cursor;
				this.target = target;
			}
		}
	}

	public class UnitOrderGenerator : BaseUnitOrderGenerator 
	{
		public UnitOrderGenerator() : base(_ => true) { }
	}

	public class RestrictedUnitOrderGenerator : BaseUnitOrderGenerator
	{
		public RestrictedUnitOrderGenerator(string orderId) : base(ot => ot.OrderID == orderId) { }

        static readonly Order[] NoOrders = {};
        public override IEnumerable<Order> Order(World world, int2 xy, MouseInput mi)
        {
            if (mi.Button == MouseButton.Right)
            {
                world.CancelInputMode();
                return NoOrders;
            }

            if (mi.Button == MouseButton.Left)
            {
                if (!mi.Modifiers.HasModifier(Modifiers.Shift))
                    world.CancelInputMode();

                mi.Button = MouseButton.Right;
            }

            return base.Order(world, xy, mi);
        }
	}
}
