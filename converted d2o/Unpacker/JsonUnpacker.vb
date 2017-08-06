
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text

Namespace D2oReader
    Public Class JsonUnpacker
        Private jsonBuilder As StringBuilder
        Private unpackedJson As JToken
        Private objectPointerTable As Dictionary(Of Integer, Integer)
        Private reader As D2oReader
        Private classDefinitions As Dictionary(Of Integer, GameDataClassDefinition)

        Public Sub New(reader As D2oReader, objectPointerTable As Dictionary(Of Integer, Integer), classDefinitions As Dictionary(Of Integer, GameDataClassDefinition))
            Me.reader = reader
            Me.objectPointerTable = objectPointerTable
            Me.classDefinitions = classDefinitions

            jsonBuilder = New StringBuilder()
        End Sub

        Public Sub Unpack()
            buildJsonString()
        End Sub

        Public Sub WriteIndentedJson()
            If IsValidJson Then
                Using writer As TextWriter = New StreamWriter("output.json")
                    writer.Write(unpackedJson.ToString(Formatting.Indented))
                End Using
            End If
        End Sub

        Public Sub WriteJson()
            If IsValidJson Then
                Using writer As TextWriter = New StreamWriter("output.json")
                    writer.Write(unpackedJson.ToString(Formatting.None))
                End Using
            End If
        End Sub

        Private Sub buildJsonString()
            addArrayOpenBracket()
            addObjects()
            addArrayCloseBracket()
        End Sub

        Public ReadOnly Property IsValidJson() As Boolean
            Get
                Try
                    unpackedJson = JToken.Parse(jsonBuilder.ToString())
                    Return True
                Catch exception As JsonReaderException
                    Console.WriteLine("Json output is invalid:")
                    Console.WriteLine(exception.Message)
                    Return False
                End Try
            End Get
        End Property


        Private Sub addObjects()
            Dim indexTableKeys As Integer() = objectPointerTable.Keys.ToArray()

            For i As Integer = 0 To indexTableKeys.Length - 1
                jsonBuilder.Append(getObjectJsonString(indexTableKeys(i))).Append(writeCommaIfHasMore(indexTableKeys.Length, i)).AppendLine()
            Next
        End Sub
        Private Shared Function writeCommaIfHasMore(count As Integer, i As Integer) As String
            If hasMoreElement(count, i) Then
                Return ","
            Else
                Return [String].Empty
            End If
        End Function

        Private Shared Function hasMoreElement(count As Integer, i As Integer) As Boolean
            Return i <> count - 1
        End Function

        Public Function getObjectJsonString(objectId As Integer) As String
            Dim objectPointer As Integer = objectPointerTable(objectId)
            reader.Pointer = objectPointer

            Dim objectClassId As Integer = reader.ReadInt()

            Dim objectBuilder As New StringBuilder()

            objectBuilder.Append(getObjectBuilder(objectClassId))

            Return objectBuilder.ToString()
        End Function

        Private Function getObjectBuilder(classId As Integer) As String
            Dim classDefinition As GameDataClassDefinition = classDefinitions(classId)
            Return getFieldsBuilder(classDefinition)
        End Function
        Private Function getFieldsBuilder(classDefinition As GameDataClassDefinition) As String
            Dim fieldsBuilder As New StringBuilder()
            Dim numberOfFields As Integer = classDefinition.Fields.Count
            fieldsBuilder.AppendLine("{")
            For i As Integer = 0 To numberOfFields - 1
                fieldsBuilder.Append(getFieldBuilder(classDefinition.Fields(i))).Append(writeCommaIfHasMore(numberOfFields, i)).AppendLine()
            Next
            fieldsBuilder.Append("}")

            Return fieldsBuilder.ToString()
        End Function
        Private Function getFieldBuilder(field As GameDataField) As String
            Dim fieldBuilder As New StringBuilder()

            fieldBuilder.Append(JsonConvert.ToString(field.fieldName)).Append(": ").Append(getFieldValueBuilder(field))

            Return fieldBuilder.ToString()
        End Function
        Private Function getFieldValueBuilder(field As GameDataField) As String
            Dim fieldValueBuilder As New StringBuilder()

            Select Case field.fieldType
                Case GameDataTypeEnum.Vector
                    fieldValueBuilder.Append("[")
                    Dim vectorLength As Integer = reader.ReadInt()

                    For i As Integer = 0 To vectorLength - 1
                        fieldValueBuilder.Append(getFieldValueBuilder(field.innerField)).Append(writeCommaIfHasMore(vectorLength, i))
                    Next

                    fieldValueBuilder.Append("]")
                    Exit Select
                Case GameDataTypeEnum.Int
                    fieldValueBuilder.Append(reader.ReadInt())
                    Exit Select
                Case GameDataTypeEnum.UInt
                    fieldValueBuilder.Append(reader.ReadUInt())
                    Exit Select
                Case GameDataTypeEnum.I18N
                    fieldValueBuilder.Append(reader.ReadInt())
                    Exit Select
                Case GameDataTypeEnum.[String]
                    fieldValueBuilder.Append(JsonConvert.ToString(reader.ReadUtf8()))
                    Exit Select
                Case GameDataTypeEnum.Bool
                    fieldValueBuilder.Append(JsonConvert.ToString(reader.ReadBool()))
                    'in json bool is true/false not True/False
                    Exit Select
                Case GameDataTypeEnum.[Double]
                    fieldValueBuilder.Append(JsonConvert.ToString(reader.ReadDouble()))
                    'handling the "," vs "." problem of the culture specifics
                    Exit Select
                Case Else
                    If field.fieldType > 0 Then
                        'if type is an object
                        Dim classId As Integer = reader.ReadInt()
                        If classDefinitions.ContainsKey(classId) Then
                            fieldValueBuilder.Append(getObjectBuilder(classId))
                        End If
                    Else
                        Console.WriteLine("Error: invalid type( {0} ) for field {1}", field.fieldType, field.fieldName)
                    End If
                    Exit Select
            End Select
            Return fieldValueBuilder.ToString()
        End Function
        Private Sub addArrayCloseBracket()
            jsonBuilder.Append("]")
        End Sub

        Private Sub addArrayOpenBracket()
            jsonBuilder.Append("[")
        End Sub
    End Class
End Namespace

