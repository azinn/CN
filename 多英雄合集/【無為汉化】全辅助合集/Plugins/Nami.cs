﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Support.Util;
using ActiveGapcloser = Support.Util.ActiveGapcloser;

namespace Support.Plugins
{
    public class Nami : PluginBase
    {
        public Nami()
        {
            Q = new Spell(SpellSlot.Q, 875);
            W = new Spell(SpellSlot.W, 725);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 2200);

            Q.SetSkillshot(1f, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 260f, 850f, false, SkillshotType.SkillshotLine);
            GameObject.OnCreate += RangeAttackOnCreate;
        }

        private double WHeal
        {
            get
            {
                int[] heal = {0, 65, 95, 125, 155, 185};
                return heal[W.Level] + Player.FlatMagicDamageMod*0.3;
            }
        }

        private void RangeAttackOnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid<Obj_SpellMissile>())
            {
                return;
            }

            var missile = (Obj_SpellMissile) sender;

            // Caster ally hero / not me
            if (!missile.SpellCaster.IsValid<Obj_AI_Hero>() || !missile.SpellCaster.IsAlly || missile.SpellCaster.IsMe ||
                missile.SpellCaster.IsMelee())
            {
                return;
            }

            // Target enemy hero
            if (!missile.Target.IsValid<Obj_AI_Hero>() || !missile.Target.IsEnemy)
            {
                return;
            }

            var caster = (Obj_AI_Hero) missile.SpellCaster;

            if (E.IsReady() && E.IsInRange(missile.SpellCaster) && ConfigValue<bool>("Misc.E.AA." + caster.ChampionName))
            {
                E.CastOnUnit(caster); // add delay
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            try
            {
                if (ComboMode)
                {
                    if (Q.CastCheck(Target, "ComboQ")) // TODO: add check for slowed targets by E or FrostQeen
                    {
                        Q.Cast(Target);
                    }

                    if (W.IsReady() && ConfigValue<bool>("ComboW"))
                    {
                        HealLogic();
                    }

                    if (R.CastCheck(Target, "ComboR"))
                    {
                        R.CastIfWillHit(Target, ConfigValue<Slider>("ComboCountR").Value);
                    }
                }

                if (HarassMode)
                {
                    if (Q.CastCheck(Target, "HarassQ"))
                    {
                        Q.Cast(Target);
                    }

                    if (W.IsReady() && ConfigValue<bool>("HarassW"))
                    {
                        HealLogic();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void HealLogic()
        {
            var ally = Helpers.AllyBelowHp(ConfigValue<Slider>("ComboHealthW").Value, W.Range);
            if (ally != null) // force heal low ally
            {
                W.CastOnUnit(ally);
                return;
            }

            if (Player.Distance(Target) > W.Range) // target out of range try bounce
            {
                var bounceTarget =
                    HeroManager.Enemies
                        .SingleOrDefault(hero => hero.IsValidAlly(W.Range) && hero.Distance(Target) < W.Range);

                if (bounceTarget != null && bounceTarget.MaxHealth - bounceTarget.Health > WHeal) // use bounce & heal
                {
                    W.CastOnUnit(bounceTarget);
                }
            }
            else // target in range
            {
                W.CastOnUnit(Target);
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (Q.CastCheck(gapcloser.Sender, "GapcloserQ"))
            {
                Q.Cast(gapcloser.Sender);
            }

            if (R.CastCheck(gapcloser.Sender, "GapcloserR"))
            {
                R.Cast(gapcloser.Sender);
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel < Interrupter2.DangerLevel.High || target.IsAlly)
            {
                return;
            }

            if (Q.CastCheck(target, "InterruptQ"))
            {
                Q.Cast(target);
            }

            if (!Q.IsReady() && R.CastCheck(target, "InterruptR"))
            {
                R.Cast(target);
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "使用 Q", true);
            config.AddBool("ComboW", "使用 W", true);
            config.AddBool("ComboR", "使用 R", true);
            config.AddSlider("ComboCountR", "Targets hit to Ult", 2, 1, 5);
            config.AddSlider("ComboHealthW", "Health to Heal", 20, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "使用 Q", true);
            config.AddBool("HarassW", "使用 W", true);
            config.AddSlider("HarassHealthW", "健康恢复", 20, 1, 100);
        }

        public override void MiscMenu(Menu config)
        {
            var sub = config.AddSubMenu(new Menu("使用 E 再攻击", "Misc.E.AA.Menu"));
            foreach (var hero in HeroManager.Allies.Where(h => !h.IsMe))
            {
                sub.AddBool("Misc.E.AA." + hero.ChampionName, hero.ChampionName, true);
            }
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserQ", "使用 Q 防止突进", true);

            config.AddBool("InterruptQ", "使用 Q 打断技能", true);
            config.AddBool("InterruptR", "使用 R 打断技能", true);
        }
    }
}