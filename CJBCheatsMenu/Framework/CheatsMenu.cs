﻿using System;
using System.Collections.Generic;
using System.Linq;
using CJBCheatsMenu.Framework.Constants;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;

namespace CJBCheatsMenu.Framework
{
    internal class CheatsMenu : IClickableMenu
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod settings.</summary>
        private readonly ModConfig Config;

        /// <summary>The cheats helper.</summary>
        private readonly Cheats Cheats;

        /// <summary>Provides translations for the mod.</summary>
        private readonly ITranslationHelper TranslationHelper;

        private readonly List<ClickableComponent> OptionSlots = new List<ClickableComponent>();
        private readonly List<OptionsElement> Options = new List<OptionsElement>();
        private readonly ClickableTextureComponent UpArrow;
        private readonly ClickableTextureComponent DownArrow;
        private readonly ClickableTextureComponent Scrollbar;
        private readonly List<ClickableComponent> Tabs = new List<ClickableComponent>();
        private readonly ClickableComponent Title;
        private const int ItemsPerPage = 10;

        private string HoverText = "";
        private int OptionsSlotHeld = -1;
        private int CurrentItemIndex;
        private bool IsScrolling;
        private Rectangle ScrollbarRunner;
        private bool CanClose;
        private readonly MenuTab CurrentTab;


        /*********
        ** Public methods
        *********/
        public CheatsMenu(MenuTab tabIndex, ModConfig config, Cheats cheats, ITranslationHelper i18n)
          : base(Game1.viewport.Width / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, Game1.viewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2)
        {
            this.Config = config;
            this.Cheats = cheats;
            this.TranslationHelper = i18n;

            this.Title = new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen, Game1.tileSize * 4, Game1.tileSize), i18n.Get("title"));
            this.CurrentTab = tabIndex;

            {
                int i = 0;
                int labelX = (int)(this.xPositionOnScreen - Game1.tileSize * 4.8f);
                int labelY = (int)(this.yPositionOnScreen + Game1.tileSize * 1.5f);
                int labelHeight = (int)(Game1.tileSize * 0.9F);

                this.Tabs.Add(new ClickableComponent(new Rectangle(labelX, labelY + labelHeight * i++, Game1.tileSize * 5, Game1.tileSize), MenuTab.PlayerAndTools.ToString(), i18n.Get("tabs.player-and-tools")));
                this.Tabs.Add(new ClickableComponent(new Rectangle(labelX, labelY + labelHeight * i++, Game1.tileSize * 5, Game1.tileSize), MenuTab.FarmAndFishing.ToString(), i18n.Get("tabs.farm-and-fishing")));
                this.Tabs.Add(new ClickableComponent(new Rectangle(labelX, labelY + labelHeight * i++, Game1.tileSize * 5, Game1.tileSize), MenuTab.Skills.ToString(), i18n.Get("tabs.skills")));
                this.Tabs.Add(new ClickableComponent(new Rectangle(labelX, labelY + labelHeight * i++, Game1.tileSize * 5, Game1.tileSize), MenuTab.Weather.ToString(), i18n.Get("tabs.weather")));
                this.Tabs.Add(new ClickableComponent(new Rectangle(labelX, labelY + labelHeight * i++, Game1.tileSize * 5, Game1.tileSize), MenuTab.Relationships.ToString(), i18n.Get("tabs.relationships")));
                this.Tabs.Add(new ClickableComponent(new Rectangle(labelX, labelY + labelHeight * i++, Game1.tileSize * 5, Game1.tileSize), MenuTab.WarpLocations.ToString(), i18n.Get("tabs.warp")));
                this.Tabs.Add(new ClickableComponent(new Rectangle(labelX, labelY + labelHeight * i++, Game1.tileSize * 5, Game1.tileSize), MenuTab.Time.ToString(), i18n.Get("tabs.time")));
                this.Tabs.Add(new ClickableComponent(new Rectangle(labelX, labelY + labelHeight * i, Game1.tileSize * 5, Game1.tileSize), MenuTab.Controls.ToString(), i18n.Get("tabs.controls")));
            }

            this.UpArrow = new ClickableTextureComponent("up-arrow", new Rectangle(this.xPositionOnScreen + this.width + Game1.tileSize / 4, this.yPositionOnScreen + Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(421, 459, 11, 12), Game1.pixelZoom);
            this.DownArrow = new ClickableTextureComponent("down-arrow", new Rectangle(this.xPositionOnScreen + this.width + Game1.tileSize / 4, this.yPositionOnScreen + this.height - Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(421, 472, 11, 12), Game1.pixelZoom);
            this.Scrollbar = new ClickableTextureComponent("scrollbar", new Rectangle(this.UpArrow.bounds.X + Game1.pixelZoom * 3, this.UpArrow.bounds.Y + this.UpArrow.bounds.Height + Game1.pixelZoom, 6 * Game1.pixelZoom, 10 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(435, 463, 6, 10), Game1.pixelZoom);
            this.ScrollbarRunner = new Rectangle(this.Scrollbar.bounds.X, this.UpArrow.bounds.Y + this.UpArrow.bounds.Height + Game1.pixelZoom, this.Scrollbar.bounds.Width, this.height - Game1.tileSize * 2 - this.UpArrow.bounds.Height - Game1.pixelZoom * 2);
            for (int i = 0; i < CheatsMenu.ItemsPerPage; i++)
                this.OptionSlots.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize / 4, this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom + i * ((this.height - Game1.tileSize * 2) / CheatsMenu.ItemsPerPage), this.width - Game1.tileSize / 2, (this.height - Game1.tileSize * 2) / CheatsMenu.ItemsPerPage + Game1.pixelZoom), string.Concat(i)));

            switch (this.CurrentTab)
            {
                case MenuTab.PlayerAndTools:
                    this.Options.Add(new OptionsElement($"{i18n.Get("player.title")}:"));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("player.infinite-stamina"), config.InfiniteStamina, value => config.InfiniteStamina = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("player.infinite-health"), config.InfiniteHealth, value => config.InfiniteHealth = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("player.increased-movement-speed"), config.IncreasedMovement, value => config.IncreasedMovement = value));
                    this.Options.Add(new CheatsOptionsSlider(i18n.Get("player.movement-speed"), 0, 10, this.Config));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("player.one-hit-kill"), config.OneHitKill, value => config.OneHitKill = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("player.max-daily-luck"), config.MaxDailyLuck, value => config.MaxDailyLuck = value));

                    this.Options.Add(new OptionsElement($"{i18n.Get("tools.title")}:"));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("tools.infinite-water"), config.InfiniteWateringCan, value => config.InfiniteWateringCan = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("tools.one-hit-break"), config.OneHitBreak, value => config.OneHitBreak = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("tools.harvest-with-sickle"), config.HarvestSickle, value => config.HarvestSickle = value));

                    this.Options.Add(new OptionsElement($"{i18n.Get("money.title")}:"));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("money.add-amount", new { amount = 100 }), 2, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("money.add-amount", new { amount = 1000 }), 3, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("money.add-amount", new { amount = 10000 }), 4, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("money.add-amount", new { amount = 100000 }), 5, this.OptionSlots[0].bounds.Width, config, cheats, i18n));

                    this.Options.Add(new OptionsElement($"{i18n.Get("casino-coins.title")}:"));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("casino-coins.add-amount", new { amount = 100 }), 6, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("casino-coins.add-amount", new { amount = 1000 }), 7, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("casino-coins.add-amount", new { amount = 10000 }), 8, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    break;

                case MenuTab.FarmAndFishing:
                    this.Options.Add(new OptionsElement($"{i18n.Get("farm.title")}:"));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("farm.water-all-fields"), 9, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("farm.durable-fences"), config.DurableFences, value => config.DurableFences = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("farm.instant-build"), config.InstantBuild, value => config.InstantBuild = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("farm.always-auto-feed"), config.AutoFeed, value => config.AutoFeed = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("farm.infinite-hay"), config.InfiniteHay, value => config.InfiniteHay = value));

                    this.Options.Add(new OptionsElement($"{i18n.Get("fishing.title")}:"));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fishing.instant-catch"), config.InstantCatch, value => config.InstantCatch = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fishing.instant-bite"), config.InstantBite, value => config.InstantBite = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fishing.always-throw-max-distance"), config.ThrowBobberMax, value => config.ThrowBobberMax = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fishing.always-treasure"), config.AlwaysTreasure, value => config.AlwaysTreasure = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fishing.durable-tackles"), config.DurableTackles, value => config.DurableTackles = value));

                    this.Options.Add(new OptionsElement($"{i18n.Get("fast-machines.title")}:"));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.cask"), config.FastCask, value => config.FastCask = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.furnace"), config.FastFurnace, value => config.FastFurnace = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.recycling-machine"), config.FastRecyclingMachine, value => config.FastRecyclingMachine = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.crystalarium"), config.FastCrystalarium, value => config.FastCrystalarium = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.incubator"), config.FastIncubator, value => config.FastIncubator = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.slime-incubator"), config.FastSlimeIncubator, value => config.FastSlimeIncubator = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.keg"), config.FastKeg, value => config.FastKeg = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.preserves-jar"), config.FastPreservesJar, value => config.FastPreservesJar = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.cheese-press"), config.FastCheesePress, value => config.FastCheesePress = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.mayonnaise-machine"), config.FastMayonnaiseMachine, value => config.FastMayonnaiseMachine = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.loom"), config.FastLoom, value => config.FastLoom = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.oil-maker"), config.FastOilMaker, value => config.FastOilMaker = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.seed-maker"), config.FastSeedMaker, value => config.FastSeedMaker = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.charcoal-kiln"), config.FastCharcoalKiln, value => config.FastCharcoalKiln = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.slime-egg-press"), config.FastSlimeEggPress, value => config.FastSlimeEggPress = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.tapper"), config.FastTapper, value => config.FastTapper = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.lightning-rod"), config.FastLightningRod, value => config.FastLightningRod = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.bee-house"), config.FastBeeHouse, value => config.FastBeeHouse = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.mushroom-box"), config.FastMushroomBox, value => config.FastMushroomBox = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("fast-machines.worm-bin"), config.FastWormBin, value => config.FastWormBin = value));
                    break;

                case MenuTab.Skills:
                    this.Options.Add(new OptionsElement($"{i18n.Get("skills.title")}:"));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("skills.increase-farming"), 200, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("skills.increase-mining"), 201, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("skills.increase-foraging"), 202, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("skills.increase-fishing"), 203, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("skills.increase-combat"), 204, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("skills.reset"), 205, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    break;

                case MenuTab.Weather:
                    this.Options.Add(new OptionsElement($"{i18n.Get("weather.title")}:"));
                    this.Options.Add(new CheatsOptionsElement($"{i18n.Get("weather.current")}", 1));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("weather.sunny"), 10, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("weather.raining"), 11, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("weather.lightning"), 12, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("weather.snowing"), 13, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    break;

                case MenuTab.Relationships:
                    this.Options.Add(new OptionsElement($"{i18n.Get("relationships.title")}:"));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("relationships.give-gifts-anytime"), config.AlwaysGiveGift, value => config.AlwaysGiveGift = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("relationships.no-decay"), config.NoFriendshipDecay, value => config.NoFriendshipDecay = value));
                    this.Options.Add(new OptionsElement($"{i18n.Get("relationships.friends")}:"));

                    foreach (NPC npc in Utility.getAllCharacters().OrderBy(p => p.name))
                    {
                        if ((npc.name != "Sandy" || Game1.player.mailReceived.Contains("ccVault")) && npc.name != "???" && npc.name != "Bouncer" && npc.name != "Marlon" && npc.name != "Gil" && npc.name != "Gunther" && !npc.IsMonster && !(npc is Horse) && !(npc is Pet))
                        {
                            if (Game1.player.friendships.ContainsKey(npc.name))
                                this.Options.Add(new CheatsOptionsNPCSlider(npc, 9999));
                        }
                    }
                    break;

                case MenuTab.WarpLocations:
                    this.Options.Add(new OptionsElement($"{i18n.Get("warp.title")}:"));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.farm"), 100, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.pierre-shop"), 101, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.blacksmith"), 102, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.museum"), 103, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.saloon"), 104, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.community-center"), 105, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.carpenter"), 106, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.adventurers-guild"), 107, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.ranch"), 113, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.mines"), 109, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.willy-shop"), 110, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.wizard-tower"), 114, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.hats"), 115, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.desert"), 112, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.sandy-shop"), 119, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.casino"), 120, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.quarry"), 108, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.new-beach"), 111, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.secret-woods"), 116, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.sewer"), 117, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("warp.bathhouse"), 118, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    break;

                case MenuTab.Time:
                    this.Options.Add(new OptionsElement($"{i18n.Get("time.title")}:"));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("time.freeze-inside"), config.FreezeTimeInside, value => config.FreezeTimeInside = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("time.freeze-caves"), config.FreezeTimeCaves, value => config.FreezeTimeCaves = value));
                    this.Options.Add(new CheatsOptionsCheckbox(i18n.Get("time.freeze-everywhere"), config.FreezeTime, value => config.FreezeTime = value));
                    this.Options.Add(new CheatsOptionsSlider(i18n.Get("time.time"), 10, 20, this.Config, width: 100));
                    break;

                case MenuTab.Controls:
                    this.Options.Add(new OptionsElement($"{i18n.Get("controls.title")}:"));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("controls.open-menu"), 1000, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("controls.freeze-time"), 1001, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("controls.grow-tree"), 1002, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    this.Options.Add(new CheatsOptionsInputListener(i18n.Get("controls.grow-crops"), 1003, this.OptionSlots[0].bounds.Width, config, cheats, i18n));
                    break;
            }
            this.SetScrollBarToCurrentIndex();
        }

        private void SetScrollBarToCurrentIndex()
        {
            if (!this.Options.Any())
                return;
            this.Scrollbar.bounds.Y = this.ScrollbarRunner.Height / Math.Max(1, this.Options.Count - CheatsMenu.ItemsPerPage + 1) * this.CurrentItemIndex + this.UpArrow.bounds.Bottom + Game1.pixelZoom;
            if (this.CurrentItemIndex != this.Options.Count - CheatsMenu.ItemsPerPage)
                return;
            this.Scrollbar.bounds.Y = this.DownArrow.bounds.Y - this.Scrollbar.bounds.Height - Game1.pixelZoom;
        }

        public override void leftClickHeld(int x, int y)
        {
            if (GameMenu.forcePreventClose)
                return;
            base.leftClickHeld(x, y);
            if (this.IsScrolling)
            {
                int num = this.Scrollbar.bounds.Y;
                this.Scrollbar.bounds.Y = Math.Min(this.yPositionOnScreen + this.height - Game1.tileSize - Game1.pixelZoom * 3 - this.Scrollbar.bounds.Height, Math.Max(y, this.yPositionOnScreen + this.UpArrow.bounds.Height + Game1.pixelZoom * 5));
                this.CurrentItemIndex = Math.Min(this.Options.Count - CheatsMenu.ItemsPerPage, Math.Max(0, (int)(this.Options.Count * (double)((y - this.ScrollbarRunner.Y) / (float)this.ScrollbarRunner.Height))));
                this.SetScrollBarToCurrentIndex();
                if (num == this.Scrollbar.bounds.Y)
                    return;
                Game1.soundBank.PlayCue("shiny4");
            }
            else
            {
                if (this.OptionsSlotHeld == -1 || this.OptionsSlotHeld + this.CurrentItemIndex >= this.Options.Count)
                    return;
                this.Options[this.CurrentItemIndex + this.OptionsSlotHeld].leftClickHeld(x - this.OptionSlots[this.OptionsSlotHeld].bounds.X, y - this.OptionSlots[this.OptionsSlotHeld].bounds.Y);
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if ((Game1.options.menuButton.Contains(new InputButton(key)) || key.ToString() == this.Config.OpenMenuKey) && this.readyToClose() && this.CanClose)
            {
                Game1.exitActiveMenu();
                Game1.soundBank.PlayCue("bigDeSelect");
                return;
            }

            this.CanClose = true;

            if (this.OptionsSlotHeld == -1 || this.OptionsSlotHeld + this.CurrentItemIndex >= this.Options.Count)
                return;
            this.Options[this.CurrentItemIndex + this.OptionsSlotHeld].receiveKeyPress(key);
        }

        public override void receiveGamePadButton(Buttons key)
        {
            if (key == Buttons.LeftShoulder || key == Buttons.RightShoulder)
            {
                // rotate tab index
                int index = this.Tabs.FindIndex(p => p.name == this.CurrentTab.ToString());
                if (key == Buttons.LeftShoulder)
                    index--;
                if (key == Buttons.RightShoulder)
                    index++;

                if (index >= this.Tabs.Count)
                    index = 0;
                if (index < 0)
                    index = this.Tabs.Count - 1;

                // open menu with new index
                MenuTab tabID = this.GetTabID(this.Tabs[index]);
                Game1.activeClickableMenu = new CheatsMenu(tabID, this.Config, this.Cheats, this.TranslationHelper);
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (GameMenu.forcePreventClose)
                return;
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && this.CurrentItemIndex > 0)
                this.UpArrowPressed();
            else
            {
                if (direction >= 0 || this.CurrentItemIndex >= Math.Max(0, this.Options.Count - CheatsMenu.ItemsPerPage))
                    return;
                this.DownArrowPressed();
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            if (GameMenu.forcePreventClose)
                return;
            base.releaseLeftClick(x, y);
            if (this.OptionsSlotHeld != -1 && this.OptionsSlotHeld + this.CurrentItemIndex < this.Options.Count)
                this.Options[this.CurrentItemIndex + this.OptionsSlotHeld].leftClickReleased(x - this.OptionSlots[this.OptionsSlotHeld].bounds.X, y - this.OptionSlots[this.OptionsSlotHeld].bounds.Y);
            this.OptionsSlotHeld = -1;
            this.IsScrolling = false;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (GameMenu.forcePreventClose)
                return;
            if (this.DownArrow.containsPoint(x, y) && this.CurrentItemIndex < Math.Max(0, this.Options.Count - CheatsMenu.ItemsPerPage))
            {
                this.DownArrowPressed();
                Game1.soundBank.PlayCue("shwip");
            }
            else if (this.UpArrow.containsPoint(x, y) && this.CurrentItemIndex > 0)
            {
                this.UpArrowPressed();
                Game1.soundBank.PlayCue("shwip");
            }
            else if (this.Scrollbar.containsPoint(x, y))
                this.IsScrolling = true;
            else if (!this.DownArrow.containsPoint(x, y) && x > this.xPositionOnScreen + this.width && (x < this.xPositionOnScreen + this.width + Game1.tileSize * 2 && y > this.yPositionOnScreen) && y < this.yPositionOnScreen + this.height)
            {
                this.IsScrolling = true;
                this.leftClickHeld(x, y);
                this.releaseLeftClick(x, y);
            }
            this.CurrentItemIndex = Math.Max(0, Math.Min(this.Options.Count - CheatsMenu.ItemsPerPage, this.CurrentItemIndex));
            for (int index = 0; index < this.OptionSlots.Count; ++index)
            {
                if (this.OptionSlots[index].bounds.Contains(x, y) && this.CurrentItemIndex + index < this.Options.Count && this.Options[this.CurrentItemIndex + index].bounds.Contains(x - this.OptionSlots[index].bounds.X, y - this.OptionSlots[index].bounds.Y - 5))
                {
                    this.Options[this.CurrentItemIndex + index].receiveLeftClick(x - this.OptionSlots[index].bounds.X, y - this.OptionSlots[index].bounds.Y + 5);
                    this.OptionsSlotHeld = index;
                    break;
                }
            }

            foreach (var tab in this.Tabs)
            {
                if (tab.bounds.Contains(x, y))
                {
                    MenuTab tabID = this.GetTabID(tab);
                    Game1.activeClickableMenu = new CheatsMenu(tabID, this.Config, this.Cheats, this.TranslationHelper);
                    break;
                }
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true) { }

        public override void performHoverAction(int x, int y)
        {
            if (GameMenu.forcePreventClose)
                return;
            this.HoverText = "";
            this.UpArrow.tryHover(x, y);
            this.DownArrow.tryHover(x, y);
            this.Scrollbar.tryHover(x, y);
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            if (!Game1.options.showMenuBackground)
                spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);

            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true);
            CJB.DrawTextBox(this.Title.bounds.X, this.Title.bounds.Y, Game1.dialogueFont, this.Title.name, 1);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);
            for (int index = 0; index < this.OptionSlots.Count; ++index)
            {
                if (this.CurrentItemIndex >= 0 && this.CurrentItemIndex + index < this.Options.Count)
                    this.Options[this.CurrentItemIndex + index].draw(spriteBatch, this.OptionSlots[index].bounds.X, this.OptionSlots[index].bounds.Y + 5);
            }
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            if (!GameMenu.forcePreventClose)
            {

                foreach (ClickableComponent tab in this.Tabs)
                {
                    MenuTab tabID = this.GetTabID(tab);
                    CJB.DrawTextBox(tab.bounds.X + tab.bounds.Width, tab.bounds.Y, Game1.smallFont, tab.label, 2, this.CurrentTab == tabID ? 1F : 0.7F);
                }

                this.UpArrow.draw(spriteBatch);
                this.DownArrow.draw(spriteBatch);
                if (this.Options.Count > CheatsMenu.ItemsPerPage)
                {
                    IClickableMenu.drawTextureBox(spriteBatch, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.ScrollbarRunner.X, this.ScrollbarRunner.Y, this.ScrollbarRunner.Width, this.ScrollbarRunner.Height, Color.White, Game1.pixelZoom, false);
                    this.Scrollbar.draw(spriteBatch);
                }
            }
            if (this.HoverText != "")
                IClickableMenu.drawHoverText(spriteBatch, this.HoverText, Game1.smallFont);

            if (!Game1.options.hardwareCursor)
                spriteBatch.Draw(Game1.mouseCursors, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.options.gamepadControls ? 44 : 0, 16, 16), Color.White, 0f, Vector2.Zero, Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
        }


        /*********
        ** Private methods
        *********/
        private void DownArrowPressed()
        {
            this.DownArrow.scale = this.DownArrow.baseScale;
            ++this.CurrentItemIndex;
            this.SetScrollBarToCurrentIndex();
        }

        private void UpArrowPressed()
        {
            this.UpArrow.scale = this.UpArrow.baseScale;
            --this.CurrentItemIndex;
            this.SetScrollBarToCurrentIndex();
        }

        /// <summary>Get the tab constant represented by a tab component.</summary>
        /// <param name="tab">The component to check.</param>
        private MenuTab GetTabID(ClickableComponent tab)
        {
            if (!Enum.TryParse(tab.name, out MenuTab tabID))
                throw new InvalidOperationException($"Couldn't parse tab name '{tab.name}'.");
            return tabID;
        }
    }
}
