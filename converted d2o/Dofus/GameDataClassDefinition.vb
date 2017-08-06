Imports System.Collections.Generic

Namespace D2oReader
    Public Class GameDataClassDefinition
        Public Name As String
        Public Fields As List(Of GameDataField)

        Public Sub New(packageName As String, className As String)
            Fields = New List(Of GameDataField)()
            Name = className
        End Sub

        Friend Sub AddField(reader As D2OReader)
            Dim field As New GameDataField(reader)

            Fields.Add(field)
        End Sub
    End Class
End Namespace
