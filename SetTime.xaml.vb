Public Class SetTime
    Private _timeInSeconds As TimeSpan

    Public Property TimeInSeconds As TimeSpan
        Get
            Return _timeInSeconds
        End Get
        Set(value As TimeSpan)
            _timeInSeconds = value
        End Set
    End Property

    Private Sub button_Click(sender As Object, e As RoutedEventArgs) Handles button.Click
        If System.Windows.Interop.ComponentDispatcher.IsThreadModal Then
            TimeInSeconds = New TimeSpan(TimePicker.Value.Value.Hour, TimePicker.Value.Value.Minute, TimePicker.Value.Value.Second)
            DialogResult = True
            Close()
        Else
            Dim MainWindow As New MainWindow
            MainWindow.TargetTime = New TimeSpan(TimePicker.Value.Value.Hour, TimePicker.Value.Value.Minute, TimePicker.Value.Value.Second)
            MainWindow.Show()
            Me.Close()
        End If
    End Sub
End Class
