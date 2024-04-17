using AntDesign;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PalServerTools.Models;
using PalServerTools.Utils;
using RestSharp;
using System.Numerics;
using System.Text;

namespace PalServerTools.Data
{
    public class PalApiService
    {
        private readonly ILogger _logger;
        private readonly PalConfigService _configService;

        public RestClient client;

        public PalApiService(ILogger logger, PalConfigService palConfigService)
        {
            _logger = logger;
            _configService = palConfigService;
            var options = new RestClientOptions($"http://127.0.0.1:{_configService.PalConfig.RESTAPIPort}/v1/api")
            {
                MaxTimeout = -1
            };
            this.client = new RestClient(options);
            this.client.AddDefaultHeader("Accept", "application/json");
            this.client.AddDefaultHeader("Authorization", $"Basic {GetEncodedAuth()}");
        }

        public string GetEncodedAuth()
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes($"admin:{_configService.PalConfig.AdminPassword}"));
        }

        public async Task<bool> IsCanConnect()
        {
            return await this.Info() != null;
        }

        /// <summary>
        /// 向服务器上的所有玩家发送一条广播消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task<bool> Broadcast(string msg)
        {
            return await ExecuteAsync(new RestRequest("announce", Method.Post).AddBody(new { message = msg }), (res) => true, (ex) => false);
        }

        /// <summary>
        /// 在指定的秒数后关闭服务器，并向所有玩家显示一条消息
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task<bool> Shutdown(int seconds, string msg = "")
        {
            return await ExecuteAsync(new RestRequest("shutdown", Method.Post).AddBody(new { waittime = seconds, message = msg }), (res) => true, (ex) => false);
        }

        /// <summary>
        /// 立即强制停止服务器
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DoExit()
        {
            return await ExecuteAsync(new RestRequest("stop", Method.Post), (res) => true, (ex) => false);
        }

        /// <summary>
        /// 将指定的玩家踢出服务器
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public async Task<bool> KickPlayer(string steamId, string msg = "")
        {
            return await ExecuteAsync(new RestRequest("kick", Method.Post).AddBody(new { userid = steamId, message = msg }), (res) => true, (ex) => false);
        }

        /// <summary>
        /// 永久禁止指定的玩家进入服务器
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public async Task<bool> BanPlayer(string steamId, string msg = "")
        {
            return await ExecuteAsync(new RestRequest("ban", Method.Post).AddBody(new { userid = steamId, message = msg }), (res) => true, (ex) => false);
        }

        /// <summary>
        /// 解禁指定的玩家进入服务器
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public async Task<bool> UnBanPlayer(string steamId)
        {
            return await ExecuteAsync(new RestRequest("unban", Method.Post).AddBody(new { userid = steamId }), (res) => true, (ex) => false);
        }

        /// <summary>
        /// 显示当前连接到服务器的所有玩家的信息
        /// </summary>
        /// <returns></returns>
        public async Task<PlayersModel?> ShowPlayers()
        {
            return await ExecuteAsync(new RestRequest("players", Method.Get), (res) => JsonConvert.DeserializeObject<PlayersModel>(res.Content), (ex) => null);
        }

        /// <summary>
        /// 显示服务器的基本信息，如版本号、当前玩家数量
        /// </summary>
        /// <returns></returns>
        public async Task<ServerInfoModel?> Info()
        {
            return await ExecuteAsync(new RestRequest("info", Method.Get), (res) => JsonConvert.DeserializeObject<ServerInfoModel>(res.Content), (ex) => null);
        }

        /// <summary>
        /// 手动保存世界数据
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Save()
        {
            return await ExecuteAsync(new RestRequest("save", Method.Post), (res) => true, (ex) => false);
        }


        /// <summary>
        /// 显示服务器的基本信息，如版本号、当前玩家数量
        /// </summary>
        /// <returns></returns>
        public async Task<T?> ExecuteAsync<T>(RestRequest restRequest, Func<RestResponse, T> successFunc, Func<Exception, T> errorFunc)
        {
            try
            {
                var res = await this.client.ExecuteAsync(restRequest);
                if (res.IsSuccessful)
                {
                    return successFunc(res);
                }
                else
                {
                    _logger.LogError(res.ErrorException, "PalworldRestApi连接失败");
                    return errorFunc(res.ErrorException);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PalworldRestApi连接失败");
                return errorFunc(ex);
            }
        }
    }
}
