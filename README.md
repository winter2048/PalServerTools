# PalServerTools 2.0
幻兽帕鲁服务器工具 (开发中，暂时只支持Windows)

# 功能
- [x] 监控服务器状态
- [x] 查看管理在线玩家
- [x] 存档定时自动备份
- [x] 存档备份恢复功能
- [x] 游戏配置参数修改
- [x] 游戏控制台
- [x] 世界喊话
- [x] UI暗色模式
- [x] 自动更新游戏版本
- [x] 内存优化
- [x] 一键安装脚本
- [x] 检查&更新工具版本
- [x] 读取&保存WorldOption.sav
- [x] 支持多开功能
- [x] 日志查询
- [ ] 代理配置(frpc)
- [ ] 服务器崩溃时提醒在线玩家即将重启
- [ ] Linux支持

# 安装

## 一键安装&升级

打开 Windows PowerShell 窗口并定位至安装目录，执行安装&升级命令。

- 安装
``` powershell
iex "& { $(irm https://raw.githubusercontent.com/winter2048/PalServerTools/master/install.ps1) }"
```

- 安装指定版本
``` powershell
iex "& { $(irm https://raw.githubusercontent.com/winter2048/PalServerTools/master/install.ps1) } -Version v2.0.0"
```

- 升级
> 双击执行安装目录下的`update.cmd`即可完成更新。

## 手动安装

- 安装.NET 6.0 Runtime [下载](https://dotnet.microsoft.com/zh-cn/download/dotnet/thank-you/runtime-6.0.26-windows-x64-installer)
- 安装ASP.NET Core 6.0 Runtime [下载](https://dotnet.microsoft.com/zh-cn/download/dotnet/thank-you/runtime-aspnetcore-6.0.26-windows-x64-installer)
- 下载[PalServerTools.zip](https://github.com/winter2048/PalServerTools/releases)并解压
- 修改配置文件`appsettings.json`中的幻兽帕鲁服务器根路径文件夹`PalServerPath`
- 双击运行`PalServerTools.exe`
- 使用浏览器打开[127.0.0.1:5000](http:127.0.0.1:5000)，默认密码`123456`

> 密码可在appsettings.json文件中配置`ToolsConfig.ToolsPassword`

# 多开功能

> v2.0.15 版本后正式支持多开功能！

- 创建配置文件`appsetting.{EnvName}.json`
- 将`appsetting.json`文件的内容复制到新创建的配置文件中，注意修改端口号`ASPNETCORE_URLS`
- 运行`PalServerTools.exe`时添加`-Env {EnvName}`参数，例如`PalServerTools.exe -Env {EnvName}`
- 每个PalServerTools可以管理不同的PalServer服务端。

# 更新内容查看

[查看最新版本更新内容](./ReleaseNotes.md)

# 预览
![image](https://github.com/winter2048/PalServerTools/assets/31879147/1c7fc6ba-2acd-43eb-84cf-d88bd6b67968)
![image](https://github.com/winter2048/PalServerTools/assets/31879147/8794e5a6-252b-425c-afe5-fead6ecdb1f2)
![image](https://github.com/winter2048/PalServerTools/assets/31879147/720b88d4-fb61-437b-ac8b-04af6b546799)
![image](https://github.com/winter2048/PalServerTools/assets/31879147/62baf5e6-b99a-413c-afd9-9182b8124427)
![image](https://github.com/winter2048/PalServerTools/assets/31879147/2a764d53-0107-4791-ab5b-53f646de3727)
![image](https://github.com/winter2048/PalServerTools/assets/31879147/eb90bf24-6b45-4580-9c04-3ded442f3abc)

# 感谢

- [palworld-save-tools](https://github.com/cheahjs/palworld-save-tools) 提供了存档解析工具实现

# 许可证

根据 [Apache2.0 许可证](LICENSE) 授权，任何转载请在 README 和文件部分标明！任何商用行为请务必告知！
