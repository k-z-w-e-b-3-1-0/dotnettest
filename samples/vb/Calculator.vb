Public Module Calculator
    Public Function Add(a As Integer, b As Integer) As Integer
        Return a + b
    End Function

    Public Function Max(a As Integer, b As Integer, c As Integer) As Integer
        Dim result As Integer = a
        If b > result Then
            result = b
        End If
        If c > result Then
            result = c
        End If
        Return result
    End Function

    Public Function Fibonacci(n As Integer) As Integer
        If n <= 1 Then
            Return n
        End If
        Dim previous As Integer = 0
        Dim current As Integer = 1
        For index As Integer = 2 To n
            Dim nextValue As Integer = previous + current
            previous = current
            current = nextValue
        Next
        Return current
    End Function
End Module
