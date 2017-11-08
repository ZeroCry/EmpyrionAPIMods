﻿using System;
using System.Collections.Generic;
using System.Linq;
using Eleon.Modding;
using ProtoBuf;


namespace XangosBackpackModule
{

    public class XangosBackpack : ModInterface
    {
        public string step;
        public IDictionary<int, ItemStack[]> vBackpackDictionary = new Dictionary<int, ItemStack[]>(){};
        public ItemStack[] EmptyExchange = new ItemStack[0];
        public Dictionary<int, string> BackpackChatDictionary = new Dictionary<int, string>() { };
        public Dictionary<int, string> IDDictionary = new Dictionary<int, string>() { };
        public Dictionary<int, string> SendDictionary = new Dictionary<int, string>() { };
        public Dictionary<int, string> RewardDictionary = new Dictionary<int, string>() { };
        public Dictionary<int, string> InboxDictionary = new Dictionary<int, string>() { };
        public Dictionary<int, string> WhichItemExchange = new Dictionary<int, string>() { };
        public List<int> PlysList;
        public FactionInfoList FactionListDump;
        public List<List<string>> CommandSetup;
        public List<int> IDRequestor;

        ModGameAPI GameAPI;

        private void Messenger(String msgType, int Priority, int player, String msg, int Duration)
        {
            if (msgType == "ChatAsServer")
            {
                string command = "SAY '" + msg + "'";
                GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_InGameMessage_AllPlayers, new Eleon.Modding.PString(command));
            }
            if (msgType == "Alert")
                GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(player, msg, Convert.ToByte(Priority), Duration));
        }


        private void LogFile(String FileName, String FileData)
        {
            if (!System.IO.File.Exists("Content\\Mods\\xango\\"+ FileName))
            {
                System.IO.File.Create("Content\\Mods\\xango\\" + FileName);
            }
            string FileData2 = FileData + Environment.NewLine;
            System.IO.File.AppendAllText("Content\\Mods\\Xango\\"+FileName, FileData2);
        }

        public ItemStack[] buildItemStack(int playerId, string job)
        {
            string[] bagLines = System.IO.File.ReadAllLines(job);
            int itemStackSize = bagLines.Count();
            ItemStack[] itStack = new ItemStack[itemStackSize];
            for (int i = 0; i < itemStackSize; ++i)
            {
                itStack[i] = new ItemStack(Convert.ToInt32(bagLines[i][1]), Convert.ToInt32(bagLines[i][2]));
                itStack[i].slotIdx = Convert.ToByte(bagLines[i][0]);
                itStack[i].ammo = Convert.ToInt32(bagLines[i][3]);
                itStack[i].decay = Convert.ToInt32(bagLines[i][4]);

            }
            vBackpackDictionary.Add(playerId, itStack);
            return itStack;
        }

        struct SetupData
        {
            private string Admin;
            private List<string> File;
            private List<string> BlockedOnPlayfieldsFile;
            private string Math ;
        }
        
            private void LoadSetup()
        {
            string[] SetupFile = System.IO.File.ReadAllLines("Content\\Mods\\Xango\\setup.txt");
            //int SetupSize = SetupFile.Count();
            //loop thru SetupFile
            for (int i = 0; i < SetupFile.Count(); ++i)
            {
                //if line starts with Space, line is command setting
                if (SetupFile[i].StartsWith("    "))
                {
                    string[] FileLine = SetupFile[i].Split(Convert.ToChar(":"));
                    if (FileLine[0] == "    Admin")
                    {
                        string CommandParameter = "Admin";
                        string CommandValue = FileLine[1];
                    }
                    else if (FileLine[0] == "    File")
                    {
                        string CommandParameter = "File";
                        string[] CommandFile = System.IO.File.ReadAllLines("Content\\Mods\\Xango\\CommandFiles\\" + FileLine[0]);
                    }
                    else if (FileLine[0] == "    BlockedOnPlayfieldsFile")
                    {
                        string CommandParameter = "BlockedOnPlayfieldsFile";
                        string[] CommandFile = System.IO.File.ReadAllLines("Content\\Mods\\Xango\\CommandFiles\\" + FileLine[0]);

                    }
                    else if (FileLine[0] == "    Math")
                    {
                        string CommandParameter = "Math";
                        string CommandValue = FileLine[1];
                    }
                    else if (FileLine[0] == "    Enabled")
                    {
                        string CommandParameter = "Enabled";
                        Boolean CommandValue = Convert.ToBoolean(FileLine[1]);
                    }

                }
                //else, line is command
                else
                {
                    string[] FileLine = SetupFile[i].Split(Convert.ToChar(":"));
                    string Command = FileLine[0];
                }

            }
        }

        public void Game_Start(ModGameAPI gameAPI)
        {

            LogFile("chat.txt", "Mod Loaded");
            GameAPI = gameAPI;
            //GameAPI.Game_Request(CmdId.Request_Get_Factions, (ushort)CmdId.Request_Get_Factions, new Id(1));
           
        }
        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            try
            {
                /*
                if (data is ItemStack[])
                {
                    bagdata = (ItemStack[])data;
                }
                */
                //int i = 0;
                //int triggerPlayer = 0;
                switch (cmdId)
                {

                    case CmdId.Event_Player_Connected:
                        Id pc = (Id)data;

                        try { string[] bagLines = System.IO.File.ReadAllLines("Content\\Mods\\Xango\\players\\EID" + pc.id + "\\VirtualBackpack.txt");
                            int itemStackSize = bagLines.Count();
                            ItemStack[] itStack = new ItemStack[itemStackSize];
                            for (int i = 0; i < itemStackSize; ++i)
                            {
                                string[] bagLinesSplit = bagLines[i].Split(',');
                                itStack[i] = new ItemStack(Convert.ToInt32(bagLinesSplit[1]), Convert.ToInt32(bagLinesSplit[2])); //1=ItemNumber, 2=StackSize
                                itStack[i].slotIdx = Convert.ToByte(bagLinesSplit[0]);//0=SlotNumber
                                itStack[i].ammo = Convert.ToInt32(bagLinesSplit[3]);//3=Ammo
                                itStack[i].decay = Convert.ToInt32(bagLinesSplit[4]);//4=Decay
                            }
                            vBackpackDictionary.Add(pc.id, itStack);
                        }
                        catch { };
                        LogFile("chat.txt", "Player " + pc.id + " Connected");
                        try { System.IO.Directory.CreateDirectory("Content\\Mods\\Xango\\players\\EID" + pc.id); } catch { };
                        try { System.IO.File.Create("Content\\Mods\\Xango\\players\\EID" + pc.id + "\\Mail.txt"); } catch { };
                        break;
                    case CmdId.Event_Player_Disconnected:
                        Id pd = (Id)data;
                        LogFile("chat.txt", "Player " + pd.id + " DisConnected");
                        vBackpackDictionary.Remove(pd.id);
                        break;
                    case CmdId.Event_Player_List:
                        IdList PlayerIDs = (IdList)data;
                        foreach (var player in PlayerIDs.list)
                        {
                            PlysList.Add(Convert.ToInt32(player));
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Id(player));
                        }
                        break;
                    case CmdId.Event_Player_Info:
                        //LogFile("Chat.txt", "Start Event_Player_Info");
                        PlayerInfo PlayerInfoReceived = (PlayerInfo)data;
                        /*
                        if (PlysList.Contains(Convert.ToInt32(PlayerInfoReceived.entityId)) == true)
                        {
                            PlysList.Remove(Convert.ToInt32(PlayerInfoReceived.entityId));
                            var factionabbr = Convert.ToString(PlayerInfoReceived.factionId);
                            if (PlayerInfoReceived.factionId == PlayerInfoReceived.entityId)
                            {
                                Messenger("ChatAsServer", 0, IDRequestor[0], PlayerInfoReceived.entityId + "  [   ]  " + PlayerInfoReceived.playerName + "  PF=" + PlayerInfoReceived.playfield, 0);
                                IDRequestor.Remove(IDRequestor[0]);
                                LogFile("plys.txt", PlayerInfoReceived.entityId + "  [   ]  " + PlayerInfoReceived.playerName + "  PlayField=" + PlayerInfoReceived.playfield);
                            }
                            else
                            {
                                foreach (FactionInfo faction in FactionListDump.factions)
                                {
                                    if (faction.factionId == PlayerInfoReceived.factionId)
                                    {
                                        factionabbr = faction.abbrev;
                                    }
                                    Messenger("ChatAsServer", 0, IDRequestor[0], PlayerInfoReceived.entityId + "  [" + factionabbr + "]  " + PlayerInfoReceived.playerName + "  PF=" + PlayerInfoReceived.playfield, 0);
                                    IDRequestor.Remove(IDRequestor[0]);
                                    LogFile("plys.txt", PlayerInfoReceived.entityId + "  [" + PlayerInfoReceived.factionId + "]  " + PlayerInfoReceived.playerName + "  PF=" + PlayerInfoReceived.playfield);
                                    break;
                                }
                            }
                        }
                        */
                        //LogFile("Chat.txt", "BackpackDictionary starts here");
                        if (BackpackChatDictionary.ContainsKey(PlayerInfoReceived.entityId))
                        {
                            BackpackChatDictionary.Remove(PlayerInfoReceived.entityId);
                            if (vBackpackDictionary.ContainsKey(PlayerInfoReceived.entityId))
                            {
                                BackpackChatDictionary.Remove(PlayerInfoReceived.entityId);
                                //LogFile("Chat.txt", "show backpack");
                                GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "Virtual Backpack", "Extra Inventory Space, Yay!", "Save", vBackpackDictionary[PlayerInfoReceived.entityId]));
                                step = "Request ItemExchange";
                            }
                            else
                            {
                                if (System.IO.File.Exists("Content\\Mods\\Xango\\players\\EID" + Convert.ToString(PlayerInfoReceived.entityId) + "\\VirtualBackpack.txt") == true)
                                {
                                    //LogFile("Chat.txt", "Show Blank Backpack");
                                    GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "Virtual Backpack", "Extra Inventory Space, Yay!", "Save", EmptyExchange));
                                }
                                else
                                {
                                    //LogFile("Chat.txt", "Build Blank Backpack");
                                    System.IO.File.Create("Content\\Mods\\Xango\\players\\EID" + Convert.ToString(PlayerInfoReceived.entityId) + "\\VirtualBackpack.txt");
                                    EmptyExchange = buildItemStack(PlayerInfoReceived.entityId, "Content\\Mods\\Xango\\players\\EID" + Convert.ToString(PlayerInfoReceived.entityId) + "\\VirtualBackpack.txt");
                                    //System.Threading.Thread.Sleep(5000);
                                    GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "Virtual Backpack", "Extra Inventory Space, Yay!", "Save", EmptyExchange));
                                    //GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "Virtual Backpack", "Extra Inventory Space, Yay!", "Save", vBackpackDictionary[PlayerInfoReceived.entityId]));
                                }
                            }
                            WhichItemExchange[PlayerInfoReceived.clientId] = "Backpack";
                        }
                        //LogFile("chat.txt", "IDDictionary starts here");
                        if (IDDictionary.ContainsKey(PlayerInfoReceived.entityId) == true)
                        {
                            //LogFile("chat.txt", "IDDictionary in PlayerInfoReceived triggered");
                            GameAPI.Game_Request(CmdId.Request_Get_Factions, (ushort)CmdId.Request_Get_Factions, new Id(1));
                            GameAPI.Game_Request(CmdId.Request_Player_List, (ushort)CmdId.Request_Player_List, null);
                        }

                        //LogFile("chat.txt", "SendDictionary starts here");
                        if (SendDictionary.ContainsKey(PlayerInfoReceived.entityId) == true)
                        {
                            //LogFile("chat.txt", "SendDictionary in PlayerInfoReceived triggered");
                            var Message = SendDictionary[PlayerInfoReceived.entityId].Split(new[] { ' ' }, 3);
                            GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "To: " + Message[1], Message[2], "Send", EmptyExchange));
                            //string timestamp = Convert.ToString(DateTime.Now);
                            //System.IO.File.Create("Content\\Mods\\Xango\\Mail\\" + timestamp + ".txt");
                            //string MailEntry = timestamp + " " + PlayerInfoReceived.entityId + "," + "New" + "," + Message[1];
                            //Prep inventory to write to file
                            //System.IO.File.AppendAllText("Content\\Mods\\Xango\\EID" + Message[0] + "\\mail.txt", "data");
                            WhichItemExchange[PlayerInfoReceived.clientId] = "MailSend";
                        }
                        //LogFile("chat.txt", "RewardDictionary ends here");
                        if (RewardDictionary.ContainsKey(PlayerInfoReceived.entityId) == true)
                        {
                            WhichItemExchange[PlayerInfoReceived.clientId] = "MailReward";
                        }
                        //LogFile("chat.txt", "InboxDictionary starts here");
                        if (InboxDictionary.ContainsKey(PlayerInfoReceived.entityId) == true)
                        {
                            //LogFile("chat.txt", "InboxDictionary in PlayerInfoReceived triggered");
                            string[] UserMail = System.IO.File.ReadAllLines("Content\\Mods\\Xango\\players\\EID" + PlayerInfoReceived.entityId + "\\mail.txt");
                            string[] FirstMessage = System.IO.File.ReadAllLines("Content\\Mods\\Xango\\Mail\\" + UserMail[0] + ".txt");
                            //string MailEntry = timestamp + " " + exchangeInfo.id + "," + "New" + "," + Message[1];
                            string[] InboxMessage = FirstMessage[0].Split(new[] { ' ' }, 4);
                            ItemStack[] MessageInventory = buildItemStack(PlayerInfoReceived.entityId, "Content\\Mods\\Xango\\Mail\\" + InboxMessage[0] + ".txt");
                            GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CmdId.Request_Player_ItemExchange, new ItemExchangeInfo(PlayerInfoReceived.entityId, "From: " + InboxMessage[1], InboxMessage[3], "Close", MessageInventory));
                            //WIP
                            //System.IO.File.ReadAllLines("Content\\Mods\\Xango\\Mail\\" + "something");
                            WhichItemExchange[PlayerInfoReceived.clientId] = "MailInbox";
                        }


                        System.IO.File.WriteAllText("Content\\Mods\\Xango\\players\\EID" + PlayerInfoReceived.entityId+ "\\PlayerInfo.txt", string.Empty);
                        LogFile("players\\EID" + PlayerInfoReceived.entityId+ "\\PlayerInfo.txt", "playerName= " + Convert.ToString(PlayerInfoReceived.playerName));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "entityId= " + Convert.ToString(PlayerInfoReceived.entityId));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "permission= " + Convert.ToString(PlayerInfoReceived.permission));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "playfield= " + Convert.ToString(PlayerInfoReceived.playfield));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "startPlayfield= " + Convert.ToString(PlayerInfoReceived.startPlayfield));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "pos= " + Convert.ToString(PlayerInfoReceived.pos.x) + ",  " + Convert.ToString(PlayerInfoReceived.pos.y) + ",  " + Convert.ToString(PlayerInfoReceived.pos.z));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "rot= " + Convert.ToString(PlayerInfoReceived.rot.x) + ",  " + Convert.ToString(PlayerInfoReceived.rot.y) + ",  " + Convert.ToString(PlayerInfoReceived.rot.z));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "clientId= " + Convert.ToString(PlayerInfoReceived.clientId));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "SteamId= " + Convert.ToString(PlayerInfoReceived.steamId));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "SteamOwnerId= " + Convert.ToString(PlayerInfoReceived.steamOwnerId));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "origin= " + Convert.ToString(PlayerInfoReceived.origin));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "ping= " + Convert.ToString(PlayerInfoReceived.ping));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "factionGroup= " + Convert.ToString(PlayerInfoReceived.factionGroup));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "factionRole= " + Convert.ToString(PlayerInfoReceived.factionRole));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "factionId= " + Convert.ToString(PlayerInfoReceived.factionId ));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "credits= " + Convert.ToString(PlayerInfoReceived.credits));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "bodyTemp= " + Convert.ToString(PlayerInfoReceived.bodyTemp));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "TempMax= " + Convert.ToString(PlayerInfoReceived.bodyTempMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "food= " + Convert.ToString(PlayerInfoReceived.food));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "foodMax= " + Convert.ToString(PlayerInfoReceived.foodMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "health= " + Convert.ToString(PlayerInfoReceived.health));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "healthmax= " + Convert.ToString(PlayerInfoReceived.healthMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "oxygen= " + Convert.ToString(PlayerInfoReceived.oxygen));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "oxygenMax= " + Convert.ToString(PlayerInfoReceived.oxygenMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "radiation= " + Convert.ToString(PlayerInfoReceived.radiation));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "radiationMax= " + Convert.ToString(PlayerInfoReceived.radiationMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "stamina= " + Convert.ToString(PlayerInfoReceived.stamina));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "staminaMax= " + Convert.ToString(PlayerInfoReceived.staminaMax));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "exp= " + Convert.ToString(PlayerInfoReceived.exp));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "upgrade= " + Convert.ToString(PlayerInfoReceived.upgrade));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "died= " + Convert.ToString(PlayerInfoReceived.died));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "kills= " + Convert.ToString(PlayerInfoReceived.kills));

                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "producedPrefabs= " + Convert.ToString(PlayerInfoReceived.producedPrefabs));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "bpInFactory= " + Convert.ToString(PlayerInfoReceived.bpInFactory));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "bpRemainingTime= " + Convert.ToString(PlayerInfoReceived.bpRemainingTime));
                        LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\PlayerInfo.txt", "bpResourcesInFactory= " + Convert.ToString(PlayerInfoReceived.bpResourcesInFactory));
                        
                        /*
                        try
                        {

                            int bagdatacounter = 0;
                            //for (int bagdatacounter = 0; bagdatacounter <= 35; bagdatacounter++)
                            while (bagdatacounter < PlayerInfoReceived.bag.Count())
                            {
                                LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\BagData.txt", Convert.ToString(PlayerInfoReceived.bag[bagdatacounter].id + ",  " + Convert.ToString(PlayerInfoReceived.bag[bagdatacounter].count) + ",  " + Convert.ToString(PlayerInfoReceived.bag[bagdatacounter].slotIdx) + ",  " + Convert.ToString(PlayerInfoReceived.bag[bagdatacounter].ammo) + ",  " + Convert.ToString(PlayerInfoReceived.bag[bagdatacounter].decay)));
                                bagdatacounter++;
                            }
                        }
                        catch { };
                        
                        try
                        {
                            for (int i = 0; i <= PlayerInfoReceived.toolbar.Count(); i++)
                            {
                                LogFile("players\\EID" + PlayerInfoReceived.entityId + "\\ToolbarData.txt", Convert.ToString(PlayerInfoReceived.toolbar[i].id + ",  " + Convert.ToString(PlayerInfoReceived.toolbar[i].count) + ",  " + Convert.ToString(PlayerInfoReceived.toolbar[i].slotIdx) + ",  " + Convert.ToString(PlayerInfoReceived.toolbar[i].ammo) + ",  " + Convert.ToString(PlayerInfoReceived.toolbar[i].decay)));
                            }
                        }
                        catch { };
                        */
                        break;
                    case CmdId.Event_Player_Inventory:
                        break;
                    case CmdId.Event_Player_ItemExchange:
                        ItemExchangeInfo exchangeInfo = (ItemExchangeInfo)data;
                        LogFile("chat.txt", "ItemExchange Triggered");
                        if (WhichItemExchange[exchangeInfo.id] == "Backpack")
                        {
                            LogFile("chat.txt", "ItemExchange Backpack");
                            vBackpackDictionary[exchangeInfo.id] = exchangeInfo.items;
                            System.IO.File.WriteAllText("Content\\Mods\\Xango\\players\\EID" + exchangeInfo.id + "\\VirtualBackpack.txt", string.Empty);
                            for (int i = 0; i <= exchangeInfo.items.Count(); i++)
                            {
                                //LogFile("ExchangeData.txt", Convert.ToString(exchangeInfo.items[i].id));
                                LogFile("players\\EID" + exchangeInfo.id + "\\VirtualBackpack.txt", Convert.ToString(exchangeInfo.items[i].slotIdx) + "," + Convert.ToString(exchangeInfo.items[i].id) + "," + Convert.ToString(exchangeInfo.items[i].count) + "," + Convert.ToString(exchangeInfo.items[i].ammo) + "," + Convert.ToString(exchangeInfo.items[i].decay));
                            }
                            step = "itemExchange complete";
                        }
                        //WIP
                        else if (WhichItemExchange[exchangeInfo.id].StartsWith("To:"))
                        {
                            LogFile("chat.txt", "ItemExchange To:");
                            string timestamp = Convert.ToString(DateTime.Now);
                            System.IO.File.Create("Content\\Mods\\Xango\\Mail\\" + timestamp + ".txt");
                            var Message = SendDictionary[Convert.ToInt32(exchangeInfo.title)].Split(new[] { ' ' }, 2);
                            string MailEntry = timestamp + " " + exchangeInfo.id + "," + "New" + "," + Message[1];
                            //Prep inventory to write to file
                            for (int i = 0; i <= exchangeInfo.items.Count(); i++)
                            {
                                //LogFile("ExchangeData.txt", Convert.ToString(exchangeInfo.items[i].id));
                                LogFile("players\\EID" + exchangeInfo.id + "\\VirtualBackpack.txt", Convert.ToString(exchangeInfo.items[i].slotIdx) + "," + Convert.ToString(exchangeInfo.items[i].id) + "," + Convert.ToString(exchangeInfo.items[i].count) + "," + Convert.ToString(exchangeInfo.items[i].ammo) + "," + Convert.ToString(exchangeInfo.items[i].decay));
                            }
                            System.IO.File.AppendAllText("Content\\Mods\\Xango\\EID" + Message[1] + "\\mail.txt", "data" + Environment.NewLine);
                        }
                        else if (WhichItemExchange[exchangeInfo.id] == "MailInbox")
                        {
                            LogFile("chat.txt", "ItemExchange MailInbox?");

                        }
                        LogFile("chat.txt", "ItemExchange Done");
                        break;
                    case CmdId.Event_Player_Credits:
                        break;
                    case CmdId.Event_Player_ChangedPlayfield:
                        break;
                    case CmdId.Event_Player_GetAndRemoveInventory:
                        break;
                    case CmdId.Event_Playfield_List:
                        break;
                    case CmdId.Event_Playfield_Stats:
                        break;
                    case CmdId.Event_Playfield_Loaded:
                        break;
                    case CmdId.Event_Playfield_Unloaded:
                        break;
                    case CmdId.Event_Playfield_Entity_List:
                        break;
                    case CmdId.Event_Dedi_Stats:
                        break;
                    case CmdId.Event_GlobalStructure_List:
                        break;
                    case CmdId.Event_Entity_PosAndRot:
                        break;
                    case CmdId.Event_Faction_Changed:
                        FactionChangeInfo factionChange = (FactionChangeInfo)data;
                        GameAPI.Game_Request(CmdId.Request_Get_Factions, (ushort)CmdId.Request_Get_Factions, new Id(1));
                        break;
                    case CmdId.Event_Get_Factions:
                        FactionInfoList factioninfo = (FactionInfoList)data;
                        FactionListDump = factioninfo;
                        //FactionInfoDict.Remove("FactionDump");
                        //FactionInfoDict.Add("FactionDump", factioninfo);
                        break;
                    case CmdId.Event_Statistics:
                        break;
                    case CmdId.Event_NewEntityId:
                        break;
                    case CmdId.Event_Player_DisconnectedWaiting:
                        Id pdw = new Id();
                        GameAPI.Console_Write("Player " + pdw.id + " Failed Login Attempt");
                        break;
                    case CmdId.Event_ChatMessage:
                        ChatInfo ci = (ChatInfo)data;
                        LogFile("Chat.txt", ci.playerId + " SAYS: " + ci.msg);
                        //GameAPI.Game_Request(CmdId.Request_Player_List, (ushort)CmdId.Request_Player_List, null);
                        if (ci.msg.StartsWith("s! "))
                        {
                            ci.msg = ci.msg.Remove(0,3);
                        }
                        //LogFile("Chat.txt", ci.playerId + " s!  strip check?:" + ci.msg);
                        ci.msg = ci.msg.ToLower();

                        if (ci.msg.StartsWith("/backpack"))
                        {
                            BackpackChatDictionary[ci.playerId] = ci.msg;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Eleon.Modding.Id(ci.playerId));
                            step = "Request Playerinfo";
                        }
                        else if (ci.msg.StartsWith("/id"))
                        {
                            //WIP
                            LogFile("chat.txt", "/id command received");
                            IDDictionary[ci.playerId] = ci.msg;
                            IDRequestor.Add(ci.playerId);
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Eleon.Modding.Id(ci.playerId));
                            step = "Request Playerinfo";
                            LogFile("chat.txt", "/id complete, requesting player info");
                        }
                        else if (ci.msg.StartsWith("/inbox"))
                        {
                            InboxDictionary[ci.playerId] = ci.msg;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Eleon.Modding.Id(ci.playerId));
                            LogFile("chat.txt", "/inbox command received");
                            step = "Request Playerinfo";
                        }
                        else if (ci.msg.StartsWith("/send"))
                        {
                            LogFile("chat.txt", "/send command received");
                            SendDictionary[ci.playerId] = ci.msg;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Eleon.Modding.Id(ci.playerId));
                            step = "Request Playerinfo";
                            LogFile("chat.txt", "/send complete, requesting player info");
                        }
                        else if (ci.msg.StartsWith("/reward"))
                        {
                            RewardDictionary[ci.playerId] = ci.msg;
                            GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Eleon.Modding.Id(ci.playerId));
                            step = "Request Playerinfo";
                        }

                        ;

                        break;
                    case CmdId.Event_Structure_BlockStatistics:
                        break;
                    case CmdId.Event_AlliancesAll:
                        break;
                    case CmdId.Event_AlliancesFaction:
                        break;
                    case CmdId.Event_BannedPlayers:
                        break;
                    case CmdId.Event_TraderNPCItemSold:
                        break;
                    case CmdId.Event_Ok:
                        /*
                        LogFile("CurrentProject.txt", "Event OK: " + step);
                        break;
                        */
                        break;
                    case CmdId.Event_Error:
                        ErrorInfo err = (ErrorInfo)data;
                        ErrorType err2 = (ErrorType)data;
                        //try { Messenger("Alert", 0, 0, "Error Info " + Convert.ToString(err), 1000); } catch { };
                        //try { Messenger("Alert", 0, 0, "Error Type " + Convert.ToString(err2), 1000); } catch { };
                        
                        LogFile("ERROR.txt", Convert.ToString(err2) + ": " + Convert.ToString(err));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                GameAPI.Console_Write(ex.Message);
            }

        }
        public void Game_Update()
        {
        }
        public void Game_Exit()
        {
        }

    }
}

