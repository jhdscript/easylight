Imports System
Imports System.Diagnostics
Imports System.Drawing
Imports System.Reflection
Imports System.Windows
Imports System.Windows.Forms

'''<summary>
'''Class implementing support for "minimize to tray" functionality.
'''</summary>
Public Class MinimizeToTray

    '''<summary>
    '''Class implementing "minimize to tray" functionality for a Window instance.
    '''</summary>
    Public Class MinimizeToTrayInstance
        Private _window As Window
        Private _notifyIcon As NotifyIcon
        Private _balloonShown As Boolean

        '''<summary>
        '''Initializes a new instance of the MinimizeToTrayInstance class.
        '''</summary>
        '''<param name="window">Window instance to attach to.</param>
        ''' 
        Public Sub New(ByRef window As Window)
            _window = window
            AddHandler _window.StateChanged, AddressOf HandleStateChanged
        End Sub


        '''<summary>
        '''Handles the Window's StateChanged event.
        '''</summary>
        ''' 
        Private Sub HandleStateChanged(ByVal sender As Object, ByVal e As EventArgs)
            If _notifyIcon Is Nothing Then
                'Initialize NotifyIcon instance "on demand"
                Call SetSystrayIcon(True)
            End If

            'Update copy of Window Title in case it has changed
            _notifyIcon.Text = _window.Title

            'Show/hide Window and NotifyIcon
            Dim minimized As Boolean = (_window.WindowState = WindowState.Minimized)
            _window.ShowInTaskbar = Not minimized
            _notifyIcon.Visible = True
            If minimized And Not _balloonShown Then
                'If this is the first time minimizing to the tray, show the user what happened
                _notifyIcon.ShowBalloonTip(1000, Nothing, _window.Title, ToolTipIcon.None)
                _balloonShown = True
            End If
        End Sub

        '''<summary>
        '''Set an icon to the systray.
        '''</summary>
        Public Sub SetSystrayIcon(ByVal UseDefaultIcon As Boolean, Optional ByVal resourcePath As String = vbNullString)
            If _notifyIcon Is Nothing Then
                _notifyIcon = New NotifyIcon()
            End If
            If UseDefaultIcon Then
                _notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location)
            Else
                Try
                    _notifyIcon.Icon = New Icon(Application.GetResourceStream(New Uri(resourcePath, UriKind.Relative)).Stream)
                Catch ex As Exception
                    _notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location)
                End Try
            End If
            If _notifyIcon IsNot Nothing Then
                AddHandler _notifyIcon.MouseDoubleClick, AddressOf HandleNotifyMouseDoubleClick
                AddHandler _notifyIcon.MouseDown, AddressOf HandleNotifyMouseDown
            End If
        End Sub

        '''<summary>
        '''Handles a click on the notify icon or its balloon.
        '''</summary>
        Private Sub HandleNotifyMouseDoubleClick(ByVal sender As Object, ByVal e As MouseEventArgs)
            _window.WindowState = WindowState.Normal 'Restore the Window
        End Sub

        '''<summary>
        '''Handles a mouse down.
        '''</summary>
        Private Sub HandleNotifyMouseDown(ByVal sender As Object, ByVal e As MouseEventArgs)
            If e.Button = MouseButtons.Right Then
                Try
                    Dim menu As System.Windows.Controls.ContextMenu = DirectCast(_window.FindResource("NotifierContextMenu"), System.Windows.Controls.ContextMenu)
                    menu.IsOpen = True
                Catch ex As Exception
                    'ne rien faire
                End Try
            End If
        End Sub

        '''<summary>
        '''Show a balloon tooltip.
        '''</summary>
        Public Function ShowBalloon(ByVal title As String, ByVal msg As String, ByVal icon As ToolTipIcon) As Boolean
            If _notifyIcon IsNot Nothing Then
                _notifyIcon.ShowBalloonTip(2000, title, msg, icon)
                _balloonShown = True
                Return True
            Else
                Return False
            End If
        End Function
    End Class

    '''<summary>
    '''Enables "minimize to tray" behavior for the specified Window.
    '''</summary>
    Public Shared Sub Enable(ByRef window As Window)
        'No need to track this instance; its event handlers will keep it alive
        CurrentSystray = New MinimizeToTrayInstance(window)
    End Sub

    '''<summary>
    '''Current MinimizeToTray object for the current application
    '''</summary>
    Public Shared Property CurrentSystray As MinimizeToTrayInstance
End Class
