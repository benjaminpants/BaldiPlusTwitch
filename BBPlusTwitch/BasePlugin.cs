using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Net;
//BepInEx stuff
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib; //god im hoping i got the right version of harmony
using BepInEx.Configuration;
using BBPlusNameAPI;
using System.Collections.Generic;
//this code is reused from BaldiMP, lol

namespace BBPlusTwitch
{
    [BepInPlugin("mtm101.rulerp.bbplus.balditwitch", "BB+ Twitch Intergration", "0.0.30.0")]



    public class BaldiTwitch : BaseUnityPlugin
    {
        public static ConfigEntry<string> Name;

        public static ConfigEntry<string> UserName;

        public static ConfigEntry<string> Oath;

        public static BaldiTwitch Instance;


        public static void CreateManagers()
        {
            GameObject newgam = new GameObject();
            newgam.name = "TwitchHandlerObject";
            newgam.AddComponent<TwitchConnectionHandler>();
            GameObject newlod = new GameObject();
            newlod.name = "MonoLogicObject";
            newlod.AddComponent<MonoLogicManager>();
            IngameCommands.AddCommands();
        }

        public static void SetFunnyMode(TwitchMode mode, float cooldown)
        {
            TwitchManager.CommandCooldown = cooldown;
            SettingsManager.Mode = mode;
            TwitchManager.CooldownEnabled = false;
            if (cooldown != 0f && mode != TwitchMode.Offline)
            {
                TwitchManager.CooldownEnabled = true;
            }
            CreateManagers();
            NameMenuManager.AllowContinue(true);
        }

        public static void SetVanilla(Name_MenuObject obj)
        {
            SetFunnyMode(TwitchMode.Vanilla, 0f);
        }

        public static void SetSpeedy(Name_MenuObject obj)
        {
            SetFunnyMode(TwitchMode.Speedy, 0f);
        }

        public static void SetChaos(Name_MenuObject obj)
        {
            SetFunnyMode(TwitchMode.Chaos, 0f);
        }

        public static void SetChaos5s(Name_MenuObject obj)
        {
            SetFunnyMode(TwitchMode.Chaos, 5f);
        }

        public static void SetChaos10s(Name_MenuObject obj)
        {
            SetFunnyMode(TwitchMode.Chaos, 10f);
        }
        public static void SetChaos15s(Name_MenuObject obj)
        {
            SetFunnyMode(TwitchMode.Chaos, 15f);
        }

        public static void SetOffline(Name_MenuObject obj)
        {
            SetFunnyMode(TwitchMode.Offline, 5f);
        }

        public static object ToggleCMDS()
        {
            SettingsManager.ShowCommands = !SettingsManager.ShowCommands;
            return SettingsManager.ShowCommands;
        }

        public static object ToggleWeights()
        {
            SettingsManager.OfflineUseWeighted = !SettingsManager.OfflineUseWeighted;
            return SettingsManager.OfflineUseWeighted;
        }

        public static object ToggleVotes()
        {
            SettingsManager.ShowVotes = !SettingsManager.ShowVotes;
            return SettingsManager.ShowVotes;
        }



        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                if (Instance != this)
                {
                    Instance = this;
                }

            }
            Logger.LogDebug("hey wassup bb+ twitch mod here to say snas");
            Name = Config.Bind(new ConfigDefinition(
                "Auth Info",
                "Name"

                ), "generic_name");

            UserName = Config.Bind(new ConfigDefinition(
                "Auth Info",
                "Channel"

                ), "channel_to_connect_to");

            Oath = Config.Bind(new ConfigDefinition(
                "Auth Info",
                "OATH"

                ), "oath:https://twitchapps.com/tmi/");

            Harmony harmony = new Harmony("mtm101.rulerp.bbplus.balditwitch");

            NameMenuManager.AddPreStartPage("mandatorytwitchshits",true);
            List<Name_MenuObject> Objects = new List<Name_MenuObject>();
            Objects.Add(new Name_MenuGeneric("startvanilla", "Vanilla", SetVanilla));
            Objects.Add(new Name_MenuGeneric("startspeedy", "Speedy", SetSpeedy));
            Objects.Add(new Name_MenuGeneric("startchaos", "Chaos", SetChaos));
            Objects.Add(new Name_MenuGeneric("startchaos5s", "Chaos(5s)", SetChaos5s));
            Objects.Add(new Name_MenuGeneric("startchaos10s", "Chaos(10s)", SetChaos10s));
            Objects.Add(new Name_MenuGeneric("startchaos15s", "Chaos(15s)", SetChaos15s));
            Objects.Add(new Name_MenuGeneric("startoffline", "Offline", SetOffline));
            NameMenuManager.AddToPageBulk("mandatorytwitchshits", Objects);


            Objects = new List<Name_MenuObject>();
            NameMenuManager.AddPage("mtm101twitchoptions","options");
            Objects.Add(new Name_MenuOption("showcmds", "Show CMDS", SettingsManager.ShowCommands, ToggleCMDS));
            Objects.Add(new Name_MenuOption("showvotes", "Show Votes", SettingsManager.ShowVotes, ToggleVotes));
            Objects.Add(new Name_MenuOption("togglefairoffline", "Balanced Offline", SettingsManager.OfflineUseWeighted, ToggleWeights));
            NameMenuManager.AddToPageBulk("mtm101twitchoptions",Objects);
            NameMenuManager.AddToPage("options",new Name_MenuFolder("totwitchoptions","BB+ Twitch","mtm101twitchoptions"));


            harmony.PatchAll();

        }
    }
}
