﻿using System;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace UltimateCarry
{
	class Swain : Champion 
	{
		public Spell Q;
		public Spell W;
		public Spell E;
		public Spell R;

		public int Delay = 300;
		public int DelayTick_Ron = 0;
		public int DelayTick_Roff = 0;
        public Swain()
        {
			LoadMenu();
			LoadSpells();

			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnUpdate += Game_OnGameUpdate;
			PluginLoaded();
		}

		private void LoadMenu()
		{
			Program.Menu.AddSubMenu(new Menu("团队作战", "TeamFight"));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "使用 Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight", "使用 W").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useE_TeamFight", "使用 E").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight", "使用 R").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("骚扰", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "使用 Q").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass", "使用 W").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useE_Harass", "使用 E").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useR_Harass", "使用 R").SetValue(true));
			AddManaManager("Harass",60);
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("hint", "它将停止 R"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("hint2", "如果法力值到达最小"));

			Program.Menu.AddSubMenu(new Menu("清兵", "LaneClear"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_LaneClear", "使用 Q").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_LaneClear", "使用 W").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useE_LaneClear", "使用 E").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useR_LaneClear", "使用 R").SetValue(true));
			AddManaManager("LaneClear", 30);
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("hint", "它将停用 R"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("hint2", "如果法力值到达最小"));

			Program.Menu.AddSubMenu(new Menu("绘制", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "禁用全部").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "绘制 Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "绘制 W").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "绘制 E").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "绘制 R").SetValue(true));

		}

		private void LoadSpells()
		{
			Q = new Spell(SpellSlot.Q, 625);

			W = new Spell(SpellSlot.W, 900);
			W.SetSkillshot(1.1f,100,float.MaxValue,false,SkillshotType.SkillshotCircle);
			
			E = new Spell(SpellSlot.E, 625);

			R = new Spell(SpellSlot.R, 650);
		}

		private void Drawing_OnDraw(EventArgs args)
		{

			if(Program.Menu.Item("Draw_Disabled").GetValue<bool>())
				return;

			if(Program.Menu.Item("Draw_Q").GetValue<bool>())
				if(Q.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

			if(Program.Menu.Item("Draw_W").GetValue<bool>())
				if(W.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

			if(Program.Menu.Item("Draw_E").GetValue<bool>())
				if(E.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

			if(Program.Menu.Item("Draw_R").GetValue<bool>())
				if(R.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
		}

		private void Game_OnGameUpdate(EventArgs args)
		{
			Cast_R_off();

			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					if (Program.Menu.Item("useQ_TeamFight").GetValue<bool>())
                        Cast_onEnemy(Q, TargetSelector.DamageType.Magical);
					if (Program.Menu.Item("useW_TeamFight").GetValue<bool>())
                        Cast_BasicCircleSkillshot_Enemy(W, TargetSelector.DamageType.Magical);				
					if (Program.Menu.Item("useE_TeamFight").GetValue<bool>())
                        Cast_onEnemy(E, TargetSelector.DamageType.Magical);
					if (Program.Menu.Item("useR_TeamFight").GetValue<bool>())
						Cast_R_on();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if(Program.Menu.Item("useQ_Harass").GetValue<bool>())
                        Cast_onEnemy(Q, TargetSelector.DamageType.Magical);
					if(Program.Menu.Item("useW_Harass").GetValue<bool>())
                        Cast_BasicCircleSkillshot_Enemy(W, TargetSelector.DamageType.Magical);
					if(Program.Menu.Item("useE_Harass").GetValue<bool>())
                        Cast_onEnemy(E, TargetSelector.DamageType.Magical);
					if(Program.Menu.Item("useR_Harass").GetValue<bool>())
						Cast_R_on();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if(Program.Menu.Item("useQ_LaneClear").GetValue<bool>())
						Cast_Basic_Farm(Q);
					if(Program.Menu.Item("useW_LaneClear").GetValue<bool>())
						Cast_BasicCircleSkillshot_AOE_Farm(W, 200);
					if(Program.Menu.Item("useE_LaneClear").GetValue<bool>())
						Cast_Basic_Farm(E);
					if(Program.Menu.Item("useR_LaneClear").GetValue<bool>())
						Cast_R_on();
					break;
			}
		}

		private void Cast_R_off()
		{
			if(!R.IsReady())
				return;
			if(Environment.TickCount - DelayTick_Roff <= Delay)
				return;
			DelayTick_Roff = Environment.TickCount;
			if(!ObjectManager.Player.HasBuff("SwainMetamorphism"))
				return;
			if (!ManaManagerAllowCast(R))
			{
				R.Cast();
				return;
			}
            if (MinionManager.GetMinions(ObjectManager.Player.Position, R.Range, MinionTypes.All, MinionTeam.NotAlly).Count + Utility.CountEnemiesInRange((int)R.Range + 100) == 0)
				R.Cast();
		}

		private void Cast_R_on()
		{
			if(!R.IsReady() || Environment.TickCount - DelayTick_Ron <= Delay)
				return;
			DelayTick_Ron = Environment.TickCount;
			if(ObjectManager.Player.HasBuff("SwainMetamorphism"))
				return;

            var countEnemy = Utility.CountEnemiesInRange((int)R.Range);
			if (countEnemy >= 1 && ManaManagerAllowCast(R))
			{
				R.Cast();
				return;
			}
			if (Program.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear) 
				return;
			if(MinionManager.GetMinions(ObjectManager.Player.Position, R.Range, MinionTypes.All, MinionTeam.NotAlly).Count >= 1 && ManaManagerAllowCast(R))
				R.Cast();
		}
	}
}
