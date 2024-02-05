using PalServerTools.Utils;

namespace PalServerTools.Data
{
    public class PalRconService
    {
        private readonly PalConfigService _configService;

        public RconClient client;
    
        public PalRconService(PalConfigService configService)
        {
            this._configService = configService;
            this.client = new RconClient();
        }

        private async Task Connect()
        {
            await client.ConnectAsync("127.0.0.1", this._configService.PalConfig.RCONPort, this._configService.PalConfig.AdminPassword);
        }

        public async Task<string> ExecuteCommand(string command)
        {
            string res = "";
            try
            {
                await this.Connect();
                res = await client.SendCommandAsync(command);
            }
            catch (Exception ex)
            {
                Console.WriteLine("RconClient: ", ex);
            }
            finally
            {
                client.Disconnect();
            }
            return res;
        }

        /// <summary>
        /// 向服务器上的所有玩家发送一条广播消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task<bool> Broadcast(string msg)
        {
            var res = await ExecuteCommand("Broadcast " + msg);
            return res != null;
        }

        /// <summary>
        /// 在指定的秒数后关闭服务器，并向所有玩家显示一条消息
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task<bool> Shutdown(int seconds, string msg = "")
        {
            var res = await ExecuteCommand($"Shutdown {seconds} {msg}");
            return res != null;
        }

        /// <summary>
        /// 立即强制停止服务器
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DoExit()
        {
            var res = await ExecuteCommand($"DoExit");
            return res != null;
        }

        /// <summary>
        /// 将指定的玩家踢出服务器
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public async Task<bool> KickPlayer(string steamId)
        {
            var res = await ExecuteCommand($"KickPlayer {steamId}");
            return res != null;
        }

        /// <summary>
        /// 永久禁止指定的玩家进入服务器
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public async Task<bool> BanPlayer(string steamId)
        {
            var res = await ExecuteCommand($"BanPlayer {steamId}");
            return res != null;
        }

        /// <summary>
        /// 传送到指定玩家的位置
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public async Task<bool> TeleportToPlayer(string steamId)
        {
            var res = await ExecuteCommand($"TeleportToPlayer {steamId}");
            return res != null;
        }

        /// <summary>
        /// 将指定玩家传送到您的位置
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public async Task<bool> TeleportToMe(string steamId)
        {
            var res = await ExecuteCommand($"TeleportToMe {steamId}");
            return res != null;
        }

        /// <summary>
        /// 显示当前连接到服务器的所有玩家的信息
        /// </summary>
        /// <returns></returns>
        public async Task<string> ShowPlayers()
        {
            return await ExecuteCommand("ShowPlayers");
        }

        /// <summary>
        /// 显示服务器的基本信息，如版本号、当前玩家数量
        /// </summary>
        /// <returns></returns>
        public async Task<string> Info()
        {
            return await ExecuteCommand("Info");
        }

        /// <summary>
        /// 手动保存世界数据
        /// </summary>
        /// <returns></returns>
        public async Task<string> Save()
        {
            return await ExecuteCommand("Save");
        }
    }
}
