using System;
using System.Collections.Generic;
using BoneLib.BoneMenu;
using MelonLoader;
using UnityEngine;

namespace ChainsawBONELABCodeMod
{
    internal static class ChainsawMenu
    {
        private static Page page;

        public static void Setup()
        {
            try
            {
                page = Page.Root.CreatePage(
                    "Chainsaw",
                    Color.red
                );

                page.CreateFunction(
                    "Spawn Chainsaw",
                    Color.green,
                    () => ChainsawSpawner.Spawn()
                );

                page.CreateFunction(
                    "Despawn Chainsaws",
                    Color.red,
                    () => ChainsawSpawner.DespawnAll()
                );

                page.CreateBool(
                    "Motor Sound",
                    Color.yellow,
                    Config.MotorSound,
                    value =>
                    {
                        Config.MotorSound = value;
                    }
                );

                page.CreateFloat(
                    "Damage",
                    Color.red,
                    Config.Damage,
                    1f,
                    50f,
                    1f,
                    value =>
                    {
                        Config.Damage = value;
                    }
                );

                page.CreateFloat(
                    "Blade Speed",
                    Color.cyan,
                    Config.BladeDegreesPerSecond,
                    0f,
                    5000f,
                    100f,
                    value =>
                    {
                        Config.BladeDegreesPerSecond = value;
                    }
                );

                page.CreateBool(
                    "Debug Mode",
                    Color.white,
                    Config.DebugMode,
                    value =>
                    {
                        Config.DebugMode = value;
                    }
                );

                MelonLogger.Msg(
                    "[Chainsaw] BoneMenu created."
                );
            }
            catch (Exception ex)
            {
                MelonLogger.Error(
                    "[Chainsaw] BoneMenu failed: " + ex
                );
            }
        }
    }
}
