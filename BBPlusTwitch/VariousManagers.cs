using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Net;
using System.Linq;
//BepInEx stuff
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
//more stuff
using System.Collections.Generic;
using System.Collections;
using StolenYetHelpfulCode;
using BepInEx.Configuration;
using System.IO;

namespace BBPlusTwitch
{
    public enum NameMenuState
    {
        ModeSelector,
        Options,
        SaveSelect,
        Loading
    }

    public enum TwitchMode
    {
        Vanilla,
        Speedy,
        Chaos,
        Offline
    }
    public static class SettingsManager
    {
        public static TwitchMode Mode = TwitchMode.Vanilla;
        public static bool ShowCommands
        {
            get
            {
                Load();
                return ShowCommandsPriv;
            }

            set
            {
                ShowCommandsPriv = value;
                Save();
            }


        }

        public static bool ShowVotes
        {
            get
            {
                Load();
                return ShowVotesPriv;
            }

            set
            {
                ShowVotesPriv = value;
                Save();
            }


        }

        public static bool OfflineUseWeighted
        {
            get
            {
                Load();
                return OfflineUseWeightedPriv;
            }

            set
            {
                OfflineUseWeightedPriv = value;
                Save();
            }


        }

        public static bool ShowCommandsPriv = false;
        public static bool ShowVotesPriv = true;
        public static bool OfflineUseWeightedPriv = true;
        private static bool HasAlreadyLoaded = false;
        const byte SaveVersionNumber = 2;

        private static void Load()
        {
            if (HasAlreadyLoaded)
            {
                return;
            }
            HasAlreadyLoaded = true;
            string pathtoload = Path.Combine(Application.persistentDataPath, "Mods", "BBTwitch","data.dat");
            if (File.Exists(pathtoload))
            {
                FileStream stream = File.OpenRead(pathtoload);
                BinaryReader reader = new BinaryReader(stream);
                try
                {
                    if (reader.ReadByte() != SaveVersionNumber)
                    {
                        reader.Close();
                        File.Delete(pathtoload);
                        Save();
                        return;
                    }
                    ShowCommandsPriv = reader.ReadBoolean();
                    ShowVotesPriv = reader.ReadBoolean();
                    OfflineUseWeightedPriv = reader.ReadBoolean();
                }
                catch(Exception E)
                {
                    UnityEngine.Debug.LogError(E.Message);
                }
                reader.Close();
            }
            else
            {
                Save();
            }
        }

        private static void Save()
        {
            string pathtosave = Path.Combine(Application.persistentDataPath,"Mods","BBTwitch");
            if (!Directory.Exists(pathtosave))
            {
                Directory.CreateDirectory(pathtosave);
            }
            DirectoryInfo fo = new DirectoryInfo(pathtosave);
            FileInfo[] filefo = fo.GetFiles("data.dat");
            if (filefo.Length != 0)
            {
                filefo[0].Delete();
            }
            FileStream stream = File.Create(Path.Combine(pathtosave, "data.dat"));
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(SaveVersionNumber);
            writer.Write(ShowCommandsPriv);
            writer.Write(ShowVotesPriv);
            writer.Write(OfflineUseWeightedPriv);

            writer.Close();
        }

    }


    public static class GeneralBaldiStuff
    {
        public static readonly ItemObject[] Items = Resources.FindObjectsOfTypeAll<ItemObject>();
        public static readonly FieldTripObject[] FieldTrips = Resources.FindObjectsOfTypeAll<FieldTripObject>();
        public static RandomEvent[] RandomEvents;


    }

    public class OfflineCommand
    {
        public string command;
        public string[] valid_params;

        public OfflineCommand(string cmd, params string[] parms)
        {
            command = cmd;
            valid_params = parms;
        }

    }

    public class WeightedOfflineCommand : WeightedSelection<OfflineCommand> //um????
    {
        public WeightedOfflineCommand(string cmd, int w, params string[] parms)
        {
            weight = w;
            selection = new OfflineCommand(cmd, parms);
        }

    }


    public class TwitchCommand
    {
        public string command;
        public string description;
        public Func<string, string, bool> functocall;
        public int MinVotes;
        public bool Enabled = true;

        public TwitchCommand(string name, string desc, Func<string, string, bool> func, int max, bool enabled)
        {
            command = name;
            description = desc;
            functocall = func;
            MinVotes = max;
            Enabled = enabled;
        }
    }


   

    public static class TwitchManager
    {

        public static Dictionary<string, TwitchCommand> Commands = new Dictionary<string, TwitchCommand>();

        public static Dictionary<string, List<string[]>> CommandVotes = new Dictionary<string, List<string[]>>();

        public static List<WeightedOfflineCommand> WeightedCommands = new List<WeightedOfflineCommand>();

        public static float CommandCooldown = 5f;

        public static bool CooldownEnabled = false;

        public static void AddWeightedCommand(string name,int weight, params string[] parms)
        {
            WeightedCommands.Add(new WeightedOfflineCommand(name,weight,parms));
        }

        public static bool AddCommand(string cmd, Func<string,string, bool> func, int min = -1)
        {
            UnityEngine.Debug.Log("Attempting to add command:" + cmd);
            CommandVotes.Add(cmd,new List<string[]>());
            try
            {
                Commands.Add(cmd, new TwitchCommand(cmd, "description missing", func, BaldiTwitch.Instance.Config.Bind(new ConfigDefinition(
                "Command: " + cmd,
                "Votes Needed"
                ), min).Value, BaldiTwitch.Instance.Config.Bind(new ConfigDefinition(
                "Command: " + cmd,
                "Enabled"
                ), true).Value)); //this is really fucking stupid, remind me to create a custom method of storing this shit
            }
            catch
            {
                return false;
            }
            return true;
        }
    }


}
