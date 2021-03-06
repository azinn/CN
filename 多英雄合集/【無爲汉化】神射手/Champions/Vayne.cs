﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace Sharpshooter.Champions
{
    public static class Vayne
    {
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Orbwalking.Orbwalker Orbwalker { get { return SharpShooter.Orbwalker; } }

        static Spell Q, W, E;

        static readonly int DeafaltRange = 615;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, (DeafaltRange + 300));
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, DeafaltRange);

            E.SetTargetted(0.25f, 2400f);

            var drawDamageMenu = new MenuItem("Draw_RDamage", "显示 (W) 伤害", true).SetValue(true);
            var drawFill = new MenuItem("Draw_Fill", "显示 (W) 补充伤害", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));

            SharpShooter.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseQ", "使用 Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseE", "使用 E", true).SetValue(true));

            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseQ", "使用 Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseE", "使用 E", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassMana", "蓝量 % >", true).SetValue(new Slider(50, 0, 100)));

            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearUseQ", "使用 Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearMana", "蓝量 % >", true).SetValue(new Slider(60, 0, 100)));

            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseQ", "使用 Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseE", "使用 E", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearMana", "蓝量 % >", true).SetValue(new Slider(20, 0, 100)));

            SharpShooter.Menu.SubMenu("Misc").AddItem(new MenuItem("antigapcloser", "使用防突进", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Misc").AddItem(new MenuItem("autointerrupt", "使用打断技能", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Misc").AddItem(new MenuItem("AutoRQ", "当大招时自动Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Misc").AddItem(new MenuItem("LogicalQ", "使用逻辑 Q (?)", true).SetValue(true));

            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingAA", "真正的 AA 范围", true).SetValue(new Circle(true, Color.FromArgb(183,0,0))));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingP", "被动范围", true).SetValue(new Circle(true, Color.FromArgb(183, 0, 0))));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingQ", "Q 范围", true).SetValue(new Circle(true, Color.FromArgb(183, 0, 0))));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingE", "E 范围", true).SetValue(new Circle(false, Color.FromArgb(183, 0, 0))));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingRtimer", "R 计时器", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingCond", "E 碰撞预判", true).SetValue(new Circle(true, Color.Red)));

            SharpShooter.Menu.SubMenu("Drawings").AddItem(drawDamageMenu);
            SharpShooter.Menu.SubMenu("Drawings").AddItem(drawFill);

            DamageIndicator.DamageToUnit = GetComboDamage;
            DamageIndicator.Enabled = drawDamageMenu.GetValue<bool>();
            DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
            DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;

            drawDamageMenu.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            drawFill.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                Combo();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                Harass();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Laneclear();
                Jungleclear();
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawingAA = SharpShooter.Menu.Item("drawingAA", true).GetValue<Circle>();
            var drawingP = SharpShooter.Menu.Item("drawingP", true).GetValue<Circle>();
            var drawingQ = SharpShooter.Menu.Item("drawingQ", true).GetValue<Circle>();
            var drawingE = SharpShooter.Menu.Item("drawingE", true).GetValue<Circle>();
            var drawingCond = SharpShooter.Menu.Item("drawingCond", true).GetValue<Circle>();

            if (drawingAA.Active)
                Render.Circle.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(Player), drawingAA.Color);

            if (drawingP.Active)
                Render.Circle.DrawCircle(Player.Position, 2000, drawingP.Color);

            if (drawingQ.Active && Q.IsReady())
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawingQ.Color);

            if (drawingE.Active && E.IsReady())
                Render.Circle.DrawCircle(Player.Position, E.Range, drawingE.Color);

            if (drawingCond.Active)
            {
                foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(1100)))
                {
                    var EPred = E.GetPrediction(En);
                    int pushDist = 425;

                    for (int i = 0; i < pushDist; i += (int)En.BoundingRadius)
                    {
                        Vector3 loc3 = EPred.UnitPosition.To2D().Extend(Player.Position.To2D(), -i).To3D();

                        if (loc3.IsWall())
                            Render.Circle.DrawCircle(loc3, En.BoundingRadius, drawingCond.Color);

                    }
                }
            }

            if (SharpShooter.Menu.Item("drawingRtimer", true).GetValue<Boolean>())
            {
                foreach (var buff in Player.Buffs)
                {
                    if (buff.Name == "vayneinquisition")
                    {
                        var targetpos = Drawing.WorldToScreen(Player.Position);
                        Drawing.DrawText(targetpos[0] - 10, targetpos[1], Color.Gold, "" + (buff.EndTime - Game.ClockTime));
                        break;
                    }
                }
            }

            if(Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LastHit)
            {
                if(Q.IsReady())
                    Render.Circle.DrawCircle(Game.CursorPos, 50, Color.Red);
            }
        }


        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!SharpShooter.Menu.Item("antigapcloser", true).GetValue<Boolean>() || Player.IsDead)
                return;

            if (gapcloser.Sender.IsValidTarget(1000))
            {
                Render.Circle.DrawCircle(gapcloser.Sender.Position, gapcloser.Sender.BoundingRadius, Color.Gold, 5);
                var targetpos = Drawing.WorldToScreen(gapcloser.Sender.Position);
                Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, Color.Gold, "Gapcloser");
            }

            if (E.CanCast(gapcloser.Sender))
                E.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!SharpShooter.Menu.Item("autointerrupt", true).GetValue<Boolean>() || Player.IsDead)
                return;

            if (sender.IsValidTarget(1000))
            {
                Render.Circle.DrawCircle(sender.Position, sender.BoundingRadius, Color.Gold, 5);
                var targetpos = Drawing.WorldToScreen(sender.Position);
                Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, Color.Gold, "Interrupt");
            }

            if (E.CanCast(sender))
                E.Cast(sender);
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (SharpShooter.Menu.Item("AutoRQ", true).GetValue<Boolean>())
            {
                if(sender.IsMe && args.SData.Name == "vayneinquisition" && Q.IsReady())
                    Q.Cast(Game.CursorPos);
            }
        }

        static bool isAllyFountain(Vector3 Position)
        {
            float fountainRange = 750;
            var map = Utility.Map.GetMap();
            if (map != null && map.Type == Utility.Map.MapType.SummonersRift)
            {
                fountainRange = 1050;
            }
            return
                ObjectManager.Get<GameObject>().Where(spawnPoint => spawnPoint is Obj_SpawnPoint && spawnPoint.IsAlly).Any(spawnPoint => Vector2.Distance(Position.To2D(), spawnPoint.Position.To2D()) < fountainRange);
        }

        static float GetComboDamage(Obj_AI_Base enemy)
        {
            foreach (var Buff in enemy.Buffs)
	        {
		        if(Buff.Name == "vaynesilvereddebuff" && Buff.Count == 2)
                {
                    return W.GetDamage(enemy) + (float)Player.GetAutoAttackDamage(enemy, true);
                }
	        }

            return 0;
        }

        static void LogicalQ(Boolean combomode)
        {
            if (!SharpShooter.Menu.Item("LogicalQ", true).GetValue<Boolean>())
            {
                Q.Cast(Game.CursorPos);
                return;
            }

            var QEndpos = Player.ServerPosition.Extend(Game.CursorPos, 300);

            foreach (var enemy in QEndpos.GetEnemiesInRange(308))
            {
                if (enemy.IsMelee())
                    return;
            }

            if(!combomode)
            {
                if (QEndpos.UnderTurret(true) || QEndpos.CountEnemiesInRange(800) > 0)
                    return;
            }
           
            Q.Cast(Game.CursorPos);
        }

        static void Combo()
        {
            if (!Orbwalking.CanMove(1))
                return;

            if (SharpShooter.Menu.Item("comboUseQ", true).GetValue<Boolean>() && !Player.IsWindingUp)
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical, true);

                if (Q.CanCast(Qtarget))
                    LogicalQ(true);
            }
                
            if (E.IsReady() && SharpShooter.Menu.Item("comboUseE", true).GetValue<Boolean>())
            {
                foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(E.Range) && !hero.HasBuffOfType(BuffType.SpellShield) && !hero.HasBuffOfType(BuffType.SpellImmunity)))
                {
                    //Part of VayneHunterRework

                    var EPred = E.GetPrediction(En);
                    int pushDist = 425;
                    var FinalPosition = EPred.UnitPosition.To2D().Extend(Player.ServerPosition.To2D(), -pushDist).To3D();

                    for (int i = 1; i < pushDist; i += (int)En.BoundingRadius)
                    {
                        Vector3 loc3 = EPred.UnitPosition.To2D().Extend(Player.ServerPosition.To2D(), -i).To3D();

                        if (loc3.IsWall() || isAllyFountain(FinalPosition))
                            E.Cast(En);
                    }
                }
            }

        }

        static void Harass()
        {
            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > SharpShooter.Menu.Item("harassMana", true).GetValue<Slider>().Value))
                return;

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical, true);

            if (Q.CanCast(target) && SharpShooter.Menu.Item("harassUseQ", true).GetValue<Boolean>() && !Player.IsWindingUp)
                LogicalQ(true);

            if (E.CanCast(target) && SharpShooter.Menu.Item("harassUseE", true).GetValue<Boolean>())
            {
                foreach (var buff in target.Buffs)
                {
                    if (buff.Name == "vaynesilvereddebuf" && buff.Count == 2)
                    {
                        E.Cast(target);
                        break;
                    }
                }
            }
        }

        static void Laneclear()
        {
            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > SharpShooter.Menu.Item("laneclearMana", true).GetValue<Slider>().Value))
                return;

            var Minions = MinionManager.GetMinions(Player.ServerPosition.Extend(Game.CursorPos, 300), Orbwalking.GetRealAutoAttackRange(Player), MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (Q.IsReady() && SharpShooter.Menu.Item("laneclearUseQ", true).GetValue<Boolean>())
                LogicalQ(false);
        }

        static void Jungleclear()
        {
            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > SharpShooter.Menu.Item("jungleclearMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (Q.CanCast(Mobs[0]) && SharpShooter.Menu.Item("jungleclearUseQ", true).GetValue<Boolean>())
                LogicalQ(false);

            if (E.CanCast(Mobs[0]) && SharpShooter.Menu.Item("jungleclearUseE", true).GetValue<Boolean>())
                E.Cast(Mobs[0]);
        }
    }
}
