Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Security.Principal
Imports System.Text

Public Class PageOtherTest

    Private IsLoad As Boolean = False
    Private IsCaveEnabled As Boolean = True
    Private Shared IsMemoryOptimizeRunning As Boolean = False

#Region "页面加载"

    Private Sub PageOtherTest_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If IsLoad Then Return
        IsLoad = True
        TextDownloadFolder.Text = Settings.Get(Of String)("CacheDownloadFolder")
        TextDownloadFolder.Validate()
        If Not TextDownloadFolder.IsValidated OrElse TextDownloadFolder.Text = "" Then
            TextDownloadFolder.Text = PathData & "MyDownload\"
        End If
        TextDownloadFolder.Validate()
        TextDownloadName.Validate()
        StartButtonRefresh()
    End Sub

    Private Sub SaveCacheDownloadFolder()
        Settings.Set("CacheDownloadFolder", TextDownloadFolder.Text)
        For Each Rule In TextDownloadName.ValidateRules
            Dim FileRule = TryCast(Rule, ValidateFileName)
            If FileRule IsNot Nothing Then FileRule.ParentFolder = TextDownloadFolder.Text
        Next
        TextDownloadName.Validate()
    End Sub

    Private Sub StartButtonRefresh()
        BtnDownloadStart.IsEnabled = Not String.IsNullOrWhiteSpace(TextDownloadUrl.Text) AndAlso
            Not String.IsNullOrWhiteSpace(TextDownloadFolder.Text) AndAlso
            Not String.IsNullOrWhiteSpace(TextDownloadName.Text)
        BtnDownloadOpen.IsEnabled = Not String.IsNullOrWhiteSpace(TextDownloadFolder.Text)
    End Sub

#End Region

#Region "今日人品"

    Public Sub BtnJrrp_Click(sender As Object, e As MouseButtonEventArgs)
        Jrrp()
    End Sub

    Public Shared Sub Jrrp()
        Dim Seed As Integer = CInt(Math.Round(Math.Abs((StableHash($"asdfgbn{DateTime.Now.DayOfYear}12#3$45{DateTime.Now.Year}IUY") / 3.0 +
            StableHash($"QWERTY{GetIdentify()}0*8&6{DateTime.Now.Day}kjhg") / 3.0) / 527.0) Mod 1001.0))
        Dim Value As Integer = If(Seed < 970, CInt(Math.Round(Seed / 969.0 * 99.0)), 100)
        Dim Message As String
        Select Case Value
            Case 100
                Message = "！100！100！！！！！" & If(ThemeUnlock(13, False), vbCrLf & "隐藏主题 欧皇彩 已解锁！", "")
            Case >= 98
                Message = "！差点就到 100 了呢……"
            Case >= 90
                Message = "！好评如潮！"
            Case >= 65
                Message = "！今天运气不错呢！"
            Case > 50
                Message = "！还行啦，还行啦。"
            Case 50
                Message = "！五五开……"
            Case >= 40
                Message = "！勉强还行吧……？"
            Case >= 20
                Message = "！呜……"
            Case >= 11
                Message = "？！不会吧……"
            Case >= 1
                Message = "……（是百分制哦）"
            Case Else
                Message = "？！"
        End Select
        MyMsgBox($"你今天的人品值是：{Value}{Message}", $"今日人品 - {DateTime.Now:yyyy/M/d}", "我知道了", IsWarn:=Value < 20)
    End Sub

    Private Shared Function StableHash(Text As String) As Integer
        Dim Hash As Long = &H811C9DC5L
        For Each Ch As Char In Text
            Hash = ((Hash Xor AscW(Ch)) * 16777619L) And &HFFFFFFFFL
        Next
        If Hash > &H7FFFFFFFL Then Hash -= &H100000000L
        Return CInt(Hash)
    End Function

#End Region

#Region "内存优化"

    Public Sub BtnMemory_Click(sender As Object, e As MouseButtonEventArgs)
        RunInThread(Sub() MemoryOptimize(True))
    End Sub

    Public Shared Sub MemoryOptimize(ShowHint As Boolean)
        If IsMemoryOptimizeRunning Then
            If ShowHint Then Hint("内存优化尚未结束，请稍等！")
            Return
        End If
        IsMemoryOptimizeRunning = True
        Try
            If ShowHint Then Hint("正在进行内存优化……")
            Dim Before As Long = My.Computer.Info.AvailablePhysicalMemory
            If Not IsAdmin() Then
                Logger.Info("没有管理员权限，将以命令行方式进行内存优化")
                Try
                    Dim Proc As Process = Process.Start(New ProcessStartInfo(PathExe, "--memory") With {.Verb = "runas", .UseShellExecute = True})
                    Proc.WaitForExit()
                    If Proc.ExitCode < 0 Then Return
                    Before -= Proc.ExitCode * 1024L
                Catch ex As Exception
                    Logger.Warn(ex, "命令行形式内存优化失败")
                    If ShowHint Then Hint("获取管理员权限失败，请尝试右键 PCL，选择「以管理员身份运行」！", HintType.Red)
                    Return
                End Try
            End If
            Dim UsedFallback As Boolean = False
            Try
                MemoryOptimizeInternal(ShowHint)
            Catch ex As Exception
                Logger.Warn(ex, "系统内存优化失败，改为清理进程工作集")
                UsedFallback = True
                Try
                    For Each Proc In Process.GetProcesses()
                        Try
                            SetProcessWorkingSetSize(Proc.Handle, New IntPtr(-1), New IntPtr(-1))
                        Catch
                        End Try
                    Next
                Catch fallbackEx As Exception
                    Logger.Warn(fallbackEx, "清理进程工作集失败")
                    If ShowHint Then Hint("内存优化失败，请右键 PCL 选择「以管理员身份运行」后重试！", HintType.Red)
                    Return
                End Try
            End Try
            Dim Delta As Long = CLng(My.Computer.Info.AvailablePhysicalMemory) - Before
            Dim Current As String = StringUtils.FormatByteSize(My.Computer.Info.AvailablePhysicalMemory)
            If Delta > 0 Then
                If ShowHint Then Hint($"内存优化完成{(If(UsedFallback, "（已清理进程工作集）", ""))}，可用内存增加了 {StringUtils.FormatByteSize(CLng(Delta * 0.8))}，目前剩余内存 {Current}！", HintType.Green)
            ElseIf ShowHint Then
                Hint($"内存优化完成，目前剩余内存 {Current}！")
            End If
        Finally
            IsMemoryOptimizeRunning = False
        End Try
    End Sub

    Public Shared Sub MemoryOptimizeInternal(ShowHint As Boolean)
        If Not IsAdmin() Then Throw New Exception("内存优化功能需要管理员权限！")
        Dim Identity As WindowsIdentity = WindowsIdentity.GetCurrent(TokenAccessLevels.Query Or TokenAccessLevels.AdjustPrivileges)
        Dim State As New PrivilegeToken With {.PrivilegeCount = 1, .Attributes = 2}
        If Not LookupPrivilegeValue(Nothing, "SeProfileSingleProcessPrivilege", State.Luid) Then
            Throw New Exception($"获取内存优化权限失败（错误代码：{Marshal.GetLastWin32Error()}）")
        End If
        Dim Previous As IntPtr = IntPtr.Zero
        Dim ReturnLength As Integer = 0
        If Not AdjustTokenPrivileges(Identity.Token, False, State, Marshal.SizeOf(State), Previous, ReturnLength) Then
            Throw New Exception($"获取内存优化权限失败（错误代码：{Marshal.GetLastWin32Error()}）")
        End If
        For Index As Integer = 2 To 4
            Dim Handle As GCHandle = GCHandle.Alloc(Index, GCHandleType.Pinned)
            Dim Result As UInteger = NtSetSystemInformation(80, Handle.AddrOfPinnedObject(), Marshal.SizeOf(Index))
            Handle.Free()
            If Result <> 0 Then Throw New Exception($"内存优化需要系统权限，请右键 PCL 选择「以管理员身份运行」后重试（错误代码：{Result}）")
        Next
    End Sub

    Private Shared Function IsAdmin() As Boolean
        Try
            Return New WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)
        Catch
            Return False
        End Try
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Private Structure PrivilegeToken
        Public PrivilegeCount As Integer
        Public Luid As Long
        Public Attributes As Integer
    End Structure

    <DllImport("ntdll.dll")>
    Private Shared Function NtSetSystemInformation(SystemInformationClass As Integer, SystemInformation As IntPtr, SystemInformationLength As Integer) As UInteger
    End Function

    <DllImport("advapi32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function LookupPrivilegeValue(lpSystemName As String, lpName As String, ByRef lpLuid As Long) As Boolean
    End Function

    <DllImport("advapi32.dll", SetLastError:=True)>
    Private Shared Function AdjustTokenPrivileges(TokenHandle As IntPtr, DisableAllPrivileges As Boolean, ByRef NewState As PrivilegeToken,
                                                  BufferLength As Integer, ByRef PreviousState As IntPtr, ByRef ReturnLength As Integer) As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function SetProcessWorkingSetSize(hProcess As IntPtr, dwMinimumWorkingSetSize As IntPtr, dwMaximumWorkingSetSize As IntPtr) As Boolean
    End Function

#End Region

#Region "清理垃圾"

    Public Sub BtnClear_Click(sender As Object, e As MouseButtonEventArgs)
        RubbishClear()
    End Sub

    Public Shared Sub RubbishClear()
        If McInstanceLock.IsAnyLocked OrElse HasDownloadingTask() Then
            Hint("请先关闭所有运行中的游戏与下载任务！")
            Return
        End If
        RunInNewThread(
        Sub()
            Try
                If MyMsgBox("即将清理游戏日志、错误报告、缓存等文件。" & vbCrLf & "虽然应该没人往这些地方放重要文件，但还是问一下，是否确认继续？" & vbCrLf & vbCrLf & "在完成清理后，需要重启 PCL。", "清理确认", "确定", "取消") = 2 Then Return
                DirectoryUtils.Delete(PathTemp & "Cache\", False)
                For Each Folder In McFolderList
                    DirectoryUtils.Delete(Folder.Location & "logs\", False)
                    DirectoryUtils.Delete(Folder.Location & "crash-reports\", False)
                    For Each Version In DirectoryUtils.EnumerateDirectories(Folder.Location & "versions")
                        DirectoryUtils.Delete(Version & "\logs\", False)
                        DirectoryUtils.Delete(Version & "\crash-reports\", False)
                    Next
                Next
                Logger.Info("清理游戏垃圾完成")
                Hint("清理完成，请重启 PCL！", HintType.Green)
            Catch ex As Exception
                Logger.Error(ex, "清理游戏垃圾失败")
                Hint("清理失败：" & ex.Message, HintType.Red)
            End Try
        End Sub, "RubbishClear")
    End Sub

#End Region

#Region "千万别点"

    Public Sub BtnClick_Click(sender As Object, e As MouseButtonEventArgs)
        If MyMsgBox("恭喜你发现了彩蛋！" & vbCrLf & vbCrLf & "这里只有一句提醒：不要再点啦！", "千万别点", "我知道了", "再来一次") = 1 Then
            If ThemeUnlock(13, False) Then Hint("隐藏主题 欧皇彩 已解锁！", HintType.Green)
        End If
    End Sub

#End Region

#Region "回声洞"

    Public Sub BtnCave_Click(sender As Object, e As EventArgs)
        OpenWebsite("https://jinshuju.net/f/esXHQF")
    End Sub

    Public Sub CardCave_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
        If Not IsCaveEnabled Then Return
        IsCaveEnabled = False
        LabCave.Text = GetRandomCave()
        AniStart({
            AaOpacity(LabCave, -LabCave.Opacity + 0.2, 80),
            AaOpacity(LabCave, 1 - LabCave.Opacity, 120, 80)
        }, "Cave")
        RunInNewThread(
        Sub()
            Thread.Sleep(300)
            RunInUi(Sub() IsCaveEnabled = True)
        End Sub)
    End Sub

    Public Shared Function GetRandomCave() As String
        Return RandomOne(New String() {
            "来硬的！", "钻石！", "我们需要再深入些！", "结束了？", "君临天下！", "天空即为极限！",
            "这是？工作台！", "结束了。开始了？", "这交易不错！", "你的世界！", "这是特性！",
            "It's a FEATURE! Not a BUG!", "苦力怕！不是爬行者！", "粉色羊是隐性纯合子！",
            "这么多留言中你偏偏看见我的，爱你么么哒 ╰(￣ω￣ｏ)！", "Deadline 是第一生产力！",
            "今天也努力当咸鱼了呢（瘫）", "MC 是我们的青春啊！", "我的世界，启动！",
            "正于此地，愿你找到想要的书。", "To be continued.", "脑子放假去了！",
            "希望这个游戏可以给你带来更多的快乐！", "Also try Celeste!", "要恰饭的嘛！",
            "人生没有标准答案，去做自己认为正确的事！", "支持正版 MC，加油！",
            "失之东隅，收之桑榆。", "你的旅途……到此为止……", "那么，我们开始吧？",
            "我可以一个人很好，但有你会更好。", "相信我，一切都会变好的！",
            "长风破浪会有时，直挂云帆济沧海！", "希望人有事.jpg", "修了的叫 Bug，没修的叫特性。",
            "世界和平！", "Minecraft 2.0 正在启动中！", "开学哩！", "别点了！手不累吗？",
            "反复下载的是游戏，回不到的是童年。", "我的世界我做主！", "宁可我负天下人，不可天下人负我！",
            "挖矿免费玩！", "宁可没用，不能没有！", "希望可爱的龙猫猫能更容易看懂自己写的代码。",
            "PCL 的代码只有龙猫和上帝知道，然而龙猫忘记了。", "基岩真好吃！",
            "在没有错误日志的情况下诊断任何问题无异于闭眼开车。", "犹豫，就会白给！",
            "龙猫是龙还是猫？", "我爱吃薯条 awa", "恭喜你，你被恭喜了！",
            "你这什么游戏啊？手机能玩吗？要不要钱啊？", "多行不义必自毙。",
            "喵喵喵，在这里放一只梵猫~他会盯着你的 awa", "众所周知，别人的世界和我的世界不是同一款游戏。",
            "这启动器也太好用了，其实捐 RMB 可以获得更多功能，但不捐基础功能也是很多的。",
            "你知道吗：千万别惹玄素，否则会被夹得很惨！", "龙猫不是鸽子，只是太忙了（确信）！",
            "问君能有几多愁？恰似一缸龙猫向东流。", "Make Minecraft Great Again!",
            "今日大无语事件：外卖点了个黑椒牛排套餐，结果商家忘了放牛排……",
            "虽然上面那个按钮叫千万别点，但是还有好多人去点它！", "友谊是魔法！",
            "我们都是阴沟里的虫子，但总还是得有人仰望星空。", "开学愉快！",
            "祝我下个池子出水大叔！", "Bugjump 自古特性多，可与育碧争霸王！",
            "错误代码：-118" & vbCrLf & "无法载入网页（未知错误）", "获得成就：别人的世界！",
            "冷知识：这其实是一条热知识！", "问君能有几多愁？恰似一缸龙猫向东流。",
            "结束了吗？开始了。", "希望你能在 PCL 里玩得开心！"
        })
    End Function

    Public Shared Function GetRandomHint() As String
        Try
            If FileUtils.Exists(PathData & "hints.txt") Then
                Dim Lines = FileUtils.ReadAsLines(PathData & "hints.txt").Where(Function(s) Not String.IsNullOrWhiteSpace(s)).ToList()
                If Lines.Any() Then Return RandomOne(Lines)
            End If
        Catch ex As Exception
            Logger.Warn(ex, "获取自定义提示失败")
        End Try
        Return GetRandomPresetHint()
    End Function

    Public Shared Function GetRandomPresetHint() As String
        Return RandomOne(New String() {
            "在版本选择页面，右键某个版本也能进入版本设置页面！",
            "你可以在版本设置中调整分类，以将特定版本隐藏！",
            "使用 --memory 参数启动 PCL 可以静默进行内存优化！",
            "在第一次启动游戏时，PCL 会自动将语言设置为中文！",
            "在版本设置中可以开启第三方登录验证，例如 Little Skin 登录！",
            "自动配置游戏内存时，PCL 将会根据剩余内存与 Mod 数量动态决定分配的内存！",
            "主页可以使用特定的预设，在设置中看看吧！",
            "在高级启动选项中，可以设置在游戏启动前运行特定程序！",
            "将鼠标悬浮在设置页的左边栏，可以找到重置设置按钮！",
            "版本设置只对当前版本生效，而设置页面的设置对所有版本生效！",
            "要将已有的 MC 文件夹加入 PCL，可以在版本选择页的左侧选择「添加已有文件夹」！",
            "如果同时安装了 OptiFine 与对应的原版，PCL 会展示 OptiFine 版本，折叠原版！",
            "版本选择的常规版本分类中，只会列出最新的一个快照或预发布版。",
            "在版本选择页面，右键游戏文件夹可以进行打开、重命名、删除等操作！",
            "如果你在其他地方修改了皮肤，需要手动选择「刷新皮肤」才能更新登录页面的头像……",
            "下载 Mod 时，PCL 会自动定位对应版本的 Mod 文件夹！",
            "如果缺少 Java，PCL 也能自动下载，不必自己安装啦！",
            "如果你打开了调试模式，启动页右侧就会显示启动日志！",
            "将鼠标悬浮在下载页的左边栏，可以找到刷新按钮！",
            "将鼠标指向下载页的 MC 版本，可以在右侧找到更新日志选项！",
            "直接把 Mod 或整合包拖进 PCL 窗口就能安装了！",
            "在 PCL 文件夹下新建 hints.txt，可以自定义「你知道吗」的内容！",
            "设置中可以自定义离线皮肤，但这只对单人游戏有效！",
            "如果不想用某项功能，可以在个性化设置中把它隐藏掉！",
            "你可以使用主页来自定义快捷方式！",
            "点击版本右侧的心形就能将该版本加入收藏夹，便于查找。",
            "PCL 的第一个内部版本制作于 2018 年 8 月 13 日！",
            "PCL 的绝大多数代码都在 GitHub 开源了！",
            "PCL 的开发者龙腾猫跃经常被简称为龙猫，但和那只龙猫没有任何关系！"
        })
    End Function

    Private Shared Function RandomOne(Of T)(Items As T()) As T
        Return Items(RandomInteger(0, Items.Length - 1))
    End Function

    Private Shared Function RandomOne(Items As IEnumerable(Of String)) As String
        Dim List = Items.ToList()
        Return List(RandomInteger(0, List.Count - 1))
    End Function

#End Region

#Region "自定义下载"

    Public Sub DownloadText_Changed(sender As Object, e As TextChangedEventArgs)
        StartButtonRefresh()
    End Sub

    Public Sub BtnDownloadStart_Click(sender As Object, e As MouseButtonEventArgs)
        Dim Url As String = TextDownloadUrl.Text.Trim()
        If Not Url.StartsWithF("http://", True) AndAlso Not Url.StartsWithF("https://", True) Then
            Hint("请输入以 http:// 或 https:// 开头的下载地址！", HintType.Red)
            Return
        End If
        If String.IsNullOrWhiteSpace(TextDownloadName.Text) Then
            Hint("请输入文件名！", HintType.Red)
            Return
        End If
        StartCustomDownload(Url, TextDownloadName.Text.Trim(), TextDownloadFolder.Text)
        TextDownloadUrl.Text = ""
        TextDownloadUrl.Validate()
        TextDownloadUrl.ForceShowAsSuccess()
        TextDownloadName.Text = ""
        TextDownloadName.Validate()
        TextDownloadName.ForceShowAsSuccess()
        StartButtonRefresh()
    End Sub

    Public Shared Sub StartCustomDownload(Url As String, FileName As String, Optional Folder As String = Nothing)
        Try
            If String.IsNullOrWhiteSpace(Folder) Then
                Dim Dialog As New Microsoft.Win32.SaveFileDialog With {.FileName = FileName, .Title = "选择文件保存位置"}
                If Dialog.ShowDialog() <> True Then Return
                FileName = PathUtils.GetLastPart(Dialog.FileName)
                Folder = PathUtils.RemoveLastPart(Dialog.FileName)
            End If
            Folder = PathUtils.AddSlashSuffix(Folder.Replace("/", "\"))
            Try
                DirectoryUtils.Create(Folder)
                CheckPermissionWithException(Folder)
            Catch ex As Exception
                Logger.Error(ex, $"访问文件夹失败（{Folder}）", LogBehavior.Toast)
                Return
            End Try
            Logger.Info($"自定义下载文件名：{FileName}")
            Logger.Info($"自定义下载文件目标：{Folder}")
            Dim Uuid As Integer = GetUuid()
            Dim Download As New LoaderDownload("自定义下载文件：" & FileName, {New NetFile({Url}, Folder & FileName, Nothing, SimulateBrowserHeaders:=True)})
            Dim Combo As New LoaderCombo(Of Integer)("自定义下载 (" & Uuid & ")", New List(Of LoaderBase) From {Download})
            AddHandler Combo.OnStateChangedUi, Sub(Loader, NewState, OldState) DownloadState(Combo)
            Combo.Start()
            LoaderTaskbarAdd(Combo)
            FrmMain.BtnExtraDownload.ShowRefresh()
            FrmMain.BtnExtraDownload.Ribble()
        Catch ex As Exception
            Logger.Error(ex, "开始自定义下载失败", LogBehavior.Toast)
        End Try
    End Sub

    Public Sub BtnDownloadSelect_Click(sender As Object, e As EventArgs)
        Dim Dialog As New System.Windows.Forms.FolderBrowserDialog With {.Description = "选择目标文件夹"}
        If Dialog.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            TextDownloadFolder.Text = Dialog.SelectedPath
        End If
    End Sub

    Public Sub BtnDownloadOpen_Click(sender As Object, e As MouseButtonEventArgs)
        Try
            DirectoryUtils.Create(TextDownloadFolder.Text)
            StartProcess(TextDownloadFolder.Text)
        Catch ex As Exception
            Logger.Warn(ex, "打开下载文件夹失败")
        End Try
    End Sub

    Private Shared Sub DownloadState(Loader As LoaderCombo(Of Integer))
        Select Case Loader.State
            Case LoadState.Finished
                Hint(Loader.Name & "完成！", HintType.Green)
                Interaction.Beep()
            Case LoadState.Failed
                Logger.Error(Loader.Error, Loader.Name & "失败", LogBehavior.Toast)
                Interaction.Beep()
            Case LoadState.Canceled
                Hint(Loader.Name & "已取消！")
        End Select
    End Sub

#End Region

End Class
