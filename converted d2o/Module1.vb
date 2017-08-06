
Imports System.Diagnostics
Imports converted_d2o.D2oReader

Module Module1

    Sub Main()
        Try
            Dim temp As String = Nothing
            temp = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\Items.d2o"
            Dim tester As New App(temp)

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Console.ReadKey()
        End Try
    End Sub

End Module


