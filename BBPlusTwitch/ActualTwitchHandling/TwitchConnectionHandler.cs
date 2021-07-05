using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Net;
using System.IO;
using System.Text;
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
using System.Runtime.Serialization.Formatters.Binary;
using StolenYetHelpfulCode;
using BBPlusTwitch;
using System.Net.Sockets;


public enum ConnectionStatus
{
    NotAttempted,
    Retry,
    Connecting,
    Connected,
    Failed,
    Offline
}

public class TwitchConnectionHandler : MonoBehaviour
{

    public static TwitchConnectionHandler Instance;

    TcpClient Twitch;
    StreamReader Reader;
    StreamWriter Writer;

    const string URL = "irc.chat.twitch.tv";
    const int PORT = 6667;
    const float PingInterval = 60f;
    const string Prefix = "!";

    public float PingCounter;
    public float CommandCooldown;

    public ConnectionStatus ConnectionStat;


    readonly string[] username_begginings = new string[]{
        "Baldi",
        "Playtime",
        "Principal",
        "Pomp",
        "Billy",
        "Apple",
        "Shrek",
        "Meme",
        "Epic",
        "Pepsi",
        "BSODA",
        "Lemonade",
        "Orange",
        "Vegetable",
        "Ben",
        "Dude",
        "Gal",
        "Swamp",
        "Donkey",
        "Bear",
        "Twitch",
        "Dab",
        "Fortnite",
        "SFW",
        "Jeffy"
    };

    readonly string[] username_middle = new string[]{
        "_Fan",
        "_Enjoyer",
        "_Lover",
        "_Hater",
        "_Cool_Dude",
        "_Bad_Dude",
        "_Artist",
        "_Man",
        "_John",
        "_Carl",
        "_Jr",
        "_Jeff",
        "_"
    };

    readonly string[] username_end = new string[]{
        "_Real",
        "",
        "",
        "_Official",
        "_Alt",
        "",
        ""
    };

    public System.Random RNG;


    public string GenerateUsername()
    {
        return username_begginings[UnityEngine.Random.Range((int)0, username_begginings.Length - 1)] + username_middle[UnityEngine.Random.Range((int)0, username_middle.Length - 1)] + username_end[UnityEngine.Random.Range((int)0, username_end.Length - 1)] + UnityEngine.Random.Range((int)0, 9999);
    }



    public void ConnectToTwitch()
    {
        if ((ConnectionStat != ConnectionStatus.NotAttempted) && (ConnectionStat != ConnectionStatus.Retry)) return;
        if (SettingsManager.Mode == TwitchMode.Offline)
        {
            ConnectionStat = ConnectionStatus.Offline;
            UnityEngine.Debug.Log("Currently in Offline mode!");
            return;
        }
        UnityEngine.Debug.Log("Connecting...");
        ConnectionStat = ConnectionStatus.Connecting;
        Twitch = new TcpClient(URL, PORT);
        Reader = new StreamReader(Twitch.GetStream());
        Writer = new StreamWriter(Twitch.GetStream());

        Writer.WriteLine("PASS " + BaldiTwitch.Oath.Value);
        Writer.WriteLine("NICK " + BaldiTwitch.Name.Value);
        Writer.WriteLine("JOIN #" + BaldiTwitch.UserName.Value.ToLower());
        Writer.Flush();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this)
            {
                GameObject.Destroy(Instance);
                Instance = this;
            }
            
        }
        DontDestroyOnLoad(this);
        ConnectToTwitch();
        RNG = new System.Random();
    }


    public string ReturnRandomCommand()
    {
        OfflineCommand cmd;
        if (SettingsManager.OfflineUseWeighted)
        {
            WeightedSelection<OfflineCommand>[] offlinecmds = TwitchManager.WeightedCommands.ToArray();
            cmd = WeightedSelection<OfflineCommand>.ControlledRandomSelection(offlinecmds, RNG);
        }
        else
        {
            cmd = TwitchManager.WeightedCommands[RNG.Next(0, TwitchManager.WeightedCommands.Count - 1)].selection;
        }
        int param = RNG.Next(0, cmd.valid_params.Length - 1 == -1 ? 0 : cmd.valid_params.Length - 1);
        return Prefix + cmd.command + " " + (cmd.valid_params.Length == 0 ? "" : cmd.valid_params[param]);
    }



    public void ShowText(string textoshow)
    {
        if (!SettingsManager.ShowCommands) return;
        if ((bool)Singleton<CoreGameManager>.Instance)
        {
            Singleton<CoreGameManager>.Instance.GetHud(0).ShowEventText(textoshow,2f);
        }
    }

    public void HandleCommand(string msg, string chatter, bool ignorevotes)
    {
        if (msg.StartsWith(Prefix))
        {
            //if (CommandCooldown > TwitchManager.CommandCooldown || !TwitchManager.CooldownEnabled)
            //this is a command, do shit
            int indexof = msg.IndexOf(" ");
            string cmd = indexof == -1 ? msg : msg.Substring(0, indexof);
            int cutoff = cmd.Length + 1;
            cmd = cmd.Substring(Prefix.Length);
            string param = "";
            if (cutoff < msg.Length)
            {
                param = msg.Substring(cutoff);
            }
            TwitchCommand com;
            if (TwitchManager.Commands.TryGetValue(cmd, out com))
            {
                if (!com.Enabled) return; //not actually a valid command fuck you
                UnityEngine.Debug.Log("Found a valid command!");
                if (com.MinVotes == -1 || (SettingsManager.Mode == TwitchMode.Chaos || ignorevotes))
                {
                    if ((CommandCooldown > TwitchManager.CommandCooldown) || !TwitchManager.CooldownEnabled)
                    {
                        CommandCooldown = 0f;
                        if (com.functocall(chatter, param))
                        {
                            ShowText("<color=#FFFF00FF>" + chatter + "<color=\"white\">: " + msg);
                        }
                        
                    }
                }
                else
                {
                    System.Random rng = new System.Random();
                    int votestowin = (int)((float)com.MinVotes * (SettingsManager.Mode == TwitchMode.Speedy ? 0.5f : 1f));
                    List<string[]> votes = TwitchManager.CommandVotes[com.command];
                    string[] dup = votes.Find(x => x[0] == chatter);
                    if (dup == null ? true : dup.Length == 0)
                    {
                        TwitchManager.CommandVotes[com.command].Add(new string[2] {
                                chatter,
                                param
                            });
                    }
                    else
                    {
                        UnityEngine.Debug.Log("Attempted duplicate vote: " + chatter);
                    }

                    if (votes.Count >= com.MinVotes)
                    {
                        string[] persontocall = votes[rng.Next(0, votes.Count - 1)];
                        if (com.functocall(persontocall[0], persontocall[1]))
                        {
                            ShowText("<color=#FFFF00FF>" + persontocall[0] + "<color=\"white\">: " + Prefix + com.command + " " + persontocall[1]);
                        }
                        
                        if ((CommandCooldown > TwitchManager.CommandCooldown) || !TwitchManager.CooldownEnabled)
                        {
                            CommandCooldown = 0f;
                        }
                        TwitchManager.CommandVotes[com.command] = new List<string[]>();
                    }

                    if (Singleton<BaseGameManager>.Instance)
                    {
                        Singleton<BaseGameManager>.Instance.CollectNotebooks(0); //this is really FUCKING stupid
                    }


                }
            }

        }
    }

    void Update()
    {
        PingCounter += Time.unscaledDeltaTime;
        CommandCooldown += Time.unscaledDeltaTime;

        if ((CommandCooldown > TwitchManager.CommandCooldown) && SettingsManager.Mode == TwitchMode.Offline)
        {
            CommandCooldown = 0f;
            HandleCommand(ReturnRandomCommand(), GenerateUsername(), true);
        }

        if (SettingsManager.Mode == TwitchMode.Offline) return;

        if (PingCounter > PingInterval)
        {
            Writer.WriteLine("PING " + URL);
            Writer.Flush();
            PingCounter = 0f;
        }

        if (!Twitch.Connected && ConnectionStat == ConnectionStatus.Connected) //we got disconnected!
        {
            ConnectionStat = ConnectionStatus.Retry;
            ConnectToTwitch();
        }



        if (Twitch.Available > 0)
        {
            string message = Reader.ReadLine();

            UnityEngine.Debug.Log(message);

            if (message.Contains("PRIVMSG"))
            {
                int splitPoint = message.IndexOf("!");
                string chatter = message.Substring(1, splitPoint - 1);

                splitPoint = message.IndexOf(":", 1);
                string msg = message.Substring(splitPoint + 1);

                HandleCommand(msg, chatter, false);
            }
            else if (message.Contains("PING"))
            {
                Writer.WriteLine("PONG " + URL);
                PingCounter = 0f; //reset this because we just sent a notice that we're still here
                Writer.Flush();
            }
            else if (message.Contains("JOIN") && ConnectionStat == ConnectionStatus.Connecting) //since this is an else if, this will only trigger if it does not contain PRIVMSG
            {
                UnityEngine.Debug.Log("Connection Successful!");
                ConnectionStat = ConnectionStatus.Connected;
            }
        }

    }
}

