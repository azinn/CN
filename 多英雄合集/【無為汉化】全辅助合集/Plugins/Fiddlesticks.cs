﻿using System;
using LeagueSharp;
using LeagueSharp.Common;
using Support.Util;
using ActiveGapcloser = Support.Util.ActiveGapcloser;

namespace Support.Plugins
{
    public class FiddleSticks : PluginBase
    {
        public FiddleSticks()
        {
            Q = new Spell(SpellSlot.Q, 575);
            W = new Spell(SpellSlot.W, 575);
            E = new Spell(SpellSlot.E, 750);
            R = new Spell(SpellSlot.R, 800);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.CastCheck(Target, "Combo.Q"))
                {
                    Q.CastOnUnit(Target);
                }

                if (E.CastCheck(Target, "Combo.E"))
                {
                    E.CastOnUnit(Target);
                }
            }

            if (HarassMode)
            {
                if (E.CastCheck(Target, "Harass.E"))
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

            if (Q.CastCheck(gapcloser.Sender, "Gapcloser.Q"))
            {
                Q.CastOnUnit(gapcloser.Sender);
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel < Interrupter2.DangerLevel.High || target.IsAlly)
            {
                return;
            }

            if (Q.CastCheck(target, "Interrupt.Q"))
            {
                Q.CastOnUnit(target);
                return;
            }

            if (E.CastCheck(target, "Interrupt.E"))
            {
                E.CastOnUnit(target);
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("Combo.Q", "使用 Q", true);
            config.AddBool("Combo.E", "使用 E", true);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("Harass.E", "使用 E", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Gapcloser.Q", "使用 Q 防止突进", true);

            config.AddBool("Interrupt.Q", "使用 Q 打断技能", true);
            config.AddBool("Interrupt.E", "使用 E 打断技能", true);
        }
    }
}