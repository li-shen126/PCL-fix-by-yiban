# PCL fix by yiban修复版

本包基于官方开源仓库的主分支源码提交修改而来。

## 修改内容

### 1. 解除下载限制

- `Modules/ModSecret.vb`：`RestrictMcDownload` 改为 `False`。
- `Pages/PageDownload/PageDownloadLeft.xaml.vb`：不再隐藏「原版游戏」入口。
- `Pages/PageSelectLeft.xaml.vb`：恢复「导入整合包」入口。
- `Modules/Minecraft/ModDownload.vb`：移除 NeoForge `47.1.82` 的过滤。

### 2. 主题全解锁与配色

- `Pages/PageSetup/PageSetupUI.xaml`：隐藏主题全部可选，移除赞助遮罩。
- `Modules/ModSecret.vb`：为每个主题补齐色调、饱和度、亮度；滑稽彩随机换色，欧皇彩随机配色，自定义主题读取 HSL 滑块；补齐玄素黑深色标题栏等特殊处理。
- `ThemeUnlock` 不再触发解锁弹窗。

### 3. 还原百宝箱

- `Pages/PageOther/PageOtherTest.xaml`：恢复百宝箱页面。
- `Pages/PageOther/PageOtherTest.xaml.vb`：实现今日人品、内存优化、清理游戏垃圾、回声洞、千万别点、自定义下载。

### 4. 移除开源版提示

- 分支标识改为 `Main`，删除启动时的开源版功能说明弹窗。

### 5. 启动与缓存修复

- `Modules/ThirdParty/DragHelper.vb`：无法修改消息过滤器时不再中断启动。
- `Application.xaml.vb`：缓存目录依次尝试设置值`%TEMP%\PCL`、`%LOCALAPPDATA%\PCL`、`%APPDATA%\PCL\Cache`

### 6. 恢复正版登录与 CurseForge

- `Modules/ModSecret.vb`：填入官方 `OAuthClientId` 与 CurseForge API Key。

### 11. 数据目录回退

- `Application.xaml.vb`：启动时检测 exe 旁的 `PCL` 数据目录是否可写；不可写时自动改用 `%LOCALAPPDATA%\PCL`，日志、`Setup.ini`、图片、音乐等数据不再尝试写入只读位置

## 编译方式

1. 安装 Visual Studio 2022（“.NET 桌面开发”工作负载与 .NET Framework 4.8 开发人员包）。
2. 打开 `Plain Craft Launcher 2.sln`，选择 `Release` 生成。
3. 产物位于 `Plain Craft Launcher 2/bin/`，本次发布只取 `Plain Craft Launcher 2.exe`。

运行环境：Windows 10/11（64 位），.NET Framework 4.8。
