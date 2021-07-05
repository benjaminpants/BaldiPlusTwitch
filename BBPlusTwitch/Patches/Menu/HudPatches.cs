using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Net;
using System.Text;
using System.IO;
//BepInEx stuff
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
//more stuff
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using StolenYetHelpfulCode;
using TMPro;

namespace BBPlusTwitch
{
    [HarmonyPatch(typeof(HudManager))]
    [HarmonyPatch("UpdateText")]
    class UpdateTextPatch
    {
        public static float TextSize = -1f;
        static bool Prefix(UpdateTextPatch __instance, ref int textVal, ref string text, ref TMP_Text[] ___textBox) //handle showing votes
        {
            if (textVal == 0)
            {
                if (!SettingsManager.ShowVotes) return true;
                StringBuilder builder = new StringBuilder();
                builder.AppendLine();
                foreach (KeyValuePair<string,List<string[]>> kvp in TwitchManager.CommandVotes)
                {
                    if (TwitchManager.Commands[kvp.Key].MinVotes != -1 && (SettingsManager.Mode != TwitchMode.Chaos && SettingsManager.Mode != TwitchMode.Offline) && TwitchManager.Commands[kvp.Key].Enabled)
                    {
                        builder.AppendLine(kvp.Key + "(" + (kvp.Value.Count) + ")");
                    }
                }
                text += builder.ToString();
                if (TextSize == -1f)
                {
                    TextSize = ___textBox[textVal].fontSize * 0.5f;
                }
                ___textBox[textVal].fontSize = TextSize;
            }
            return true;
        }
    }


}
