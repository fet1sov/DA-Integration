using Terraria;
using TShockAPI;

namespace DA_Integration.Handlers
{
    public static class DonateHandler
    {
        private static readonly Random RandomInstance = new Random();

        public static void HandleDonate(string name, string currencyCode, int amount)
        {
            TShock.Utils.Broadcast($"Донат: от {name} - {amount} {currencyCode}", 255, 255, 0);

            int badThings = RandomInstance.Next(1, 11);
            int playerID = RandomInstance.Next(0, TShock.Players.Length);

            TSPlayer targetPlayer = TShock.Players[playerID];
            if (targetPlayer == null || !targetPlayer.Active)
            {
                return;
            }

            const int spamCount = 30;
            const int zombieCount = 30;
            const int batsCount = 50;

            switch (badThings)
            {
                case 1:
                    break;

                case 2:
                    NPC eye = TShock.Utils.GetNPCById(4);
                    TSPlayer.Server.SetTime(false, 0.0);
                    TSPlayer.Server.SpawnNPC(eye.type, name, 1, targetPlayer.TileX, targetPlayer.TileY);
                    break;

                case 3:
                    foreach (TSPlayer player in TShock.Players)
                    {
                        if (player != null && player.Active)
                        {
                            player.KillPlayer();
                        }
                    }
                    break;

                case 4:
                    NPC prime = TShock.Utils.GetNPCById(127);
                    TSPlayer.Server.SetTime(false, 0.0);
                    TSPlayer.Server.SpawnNPC(prime.type, name, 1, targetPlayer.TileX, targetPlayer.TileY);
                    break;

                case 5:
                    TSPlayer.Server.SetTime(false, 0.0);
                    break;

                case 6:
                    targetPlayer.Kick("SORRY FOR DONATION :)", false, true);
                    break;

                case 7:
                    for (int i = 0; i < spamCount; i++)
                    {
                        TShock.Utils.Broadcast("MUHHAHAHAHAHA", 255, 0, 0);
                    }
                    break;

                case 8:
                    NPC zombie = TShock.Utils.GetNPCById(3);
                    TSPlayer.Server.SetTime(false, 0.0);
                    for (int i = 0; i < zombieCount; i++)
                    {
                        TSPlayer.Server.SpawnNPC(zombie.type, name, 1, targetPlayer.TileX, targetPlayer.TileY);
                    }
                    break;

                case 9:
                    TSPlayer.Server.SetTime(false, 12.0);
                    break;

                case 10:
                    NPC bat = TShock.Utils.GetNPCById(51);
                    TSPlayer.Server.SetTime(false, 0.0);
                    for (int i = 0; i < batsCount; i++)
                    {
                        TSPlayer.Server.SpawnNPC(bat.type, name, 1, targetPlayer.TileX, targetPlayer.TileY);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}