using System;
using System.IO;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

/*
 * Simple Donation Alerts Integration Plugin
 * Plugin for Tshock (Terraria Server Core)
 * Made by dxAugust (aka fet1sov)
 * 
 * Hope it will make your streams funnier :)
 */

namespace StreamIntegration
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        public override string Author => "dxAugust";
        public override string Description => "Plugin which integrates DonationAlerts events into Terraria";
        public override string Name => "DonationAlerts Integration";

        private static Random random;

        /* Getting directory of config.ini file */
        public static string DirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "ServerPlugins");
        public static string FolderPath = Path.Combine(DirectoryPath, "DAIntegration");
        public static string FilePath = Path.Combine(FolderPath, "config.ini");
        /* =================================== */

        public Plugin(Main game) : base(game)
        {

        }

        public override void Initialize()
        {
            /* Gets the ini config with auth code */
            if (File.Exists(@FilePath))
            {
                IniParser parser = new IniParser(@FilePath);
                Config.authCode = parser.GetSetting("authconfig", "authcode");
            } else {

            }
            /* ================================== */
            
            if (Config.authCode.Length != 0)
            {
                DAPI donateAPI = new DAPI(); // Initilization of the DonationAlerts API
                random = new Random();
            } else {
                Debugger.errorOutput("You have to install authcode in your config file");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            base.Dispose(disposing);
        }

        /* Donation Event Handler is here */
        public static void handleDonate(string name, string currencyCode, int amount)
        {
            TShock.Utils.Broadcast(string.Format("Донат: от {0} - {1} {2}", name, amount, currencyCode), 255, 255, 0);

            /*
            1 - Nothing
            2 - Selects one player on the server and then spawn Eye of Cthulu on his position
            3 - Killing all players on server
            4 - Spawns Skeleton Prime to random player position
            5 - Sets time to night
            6 - Kicks random player from server
            7 - Spams to all player "MUHAHAHAHAHAHAHHAHAHAHA"
            8 - Spawns 20 zombie near random player
            9 - Sets time to day
            10 - Spawn 50 bats near random player
            */

            int badThings = random.Next(1, 10);

            int playerID = 0;
            int spamCount = 30;
            int zombieCount = 30;
            int batsCount = 50;

            switch (badThings)
            {
                case 1:
                    break;

                case 2:
                    playerID = random.Next(1, TShock.Players.Length);

                    NPC eye = TShock.Utils.GetNPCById(4);
                    TSPlayer.Server.SetTime(false, 0.0);
                    TSPlayer.Server.SpawnNPC(eye.type, name, 1, TShock.Players[playerID].TileX, TShock.Players[playerID].TileY);
                    break;

                case 3:
                    for (int i = 0; i < TShock.Players.Length; i++)
                    {
                        TShock.Players[i].KillPlayer();
                    }
                    break;

                case 4:
                    playerID = random.Next(1, TShock.Players.Length);

                    NPC prime = TShock.Utils.GetNPCById(127);
                    TSPlayer.Server.SetTime(false, 0.0);
                    TSPlayer.Server.SpawnNPC(prime.type, name, 1, TShock.Players[playerID].TileX, TShock.Players[playerID].TileY);
                    break;

                case 5:
                    TSPlayer.Server.SetTime(false, 0.0);
                    break;

                case 6:
                    playerID = random.Next(1, TShock.Players.Length);
                    TShock.Players[playerID].Kick("SORRY FOR DONATION :)", false, true);
                    break;

                case 7:
                    for (int i = 0; i < spamCount; i++)
                    {
                        TShock.Utils.Broadcast("MUHHAHAHAHAHA", 255, 0, 0);
                    }
                    break;

                case 8:
                    playerID = random.Next(1, TShock.Players.Length);

                    for (int i = 0; i < zombieCount; i++)
                    {
                        NPC zombies = TShock.Utils.GetNPCById(3);
                        TSPlayer.Server.SetTime(false, 0.0);
                        TSPlayer.Server.SpawnNPC(zombies.type, name, 1, TShock.Players[playerID].TileX, TShock.Players[playerID].TileY);
                    }
                    break;

                case 9:
                    TSPlayer.Server.SetTime(false, 12.0);
                    break;

                case 10:
                    playerID = random.Next(1, TShock.Players.Length);

                    for (int i = 0; i < batsCount; i++)
                    {
                        NPC zombies = TShock.Utils.GetNPCById(51);
                        TSPlayer.Server.SetTime(false, 0.0);
                        TSPlayer.Server.SpawnNPC(zombies.type, name, 1, TShock.Players[playerID].TileX, TShock.Players[playerID].TileY);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}