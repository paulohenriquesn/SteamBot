using System;
using System.Text;
using System.Collections.Generic;
using SteamKit2;


namespace SteamBot_
{
    class Program
    {
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

        static void Main(string[] args)
        {
           if(currentStatus == scenes.Login)
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
