Public Class OrderProcessor
    Public Function CalculateDiscount(total As Decimal, customerLevel As String) As Decimal
        Dim discount As Decimal = 0D
        Select Case customerLevel
            Case "Platinum"
                discount = 0.2D
            Case "Gold"
                discount = 0.1D
            Case "Silver"
                discount = 0.05D
            Case Else
                discount = 0D
        End Select

        If total > 500D Then
            discount += 0.05D
        ElseIf total > 250D Then
            discount += 0.02D
        End If

        Return total * discount
    End Function

    Public Sub NotifyCustomer(isPreferred As Boolean, hasEmail As Boolean)
        If isPreferred AndAlso hasEmail Then
            SendEmail()
        ElseIf hasEmail Then
            SendStandardEmail()
        Else
            LogMissingEmail()
        End If
    End Sub

    Private Sub SendEmail()
    End Sub

    Private Sub SendStandardEmail()
    End Sub

    Private Sub LogMissingEmail()
    End Sub
End Class
