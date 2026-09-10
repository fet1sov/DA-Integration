using Terraria;
using Terraria.ID;
using TShockAPI;
using DA_Integration.Models;

namespace DA_Integration.Handlers
{
    public static class DonateHandler
    {
        private static readonly Random RandomInstance = new Random();

        private static readonly int[] DefaultBosses = { NPCID.EyeofCthulhu, NPCID.EaterofWorldsHead, NPCID.SkeletronHead, NPCID.SkeletronPrime };
        private static readonly int[] DefaultMobs = { NPCID.Zombie, NPCID.DemonEye, NPCID.CaveBat, NPCID.Skeleton };

        public static void HandleDonate(string name, string currencyCode, int amount, List<DonateEventConfig> events)
        {
            TShock.Utils.Broadcast($"Донат от {name}: {amount} {currencyCode}", 255, 255, 0);

            if (events == null || events.Count == 0) return;

            var eligibleEvents = events.Where(e => !e.MinAmount.HasValue || amount >= e.MinAmount.Value).ToList();
            if (eligibleEvents.Count == 0) return;

            DonateEventConfig selectedEvent = SelectRandomEventByWeight(eligibleEvents);
            if (selectedEvent == null) return;

            var activePlayers = TShock.Players.Where(p => p != null && p.Active).ToList();
            if (activePlayers.Count == 0) return;

            TSPlayer targetPlayer = activePlayers[RandomInstance.Next(activePlayers.Count)];

            ExecuteEvent(selectedEvent, targetPlayer, name);
        }

        private static DonateEventConfig SelectRandomEventByWeight(List<DonateEventConfig> events)
        {
            double totalWeight = events.Sum(e => e.Probability <= 0 ? 0 : e.Probability);
            if (totalWeight <= 0) return null;

            double randomValue = RandomInstance.NextDouble() * totalWeight;
            double currentSum = 0;

            foreach (var ev in events)
            {
                if (ev.Probability <= 0) continue;

                currentSum += ev.Probability;
                if (randomValue <= currentSum)
                {
                    return ev;
                }
            }

            return events.FirstOrDefault();
        }

        private static void ExecuteEvent(DonateEventConfig config, TSPlayer targetPlayer, string donorName)
        {
            switch (config.Type)
            {
                case DonateEventType.SPAWN_BOSS:
                    {
                        int bossId = config.NpcId ?? DefaultBosses[RandomInstance.Next(DefaultBosses.Length)];
                        TSPlayer.Server.SetTime(false, 0.0);
                        TSPlayer.Server.SpawnNPC(bossId, donorName, 1, targetPlayer.TileX, targetPlayer.TileY);
                        break;
                    }

                case DonateEventType.SPAWN_MOBS:
                    {
                        int mobId = config.NpcId ?? DefaultMobs[RandomInstance.Next(DefaultMobs.Length)];
                        int count = config.Count ?? RandomInstance.Next(5, 21);

                        for (int i = 0; i < count; i++)
                        {
                            TSPlayer.Server.SpawnNPC(mobId, donorName, 1, targetPlayer.TileX, targetPlayer.TileY);
                        }
                        break;
                    }

                case DonateEventType.KILL_ALL_EVENT:
                    foreach (TSPlayer player in TShock.Players.Where(p => p != null && p.Active))
                    {
                        player.KillPlayer();
                    }
                    break;

                case DonateEventType.KILL_RANDOM_PLAYER:
                    targetPlayer.KillPlayer();
                    break;

                case DonateEventType.SPAWN_BOMB_UNDER_PLAYER:
                    {
                        int bombId = config.ProjectileId ?? ProjectileID.Bomb;
                        int damage = config.Damage ?? RandomInstance.Next(30, 81);

                        Projectile.NewProjectile(null, targetPlayer.X, targetPlayer.Y, 0, 0, bombId, damage, 0, Main.myPlayer);
                        break;
                    }

                case DonateEventType.SPAWN_DYNAMITE_UNDER_PLAYER:
                    {
                        int dynamiteId = config.ProjectileId ?? ProjectileID.Dynamite;
                        int damage = config.Damage ?? RandomInstance.Next(100, 251);

                        Projectile.NewProjectile(null, targetPlayer.X, targetPlayer.Y, 0, 0, dynamiteId, damage, 0, Main.myPlayer);
                        break;
                    }

                case DonateEventType.TELEPORT_PLAYER_IN_RANDOM:
                    {
                        int radius = config.TeleportRadius ?? RandomInstance.Next(15, 61);

                        int offsetX = RandomInstance.Next(-radius, radius + 1);
                        int offsetY = RandomInstance.Next(-radius, radius + 1);

                        targetPlayer.Teleport((targetPlayer.TileX + offsetX) * 16, (targetPlayer.TileY + offsetY) * 16);
                        break;
                    }

                case DonateEventType.DAMAGE_BY_STAND_BLOCK:
                    {
                        int damage = config.Damage ?? RandomInstance.Next(10, 51);
                        targetPlayer.DamagePlayer(damage);
                        break;
                    }
            }
        }
    }
}