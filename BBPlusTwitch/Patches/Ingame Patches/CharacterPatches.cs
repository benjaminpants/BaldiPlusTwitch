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
    [HarmonyPatch(typeof(Principal))]
    [HarmonyPatch("Scold")]
    class ScoldUpdatePatch
    {
        static bool Prefix(Principal __instance, ref string brokenRule, ref AudioManager ___audMan, ref SoundObject ___audDetention, ref SoundObject ___audNoEating)
        {
            if (brokenRule == "No Reason")
            {
                ___audMan.FlushQueue(__instance);
                ___audMan.QueueAudio(___audDetention);
                return false;
            }
            if (brokenRule == "Eating")
            {
                ___audMan.FlushQueue(__instance);
                ___audMan.QueueAudio(___audNoEating);
                return false;
            }
            return true;
        }
    }
}
