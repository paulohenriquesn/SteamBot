using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using SteamKit2;
using Newtonsoft.Json.Linq;
using System.Threading;
using Dapper;
using System.IO;
using System.Data.SQLite;

namespace SteamBot_
{

    public class RoomBehaviour
    {
        public int RoomID { get; set; }
        public string CodeRoom { get; set; }
        public SteamID OwnerRoom { get; set; }
        public List<SteamID> UsersRoom = new List<SteamID>();
    }

    public class Command
    {
        public string command_;
        public Action action;
    }
    class Program
    {
        private static Braintwo bw = new Braintwo();
        private static string[] memory_ = new string[2] { String.Empty, String.Empty };
        private static List<RoomBehaviour> Rooms = new List<RoomBehaviour>();
        private static Dictionary<string, int> Rooms_dic = new Dictionary<string, int>();

        private static List<SteamID> ListPlayingGame = new List<SteamID>();
        private static int CountToAddExp = 0;
        private static SQLiteConnection dbConnection = new SQLiteConnection(@"Data Source=bot.db;Pooling=true;FailIfMissing=false;Version=3");
        //private static VistaDBConnection dbConnection = new VistaDBConnection(@"Data source='C:\Users\Paulo Henrique\OneDrive\databases\steambot.vdb5'");
        private static List<SteamID> listFriendsSteamID = new List<SteamID>();

        public static string[] Argument = new string[128];
        private static SteamID steamIDMemory;
        static Random random = new Random();

        private static Dictionary<string, Command> Commands = new Dictionary<string, Command>();

        public static Brainfuck BrainfuckClient;
        public static void sayBrainFuck(string _)
        {
            steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, _);
        }

        private static SteamClient steamClient;
        private static CallbackManager callbackManager;
        private static SteamUser steamUser;
        private static SteamFriends steamFriends;

        private static bool botIsRunning;

        private static BOT bot;

        public enum statusConsole
        {
            ERROR = 1,
            LOGIN_DENIED = 2
        }
        public enum scenes
        {
            Login = 0,
            TryingConnect = 1,
            Running = 2
        }
        private static scenes currentStatus = scenes.Login;

        static string ReadPassword()
        {
            StringBuilder pass = new StringBuilder();
            ConsoleKeyInfo key = Console.ReadKey(true);
            Console.Write("*");
            while (key.Key != ConsoleKey.Enter || key.Modifiers > 0)
            {
                pass.Append(key.KeyChar);
                key = Console.ReadKey(true);
                Console.Write("*");
            }

            return pass.ToString();
        }


        static void ConsoleStatus(statusConsole status, string error_)
        {
            switch (status)
            {
                case statusConsole.LOGIN_DENIED:
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR [LOGIN_DENIED]: {error_} ");
                    Console.ResetColor();
                    break;
                case statusConsole.ERROR:
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: {error_}");
                    Console.ResetColor();
                    break;
            }
        }
        static void CreateCommand(string command, Action action_)
        {
            Commands.Add(command, new Command() { command_ = command, action = action_ });
        }
        static void ExecuteCommand(string command)
        {
            Commands[command].action();
            for (int i = 0; i < Argument.Length; i++) { Argument[i] = String.Empty; }
        }

        static void Main(string[] args)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            //Commands           
            ; try
            {
                CreateCommand("@braintwo", new Action(delegate ()
                {
                    bw.Interpreter("$0x", Argument[0]);
                }));
                CreateCommand("@brainfuck", new Action(delegate ()
                {
                    BrainfuckClient = new Brainfuck(Argument[0]);
                    BrainfuckClient.RunCommand(Argument[0]);
                }));
                CreateCommand("@xvideos", new Action(delegate ()
                {
                    steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, "calma ae xo ve");
                restart:
                    try
                    {
                        List<string> commentsList = new List<string>();

                        string comment_(string a)
                        {
                            bool count = true;
                            string result = String.Empty;
                            for (int i = 0; i < a.Length; i++)
                            {
                                if (count)
                                    if (a[i] != ',')
                                    {
                                        result = result + a[i];
                                    }
                                    else
                                    {
                                        count = false;
                                    }
                            }
                            return result.Replace("\"", String.Empty).Replace(": ", String.Empty);
                        }
                        WebClient web = new WebClient();
                        int numb = random.Next(1, 37);
                        string link = $"https://www.xvideos.com/lang/portugues/{numb}";
                        string html = web.DownloadString(link);
                        var id = Regex.Match(html, @"xv\.thumbs\.prepareVideo\(([0-9]+)\);").Groups[1].Value;
                        string apicomment = $"https://www.xvideos.com/threads/video-comments/get-posts/top/{id}/0/0";
                        List<string> namesList = new List<string>();
                        using (WebClient wc = new WebClient())
                        {
                            var json = wc.DownloadString(apicomment);
                            JObject obj = JObject.Parse(json);
                            var comments = obj["posts"]["posts"];
                            foreach (var key in comments)
                            {
                                string[] lName = key.ToString().Split(new string[] { "profile" }, StringSplitOptions.None);
                                string[] l = key.ToString().Split(new string[] { "message" }, StringSplitOptions.None);
                                commentsList.Add(comment_(l[1]));
                                namesList.Add(comment_(lName[1]));
                            }
                            steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"{namesList[random.Next(0, namesList.Count)]} falou: {commentsList[random.Next(0, commentsList.Count)].Replace("&", String.Empty).Replace("<", String.Empty).Replace(";", String.Empty).Replace(">", String.Empty)}");
                            commentsList.Clear();
                            namesList.Clear();
                        }
                    }
                    catch { goto restart; }

                }));
                CreateCommand("@friends", new Action(delegate ()
                {
                    for (int i = 0; i < listFriendsSteamID.Count; i++)
                    {
                        Thread.Sleep(15);
                        steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, steamFriends.GetFriendPersonaName(listFriendsSteamID[i]));
                    }
                }));
                CreateCommand("@rpg", new Action(delegate ()
                {
                Check:
                    var account = dbConnection.Query("SELECT * from rpg_users WHERE id=@myid", new { myid = steamIDMemory.AccountID }).FirstOrDefault();
                    if (account != null)
                    {

                        steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"Name: {account.UName}");
                        steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"Class: {account.Class}");
                        steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"Level: {account.Level}");
                        steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"Exp: {account.Exp_}/15");

                    }
                    else
                    {
                        try
                        {
                            var CreateAccount = dbConnection.Query(
                                "INSERT INTO rpg_users(ID,Level,Exp_,Class,UName) VALUES (@id,@level,@exp,@class_,@uname)",
                                new
                                {
                                    id = steamIDMemory.AccountID,
                                    level = 0,
                                    exp = 0,
                                    class_ = "Archer",
                                    uname = steamFriends.GetFriendPersonaName(steamIDMemory)
                                });
                            Thread.Sleep(30);
                            goto Check;
                        }
                        catch { steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"Error to create your account."); }
                    }

                    steamIDMemory = null;

                }));
                CreateCommand("@temp", new Action(delegate ()
                {
                    using (WebClient wc = new WebClient())
                    {
                        try
                        {
                            var json = wc.DownloadString($@"https://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20weather.forecast%20where%20woeid%20in%20(select%20woeid%20from%20geo.places(1)%20where%20text%3D%22{Argument[0]}%22)%20and%20u%3D%22c%22&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys");
                            JObject obj = JObject.Parse(json);
                            var obj_ = obj["query"]["results"]["channel"]["item"].Select(x => new
                            {
                                temp = float.Parse(obj["query"]["results"]["channel"]["item"]["condition"]["temp"].ToString()),
                                title = obj["query"]["results"]["channel"]["item"]["title"]
                            }).ToArray();
                            steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"{obj_[0].title}");
                            steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"Temperature: {obj_[0].temp} ºC");
                        }
                        catch
                        {
                            steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"Error to get temperature from this local.");
                        }
                    }
                }));
                CreateCommand("@bitcoin", new Action(delegate ()
                {
                    using (WebClient wc = new WebClient())
                    {
                        try
                        {
                            var json = wc.DownloadString("https://blockchain.info/pt/ticker");
                            JObject obj = JObject.Parse(json);
                            var price = obj[Argument[0]]["15m"];
                            var symbol = obj[Argument[0]]["symbol"];
                            steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"Price Bitcoin from {Argument[0]}: {price} {symbol}");
                        }
                        catch { steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"Error to get price bitcoin from {Argument[0]}"); }
                    }

                }));
                CreateCommand("@math", new Action(delegate ()
                 {
                     using (WebClient wc = new WebClient())
                     {
                         try
                         {
                             var json = wc.DownloadString($"https://newton.now.sh/simplify/{Argument[0]}");
                             JObject obj = JObject.Parse(json);
                             var obj_ = obj["result"];
                             steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"{obj_}");
                         }
                         catch { steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"Error to expression {Argument[0]}"); }
                     };
                 }));
                CreateCommand("@qrcode", new Action(delegate ()
                {
                    steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={Argument[0]}");
                }));
                CreateCommand("@help", new Action(delegate ()
                 {
                     var KeysCommands = Commands.Keys.ToArray();
                     steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, "==============================");
                     for (int i = 0; i < KeysCommands.Length; i++)
                     {
                         steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, KeysCommands[i]);
                     }
                     steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, "==============================");
                 }));
                CreateCommand("@room", new Action(delegate ()
                 {
                     try
                     {
                         if (Argument[0] == "create")
                         {
                             RoomBehaviour room_ = new RoomBehaviour() { OwnerRoom = steamIDMemory, RoomID = steamIDMemory.GetHashCode(), CodeRoom = Argument[1] };
                             room_.UsersRoom.Add(steamIDMemory);
                             Rooms_dic.Add(steamIDMemory.GetHashCode().ToString(), int.Parse(Argument[1]));
                             Rooms.Add(room_);
                             steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"Sucess to create room! id: {Argument[1]}");


                         }
                         if (Argument[0] == "list")
                         {
                             for (int i = 0; i < Rooms.Count; i++)
                             {
                                 steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, Rooms[i].CodeRoom);
                             }
                         }
                         if (Argument[0] == "join")
                         {
                             if (Rooms_dic.Values.Contains(int.Parse(Argument[1])))
                             {
                                 for (int i = 0; i < Rooms.Count; i++)
                                 {
                                     if (Rooms[i].CodeRoom == Argument[1])
                                     {
                                         Rooms[i].UsersRoom.Add(steamIDMemory);
                                         steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, "Sucess to join in room!");
                                     }
                                 }
                             }
                         }

                         if (Argument[0] == "leave")
                         {
                             for (int i = 0; i < Rooms.Count; i++)
                             {
                                 if (Rooms[i].UsersRoom.Contains(steamIDMemory))
                                 {
                                     Rooms[i].UsersRoom.Remove(steamIDMemory);
                                     steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, "Sucess leave to room!");
                                 }
                             }
                         }

                         if (Argument[0] == "remove")
                         {
                             if (Rooms_dic.Values.Contains(int.Parse(Argument[1])))
                             {
                                 for (int i = 0; i < Rooms.Count; i++)
                                 {
                                     for (int x = 0; x < Rooms[i].UsersRoom.Count; x++)
                                     {
                                         Rooms[i].UsersRoom.RemoveAt(x);
                                     }

                                     if (Rooms[i].OwnerRoom == steamIDMemory)
                                     {
                                         Rooms_dic.Remove(steamIDMemory.GetHashCode().ToString());
                                         Rooms.RemoveAt(i);
                                         steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, "Sucess to remove room!");
                                     }
                                 }
                             }
                             else { steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, "Room not found!"); }
                         }
                         if (Argument[0] == "say")
                         {
                             string msg = String.Empty;
                             for (int i = 0; i < Rooms.Count; i++)
                             {
                                 if (Rooms[i].UsersRoom.Contains(steamIDMemory))
                                 {
                                     for (int a = 0; a < Rooms[i].UsersRoom.Count; a++)
                                     {
                                         for (int c = 1; c < Argument.Length; c++)
                                         {
                                             try
                                             {
                                                 if (!String.IsNullOrWhiteSpace(Argument[c]))
                                                 {
                                                     msg = msg + " " + Argument[c];
                                                 }
                                             }
                                             catch { }
                                         }
                                         steamFriends.SendChatMessage(Rooms[i].UsersRoom[a], EChatEntryType.ChatMsg, $"{steamFriends.GetFriendPersonaName(steamIDMemory)}: {msg}");
                                     }
                                 }
                             }
                         }
                     }
                     catch (Exception ex) { steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"Error to execute command in room!"); }
                 }));
            }
            catch { } // Invalids Commands Ignore!

            //

            if (currentStatus == scenes.Login)
            {
                using (StreamReader r = new StreamReader("SteamBot.json"))
                {
                    string json_data = r.ReadToEnd();
                    JObject bot_data = JObject.Parse(json_data);
                    if (bot_data["bot_data"]["autologin"].ToString() != "true")
                    {
                        Console.Write("Username: ");
                        string Username = Console.ReadLine();
                        Console.Clear();
                        Console.Write("Password: ");
                        string Password = ReadPassword();//Console.ReadLine();

                        bot = new BOT()
                        {
                            name = Username,
                            password = Password,
                            personaname = bot_data["bot_data"]["bot_name"].ToString()
                        };
                        memory_[0] = bot_data["bot_data"]["bot_name"].ToString();
                        currentStatus = scenes.TryingConnect;
                    }
                    else
                    {
                        bot = new BOT()
                        {
                            name = bot_data["bot_login"]["bot_user"].ToString(),
                            password = bot_data["bot_login"]["bot_password"].ToString(),
                            personaname = bot_data["bot_data"]["bot_name"].ToString()
                        };
                        memory_[0] = bot_data["bot_data"]["bot_name"].ToString();
                        currentStatus = scenes.TryingConnect;
                    }
                }
            }
            if (currentStatus == scenes.TryingConnect)
            {
                steamClient = new SteamClient();
                callbackManager = new CallbackManager(steamClient);
                steamUser = steamClient.GetHandler<SteamUser>();
                steamFriends = steamClient.GetHandler<SteamFriends>();

                callbackManager.Subscribe<SteamClient.ConnectedCallback>(onConnected);
                callbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
                callbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
                callbackManager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
                callbackManager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
                callbackManager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);
                callbackManager.Subscribe<SteamFriends.PersonaStateCallback>(OnPersonaState);
                callbackManager.Subscribe<SteamFriends.FriendMsgCallback>(OnFriendMsg);

                botIsRunning = true;

                Console.Clear();


                Console.WriteLine($"BOT {bot.name} Connecting to Steam...");
                steamClient.Connect();

            }
            while (botIsRunning)
            {
                callbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
                CountToAddExp += 1;
                if (CountToAddExp >= 100)
                {
                    CountToAddExp = 0;
                    for (int i = 0; i < ListPlayingGame.Count; i++)
                    {
                        try
                        {
                            if (ListPlayingGame.Count > 0)
                            {
                                var QueryToAdd = dbConnection.Query("UPDATE rpg_users set Exp_ = Exp_ + 1 WHERE id=@myid",
                                    new { myid = ListPlayingGame[i].AccountID });
                                var SelectCheck = dbConnection.Query("SELECT * FROM rpg_users WHERE id=@myid",
                                    new { myid = ListPlayingGame[i].AccountID }).FirstOrDefault();

                                if (SelectCheck != null && SelectCheck.Exp_ >= 15)
                                {
                                    var AddLevel = dbConnection.Query(
                                        "UPDATE rpg_users set Level = Level + 1 WHERE id=@myid",
                                        new { myid = ListPlayingGame[i].AccountID });
                                    var RemoveExp = dbConnection.Query("UPDATE rpg_users set Exp_ = 0 WHERE id=@myid",
                                        new { myid = ListPlayingGame[i].AccountID });
                                    steamFriends.SendChatMessage(ListPlayingGame[i], EChatEntryType.ChatMsg, "[RPG] You have reached a new Level");

                                }

                            }
                        }
                        catch { }
                    }
                }
            }
            Console.ReadLine();
        }

        private static void OnFriendMsg(SteamFriends.FriendMsgCallback obj)
        {

            string[] ParamsSepearator = obj.Message.Split(' ');
            if (Commands.ContainsKey(ParamsSepearator[0]))
            {
                for (int i = 1; i < ParamsSepearator.Length; i++)
                {
                    for (int a = 0; a < Argument.Length; a++)
                    {
                        try
                        {
                            Argument[a] = ParamsSepearator[i];
                        }
                        catch { }
                        i += 1;
                    }
                }
                steamIDMemory = obj.Sender;
                ExecuteCommand(ParamsSepearator[0]);
            }
        }

        private static void OnPersonaState(SteamFriends.PersonaStateCallback obj)
        {
            if (obj.State != EPersonaState.Offline)
            {
                ListPlayingGame.Add(obj.FriendID);
            }
            else
            {
                if (ListPlayingGame.Contains(obj.FriendID))
                    ListPlayingGame.Remove(obj.FriendID);
            }


            Console.WriteLine("State change: {0}", obj.Name);
        }

        private static void OnFriendsList(SteamFriends.FriendsListCallback obj)
        {
            foreach (var friend in obj.FriendList)
            {
                if (friend.Relationship == EFriendRelationship.Friend)
                {
                    if (!listFriendsSteamID.Contains(friend.SteamID))
                        listFriendsSteamID.Add(friend.SteamID);

                }
                if (friend.Relationship == EFriendRelationship.RequestRecipient)
                {

                    steamFriends.AddFriend(friend.SteamID);
                    steamFriends.SendChatMessage(friend.SteamID, EChatEntryType.ChatMsg, "Olá você pode ver meus comandos digitando @help");

                }
            }
        }

        private static void OnAccountInfo(SteamUser.AccountInfoCallback obj)
        {
            steamFriends.SetPersonaState(EPersonaState.Online);
        }

        private static void OnLoggedOff(SteamUser.LoggedOffCallback obj)
        {
            Console.WriteLine($"BOT {bot.name} Logged off of Steam: {0}", obj.Result);
        }

        private static void OnLoggedOn(SteamUser.LoggedOnCallback obj)
        {
            if (obj.Result != EResult.OK)
            {
                ConsoleStatus(statusConsole.ERROR, $"Unable to logon to Steam: {obj.Result}/{obj.ExtendedResult}");
                botIsRunning = false;
                return;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"BOT {bot.name} Successfully logged on!");
            steamFriends.SetPersonaName(memory_[0]);
            Console.ResetColor();
            currentStatus = scenes.Running;
        }

        private static void OnDisconnected(SteamClient.DisconnectedCallback obj)
        {
            Console.WriteLine($"BOT {bot.name} Disconnected from Steam");

            botIsRunning = false;
        }

        private static void onConnected(SteamClient.ConnectedCallback obj)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Connected to Steam! Logging in '{0}'...", bot.name);
            Console.ResetColor();
            steamUser.LogOn(new SteamUser.LogOnDetails { Username = bot.name, Password = bot.password });
        }
    }
}
