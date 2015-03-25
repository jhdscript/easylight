Imports System.Threading
Imports System.Reflection
Imports Microsoft.Win32
Imports System.Collections.ObjectModel
Imports System.IO

Class MainWindow

#Region "Enums"
    Private Enum Mode
        DYNAMIC = 0
        [STATIC] = 1
    End Enum

    Private Enum ArduinoStatus
        NOTCONNECTED = -1
        PENDING = 0
        CONNECTED = 1
    End Enum
#End Region

#Region "Properties"
    Private Const _WebsiteUrl As String = "http://www.zem.fr"
    Private Const _CpuAlert As Integer = 15 'en pourcentage
    Private Const _MemoryAlert As Integer = 100 * 1024 * 1024 'en MegaBytes

    Private Const _ArduinoByteStartCommand As Byte = 40 'Byte de debut de commande
    Private Const _ArduinoByteEndCommand As Byte = 41 'Byte de fin de commande
    Private Const _ArduinoByteCommandDynamic As Byte = Asc("D") 'D pour dynamic
    Private Const _ArduinoByteCommandStatic As Byte = Asc("F") 'D pour fixed
    Private Const _ArduinoByteSeparatorSeries As Byte = Asc("-") 'pour separer les series

    Private _ColorRed As New SolidColorBrush(Colors.Red)
    Private _ColorGreen As New SolidColorBrush(Colors.Green)
    Private _ColorBlack As New SolidColorBrush(Colors.Black)

    Private Const _PollerSleep As Integer = 25 'temps a attendre entre 2 captures
    Private PollerStarted As Boolean = False
    Private PollerThread As Thread = Nothing
    Private DicoOfBorders As New Dictionary(Of Integer, Border)

    Private WithEvents CurrentArduino As Arduino = Nothing
    Private ArduinoRetries As Integer = 0

    Private NbLoopPerSeconds As Integer = 0
    Private WithEvents TimerNbLoopPerSeconds As New Timers.Timer(1000)

    Private ListOfMsgLog As New ObservableCollection(Of String) 'permet de stocker les messages de log

    Private Delegate Function DelegateUpdateIhmGrid(ByRef dico As Dictionary(Of Integer, System.Windows.Media.Color)) As Boolean
    Private Delegate Sub DelegateUpdateIhmStatusBar(ByVal nbloops As Integer)
    Private Delegate Sub DelegateListOfMsgLogAdd(ByVal msg As String)
    Private Delegate Sub DelegateArduinoStatus(ByVal status As ArduinoStatus)

    Private CurrentPix As EasyLight
    Private BtExitClicked As Boolean = False
#End Region

#Region "Constructeur / Loaded"
    'Constructeur
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
    End Sub

    'Handle sur le closing
    Private Sub MainWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles Me.Closing
        If BtExitClicked = False Then
            e.Cancel = True
            Me.WindowState = Windows.WindowState.Minimized
        Else
            BtExitClicked = False
        End If
    End Sub

    'Handle sur le loaded
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Minimizer dans la barre des taches
        MinimizeToTray.Enable(Me)
        MinimizeToTray.CurrentSystray.SetSystrayIcon(True, vbNullString)
        If My.Settings.RunMinimized Then
            Me.WindowState = Windows.WindowState.Minimized
        End If

        'On associe ListOfMsgLog a la ListBox
        ListBoxArduinoLogs.ItemsSource = ListOfMsgLog

        'Global Settings
        ChkRunMinimized.IsChecked = My.Settings.RunMinimized
        ChkAutoStart.IsChecked = My.Settings.AutoStart
        If My.Settings.Mode = Mode.DYNAMIC Then
            CbiModeDynamicColor.IsSelected = True
        Else
            CbiModeStaticColor.IsSelected = True
        End If

        'Arduino Settings
        Call GetSerialPortNames()
        For i As Integer = 0 To CboBaudRate.Items.Count - 1
            If CType(CboBaudRate.Items(i), ComboBoxItem).Content.ToString = My.Settings.ArduinoBaudRate Then
                CboBaudRate.SelectedIndex = i
                Exit For
            End If
        Next

        For i As Integer = 0 To CboPortCom.Items.Count - 1
            If CType(CboPortCom.Items(i), ComboBoxItem).Content.ToString = My.Settings.ArduinoSerialPort Then
                CboPortCom.SelectedIndex = i
                Exit For
            End If
        Next

        For i As Integer = 0 To CboMaxRetries.Items.Count - 1
            If CInt(CType(CboMaxRetries.Items(i), ComboBoxItem).Content) = My.Settings.ArduinoMaxRetries Then
                CboMaxRetries.SelectedIndex = i
                Exit For
            End If
        Next

        'Dynamic Mode Settings
        ChkSkipDarkPixels.IsChecked = My.Settings.SkipDarkPixel
        For i As Integer = 0 To CboProportional.Items.Count - 1
            If CType(CboProportional.Items(i), ComboBoxItem).Content.ToString = My.Settings.Proportional Then
                CboProportional.SelectedIndex = i
                Exit For
            End If
        Next

        For i As Integer = 0 To CboLayer.Items.Count - 1
            If CType(CboLayer.Items(i), ComboBoxItem).Content.ToString = My.Settings.Layer Then
                CboLayer.SelectedIndex = i
                Exit For
            End If
        Next

        For i As Integer = 0 To CboMethod.Items.Count - 1
            If CType(CboMethod.Items(i), ComboBoxItem).Content.ToString = My.Settings.Method Then
                CboMethod.SelectedIndex = i
                Exit For
            End If
        Next

        For i As Integer = 0 To CboHscan.Items.Count - 1
            If CType(CboHscan.Items(i), ComboBoxItem).Content.ToString = My.Settings.Hscan Then
                CboHscan.SelectedIndex = i
                Exit For
            End If
        Next

        For i As Integer = 0 To CboVscan.Items.Count - 1
            If CType(CboVscan.Items(i), ComboBoxItem).Content.ToString = My.Settings.Vscan Then
                CboVscan.SelectedIndex = i
                Exit For
            End If
        Next

        'Static Mode Settings
        ColorPickerTop.SelectedColor = HexToColor(My.Settings.StaticColorTop)
        ColorPickerBottom.SelectedColor = HexToColor(My.Settings.StaticColorBottom)
        ColorPickerLeft.SelectedColor = HexToColor(My.Settings.StaticColorLeft)
        ColorPickerRight.SelectedColor = HexToColor(My.Settings.StaticColorRight)

        'on lance après le load pour être sur d'ajouter au demarrage de windows lors du tout premier demarrage
        Call CheckAutoStart()

        'les paramètres sont chargés alors on démarre le poller
        Call StartPoller()
    End Sub
#End Region

#Region "Clic Boutons"
    'Clic sur le bouton de Sauvegarde
    Private Sub BtSave_Click(sender As Object, e As RoutedEventArgs)
        Call StopPoller()

        'Global Settings
        My.Settings.AutoStart = CBool(ChkAutoStart.IsChecked)
        My.Settings.RunMinimized = CBool(ChkRunMinimized.IsChecked)
        If CbiModeDynamicColor.IsSelected Then
            My.Settings.Mode = Mode.DYNAMIC
        Else
            My.Settings.Mode = Mode.STATIC
        End If

        'Arduino Settings
        If CboPortCom.SelectedValue IsNot Nothing Then
            My.Settings.ArduinoSerialPort = CType(CboPortCom.SelectedValue, ComboBoxItem).Content.ToString
        End If
        My.Settings.ArduinoBaudRate = CType(CboBaudRate.SelectedValue, ComboBoxItem).Content.ToString
        My.Settings.ArduinoMaxRetries = CInt(CType(CboMaxRetries.SelectedValue, ComboBoxItem).Content)

        'Dynamic Mode Settings
        My.Settings.SkipDarkPixel = CBool(ChkSkipDarkPixels.IsChecked)
        My.Settings.Hscan = CType(CboHscan.SelectedValue, ComboBoxItem).Content.ToString
        My.Settings.Layer = CType(CboLayer.SelectedValue, ComboBoxItem).Content.ToString
        My.Settings.Method = CType(CboMethod.SelectedValue, ComboBoxItem).Content.ToString
        My.Settings.Proportional = CType(CboProportional.SelectedValue, ComboBoxItem).Content.ToString
        My.Settings.Vscan = CType(CboVscan.SelectedValue, ComboBoxItem).Content.ToString

        'Static Mode Settings
        My.Settings.StaticColorTop = Right(ColorPickerTop.SelectedColor.ToString, 6)
        My.Settings.StaticColorBottom = Right(ColorPickerBottom.SelectedColor.ToString, 6)
        My.Settings.StaticColorLeft = Right(ColorPickerLeft.SelectedColor.ToString, 6)
        My.Settings.StaticColorRight = Right(ColorPickerRight.SelectedColor.ToString, 6)

        'on sauve et on démarre
        My.Settings.Save()
        Call CheckAutoStart()
        Call StartPoller()
    End Sub

    'Clic sur le bouton d'Exit
    Private Sub BtExit_Click(sender As Object, e As RoutedEventArgs)
        Call StopPoller()
        BtExitClicked = True
        Me.Close()
    End Sub
#End Region

#Region "Fonctions pour la GridDraw"
    Private Sub DrawGrid()
        DicoOfBorders.Clear()
        GridDraw.Children.Clear()
        GridDraw.ColumnDefinitions.Clear()
        GridDraw.RowDefinitions.Clear()

        Dim nbCol As Integer = 0
        Dim nbRow As Integer = 4
        If My.Settings.Layer = CbiLayer2x2.Content.ToString Then
            nbCol = 2
        ElseIf My.Settings.Layer = CbiLayer3x1.Content.ToString Then
            nbCol = 3
            nbRow = 3
        ElseIf My.Settings.Layer = CbiLayer3x2.Content.ToString Then
            nbCol = 3
        ElseIf My.Settings.Layer = CbiLayer4x2.Content.ToString Then
            nbCol = 4
        End If
        For i As Integer = 0 To nbRow - 1
            GridDraw.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
        Next
        For i As Integer = 0 To nbCol - 1
            GridDraw.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
        Next

        'on draw les borders
        Dim name As Integer = 0
        For i As Integer = 0 To nbRow - 1
            For j As Integer = 0 To nbCol - 1
                If i = 0 Or j = 0 Or i = nbRow - 1 Or j = nbCol - 1 Then
                    Dim t As New TextBlock With {.Text = name.ToString, .HorizontalAlignment = Windows.HorizontalAlignment.Center, .VerticalAlignment = Windows.VerticalAlignment.Center}
                    Dim b As New Border With {.Name = "Border" & name, .BorderThickness = New Thickness(1), .BorderBrush = New SolidColorBrush(Colors.Black), .Child = t}
                    Grid.SetColumn(b, j)
                    Grid.SetRow(b, i)
                    GridDraw.Children.Add(b)
                    DicoOfBorders(name) = b
                    name += 1
                End If
            Next
        Next
    End Sub
#End Region

#Region "Fonctions pour le poller"
    'Fonction qui demarre le poller
    Private Sub StartPoller()
        PollerStarted = True
        If PollerThread IsNot Nothing Then
            PollerThread.Abort()
        End If
        Call DrawGrid()
        TimerNbLoopPerSeconds.Start()
        Dim layer As EasyLight.EnumLayer
        Select Case My.Settings.Layer.ToLower
            Case "2 x 2" : layer = EasyLight.EnumLayer.L2XH2
            Case "3 x 2" : layer = EasyLight.EnumLayer.L3XH2
            Case "3 x 1" : layer = EasyLight.EnumLayer.L3XH1
            Case Else : layer = EasyLight.EnumLayer.L4XH2
        End Select

        Dim method As EasyLight.EnumMethod
        Select Case My.Settings.Method.ToLower
            Case "max" : method = EasyLight.EnumMethod.MAX
            Case "average" : method = EasyLight.EnumMethod.AVERAGE
        End Select
        CurrentPix = New EasyLight(CInt(My.Settings.Proportional), method, layer, CInt(Trim(Replace(My.Settings.Hscan, "%", ""))), CInt(Trim(Replace(My.Settings.Vscan, "%", ""))), My.Settings.SkipDarkPixel)

        Call ArduinoStart()

        PollerThread = New Thread(AddressOf Poll)
        PollerThread.SetApartmentState(ApartmentState.STA)
        PollerThread.Start()
    End Sub

    'Fonction qui arrete le poller
    Private Sub StopPoller()
        PollerStarted = False
        If PollerThread IsNot Nothing Then
            PollerThread.Abort()
        End If
        TimerNbLoopPerSeconds.Stop()
        NbLoopPerSeconds = 0

        Call ArduinoStop()
    End Sub

    'Fonction qui lance l'analyse de la capture
    Private Sub Poll()
        While PollerStarted
            Try
                Dim dico As Dictionary(Of Integer, System.Windows.Media.Color) = CurrentPix.AnalysePixels()
                NbLoopPerSeconds += 1

                Call ArduinoSend(dico)

                Me.Dispatcher.BeginInvoke(New DelegateUpdateIhmGrid(AddressOf UpdateIhmGrid), {dico})
            Catch ex As Exception
            End Try
            Thread.Sleep(_PollerSleep)
        End While
    End Sub

    'Fonction qui met à jour la grid de l IHM
    Private Function UpdateIhmGrid(ByRef dico As Dictionary(Of Integer, System.Windows.Media.Color)) As Boolean
        Dim ret As Boolean = True
        Try
            If Me.WindowState = Windows.WindowState.Maximized Or Me.WindowState = Windows.WindowState.Normal Then
                For Each itm As KeyValuePair(Of Integer, System.Windows.Media.Color) In dico
                    If DicoOfBorders.ContainsKey(itm.Key) Then
                        DicoOfBorders(itm.Key).Background = New SolidColorBrush(itm.Value)
                    End If
                Next
            End If
        Catch ex As Exception
            ret = False
        End Try
        Return ret
    End Function

    'Fonction qui met a jour le nombre de rafraichissement par seconde
    Private Sub TimerNbLoopPerSeconds_Elapsed(ByVal sender As Object, ByVal e As Timers.ElapsedEventArgs) Handles TimerNbLoopPerSeconds.Elapsed
        Me.Dispatcher.BeginInvoke(New DelegateUpdateIhmStatusBar(AddressOf UpdateIhmStatusBar), {NbLoopPerSeconds})
        NbLoopPerSeconds = 0
    End Sub

    'Fonction qui met à jour la statusbar
    Private Sub UpdateIhmStatusBar(ByVal nbloops As Integer)
        TxtStatusNbLoop.Text = nbloops.ToString

        Dim cpu As Double = MyProcessInfo.GetCpuPercentage()
        If cpu = -1 Then
            TxtStatusCpu.Text = "n/a"
            TxtStatusCpu.Foreground = _ColorBlack
        Else
            TxtStatusCpu.Text = cpu.ToString("0.00") & "%"
            If cpu < _CpuAlert Then
                TxtStatusCpu.Foreground = _ColorGreen
            Else
                TxtStatusCpu.Foreground = _ColorRed
            End If
        End If

        Dim mem As Double = MyProcessInfo.GetMemoryUsedSize()
        If mem = -1 Then
            TxtStatusMemory.Text = "n/a"
            TxtStatusMemory.Foreground = _ColorBlack
        Else
            TxtStatusMemory.Text = ConvertToHumanReadable(mem)
            If mem < _MemoryAlert Then
                TxtStatusMemory.Foreground = _ColorGreen
            Else
                TxtStatusMemory.Foreground = _ColorRed
            End If
        End If
    End Sub
#End Region

#Region "Fonctions pour Arduino (Wrapper)"
    'Fonction qui démarre le arduino
    Private Sub ArduinoStart()
        Call ArduinoStop()
        Me.Dispatcher.BeginInvoke(New DelegateArduinoStatus(AddressOf ArduinoStatusChanged), {ArduinoStatus.PENDING})
        CurrentArduino = New Arduino(My.Settings.ArduinoSerialPort, CInt(My.Settings.ArduinoBaudRate))
        AddHandler CurrentArduino.WatchdogReceived, AddressOf CurrentArduino_WatchdogReceived
        AddHandler CurrentArduino.ConnectionLost, AddressOf CurrentArduino_ConnectionLost
        AddHandler CurrentArduino.LogMessageReceived, AddressOf CurrentArduino_LogMessageReceived
        CurrentArduino.Start()
    End Sub

    'Fonction qui arrete le arduino
    Private Sub ArduinoStop()
        If CurrentArduino IsNot Nothing Then
            CurrentArduino.Stop()
            RemoveHandler CurrentArduino.WatchdogReceived, AddressOf CurrentArduino_WatchdogReceived
            RemoveHandler CurrentArduino.ConnectionLost, AddressOf CurrentArduino_ConnectionLost
            RemoveHandler CurrentArduino.LogMessageReceived, AddressOf CurrentArduino_LogMessageReceived
        End If
        CurrentArduino = Nothing
    End Sub

    'Handle sur le Watchdog
    Private Sub CurrentArduino_WatchdogReceived()
        ArduinoRetries = 0 'on a reçu un watchdog c'est donc qu'on est connecté :)
        Me.Dispatcher.BeginInvoke(New DelegateArduinoStatus(AddressOf ArduinoStatusChanged), {ArduinoStatus.CONNECTED})
    End Sub

    'Handle sur le ConnectionLost
    Private Sub CurrentArduino_ConnectionLost()
        Me.Dispatcher.BeginInvoke(New DelegateArduinoStatus(AddressOf ArduinoStatusChanged), {ArduinoStatus.NOTCONNECTED})
        If ArduinoRetries < My.Settings.ArduinoMaxRetries Then
            ArduinoRetries += 1
            Call ArduinoStart()
        End If
    End Sub

    'Handle sur la reception des messages
    Private Sub CurrentArduino_LogMessageReceived(ByVal msg As String)
        Me.Dispatcher.BeginInvoke(New DelegateListOfMsgLogAdd(AddressOf ListOfMsgLogAdd), {msg})
    End Sub

    'Fonction qui envoie une commande au Arduino
    Private Sub ArduinoSend(ByRef dico As Dictionary(Of Integer, System.Windows.Media.Color))
        Dim tmp As New List(Of Byte)

        tmp.Add(_ArduinoByteStartCommand)
        If My.Settings.Mode = Mode.DYNAMIC Then ' (D-1:40:255:245-2:45:111:214-3:45:45:87)
            tmp.Add(_ArduinoByteCommandDynamic)
            tmp.Add(_ArduinoByteSeparatorSeries)
            Dim n As Integer = 0
            Dim enddico As Integer = dico.Count - 1
            For Each itm As KeyValuePair(Of Integer, System.Windows.Media.Color) In dico
                tmp.Add(CByte(itm.Key))
                tmp.Add(itm.Value.R)
                tmp.Add(itm.Value.G)
                tmp.Add(itm.Value.B)
                If n < enddico Then
                    tmp.Add(_ArduinoByteSeparatorSeries)
                End If
                n += 1
            Next
        Else ' (F-1:40:255:245-2:40:255:245-3:45:45:87-4:45:45:87) '-> 1: Top, 2: Bottom, 3: Left, 4: Right
            Dim top As System.Windows.Media.Color = HexToColor(My.Settings.StaticColorTop)
            Dim bottom As System.Windows.Media.Color = HexToColor(My.Settings.StaticColorBottom)
            Dim left As System.Windows.Media.Color = HexToColor(My.Settings.StaticColorLeft)
            Dim right As System.Windows.Media.Color = HexToColor(My.Settings.StaticColorRight)
            tmp.Add(_ArduinoByteCommandStatic)
            tmp.Add(_ArduinoByteSeparatorSeries)
            tmp.Add(1)
            tmp.Add(top.R)
            tmp.Add(top.G)
            tmp.Add(top.B)
            tmp.Add(2)
            tmp.Add(bottom.R)
            tmp.Add(bottom.G)
            tmp.Add(bottom.B)
            tmp.Add(3)
            tmp.Add(left.R)
            tmp.Add(left.G)
            tmp.Add(left.B)
            tmp.Add(4)
            tmp.Add(right.R)
            tmp.Add(right.G)
            tmp.Add(right.B)
        End If
        tmp.Add(_ArduinoByteEndCommand)

        CurrentArduino.SendCommand(tmp.ToArray)
    End Sub

    'Fonction qui met a jour le status de l'arduino dans la status bar
    Private Sub ArduinoStatusChanged(ByVal status As ArduinoStatus)
        Select Case status
            Case ArduinoStatus.CONNECTED : ImgStatusArduinoNotConnected.Visibility = Windows.Visibility.Collapsed : ImgStatusArduinoPending.Visibility = Windows.Visibility.Collapsed : ImgStatusArduinoConnected.Visibility = Windows.Visibility.Visible
            Case ArduinoStatus.NOTCONNECTED : ImgStatusArduinoNotConnected.Visibility = Windows.Visibility.Visible : ImgStatusArduinoPending.Visibility = Windows.Visibility.Collapsed : ImgStatusArduinoConnected.Visibility = Windows.Visibility.Collapsed
            Case ArduinoStatus.PENDING : ImgStatusArduinoNotConnected.Visibility = Windows.Visibility.Collapsed : ImgStatusArduinoPending.Visibility = Windows.Visibility.Visible : ImgStatusArduinoConnected.Visibility = Windows.Visibility.Collapsed
        End Select
    End Sub
#End Region

#Region "Fonctions pour la log Arduino"
    'Fonction qui ajoute un msg a la log
    Private Sub ListOfMsgLogAdd(ByVal msg As String)
        SyncLock ListOfMsgLog
            Dim o As String = Now.ToString("yyyy-MM-dd HH:mm.ss") & vbTab & msg
            ListOfMsgLog.Add(o)
            ListBoxArduinoLogs.ScrollIntoView(o)
        End SyncLock
    End Sub

    'Fonction qui sauve la log dans un fichier texte
    Private Sub MenuListBoxSave_Click(sender As Object, e As RoutedEventArgs)
        Dim f As New SaveFileDialog With {.FileName = "EasyLight.log", .DefaultExt = ".text", .Filter = "Text documents (.txt)|*.txt"}
        Dim res As Boolean? = f.ShowDialog
        If res = True Then
            Dim filename As String = f.FileName
            Dim sw As StreamWriter = Nothing
            Dim saveOk As Boolean = True
            Try
                sw = New StreamWriter(filename, False)
                SyncLock ListOfMsgLog
                    For Each s As String In ListOfMsgLog
                        sw.WriteLine(s)
                    Next
                End SyncLock
            Catch ex As Exception
                saveOk = False
            Finally
                If sw IsNot Nothing Then
                    sw.Close()
                End If
            End Try
            If saveOk Then
                MsgBox("Log is available in: " & filename, MsgBoxStyle.Information, "Arduino Log")
            Else
                MsgBox("An error occured", MsgBoxStyle.Exclamation, "Arduino Log")
            End If
        End If
    End Sub

    'Fonction qui vide la log
    Private Sub MenuListBoxArduinoClear_Click(sender As Object, e As RoutedEventArgs)
        SyncLock ListOfMsgLog
            ListOfMsgLog.Clear()
        End SyncLock
    End Sub
#End Region

#Region "Fonctions pour le menu"
    Private Sub MenuMaximize_Click(sender As Object, e As RoutedEventArgs)
        Me.WindowState = Windows.WindowState.Maximized
    End Sub

    Private Sub MenuExit_Click(sender As Object, e As RoutedEventArgs)
        Call BtExit_Click(Nothing, Nothing)
    End Sub

    Private Sub MenuWebsite_Click(sender As Object, e As RoutedEventArgs)
        If Not OpenInternet(_WebsiteUrl) Then
            MsgBox("An error occured", MsgBoxStyle.Exclamation, "Go to WebSite")
        End If
    End Sub
#End Region

#Region "Fonctions Diverses"
    'Fonction qui récupère les différents ports COM disponibles
    Private Sub GetSerialPortNames()
        For i As Integer = 0 To My.Computer.Ports.SerialPortNames.Count - 1
            Dim cbi As New ComboBoxItem With {.Content = My.Computer.Ports.SerialPortNames(i)}
            If i = 0 Then
                cbi.IsSelected = True
            End If
            CboPortCom.Items.Add(cbi)
        Next
    End Sub

    'Fonction qui permet le demarre de l'executable en meme temps que Windows
    Public Sub CheckAutoStart()
        Dim Assembly As Assembly = Application.ResourceAssembly
        Dim ExePath As String = (New Uri(Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath
        Dim AssemblyName As String = Split(Assembly.FullName, ",").First
        Dim RegistryKeyName As RegistryKey
        Try
            RegistryKeyName = Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run\", True)
            If My.Settings.AutoStart Then
                RegistryKeyName.SetValue(AssemblyName, ExePath)
            Else
                If RegistryKeyName.GetValue(AssemblyName) IsNot Nothing Then
                    RegistryKeyName.DeleteValue(AssemblyName)
                End If
            End If
        Catch ex As Exception
            'on ne fait rien
        End Try
    End Sub

    'Fonction qui lance un navigateur avec une url demandée
    Private Shared Function OpenInternet(ByRef url As String) As Boolean
        Dim processHttp As New Process
        If String.IsNullOrEmpty(url) = True Then
            Return False
        Else
            If url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) Or url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then
                processHttp.StartInfo.FileName = url
            Else
                processHttp.StartInfo.FileName = "http://" & url
            End If
            processHttp.StartInfo.UseShellExecute = True
            Try
                processHttp.Start()
            Catch ex As Exception
                Return False
            End Try
            Return True
        End If
    End Function

    'Fonction qui convertit un nombre en format Human Readable
    Private Shared Function ConvertToHumanReadable(ByVal value As Double, Optional ByVal diviser As Integer = 1024, Optional ByVal nbDecimal As Integer = 0) As String
        Dim _units() As String = {"", "K", "M", "G", "T", "P", "E", "Z", "Y"}
        Dim size As Double = value
        Dim i As Integer = 0
        While size >= diviser
            size /= diviser
            i += 1
        End While
        Return Math.Round(size, nbDecimal) & _units(i)
    End Function

    'Fonction qui convertit une couleur hexadecimale en couleur
    Private Shared Function HexToColor(ByVal hex As String) As System.Windows.Media.Color
        hex = hex.Replace("#", "")
        Dim r As Byte = 0
        Dim g As Byte = 0
        Dim b As Byte = 0

        If hex.Length = 6 Then '#RRGGBB
            r = Byte.Parse(hex.Substring(0, 2), Globalization.NumberStyles.AllowHexSpecifier)
            g = Byte.Parse(hex.Substring(2, 2), Globalization.NumberStyles.AllowHexSpecifier)
            b = Byte.Parse(hex.Substring(4, 2), Globalization.NumberStyles.AllowHexSpecifier)
        ElseIf hex.Length = 3 Then '#RGB
            r = Byte.Parse(hex(0).ToString() + hex(0).ToString(), Globalization.NumberStyles.AllowHexSpecifier)
            g = Byte.Parse(hex(1).ToString() + hex(1).ToString(), Globalization.NumberStyles.AllowHexSpecifier)
            b = Byte.Parse(hex(2).ToString() + hex(2).ToString(), Globalization.NumberStyles.AllowHexSpecifier)
        End If
        Return System.Windows.Media.Color.FromRgb(r, g, b)
    End Function

#End Region

End Class
