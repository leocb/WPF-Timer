Public Class Settings
    Private Sub OKBtn_Click(sender As Object, e As RoutedEventArgs) Handles OKBtn.Click
        MySettings.Default.Save()
        DialogResult = True
        Me.Close()
    End Sub
End Class
