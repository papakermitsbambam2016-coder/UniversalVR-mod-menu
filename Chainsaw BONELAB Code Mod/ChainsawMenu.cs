using System;
using BoneLib.BoneMenu;
using MelonLoader;
using UnityEngine;

namespace ChainsawBONELABCodeMod
{
    internal static class ChainsawMenu
    {
        private static Page page;
        private static bool initialized;

        public static void Setup()
        {
            if (initialized)
                return;

            try
            {
                page = Page.Root.CreatePage("Chainsaw", Color.red);

                page.CreateFunction("Spawn Chainsaw", Color.green, () =>
                {
                    MelonLogger.Msg("[Chainsaw] Spawn button pressed.");

                    try
                    {
                        ChainsawSpawner.Spawn();
                        MelonLogger.Msg("[Chainsaw] Spawn callback finished.");
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error("[Chainsaw] Spawn callback crashed: " + ex);
                    }
                });

                page.CreateFunction("Despawn Chainsaws", Color.red, () =>
                {
                    MelonLogger.Msg("[Chainsaw] Despawn button pressed.");
                    ChainsawSpawner.DespawnAll();
                });

                page.CreateBool("Enabled", Color.green, Config.Enabled, value =>
                {
                    Config.Enabled = value;
                });

                page.CreateBool("Motor Sound", Color.yellow, Config.MotorSound, value =>
                {
                    Config.MotorSound = value;
                });

                page.CreateFloat("Damage", Color.red, Config.Damage, 1f, 0f, 50f, value =>
                {
                    Config.Damage = value;
                });

                page.CreateFloat("Blade Speed", Color.cyan, Config.BladeDegreesPerSecond, 100f, 0f, 5000f, value =>
                {
                    Config.BladeDegreesPerSecond = value;
                });

                page.CreateFloat("Damage Interval", Color.magenta, Config.DamageInterval, 0.025f, 0.025f, 0.5f, value =>
                {
                    Config.DamageInterval = value;
                });

                page.CreateBool("Debug Mode", Color.white, Config.DebugMode, value =>
                {
                    Config.DebugMode = value;
                });

                initialized = true;
                MelonLogger.Msg("[Chainsaw] BoneMenu page created.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[Chainsaw] BoneMenu setup failed: " + ex);
            }
        }
    }
}
