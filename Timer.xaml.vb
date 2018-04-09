Imports System.Windows.Interop

Class MainWindow
    Dim IsSnapped As Boolean

    Dim WindowBeforeSnap As System.Drawing.Rectangle
    Dim WindowSnapBounds As System.Drawing.Rectangle
    Dim CurrentScreenBounds As System.Drawing.Rectangle
    Dim CurrentScreenBoundsDPI As System.Drawing.Rectangle

    Private _TargetTime As TimeSpan
    Dim StartTime As TimeSpan
    Dim ElapsedTime As TimeSpan
    Dim Pause As Boolean

    Dim MousePosOffset As Point
    Dim MousePosPrevious As Point

    Dim dispatcherTimer As New System.Windows.Threading.DispatcherTimer()

    Dim Snap As Integer = 30

    Public Property TargetTime As TimeSpan
        Get
            Return _TargetTime
        End Get
        Set(value As TimeSpan)
            _TargetTime = TimeSpan.FromMilliseconds(value.TotalMilliseconds + 500)

            dispatcherTimer.Stop()
            ElapsedTime = New TimeSpan(0, 0, 0)
            Pause = False

            updateText(TargetTime)

        End Set
    End Property

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        GetCurrentScreenBounds()

        ResizeMode = ResizeMode.CanResizeWithGrip

        AddHandler dispatcherTimer.Tick, AddressOf dispatcherTimer_Tick
        dispatcherTimer.Interval = New TimeSpan(0, 0, 1)

#Region "Context menu"
        Dim CM As New ContextMenu
        Dim CMI As New MenuItem
        Dim SP As New Separator

        CMI = New MenuItem
        CMI.Header = "Definir Tempo..."
        AddHandler(CMI.Click), AddressOf CMDefinirTempo
        CM.Items.Add(CMI)
        SP = New Separator
        CM.Items.Add(SP)

        CMI = New MenuItem
        CMI.Header = "Iniciar"
        AddHandler(CMI.Click), AddressOf CMIniciar
        CM.Items.Add(CMI)
        CMI = New MenuItem
        CMI.Header = "Parar"
        AddHandler(CMI.Click), AddressOf CMParar
        CM.Items.Add(CMI)
        CMI = New MenuItem
        CMI.Header = "Reiniciar"
        AddHandler(CMI.Click), AddressOf CMReiniciar
        CM.Items.Add(CMI)
        SP = New Separator
        CM.Items.Add(SP)

        CMI = New MenuItem
        CMI.Header = "Configurações..."
        AddHandler(CMI.Click), AddressOf CMConfigurações
        CM.Items.Add(CMI)
        CMI = New MenuItem
        CMI.Header = "Sobre"
        AddHandler(CMI.Click), AddressOf CMSobre
        CM.Items.Add(CMI)
        CMI = New MenuItem
        CMI.Header = "Sair"
        AddHandler(CMI.Click), AddressOf CMSair
        CM.Items.Add(CMI)

        Me.ContextMenu = CM
#End Region
    End Sub

    Private Sub dispatcherTimer_Tick(sender As Object, e As EventArgs)

        ElapsedTime = (TimeSpan.FromMilliseconds(Environment.TickCount) - StartTime)
        Dim DisplayTime As TimeSpan = TargetTime - ElapsedTime


        If DisplayTime.TotalSeconds <= 0 Then
            TimeText.Content = "00:00"
            If MySettings.Default.CloseWhenZero Then
                End
            Else
                dispatcherTimer.Stop()
                Pause = True
            End If
        Else
            updateText(DisplayTime)
        End If

    End Sub

    Private Sub MainWindow_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseDown
        If e.ChangedButton = MouseButton.Left Then

            MousePosOffset = Mouse.GetPosition(Me) 'in DPI

            If (Not ResizeMode = ResizeMode.NoResize) Then
                ResizeMode = ResizeMode.NoResize
                UpdateLayout()
            End If

            Me.CaptureMouse()
        End If

        If e.ChangedButton = MouseButton.Right Then
            Me.ContextMenu.IsOpen = True
        End If

    End Sub
    Private Sub MainWindow_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseUp
        If e.ChangedButton = MouseButton.Left Then
            Me.ReleaseMouseCapture()
            If IsSnapped Then
                ResizeMode = ResizeMode.NoResize
            Else
                ResizeMode = ResizeMode.CanResizeWithGrip
            End If
        End If
    End Sub
    Private Sub MainWindow_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        If e.LeftButton = MouseButtonState.Pressed Then 'if we are dragging

            'Get Absolute MousePos
            Dim MousePos As System.Drawing.Point = System.Windows.Forms.Control.MousePosition
            Dim MousePosDPI As System.Drawing.Point = MousePos
            ConvertToUIP(MousePosDPI.X, MousePosDPI.Y)

            'Check what screen we are in, get bounds if different
            If MousePos.X < CurrentScreenBounds.Left Or
                MousePos.X > CurrentScreenBounds.Right Or
                MousePos.Y < CurrentScreenBounds.Top Or
                MousePos.Y > CurrentScreenBounds.Bottom Then
                GetCurrentScreenBounds()
            End If

            'If we are on top snap treshold
            If MousePosDPI.Y <= CurrentScreenBoundsDPI.Top + Snap Then
                IsSnapped = True
                SetBounds(CurrentScreenBoundsDPI.Left, CurrentScreenBoundsDPI.Top, CurrentScreenBoundsDPI.Width, Me.Height)

                'If we are on bottom snap treshold
            ElseIf MousePosDPI.Y >= CurrentScreenBoundsDPI.Bottom - Snap Then
                IsSnapped = True
                SetBounds(CurrentScreenBoundsDPI.Left, CurrentScreenBoundsDPI.Bottom - Me.Height, CurrentScreenBoundsDPI.Width, Me.Height)

                'Just moving along the screen
            Else

                'We were not snapped, so move normaly
                If Not IsSnapped Then
                    SetBounds(MousePosDPI.X - MousePosOffset.X, MousePosDPI.Y - MousePosOffset.Y, Me.Width, Me.Height)
                    WindowBeforeSnap = Me.GetBounds 'In DPI

                    'We were snapped, so set our position to the center of the mouse then move normaly
                Else
                    SetBounds(MousePosDPI.X - WindowBeforeSnap.Width / 2, MousePosDPI.Y - WindowBeforeSnap.Height / 2, WindowBeforeSnap.Width, WindowBeforeSnap.Height)
                    MousePosOffset = New Point(WindowBeforeSnap.Width / 2, WindowBeforeSnap.Height / 2)  'in DPI
                    IsSnapped = False
                End If
            End If

        End If
    End Sub

    Private Sub CMIniciar()
        dispatcherTimer.Start()
        If Pause Then
            StartTime = TimeSpan.FromMilliseconds(Environment.TickCount) - ElapsedTime
        Else
            StartTime = TimeSpan.FromMilliseconds(Environment.TickCount)
        End If
    End Sub
    Private Sub CMParar()
        dispatcherTimer.Stop()
        Pause = True
    End Sub
    Private Sub CMReiniciar()

        updateText(TargetTime)

        If dispatcherTimer.IsEnabled Then
            dispatcherTimer.Stop()
            dispatcherTimer.Start()
        End If

        StartTime = TimeSpan.FromMilliseconds(Environment.TickCount)
        Pause = False
    End Sub
    Private Sub CMDefinirTempo()
        Dim SetTime As New SetTime
        If SetTime.ShowDialog() = True Then
            TargetTime = SetTime.TimeInSeconds
        End If
    End Sub
    Private Sub CMConfigurações()
        Dim configWindows As New Settings
        configWindows.ShowDialog()
    End Sub
    Private Sub CMSair()
        End
    End Sub
    Private Sub CMSobre()
        MsgBox("Programa e Design por Leonardo Bottaro" & vbNewLine &
               "leobottaro@gmail.com" & vbNewLine & vbNewLine &
               "Icones feitos por freepik.com de flaticon.com" & vbNewLine &
               "sob a licença Creative Commons 3.0" & vbNewLine & vbNewLine &
               "Programa de distribuição gratuita. Venda proibida!")
    End Sub

    Private Sub updateText(t As TimeSpan)
        If t.Hours > 0 Then
            TimeText.Content = String.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds)
        Else
            TimeText.Content = String.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds)
        End If
    End Sub

    Public Sub GetCurrentScreenBounds()

        Dim MousePos As System.Drawing.Point = System.Windows.Forms.Control.MousePosition

        For Each s As Forms.Screen In Forms.Screen.AllScreens
            Dim b As System.Drawing.Rectangle = s.Bounds
            If (MousePos.X >= s.Bounds.X And
                MousePos.X <= s.Bounds.Right And
                MousePos.Y >= s.Bounds.Y And
                MousePos.Y <= s.Bounds.Bottom) Then

                CurrentScreenBounds = b

                ConvertToUIP(b.X, b.Y)
                ConvertToUIP(b.Width, b.Height)

                CurrentScreenBoundsDPI = b

            End If
        Next
    End Sub

    Private Sub ConvertToUIP(Optional ByRef X As Integer = 0, Optional ByRef Y As Integer = 0) 'WPF uses Unit(DPI) Independet Pixels by default, screen bounds doesn't, so we have to convert it

        Dim Matrix As Matrix
        Dim source = PresentationSource.FromVisual(Me)
        If (source IsNot Nothing) Then
            Matrix = source.CompositionTarget.TransformToDevice
        Else
            Dim src = New HwndSource(New HwndSourceParameters())
            Matrix = src.CompositionTarget.TransformToDevice
        End If

        X = (X / Matrix.M11)
        Y = (Y / Matrix.M22)
    End Sub
    Private Function GetBounds() As System.Drawing.Rectangle
        Return New System.Drawing.Rectangle(Me.Left, Me.Top, Me.ActualWidth, Me.ActualHeight)
    End Function
    Private Sub SetBounds(X As Double, Y As Double, Width As Double, Height As Double)
        Using Dispatcher.DisableProcessing()
            Me.Left = X
            Me.Top = Y
            Me.Width = Width
            Me.Height = Height
        End Using
    End Sub
    Private Sub SetBounds(Bounds As System.Drawing.Rectangle)
        Using Dispatcher.DisableProcessing()
            Me.Left = Bounds.X
            Me.Top = Bounds.Y
            Me.Width = Bounds.Width
            Me.Height = Bounds.Height
        End Using
    End Sub
    Private Sub SetLocation(X As Double, Y As Double)
        Me.Left = X
        Me.Top = Y
    End Sub
End Class
