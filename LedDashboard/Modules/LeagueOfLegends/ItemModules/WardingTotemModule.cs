﻿using LedDashboard.Modules.BasicAnimation;
using LedDashboard.Modules.LeagueOfLegends.ChampionModules.Common;
using LedDashboard.Modules.LeagueOfLegends.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedDashboard.Modules.LeagueOfLegends.ItemModules
{
    [Item(ITEM_ID)]
    class WardingTotemModule : ItemModule
    {
        public const int ITEM_ID = 3340;
        public const string ITEM_NAME = "WardingTotem";


        // Variables


        // Cooldown

        private int cooldownPerCharge = 0;

        /// <summary>
        /// Creates a new champion instance.
        /// </summary>
        /// <param name="ledCount">Number of LEDs in the strip</param>
        /// <param name="gameState">Game state data</param>
        /// <param name="preferredLightMode">Preferred light mode</param>
        /// <param name="preferredCastMode">Preferred ability cast mode (Normal, Quick Cast, Quick Cast with Indicator)</param>
        public static WardingTotemModule Create(int ledCount, GameState gameState, int itemSlot, LightingMode preferredLightMode, AbilityCastPreference preferredCastMode = AbilityCastPreference.Normal)
        {
            return new WardingTotemModule(ledCount, gameState, ITEM_ID, itemSlot, preferredLightMode, preferredCastMode);
        }

        private WardingTotemModule(int ledCount, GameState gameState, int itemID, int itemSlot, LightingMode preferredLightMode, AbilityCastPreference preferredCastMode)
            : base(ledCount, itemID, ITEM_NAME, itemSlot, gameState, preferredLightMode, preferredCastMode, true)
        {
            // Initialization for the item module occurs here.

            // TODO: This is because of a game bug, IT WILL GET FIXED and this will have to be changed
            ItemCooldownController.SetCooldown(this.ItemID, GetCooldownPerCharge());
        }

        protected override AbilityCastMode GetItemCastMode() => AbilityCastMode.Normal();

        protected override void OnItemActivated(object s, EventArgs e)
        {
            int wardCharges = 0;
            int cd = ItemCooldownController.GetCooldownRemaining(this.ItemID);
            int cdpercharge = GetCooldownPerCharge();
            int rechargedSecondCharge = -1;
            if (cd > cdpercharge)
                wardCharges = 0;
            else if (cd > 0)
            {
                wardCharges = 1;
                rechargedSecondCharge = cdpercharge - cd;
            }
            else
            {
                wardCharges = 2;
            }

            if (wardCharges > 0)
            {
                if (wardCharges > 1)
                {
                    ItemCooldownController.SetCooldown(ITEM_ID, GetCooldownPerCharge() + 1800); // Warding small 2s cooldown
                }
                else
                {
                    // some magic here regarding trinket cooldowns to handle edge cases when you swap trinkets.
                    ItemCooldownController.SetCooldown(ITEM_ID, cdpercharge * 2 - rechargedSecondCharge - 100);
                    ItemCooldownController // this trinket affects the other trinket cooldowns
                        .SetCooldown(
                                        FarsightAlterationModule.ITEM_ID,
                                        FarsightAlterationModule.GetCooldownDuration(GameState.AverageChampionLevel) - rechargedSecondCharge - 100);
                    ItemCooldownController
                        .SetCooldown(
                                        OracleLensModule.ITEM_ID,
                                        OracleLensModule.GetCooldownDuration(GameState.AverageChampionLevel) - rechargedSecondCharge - 100);

                    //CooldownDuration = cooldownPerCharge - 100; // substract some duration to account for other delays;
                }
            }
        }

        public bool HasCharge => ItemCooldownController.GetCooldownRemaining(ITEM_ID) < GetCooldownPerCharge();

        private int GetCooldownPerCharge()
        {
            return GetCooldownDuration(GameState.AverageChampionLevel);
        }

        public static int GetCooldownDuration(double averageLevel) => GetCooldownDuration(247.059, 7.059, averageLevel);

        public static WardingTotemModule Current { get; set; } // HACK: Access to current instance
    }
}
