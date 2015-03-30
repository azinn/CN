﻿using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace UltimateCarry
{
	class Overlay
	{
		public Render.Sprite Hud;
		public Overlay()
		{
			if(Drawing.Width != 1920 || Drawing.Height != 1080 || Utility.Map.GetMap().Type != Utility.Map.MapType.SummonersRift)
				return;
			Program.Menu.AddSubMenu(new Menu("显示", "HUD"));
			Program.Menu.SubMenu("HUD").AddItem(new MenuItem("showHud", "显示HUD").SetValue(true));

			Hud = new Render.Sprite(Properties.Resources.Overlay2, new Vector2(1, 1));
			Hud.Add();
			Drawing.OnDraw += Drawing_OnDraw;
		}

		private void Drawing_OnDraw(EventArgs args)
		{
			if (Program.Menu.Item("showHud").GetValue<bool>())
			{
				Hud.Visible = true;
				Drawing.DrawLine(new Vector2(1275, 860), new Vector2(1610, 860), 200, Color.Black);
			}
			else
			{
				Hud.Visible = false;
			}
		}
	}
}
