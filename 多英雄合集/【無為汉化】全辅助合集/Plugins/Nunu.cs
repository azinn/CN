﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Support.Util;
using ActiveGapcloser = Support.Util.ActiveGapcloser;

namespace Support.Plugins
{
    public class Nunu : PluginBase
    {
        public Nunu()
        {
            Q = new Spell(SpellSlot.Q, 125);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 550);
            R = new Spell(SpellSlot.R, 650);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.IsReady() && ConfigValue<bool>("Combo.Q") &&
                    Player.HealthPercentage() < ConfigValue<Slider>("Combo.Q.Health").Value)
                {
                    var minion = MinionManager.GetMinions(Player.Position, Q.Range).FirstOrDefault();
                    if (minion.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(minion);
                    }
                }

                var allys = Helpers.AllyInRange(W.Range).OrderByDescending(h => h.FlatPhysicalDamageMod).ToList();
                if (W.IsReady() && allys.Count > 0 && ConfigValue<bool>("Combo.W"))
                {
                    W.CastOnUnit(allys.FirstOrDefault());
                }

                if (W.IsReady() && Target.IsValidTarget(AttackRange) && ConfigValue<bool>("Combo.W"))
                {
                    W.CastOnUnit(Player);
                }

                if (E.IsReady() && Target.IsValidTarget(E.Range) && ConfigValue<bool>("Combo.E"))
                {
                    E.CastOnUnit(Target);
                }
            }

            if (HarassMode)
            {
                if (Q.IsReady() && ConfigValue<bool>("Harass.Q") &&
                    Player.HealthPercentage() < ConfigValue<Slider>("Harass.Q.Health").Value)
                {
                    var minion = MinionManager.GetMinions(Player.Position, Q.Range).FirstOrDefault();
                    if (minion.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(minion);
                    }
                }

                var allys = Helpers.AllyInRange(W.Range).OrderByDescending(h => h.FlatPhysicalDamageMod).ToList();
                if (W.IsReady() && allys.Count > 0 && ConfigValue<bool>("Harass.W"))
                {
                    W.CastOnUnit(allys.FirstOrDefault());
                }

                if (W.IsReady() && Target.IsValidTarget(AttackRange) && ConfigValue<bool>("Harass.W"))
                {
                    W.CastOnUnit(Player);
                }

                if (E.IsReady() && Target.IsValidTarget(E.Range) && ConfigValue<bool>("Harass.E"))
                {
                    E.CastOnUnit(Target);
                }
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (E.CastCheck(gapcloser.Sender, "Gapcloser.E"))
            {
                E.CastOnUnit(gapcloser.Sender);

                if (W.IsReady())
                {
                    W.CastOnUnit(Player);
                }
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("Combo.Q", "使用 Q", true);
            config.AddBool("Combo.W", "使用 W", true);
            config.AddBool("Combo.E", "使用 E", true);
            config.AddSlider("Combo.Q.Health", "血量低 %HP", 50, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("Harass.Q", "使用 Q", true);
            config.AddBool("Harass.W", "使用 W", false);
            config.AddBool("Harass.E", "使用 E", true);
            config.AddSlider("Harass.Q.Health", "消耗血量低", 50, 1, 100);
        }

        public override void MiscMenu(Menu config)
        {
            config.AddList("Misc.Laugh", "笑的动作", new[] { "OFF", "ON", "ON + Mute" });
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Gapcloser.E", "使用 E 防止突进", true);
        }
    }
}