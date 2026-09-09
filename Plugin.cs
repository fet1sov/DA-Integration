using System;
using System.Threading.Tasks;
using DA_Integration;
using DA_Integration.API;
using Terraria;
using TerrariaApi.Server;

namespace DonationIntegration
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        public override string Author => "fet1sov";
        public override string Description => "Plugin which integrates DonationAlerts events into Terraria";
        public override string Name => "DonationAlerts Integration";
        public override Version Version => new Version(1, 0, 0);

        private DAPI _donateApi;

        public Plugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ConfigManager.LoadOrCreate();

            _donateApi = new DAPI();

            Task.Run(async () =>
            {
                try
                {
                    await _donateApi.InitializeAsync();
                }
                catch (Exception ex)
                {
                    TShockAPI.TShock.Log.Error($"[DonationAlerts] Ошибка при старте DAPI: {ex.Message}");
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            base.Dispose(disposing);
        }
    }
}