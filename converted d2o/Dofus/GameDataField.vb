Namespace D2oReader
    Public Class GameDataField
        Public fieldName As String
        Public fieldType As GameDataTypeEnum
        Public innerField As GameDataField

        Public Sub New(reader As D2OReader)
            fieldName = reader.ReadUtf8()

            readType(reader)
        End Sub

        Public Sub readType(reader As D2OReader)
            Dim fieldType As GameDataTypeEnum = DirectCast(reader.ReadInt(), GameDataTypeEnum)
            Me.fieldType = fieldType

            If fieldType = GameDataTypeEnum.Vector Then
                innerField = New GameDataField(reader)
            End If
        End Sub
    End Class
End Namespace