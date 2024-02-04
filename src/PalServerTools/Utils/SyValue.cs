using AntDesign;
using PalServerTools.Components;
using System.IO;

namespace PalServerTools.Utils
{
    public class SyValue
    {
        public static List<MenuItems> menuItems = new List<MenuItems>()
        {
            new MenuItems()
            {
                Title = "监控",
                Path = "/",
                Icon = "bar-chart"
            },
            new MenuItems()
            {
                Title = "玩家",
                Path = "/user",
                Icon = "user"
            },
            new MenuItems()
            {
                Title = "存档",
                Path = "/backup",
                Icon = "save"
            },
            new MenuItems()
            {
                Title = "代理",
                Path = "/proxy",
                Icon = "apartment"
            },
            new MenuItems()
            {
                Title = "配置",
                Path = "/config",
                Icon = "setting"
            } ,
            new MenuItems()
            {
                Title = "控制台",
                Path = "/console",
                Icon = "mac-command"
            }
        };
    }
}
