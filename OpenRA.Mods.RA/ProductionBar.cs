﻿#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Drawing;
using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.RA
{
	class ProductionBarInfo : ITraitInfo
	{
		public object Create(ActorInitializer init) { return new ProductionBar( init.self ); }
	}

	class ProductionBar : ISelectionBar
	{
		Actor self;
		public ProductionBar(Actor self) { this.self = self; }

		public float GetValue()
		{
			// only people we like should see our production status.
			if (self.World.LocalPlayer != null && self.Owner.Stances[self.World.LocalPlayer] != Stance.Ally)
				return 0;

			var queue = self.TraitsImplementing<ProductionQueue>().FirstOrDefault(q => q.CurrentItem() != null);
			if (queue == null)
			{
				var produces = self.Trait<Production>().Info.Produces;
				queue = self.Owner.PlayerActor.TraitsImplementing<ProductionQueue>()
					.FirstOrDefault(q => produces.Contains(q.Info.Type));
			}

			if (queue == null || queue.CurrentItem() == null)
				return 0f;

			return 1 - (float)queue.CurrentItem().RemainingCost / queue.CurrentItem().TotalCost;
		}

		public Color GetColor() { return Color.SkyBlue; }
	}
}
