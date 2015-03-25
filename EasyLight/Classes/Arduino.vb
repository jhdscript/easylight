Public Class Arduino

#Region "Properties"
    Private WithEvents CurrentSerialPort As System.IO.Ports.SerialPort
    Private Property ComPort As String = "COM8"
    Private Property BaudRate As Integer = 9600
    Private WithEvents WatchDogTimer As New System.Timers.Timer(5000) With {.Enabled = True}

    Private Receiving As Boolean
    Private BufferPointer As Integer
    Private CBuffer(30) As Byte
#End Region

#Region "Event"
    Public Event WatchdogReceived() 'Fires when a watchdog message (heartbeat) is received
    Public Event ConnectionLost() 'Fires when there has not been a watchdog message for 5 seconds
    Public Event LogMessageReceived(ByVal Message As String) 'Gives a system message from the Arduino
#End Region

#Region "Constructeur"
    Public Sub New(ByVal _comport As String, ByVal _baudrate As Integer)
        Me.ComPort = Trim(_comport)
        Me.BaudRate = _baudrate
    End Sub
#End Region

#Region "Connection / Start / Stop"

    'Fonction qui démarre la connection
    Public Sub Start()
        Call StartCommunication()
    End Sub

    'Fonction qui stoppe la connection
    Public Sub [Stop]()
        CurrentSerialPort.Close()
        WatchDogTimer.Stop()
    End Sub

    'Fonction de connection au arduino
    Private Function StartCommunication() As Boolean
        Dim ret As Boolean = False
        Try
            Dim components As System.ComponentModel.IContainer = New System.ComponentModel.Container()
            CurrentSerialPort = New System.IO.Ports.SerialPort(components)
            CurrentSerialPort.PortName = _ComPort
            CurrentSerialPort.BaudRate = _BaudRate
            CurrentSerialPort.ReceivedBytesThreshold = 1
            CurrentSerialPort.Open()

            If Not CurrentSerialPort.IsOpen Then
                WriteLog("[ERROR]" & vbTab & "[" & System.Reflection.MethodBase.GetCurrentMethod.Name & "]" & vbTab & "Unable to open port com...")
                Return ret
            Else
                CurrentSerialPort.DtrEnable = True
                WriteLog("[INFO]" & vbTab & "[" & System.Reflection.MethodBase.GetCurrentMethod.Name & "]" & vbTab & "Serial port is open")
                System.Threading.Thread.Sleep(1000)

                'on envoie une commande pour nettoyer le buffer du arduino
                Dim Command As Byte() = {40, 0, 0, 0, 41, 0}
                Me.SendCommand(Command)
                Me.SendCommand(Command)
                WatchDogTimer.Start()
                ret = True
            End If

            'On ajoute un handler pour récupérer les infos envoyées par le arduino
            AddHandler CurrentSerialPort.DataReceived, AddressOf OnReceived
        Catch ex As Exception
            WriteLog("[ERROR]" & vbTab & "[" & System.Reflection.MethodBase.GetCurrentMethod.Name & "]" & vbTab & "Error opening port com...")
            ret = False
        End Try
        Return ret
    End Function

#End Region

#Region "Reception d'une commande"
    'Fonction de traitements des données reçues (données émises par le arduino
    Private Sub OnReceived(ByVal sender As Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs)
        If Receiving = False Then
            Receiving = True
            Try
                Dim BytesRead As Integer
                While CurrentSerialPort.BytesToRead > 0
                    BytesRead = CurrentSerialPort.Read(CBuffer, BufferPointer, CurrentSerialPort.BytesToRead)
                    BufferPointer += BytesRead
                    While BufferPointer > 0
                        'on recherche le début de commande (
                        Dim CommandStart As Integer = -1
                        For i As Integer = 0 To BufferPointer
                            If CBuffer(i) = CByte(40) Then
                                CommandStart = i
                                Exit For
                            End If
                        Next

                        'si aucun debut de commande n'a été trouvé, on nettoie le buffer car la commande est invalide
                        If CommandStart = -1 Then
                            ClearCBuffer()
                        End If

                        'si la commande ne commence pas au premier byte alors on supprime les bytes avant la commande
                        If CommandStart > 0 Then
                            LeftShiftCBuffer(CommandStart)
                        End If

                        'a partir d'ici le buffer est clean

                        'on recherche la fin de commande )
                        Dim Commandend As Integer = 0
                        For i As Integer = 0 To BufferPointer
                            If CBuffer(i) = CByte(41) Then
                                Commandend = i
                                Exit For
                            End If
                        Next

                        'si la fin de commande a été trouvée alors on execute la commande
                        If Commandend > 0 Then
                            Dim CommandBytes(Commandend) As Byte
                            For i As Integer = 0 To Commandend
                                CommandBytes(i) = CBuffer(i)
                            Next
                            'on execute la commande
                            ProcessCommand(CommandBytes)

                            'on reset le buffer et le pointer
                            LeftShiftCBuffer(Commandend + 1)
                        Else
                            Exit While
                        End If
                    End While
                End While
            Catch ex As Exception
            End Try
            Receiving = False
        End If
    End Sub

    'Fonction qui nettoie le buffer de réception
    Private Sub ClearCBuffer()
        For i As Integer = 0 To CBuffer.Length - 1
            CBuffer(i) = 0
        Next
        BufferPointer = 0
    End Sub

    'Fonction qui shift le buffer vers la gauche
    Private Sub LeftShiftCBuffer(ByVal NrOfPlaces As Integer)
        For i As Integer = NrOfPlaces To CBuffer.Length - 1
            CBuffer(i - NrOfPlaces) = CBuffer(i)
        Next
        For i As Integer = CBuffer.Length - NrOfPlaces To CBuffer.Length - 1
            CBuffer(i) = 0
        Next
        BufferPointer -= NrOfPlaces
    End Sub

    'Fonction de traitement des commandes reçues
    Private Sub ProcessCommand(ByVal CommandBytes As Byte())
        Try
            If ((CommandBytes(0) = CByte(40)) And (CommandBytes(CommandBytes.Length - 1) = CByte(41))) Then 'toutes les commandes sont au format (cmd)
                Dim PType As Char = ChrW(CommandBytes(1))
                Select Case PType
                    Case CChar("S") 'Arduino send it started
                        RaiseEvent WatchdogReceived()
                        If Not IsNothing(WatchDogTimer) Then
                            WatchDogTimer.Stop()
                        End If
                        WatchDogTimer.Start()

                    Case CChar("W") 'Arduino send watchdog
                        RaiseEvent WatchdogReceived()
                        If Not IsNothing(WatchDogTimer) Then
                            WatchDogTimer.Stop()
                        End If
                        WatchDogTimer.Start()

                    Case Else
                        Dim CommandString As String = String.Empty
                        For i As Integer = 0 To CommandBytes.Length - 1
                            CommandString += CommandBytes(i).ToString + " "
                        Next
                        WriteLog("[MSG]" & vbTab & "[" & System.Reflection.MethodBase.GetCurrentMethod.Name & "]" & vbTab & CommandString)
                End Select
            Else
                Dim CommandString As String = String.Empty
                For i As Integer = 1 To CommandBytes.Length - 1
                    CommandString += CommandBytes(i).ToString
                Next
                WriteLog("[ERROR]" & vbTab & "[" & System.Reflection.MethodBase.GetCurrentMethod.Name & "]" & vbTab & "Bad command format received: " & CommandString)
            End If
        Catch ex As Exception
            WriteLog("[ERROR]" & vbTab & "[" & System.Reflection.MethodBase.GetCurrentMethod.Name & "]" & vbTab & "Bad command format received")
        End Try
    End Sub
#End Region

#Region "Emission d'une commande"
    'Fonction qui envoie une commande
    Public Sub SendCommand(ByVal Command As Byte())
        Try
            If Command.Length > 0 Then
                If CurrentSerialPort.IsOpen Then
                    CurrentSerialPort.Write(Command, 0, Command.Length - 1)
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub
#End Region

#Region "Fonctions Diverses"
    'Fonction qui permet de savoir lorsqu'on n'a pas reçu de signal du arduino depuis 5 secondes
    Private Sub WatchdogTimerElaped(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles WatchDogTimer.Elapsed
        WriteLog("[ERROR]" & vbTab & "[" & System.Reflection.MethodBase.GetCurrentMethod.Name & "]" & vbTab & "Connection lost...")
        RaiseEvent ConnectionLost()
        If Not IsNothing(WatchDogTimer) Then
            WatchDogTimer.Stop()
        End If
    End Sub

    'Fonction de notification de message
    Private Sub WriteLog(ByVal Message As String)
        RaiseEvent LogMessageReceived(Message)
    End Sub
#End Region

End Class