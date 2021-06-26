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




    public void ConnectToTwitch()
    {
        UnityEngine.Debug.Log("Connecting...");
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
    }


    void Update()
    {
        PingCounter += Time.unscaledDeltaTime;
        CommandCooldown += Time.unscaledDeltaTime;

        if (PingCounter > PingInterval)
        {
            Writer.WriteLine("PING " + URL);
            Writer.Flush();
            PingCounter = 0f;
        }


        if (!Twitch.Connected)
        {
            ConnectToTwitch();
        }
        if (Twitch.Available > 0)
        {
            string message = Reader.ReadLine();

            if (message.Contains("PRIVMSG"))
            {
                int splitPoint = message.IndexOf("!");
                string chatter = message.Substring(1, splitPoint - 1);

                splitPoint = message.IndexOf(":",1);
                string msg = message.Substring(splitPoint + 1);

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
                            UnityEngine.Debug.Log("Found a valid command!");
                            if (com.MinVotes == -1 || SettingsManager.Mode == TwitchMode.Chaos)
                            {
                                if ((CommandCooldown > TwitchManager.CommandCooldown) || !TwitchManager.CooldownEnabled)
                                {
                                    CommandCooldown = 0f;
                                    com.functocall(chatter, param);
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
                                    com.functocall(persontocall[0], persontocall[1]);
                                    if ((CommandCooldown > TwitchManager.CommandCooldown) || !TwitchManager.CooldownEnabled)
                                    {
                                        CommandCooldown = 0f;
                                    }
                                    TwitchManager.CommandVotes[com.command] = new List<string[]>();
                                }

                                if (Singleton<BaseGameManager>.Instance)
                                {
                                    Singleton<BaseGameManager>.Instance.CollectNotebooks(0); //this is really stupid
                                }


                            }
                        }

                }
            }

            UnityEngine.Debug.Log(message);
        }
    }
}

