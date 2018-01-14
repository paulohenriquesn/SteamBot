using System;
using System.Text;
using System.Collections.Generic;
using SteamKit2;
using System.Threading.Tasks;

namespace SteamBot_
{
    public class Command
    {
        public string command_;
        public Action action;
    }
    class Program
    {

        private static string[] Argument = new string[4];
        private static SteamID steamIDMemory;

        private static Dictionary<string, Command> Commands = new Dictionary<string, Command>();

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

        static void ConsoleStatus(statusConsole status,string error_)
        {
            switch (status) { case statusConsole.LOGIN_DENIED:
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
        static void CreateCommand(string command,Action action_) {
            Commands.Add(command,new Command() { command_=command,action=action_});        
        }
        static void ExecuteCommand(string command) {
            Commands[command].action();
            for (int i = 0; i < Argument.Length; i++) { Argument[i] = String.Empty; }
        }

      

        static void Main(string[] args)
        {

            //Commands

            CreateCommand("sum", new Action(delegate () {
                if (!String.IsNullOrWhiteSpace(Argument[0]) && !String.IsNullOrWhiteSpace(Argument[1]))
                {
                    int a = int.Parse(Argument[0]);
                    int b = int.Parse(Argument[1]);
                    steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"{a}+{b}={a + b}");
                }
                else
                {
                    steamFriends.SendChatMessage(steamIDMemory, EChatEntryType.ChatMsg, $"Error no Arguments");
                }
            }));

            //

            if (currentStatus == scenes.Login)
            {          
                Console.Write("Username: ");
                string Username = Console.ReadLine();
                Console.Clear();
                Console.Write("Password: ");
                string Password = Console.ReadLine();

                bot = new BOT() {
                    name = Username,
                    password = Password
                };
                currentStatus = scenes.TryingConnect;

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
            }
            Console.ReadLine();
        }

        private static void OnFriendMsg(SteamFriends.FriendMsgCallback obj)
        {
            string[] ParamsSepearator = obj.Message.Split(' ');          
            if (Commands.ContainsKey(ParamsSepearator[0]))
            {
                for(int i = 1; i < ParamsSepearator.Length; i++)
                {
                    for (int a = 0; a < Argument.Length; a++) {
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
            Console.WriteLine("State change: {0}", obj.Name);
        }   

        private static void OnFriendsList(SteamFriends.FriendsListCallback obj)
        {
            foreach (var friend in obj.FriendList)
            {
                if (friend.Relationship == EFriendRelationship.RequestRecipient)
                {

                    steamFriends.AddFriend(friend.SteamID);
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
            if ( obj.Result != EResult.OK )
            {          
                ConsoleStatus(statusConsole.ERROR, $"Unable to logon to Steam: {obj.Result}/{obj.ExtendedResult}");
                botIsRunning = false;
                return;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"BOT {bot.name} Successfully logged on!");
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
