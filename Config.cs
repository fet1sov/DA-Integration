namespace DonationIntegration
{
    class Config
    {
        /* 
           Here I`m putting some configurations for our plugin 
           I will make an ini config in future
           DO NOT TOUCH ACCESS TOKEN AND WHAT'S BELOW IT
        */



        // You have to register your app right there -> https://www.donationalerts.com/application/clients
        public static string client_id = "";                          // You can change it to your own
        public static string client_secret = "";                      // You can change it to your own
        public static string redirect_URL = "";                       // You can change it to your own

        public static bool bDebug = false;

        public static string authCode = "";
        public static string access_token = "";
        public static string refresh_token = "";

        public static string channel_id = "";
        public static string socketClient_id = "";
        public static string channelConnection_token = "";

        public static string plugin_tag = "[D-API] ";
    }
}
