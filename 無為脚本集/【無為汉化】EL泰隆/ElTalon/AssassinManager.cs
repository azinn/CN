﻿using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace ElTalon
{
    internal class AssassinManager
    {
        public AssassinManager()
        {
            Load();
        }

        private static void Load()
        {
            Talon._menu.AddSubMenu(new Menu("刺杀设置", "MenuAssassin"));
            Talon._menu.SubMenu("MenuAssassin").AddItem(new MenuItem("AssassinActive", "启用").SetValue(true));

            foreach (
                var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
            {
                Talon._menu.SubMenu("MenuAssassin")
                    .AddItem(
                        new MenuItem("Assassin" + enemy.ChampionName, enemy.ChampionName).SetValue(
                            TargetSelector.GetPriority(enemy) > 3));
            }


            Talon._menu.SubMenu("MenuAssassin")
                .AddItem(
                    new MenuItem("AssassinSelectOption", "模式: ").SetValue(
                        new StringList(new[] { "单一选择", "多选择" })));
            Talon._menu.SubMenu("MenuAssassin")
                .AddItem(new MenuItem("AssassinSetClick", "点击 增加/删除").SetValue(true));
            Talon._menu.SubMenu("MenuAssassin")
                .AddItem(
                    new MenuItem("AssassinReset", "重置列表").SetValue(
                        new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            Talon._menu.SubMenu("MenuAssassin").AddSubMenu(new Menu("显示:", "Draw"));

            Talon._menu.SubMenu("MenuAssassin")
                .SubMenu("Draw")
                .AddItem(new MenuItem("DrawSearch", "搜索范围").SetValue(new Circle(true, Color.GreenYellow)));
            Talon._menu.SubMenu("MenuAssassin")
                .SubMenu("Draw")
                .AddItem(new MenuItem("DrawActive", "主动的敌人").SetValue(new Circle(true, Color.GreenYellow)));
            Talon._menu.SubMenu("MenuAssassin")
                .SubMenu("Draw")
                .AddItem(new MenuItem("DrawNearest", "最近的敌人").SetValue(new Circle(true, Color.DarkSeaGreen)));
            Talon._menu.SubMenu("MenuAssassin")
                .SubMenu("Draw")
                .AddItem(new MenuItem("DrawStatus", "显示状态").SetValue(true));


            Talon._menu.SubMenu("MenuAssassin")
                .AddItem(new MenuItem("AssassinSearchRange", "搜索范围"))
                .SetValue(new Slider(1000, 2000));

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
        }

        private static void ClearAssassinList()
        {
            foreach (
                var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
            {
                Talon._menu.Item("Assassin" + enemy.ChampionName).SetValue(false);
            }
        }

        private static void OnGameUpdate(EventArgs args) {}

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (Talon._menu.Item("AssassinReset").GetValue<KeyBind>().Active && args.Msg == 257)
            {
                ClearAssassinList();
                Notifications.AddNotification("Assassin List is resetted.", 5);
            }

            if (args.Msg != (uint) WindowsMessages.WM_LBUTTONDOWN)
            {
                return;
            }

            if (Talon._menu.Item("AssassinSetClick").GetValue<bool>())
            {
                foreach (var objAiHero in from hero in ObjectManager.Get<Obj_AI_Hero>()
                    where hero.IsValidTarget()
                    select hero
                    into h
                    orderby h.Distance(Game.CursorPos) descending
                    select h
                    into enemy
                    where enemy.Distance(Game.CursorPos) < 150f
                    select enemy)
                {
                    if (objAiHero != null && objAiHero.IsVisible && !objAiHero.IsDead)
                    {
                        var xSelect = Talon._menu.Item("AssassinSelectOption").GetValue<StringList>().SelectedIndex;

                        switch (xSelect)
                        {
                            case 0:
                                ClearAssassinList();
                                Talon._menu.Item("Assassin" + objAiHero.ChampionName).SetValue(true);
                                Notifications.AddNotification(
                                    "Added " + objAiHero.ChampionName + " to Assassin List", 5);
                                break;
                            case 1:
                                var menuStatus = Talon._menu.Item("Assassin" + objAiHero.ChampionName).GetValue<bool>();
                                Talon._menu.Item("Assassin" + objAiHero.ChampionName).SetValue(!menuStatus);

                                //Notifications.AddNotification("Removed " + objAiHero.ChampionName + " to Assassin List", 5);
                                Game.PrintChat(
                                    string.Format(
                                        "<font color='{0}'>{1}</font> <font color='#09F000'>{2} ({3})</font>",
                                        !menuStatus ? "#FFFFFF" : "#FF8877",
                                        !menuStatus ? "Added to Assassin List:" : "Removed from Assassin List:",
                                        objAiHero.Name, objAiHero.ChampionName));
                                break;
                        }
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Talon._menu.Item("AssassinActive").GetValue<bool>())
            {
                return;
            }

            if (Talon._menu.Item("DrawStatus").GetValue<bool>())
            {
                var enemies = ObjectManager.Get<Obj_AI_Hero>().Where(xEnemy => xEnemy.IsEnemy);
                var objAiHeroes = enemies as Obj_AI_Hero[] ?? enemies.ToArray();
                Drawing.DrawText(Drawing.Width * 0.89f, Drawing.Height * 0.58f, Color.GreenYellow, "Assassin Status");
                Drawing.DrawText(Drawing.Width * 0.89f, Drawing.Height * 0.58f, Color.GhostWhite, "_____________");
                for (int i = 0; i < objAiHeroes.Count(); i++)
                {
                    var xCaption = objAiHeroes[i].ChampionName;
                    var xWidth = Drawing.Width * 0.90f;
                    if (Talon._menu.Item("Assassin" + objAiHeroes[i].ChampionName).GetValue<bool>())
                    {
                        xCaption = "+ " + xCaption;
                        xWidth = Drawing.Width * 0.8910f;
                    }
                    Drawing.DrawText(xWidth, Drawing.Height * 0.58f + (float) (i + 1) * 15, Color.Gainsboro, xCaption);
                }
            }

            var drawSearch = Talon._menu.Item("DrawSearch").GetValue<Circle>();
            var drawActive = Talon._menu.Item("DrawActive").GetValue<Circle>();
            var drawNearest = Talon._menu.Item("DrawNearest").GetValue<Circle>();

            var drawSearchRange = Talon._menu.Item("AssassinSearchRange").GetValue<Slider>().Value;
            if (drawSearch.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, drawSearchRange, drawSearch.Color);
            }

            foreach (var enemy in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(enemy => enemy.Team != ObjectManager.Player.Team)
                    .Where(
                        enemy =>
                            enemy.IsVisible && Talon._menu.Item("Assassin" + enemy.ChampionName) != null &&
                            !enemy.IsDead)
                    .Where(enemy => Talon._menu.Item("Assassin" + enemy.ChampionName).GetValue<bool>()))
            {
                if (ObjectManager.Player.Distance(enemy) < drawSearchRange)
                {
                    if (drawActive.Active)
                    {
                        Render.Circle.DrawCircle(enemy.Position, 85f, drawActive.Color);
                    }
                }
                else if (ObjectManager.Player.Distance(enemy) > drawSearchRange &&
                         ObjectManager.Player.Distance(enemy) < drawSearchRange + 400)
                {
                    if (drawNearest.Active)
                    {
                        Render.Circle.DrawCircle(enemy.Position, 85f, drawNearest.Color);
                    }
                }
            }
        }
    }
}