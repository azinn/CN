﻿#region

using System;
using System.Net;
using LeagueSharp;
using System.Linq;
using LeagueSharp.Common;
#endregion

namespace D_Shyvana
{
    internal class Program
    {
        private static Orbwalking.Orbwalker _orbwalker;

        private const string ChampionName = "Shyvana";

        private static Spell _q, _w, _e, _r;

        private static Menu _config;

        private static Obj_AI_Hero _player;

        private static Int32 _lastSkin;

        private static Items.Item _tiamat, _hydra, _blade, _bilge, _rand, _lotis;

        private static SpellSlot _igniteSlot;

        private static SpellSlot _smiteSlot = SpellSlot.Unknown;

        private static Spell _smite;
        //Credits to Kurisu
        private static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        private static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        private static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        private static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;

            _q = new Spell(SpellSlot.Q, 0);
            _w = new Spell(SpellSlot.W, 350f);
            _e = new Spell(SpellSlot.E, 925f);
            _r = new Spell(SpellSlot.R, 1000f);

            _e.SetSkillshot(0.25f, 60f, 1700, false, SkillshotType.SkillshotLine);
            _r.SetSkillshot(0.25f, 150f, 1500, false, SkillshotType.SkillshotLine);

            _bilge = new Items.Item(3144, 450f);
            _blade = new Items.Item(3153, 450f);
            _hydra = new Items.Item(3074, 250f);
            _tiamat = new Items.Item(3077, 250f);
            _rand = new Items.Item(3143, 490f);
            _lotis = new Items.Item(3190, 590f);

            _igniteSlot = _player.GetSpellSlot("SummonerDot");
            SetSmiteSlot();

            //D Shyvana
            _config = new Menu("【無為汉化】D-龙女", "D-Shyvana", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("目标选择器", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            _config.AddSubMenu(new Menu("走砍", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Combo
            _config.AddSubMenu(new Menu("连招", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseIgnite", "使用点燃")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("smitecombo", "使用惩戒")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQC", "使用 Q")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWC", "使用 W")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseEC", "使用 E")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRC", "使用 R(可击杀)")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRE", "自动 R 最少目标")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("MinTargets", "（连招使用大招）当敌人数>").SetValue(new Slider(2, 1, 5)));
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "连招!").SetValue(new KeyBind(32, KeyBindType.Press)));

            _config.AddSubMenu(new Menu("项目", "items"));
            _config.SubMenu("items").AddSubMenu(new Menu("进攻", "Offensive"));
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Tiamat", "使用提亚马特")).SetValue(true);
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Hydra", "使用九头蛇")).SetValue(true);
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Bilge", "使用弯刀")).SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BilgeEnemyhp", "敌人血量低于 <").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Bilgemyhp", "或者你的血量低于 < ").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Blade", "使用弯刀")).SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BladeEnemyhp", "敌人血量低于 <").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Blademyhp", "或者你的血量低于 <").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items").AddSubMenu(new Menu("防御", "Deffensive"));
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omen", "使用兰顿"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omenenemys", "敌人人数>").SetValue(new Slider(2, 1, 5)));
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotis", "使用索拉里"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotisminhp", "血量<").SetValue(new Slider(35, 1, 100)));
            _config.SubMenu("items").AddSubMenu(new Menu("药水", "Potions"));
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usehppotions", "使用红药/水晶瓶/饼干"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionhp", "如果血量%<").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usemppotions", "使用蓝药/水晶瓶/饼干"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionmp", "如果蓝量%<").SetValue(new Slider(85, 1, 100)));

            //Harass
            _config.AddSubMenu(new Menu("骚扰", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQH", "使用 Q")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseWH", "使用 W")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseEH", "使用 E")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseItemsharass", "使用 提亚玛特/九头蛇")).SetValue(true);
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("harasstoggle", "自动骚扰 (切换)").SetValue(new KeyBind("G".ToCharArray()[0],
                        KeyBindType.Toggle)));
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ActiveHarass", "骚扰!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //LaneClear
            _config.AddSubMenu(new Menu("发育", "Farm"));
            _config.SubMenu("Farm").AddSubMenu(new Menu("补兵", "LastHit"));
            _config.SubMenu("Farm").SubMenu("LastHit").AddItem(new MenuItem("UseQLH", "Q 补兵")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("LastHit").AddItem(new MenuItem("UseWLH", "W 补兵")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("LastHit").AddItem(new MenuItem("UseELH", "E 补兵")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("LastHit")
                .AddItem(
                    new MenuItem("ActiveLast", "补兵!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            _config.SubMenu("Farm").AddSubMenu(new Menu("清兵", "LaneClear"));
            _config.SubMenu("Farm")
                .SubMenu("LaneClear")
                .AddItem(new MenuItem("UseItemslane", "使用物品"))
                .SetValue(true);
            _config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("UseQL", "Q 清兵")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("UseWL", "W 清兵")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("UseEL", "E 清兵")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("LaneClear")
                .AddItem(
                    new MenuItem("ActiveLane", "清兵!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            _config.SubMenu("Farm").AddSubMenu(new Menu("清野", "JungleClear"));
            _config.SubMenu("Farm")
                .SubMenu("LaneClear")
                .AddItem(new MenuItem("UseItemsjungle", "使用清野项目"))
                .SetValue(true);
            _config.SubMenu("Farm").SubMenu("JungleClear").AddItem(new MenuItem("UseQJ", "Q 清野")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("JungleClear").AddItem(new MenuItem("UseWJ", "W 清野")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("JungleClear").AddItem(new MenuItem("UseEJ", "E 清野")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("JungleClear")
                .AddItem(
                    new MenuItem("ActiveJungle", "清野!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            //Smite 
            _config.AddSubMenu(new Menu("惩戒", "Smite"));
            _config.SubMenu("Smite")
                .AddItem(
                    new MenuItem("Usesmite", "使用惩戒（切换)").SetValue(new KeyBind("H".ToCharArray()[0],
                        KeyBindType.Toggle)));
            _config.SubMenu("Smite").AddItem(new MenuItem("Usered", "红buff ")).SetValue(true);
            _config.SubMenu("Smite")
                .AddItem(new MenuItem("healthJ", "血量% <").SetValue(new Slider(35, 1, 100)));

            //Forest
            _config.AddSubMenu(new Menu("逃跑", "Forest Gump"));
            _config.SubMenu("Forest Gump").AddItem(new MenuItem("UseWF", "使用 W ")).SetValue(true);
            _config.SubMenu("Forest Gump").AddItem(new MenuItem("UseEF", "使用 E ")).SetValue(true);
            _config.SubMenu("Forest Gump").AddItem(new MenuItem("UseRF", "使用 R ")).SetValue(true);
            _config.SubMenu("Forest Gump")
                .AddItem(
                    new MenuItem("Forest", "逃跑!").SetValue(new KeyBind("Z".ToCharArray()[0],
                        KeyBindType.Press)));


            //Misc
            _config.AddSubMenu(new Menu("杂项", "Misc"));
            _config.SubMenu("Misc").AddItem(new MenuItem("UseEM", "使用 E 抢人头")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseRM", "使用 R 抢人头")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("Gap_E", "R 防突进")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseRInt", "R 打断技能")).SetValue(true);
            // _config.SubMenu("Misc").AddItem(new MenuItem("MinTargetsgap", "min enemy >=(GapClosers)").SetValue(new Slider(2, 1, 5)));
            _config.SubMenu("Misc").AddItem(new MenuItem("skinshy", "使用自定义皮肤").SetValue(false));
            _config.SubMenu("Misc").AddItem(new MenuItem("skinshyvana", "更换皮肤").SetValue(new Slider(4, 1, 6)));
           // _config.SubMenu("Misc").AddItem(new MenuItem("usePackets", "Usepackes")).SetValue(true);

            //Misc
            _config.AddSubMenu(new Menu("击中几率", "HitChance"));
            _config.SubMenu("HitChance")
                .AddItem(new MenuItem("Echange", "E 几率").SetValue(
                    new StringList(new[] {"低", "中等", "高", "非常高"})));
            _config.SubMenu("HitChance")
                .AddItem(new MenuItem("Rchange", "R 几率").SetValue(
                    new StringList(new[] {"低", "中等", "高", "非常高"})));

            //Drawings
            _config.AddSubMenu(new Menu("绘制", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "范围 Q")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "范围 W")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "范围 E")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "范围 R")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("Drawsmite", "惩戒范围")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "简易线圈").SetValue(true));
            _config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleQuality", "圈质量").SetValue(new Slider(100, 100, 10)));
            _config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleThickness", "圈厚度").SetValue(new Slider(1, 10, 1)));
            _config.AddSubMenu(new Menu("無爲汉化", "by wuwei"));
            _config.SubMenu("by wuwei").AddItem(new MenuItem("qunhao", "L#汉化群：386289593"));
            _config.SubMenu("by wuwei").AddItem(new MenuItem("qunhao2", "娃娃群：158994507"));
            _config.AddToMainMenu();
            Game.PrintChat("<font color='#881df2'>D-榫欏コ by Diabaths</font> 鍔犺浇鎴愬姛.");
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;

            if (_config.Item("skinshy").GetValue<bool>())
            {
                GenModelPacket(_player.ChampionName, _config.Item("skinshyvana").GetValue<Slider>().Value);
                _lastSkin = _config.Item("skinshyvana").GetValue<Slider>().Value;
            }
            Game.PrintChat(
                "<font color='#FF0000'>If You like my work and want to support, and keep it always up to date plz donate via paypal in </font> <font color='#FF9900'>ssssssssssmith@hotmail.com</font> (10) S");

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_config.Item("skinshy").GetValue<bool>() && SkinChanged())
            {
                GenModelPacket(_player.ChampionName, _config.Item("skinshyvana").GetValue<Slider>().Value);
                _lastSkin = _config.Item("skinshyvana").GetValue<Slider>().Value;
            }
            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (_config.Item("ActiveHarass").GetValue<KeyBind>().Active ||
                _config.Item("harasstoggle").GetValue<KeyBind>().Active)
            {
                Harass();

            }
            if (_config.Item("ActiveLane").GetValue<KeyBind>().Active)
            {
                Laneclear();
            }
            if (_config.Item("ActiveJungle").GetValue<KeyBind>().Active)
            {
                JungleClear();
            }
            if (_config.Item("ActiveLast").GetValue<KeyBind>().Active)
            {
                LastHit();
            }
            if (_config.Item("Forest").GetValue<KeyBind>().Active)
            {
                Forest();
            }
            Usepotion();
            if (_config.Item("Usesmite").GetValue<KeyBind>().Active)
            {
                Smiteuse();
            }
            _player = ObjectManager.Player;

            _orbwalker.SetAttack(true);

            KillSteal();
         }

        private static void GenModelPacket(string champ, int skinId)
        {
            Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(_player.NetworkId, skinId, champ))
                .Process();
        }

        private static bool SkinChanged()
        {
            return (_config.Item("skinshyvana").GetValue<Slider>().Value != _lastSkin);
        }

        private static float ComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            if (_igniteSlot != SpellSlot.Unknown &&
                _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            if (Items.HasItem(3077) && Items.CanUseItem(3077))
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Tiamat);
            if (Items.HasItem(3074) && Items.CanUseItem(3074))
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Hydra);
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Botrk);
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Bilgewater);
            if (_q.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.Q)*1.2;
            if (_q.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.W)*3;
            if (_e.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.E);
            if (_r.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.R);
            damage += _player.GetAutoAttackDamage(enemy, true)*2;
            return (float) damage;
        }
        private static void Smiteontarget(Obj_AI_Hero target)
        {
            var usesmite = _config.Item("smitecombo").GetValue<bool>();
            var itemscheck = SmiteBlue.Any(i => Items.HasItem(i)) || SmiteRed.Any(i => Items.HasItem(i));
            if (itemscheck && usesmite &&
                ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready &&
                target.Distance(_player.Position) < _smite.Range)
            {
                ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, target);
            }
        }

        private static void Combo()
        {
            var useQ = _config.Item("UseQC").GetValue<bool>();
            var useW = _config.Item("UseWC").GetValue<bool>();
            var useE = _config.Item("UseEC").GetValue<bool>();
            var useR = _config.Item("UseRC").GetValue<bool>();
            var autoR = _config.Item("UseRE").GetValue<bool>();

            var t = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
            Smiteontarget(t);
            if (t != null && _config.Item("UseIgnite").GetValue<bool>() && _igniteSlot != SpellSlot.Unknown &&
                _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            {
                if (ComboDamage(t) > t.Health)
                {
                    _player.Spellbook.CastSpell(_igniteSlot, t);
                }
            }
            if (useR && _r.IsReady())
            {
                if (t != null && _r.GetPrediction(t).Hitchance >= Rchange())
                    if (!t.HasBuff("JudicatorIntervention") && !t.HasBuff("Undying Rage") &&
                        ComboDamage(t) > t.Health)
                        _r.CastIfHitchanceEquals(t, HitChance.Medium);
            }
            if (useW && _w.IsReady())
            {
                if (t != null && _player.Distance(t) < _e.Range)
                    _w.Cast();
            }

            if (useE && _e.IsReady())
            {

                if (t != null && _player.Distance(t) < _e.Range &&
                    _e.GetPrediction(t).Hitchance >= Echange())
                    _e.Cast(t);
            }

            if (useQ && _q.IsReady())
            {
                if (t != null && _player.Distance(t) < _w.Range)
                    _q.Cast();
            }

            if (_r.IsReady() && autoR)
            {
                if (ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.IsValidTarget(_r.Range)) >=
                    _config.Item("MinTargets").GetValue<Slider>().Value
                    && _r.GetPrediction(t).Hitchance >= Rchange())
                    _r.Cast(t);
            }

            UseItemes(t);
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
            var useQ = _config.Item("UseQH").GetValue<bool>();
            var useW = _config.Item("UseWH").GetValue<bool>();
            var useE = _config.Item("UseEH").GetValue<bool>();
            var useItemsH = _config.Item("UseItemsharass").GetValue<bool>();
            if (useQ && _q.IsReady())
            {
                var t = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Magical);
                if (t != null && t.Distance(_player.Position) < _w.Range)
                    _q.Cast();
            }
            if (useW && _w.IsReady())
            {
                var t = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Magical);
                if (t != null && _player.Distance(t) < _w.Range)
                    _w.Cast();
            }
            if (useE && _e.IsReady())
            {
                var t = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Magical);
                if (t != null && _player.Distance(t) < _e.Range && _e.GetPrediction(t).Hitchance >= Echange())
                    _e.Cast(t);
            }
            if (useItemsH && _tiamat.IsReady() && _player.Distance(target) < _tiamat.Range)
            {
                _tiamat.Cast();
            }
            if (useItemsH && _hydra.IsReady() && _player.Distance(target) < _hydra.Range)
            {
                _hydra.Cast();
            }
        }

        private static void Laneclear()
        {
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _w.Range, MinionTypes.All);
            var rangedMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _e.Range + _e.Width,
                MinionTypes.Ranged);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _e.Range + _e.Width,
                MinionTypes.All);
            var useItemsl = _config.Item("UseItemslane").GetValue<bool>();
            var useQl = _config.Item("UseQL").GetValue<bool>();
            var useWl = _config.Item("UseWL").GetValue<bool>();
            var useEl = _config.Item("UseEL").GetValue<bool>();
            if (_q.IsReady() && useQl && allMinionsW.Count > 0)
            {
                _q.Cast();
            }

            if (_w.IsReady() && useWl)
            {

                if (allMinionsW.Count >= 2)
                {
                    _w.Cast();
                }
                else
                    foreach (var minion in allMinionsW)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                            minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.W))
                            _w.Cast();
            }
            if (_e.IsReady() && useEl)
            {
                var fl1 = _e.GetLineFarmLocation(rangedMinionsE, _e.Width);
                var fl2 = _e.GetLineFarmLocation(allMinionsE, _e.Width);

                if (fl1.MinionsHit >= 3)
                {
                    _e.Cast(fl1.Position);
                }
                else if (fl2.MinionsHit >= 2 || allMinionsE.Count == 1)
                {
                    _e.Cast(fl2.Position);
                }
                else
                    foreach (var minion in allMinionsE)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                            minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.E))
                            _e.Cast(minion);
            }
            foreach (var minion in allMinionsE)
            {
                if (useItemsl && _tiamat.IsReady() && _player.Distance(minion) < _tiamat.Range)
                {
                    _tiamat.Cast();
                }
                if (useItemsl && _hydra.IsReady() && _player.Distance(minion) < _hydra.Range)
                {
                    _hydra.Cast();
                }
            }
        }

        private static void LastHit()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _e.Range, MinionTypes.All);
            var useQ = _config.Item("UseQLH").GetValue<bool>();
            var useW = _config.Item("UseWLH").GetValue<bool>();
            var useE = _config.Item("UseELH").GetValue<bool>();
            foreach (var minion in allMinions)
            {
                if (useQ && _q.IsReady() && _player.Distance(minion) < 200 &&
                    minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    _q.Cast();
                }

                if (_w.IsReady() && useW && _player.Distance(minion) < _w.Range &&
                    minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.W))
                {
                    _w.Cast();
                }
                if (_e.IsReady() && useE && _player.Distance(minion) < _e.Range &&
                    minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.E))
                {
                    _e.Cast(minion);
                }
            }
        }

        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _w.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var useItemsJ = _config.Item("UseItemsjungle").GetValue<bool>();
            var useQ = _config.Item("UseQJ").GetValue<bool>();
            var useW = _config.Item("UseWJ").GetValue<bool>();
            var useE = _config.Item("UseEJ").GetValue<bool>();
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (useQ && _q.IsReady())
                {
                    _q.Cast();
                }
                if (_w.IsReady() && useW)
                {
                    _w.Cast();
                }
                if (_e.IsReady() && useE)
                {
                    _e.Cast(mob);
                }

                if (useItemsJ && _tiamat.IsReady() && _player.Distance(mob) < _tiamat.Range)
                {
                    _tiamat.Cast();
                }
                if (useItemsJ && _hydra.IsReady() && _player.Distance(mob) < _tiamat.Range)
                {
                    _hydra.Cast();
                }
            }
        }

        private static HitChance Echange()
        {
            switch (_config.Item("Echange").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static HitChance Rchange()
        {
            switch (_config.Item("Rchange").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

       /* private static bool Packets()
        {
            return _config.Item("usePackets").GetValue<bool>();
        }*/

        private static void KillSteal()
        {

            if (_e.IsReady() && _config.Item("UseEM").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Magical);
                if (_e.GetDamage(t) > t.Health && _player.Distance(t) <= _e.Range)
                {
                    _e.CastIfHitchanceEquals(t, Echange());
                }
            }
            if (_r.IsReady() && _config.Item("UseRM").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
                if (t != null)
                    if (!t.HasBuff("JudicatorIntervention") && !t.HasBuff("Undying Rage") &&
                        _r.GetDamage(t) > t.Health && _r.GetPrediction(t).Hitchance >= Rchange())
                        _r.Cast(t);
            }
        }

        private static void UseItemes(Obj_AI_Hero target)
        {
            var iBilge = _config.Item("Bilge").GetValue<bool>();
            var iBilgeEnemyhp = target.Health <=
                                (target.MaxHealth*(_config.Item("BilgeEnemyhp").GetValue<Slider>().Value)/100);
            var iBilgemyhp = _player.Health <=
                             (_player.MaxHealth*(_config.Item("Bilgemyhp").GetValue<Slider>().Value)/100);
            var iBlade = _config.Item("Blade").GetValue<bool>();
            var iBladeEnemyhp = target.Health <=
                                (target.MaxHealth*(_config.Item("BladeEnemyhp").GetValue<Slider>().Value)/100);
            var iBlademyhp = _player.Health <=
                             (_player.MaxHealth*(_config.Item("Blademyhp").GetValue<Slider>().Value)/100);
            var iOmen = _config.Item("Omen").GetValue<bool>();
            var iOmenenemys = ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.IsValidTarget(450)) >=
                              _config.Item("Omenenemys").GetValue<Slider>().Value;
            var iTiamat = _config.Item("Tiamat").GetValue<bool>();
            var iHydra = _config.Item("Hydra").GetValue<bool>();
            var ilotis = _config.Item("lotis").GetValue<bool>();
            
            if (_player.Distance(target) <= 450 && iBilge && (iBilgeEnemyhp || iBilgemyhp) && _bilge.IsReady())
            {
                _bilge.Cast(target);

            }
            if (_player.Distance(target) <= 450 && iBlade && (iBladeEnemyhp || iBlademyhp) && _blade.IsReady())
            {
                _blade.Cast(target);

            }
            if (iTiamat && _tiamat.IsReady() && target.IsValidTarget(_tiamat.Range))
            {
                _tiamat.Cast();

            }
            if (iHydra && _hydra.IsReady() && target.IsValidTarget(_hydra.Range))
            {
                _hydra.Cast();

            }
            if (iOmenenemys && iOmen && _rand.IsReady())
            {
                _rand.Cast();

            }
            if (ilotis)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly || hero.IsMe))
                {
                    if (hero.Health <= (hero.MaxHealth*(_config.Item("lotisminhp").GetValue<Slider>().Value)/100) &&
                        hero.Distance(_player.ServerPosition) <= _lotis.Range && _lotis.IsReady())
                        _lotis.Cast();
                }
            }

        }
        private static void Usepotion()
        {
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _e.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var iusehppotion = _config.Item("usehppotions").GetValue<bool>();
            var iusepotionhp = _player.Health <=
                               (_player.MaxHealth * (_config.Item("usepotionhp").GetValue<Slider>().Value) / 100);
            var iusemppotion = _config.Item("usemppotions").GetValue<bool>();
            var iusepotionmp = _player.Mana <=
                               (_player.MaxMana * (_config.Item("usepotionmp").GetValue<Slider>().Value) / 100);
            if (ObjectManager.Player.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (ObjectManager.Player.CountEnemiesInRange(800) > 0 ||
                (mobs.Count > 0 && _config.Item("ActiveJungle").GetValue<KeyBind>().Active && (Items.HasItem(1039) ||
                 SmiteBlue.Any(i => Items.HasItem(i)) || SmiteRed.Any(i => Items.HasItem(i)) || SmitePurple.Any(i => Items.HasItem(i)) ||
                  SmiteBlue.Any(i => Items.HasItem(i)) || SmiteGrey.Any(i => Items.HasItem(i))
                     )))
            {
                if (iusepotionhp && iusehppotion &&
                     !(ObjectManager.Player.HasBuff("RegenerationPotion", true) ||
                       ObjectManager.Player.HasBuff("ItemCrystalFlask", true) ||
                       ObjectManager.Player.HasBuff("ItemMiniRegenPotion", true)))
                {
                    if (Items.HasItem(2041) && Items.CanUseItem(2041))
                    {
                        Items.UseItem(2041);
                    }
                    else if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }
                    else if (Items.HasItem(2003) && Items.CanUseItem(2003))
                    {
                        Items.UseItem(2003);
                    }
                }


                if (iusepotionmp && iusemppotion &&
                    !(ObjectManager.Player.HasBuff("FlaskOfCrystalWater", true) ||
                      ObjectManager.Player.HasBuff("ItemCrystalFlask", true) ||
                      ObjectManager.Player.HasBuff("ItemMiniRegenPotion", true)))
                {
                    if (Items.HasItem(2041) && Items.CanUseItem(2041))
                    {
                        Items.UseItem(2041);
                    }
                    else if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }
                    else if (Items.HasItem(2004) && Items.CanUseItem(2004))
                    {
                        Items.UseItem(2004);
                    }
                }
            }
        }
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_r.IsReady() && gapcloser.Sender.IsValidTarget(_r.Range) && _config.Item("Gap_E").GetValue<bool>())
            {
                _r.Cast(Game.CursorPos);
            }
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base target, InterruptableSpell spell)
        {
            if (!_config.Item("UseRInt").GetValue<bool>()) return;
            if (_player.Distance(target) < _r.Range && target != null &&
                _r.GetPrediction(target).Hitchance >= HitChance.Low)
            {
                _r.Cast(target);
            }
        }

        private static void Forest()
        {
            var target = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
            _player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (_config.Item("UseRF").GetValue<bool>() && _r.IsReady() && target != null)
            {
                _r.Cast(Game.CursorPos);
            }
            if (_config.Item("UseWF").GetValue<bool>() && _w.IsReady() && target != null)
            {
                _w.Cast();
            }
            if (_config.Item("UseEF").GetValue<bool>() && _e.IsReady() && _player.Distance(target) < _e.Range)
            {
                _e.Cast(target);
            }
        }

        //Credits to Kurisu
        private static string Smitetype()
        {
            if (SmiteBlue.Any(i => Items.HasItem(i)))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (SmiteRed.Any(i => Items.HasItem(i)))
            {
                return "s5_summonersmiteduel";
            }
            if (SmiteGrey.Any(i => Items.HasItem(i)))
            {
                return "s5_summonersmitequick";
            }
            if (SmitePurple.Any(i => Items.HasItem(i)))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }


        //Credits to metaphorce
        private static void SetSmiteSlot()
        {
            foreach (
                var spell in
                    ObjectManager.Player.Spellbook.Spells.Where(
                        spell => String.Equals(spell.Name, Smitetype(), StringComparison.CurrentCultureIgnoreCase)))
            {
                _smiteSlot = spell.Slot;
                _smite = new Spell(_smiteSlot, 700);
                return;
            }
        }
        private static int GetSmiteDmg()
        {
            int level = _player.Level;
            int index = _player.Level/5;
            float[] dmgs = {370 + 20*level, 330 + 30*level, 240 + 40*level, 100 + 50*level};
            return (int) dmgs[index];
        }

        //New map Monsters Name By SKO
        private static void Smiteuse()
        {
            var jungle = _config.Item("ActiveJungle").GetValue<KeyBind>().Active;
            if (ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) != SpellState.Ready) return;
            var usered = _config.Item("Usered").GetValue<bool>();
            var health = (100 * (_player.Mana / _player.MaxMana)) < _config.Item("healthJ").GetValue<Slider>().Value;
            string[] jungleMinions;
            if (Utility.Map.GetMap().Type.Equals(Utility.Map.MapType.TwistedTreeline))
            {
                jungleMinions = new string[] { "TT_Spiderboss", "TT_NWraith", "TT_NGolem", "TT_NWolf" };
            }
            else
            {
                jungleMinions = new string[]
                {
                    "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Red", "SRU_Krug", "SRU_Dragon",
                    "SRU_Baron", "Sru_Crab"
                };
            }
            var minions = MinionManager.GetMinions(_player.Position, 1000, MinionTypes.All, MinionTeam.Neutral);
            if (minions.Count() > 0)
            {
                int smiteDmg = GetSmiteDmg();

                foreach (Obj_AI_Base minion in minions)
                {
                    if (Utility.Map.GetMap().Type.Equals(Utility.Map.MapType.TwistedTreeline) &&
                        minion.Health <= smiteDmg &&
                        jungleMinions.Any(name => minion.Name.Substring(0, minion.Name.Length - 5).Equals(name)))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    if (minion.Health <= smiteDmg && jungleMinions.Any(name => minion.Name.StartsWith(name)) &&
                        !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    else if (jungle && usered && health && minion.Health >= smiteDmg &&
                             jungleMinions.Any(name => minion.Name.StartsWith("SRU_Red")) &&
                             !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_config.Item("Drawsmite").GetValue<bool>())
            {
                if (_config.Item("Usesmite").GetValue<KeyBind>().Active)
                {
                    Drawing.DrawText(Drawing.Width*0.90f, Drawing.Height*0.68f, System.Drawing.Color.DarkOrange,
                        "Smite Is On");
                }
                else
                    Drawing.DrawText(Drawing.Width*0.90f, Drawing.Height*0.68f, System.Drawing.Color.DarkRed,
                        "Smite Is Off");
            }
            if (_config.Item("CircleLag").GetValue<bool>())
            {
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawW").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _r.Range, System.Drawing.Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.White);
                }
                if (_config.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.White);
                }
                if (_config.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.White);
                }

                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _r.Range, System.Drawing.Color.White);
                }
            }
        }
    }
}
 
