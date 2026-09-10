using System.Text.Json.Serialization;

namespace DA_Integration.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DonateEventType
    {
        SPAWN_BOSS,
        SPAWN_MOBS,
        KILL_ALL_EVENT,
        KILL_RANDOM_PLAYER,
        SPAWN_BOMB_UNDER_PLAYER,
        SPAWN_DYNAMITE_UNDER_PLAYER,
        TELEPORT_PLAYER_IN_RANDOM,
        DAMAGE_BY_STAND_BLOCK
    }
}