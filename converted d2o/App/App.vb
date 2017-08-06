
Imports System.Collections.Generic
Imports System.IO
Imports System.Text

Namespace D2oReader
    Public Class App
        Private classCount As Integer

        Public reader As D2OReader
        Private unpacker As JsonUnpacker
        Private objectPointerTable As Dictionary(Of Integer, Integer)
        Private classDefinitions As Dictionary(Of Integer, GameDataClassDefinition)
        Private contentOffset As Integer = 0
        'or uint?
        Public Sub New(d2oFilePath As String)
            objectPointerTable = New Dictionary(Of Integer, Integer)()
            classDefinitions = New Dictionary(Of Integer, GameDataClassDefinition)()

            Using d2oFile As FileStream = File.Open(d2oFilePath, FileMode.Open)
                reader = New D2OReader(d2oFile)
                Dim headerString As String = reader.ReadAscii(3)

                    If Not headerString.Equals("D2O") Then
                        reader.Pointer = 0
                        Dim headers As String = reader.ReadUtf8()
                        Dim formatVersion As Short = reader.ReadShort()
                        Dim len As Integer = reader.ReadInt()
                        reader.Pointer = reader.Pointer + len
                        contentOffset = reader.Pointer
                        Dim streamStartIndex As Integer = (contentOffset + 7)
                        'or uint?
                        headers = reader.ReadAscii(3)
                        If Not headers.Equals("D2O") Then
                            Throw New InvalidDataException("Header doesn't equal the string 'D2O' : Corrupted file")
                        End If
                    End If

                readObjectPointerTable()
                'printObjectPointerTable();
                readClassTable()
                printClassTable()
                readGameDataProcessor()
                'TODO: implement
                unpackObjectsAsJson()
                writeJsonFile(True)
                    'printAllObjects(); //call after  unpackObjectsAsJson(); 
                    'call after  unpackObjectsAsJson(); 

                    searchObjectById()
                End Using
            reader.Dispose()
        End Sub

        Private Sub writeJsonFile(shouldWrite As Boolean)
            If shouldWrite Then
                'unpacker.WriteJson();
                unpacker.WriteIndentedJson()
            End If
        End Sub

        Private Sub printAllObjects()
            For Each objectPointer In objectPointerTable
                Console.WriteLine("Class {0}, Object Id {1}:", classDefinitions(getClassId(objectPointer.Key)).Name, objectPointer.Key)
                Console.WriteLine(unpacker.getObjectJsonString(objectPointer.Key))
                Console.WriteLine("Press any key to continue . . .")
                Console.ReadKey()
            Next
        End Sub

        Private Sub unpackObjectsAsJson()
            unpacker = New JsonUnpacker(reader, objectPointerTable, classDefinitions)

            unpacker.Unpack()
        End Sub

        Private Function getClassId(objectId As Integer) As Integer
            Dim objectPointer As Integer = objectPointerTable(objectId)
            reader.Pointer = objectPointer

            Return reader.ReadInt()
        End Function

        Private Sub searchObjectById()
            Dim objectId As Integer
            Do
                Console.Write("Search object id: ")
                objectId = Int32.Parse(Console.ReadLine())

                If objectPointerTable.ContainsKey(objectId) Then
                    Console.WriteLine("Class {0}, Object Id {1}:", classDefinitions(getClassId(objectId)).Name, objectId)
                    Console.WriteLine(unpacker.getObjectJsonString(objectId))
                Else
                    Console.WriteLine("Object of id: {0} is not present.", objectId)

                End If
            Loop While objectId <> 0
        End Sub

        Private Sub printClassTable()
            If classDefinitions.Count > 0 Then
                Console.WriteLine("Printing {0} class tables.", classDefinitions.Count)
                Console.WriteLine()
                For Each classDefinition In classDefinitions
                    Console.WriteLine("Class id:{0} - name {1}", classDefinition.Key, classDefinition.Value.Name)
                    Console.WriteLine()

                    For Each field As GameDataField In classDefinition.Value.Fields
                        printField(getFieldString(field))
                    Next
                    Console.WriteLine()
                Next
            End If
        End Sub

        Private Function getFieldString(field As GameDataField) As String
            Dim fieldBuilder As New StringBuilder()

            fieldBuilder.Append("public").Append(" ").Append(getFieldTypeString(field)).Append(" ").Append(getFieldNameString(field))

            Return fieldBuilder.ToString()
        End Function

        Private Sub printField(fieldString As String)
            Console.WriteLine(fieldString)
        End Sub

        Private Function getFieldTypeString(field As GameDataField) As String
            If isPrimitiveFieldType(field) Then
                Return getPrimitiveFieldTypeString(field)
            Else
                Return getCompositeFieldTypeString(field)
            End If
        End Function

        Private Function getCompositeFieldTypeString(field As GameDataField) As String
            Dim compositeFieldTypeBuilder As New StringBuilder()

            compositeFieldTypeBuilder.Append("vector").Append("<").Append(getFieldTypeString(field.innerField)).Append(">")

            Return compositeFieldTypeBuilder.ToString()
        End Function

        Private Function getPrimitiveFieldTypeString(field As GameDataField) As String
            Return If(field.fieldType > 0, classDefinitions(CInt(field.fieldType)).Name, field.fieldType.ToString())
        End Function

        Private Function getFieldNameString(field As GameDataField) As String
            Return field.fieldName
        End Function

        Private Shared Function isPrimitiveFieldType(field As GameDataField) As Boolean
            Return field.innerField Is Nothing
        End Function

        Private Sub readGameDataProcessor()
            'GameDataProcess(stream);
            If reader.BytesAvailable > 0 Then
            End If
        End Sub

        Private Sub readClassTable()
            classCount = reader.ReadInt()
            Dim classId As Integer

            Dim j As Integer = 0
            While j < classCount
                classId = reader.ReadInt()
                readClassDefinition(classId)

                j += 1
            End While
        End Sub

        Private Sub readClassDefinition(classId As Integer)
            Dim className As String = reader.ReadUtf8()
            Dim packageName As String = reader.ReadUtf8()
            Dim classDefinition As New GameDataClassDefinition(packageName, className)
            Console.WriteLine("ClassId: {0} ClassMemberName: {1} ClassPkgName {2}", classId, className, packageName)
            Dim fieldsCount As Integer = reader.ReadInt()
            Dim i As UInteger = 0
            While i < fieldsCount
                classDefinition.AddField(reader)
                i += 1
            End While
            classDefinitions.Add(classId, classDefinition)
        End Sub

        Private Sub printObjectPointerTable()
            If objectPointerTable.Count > 0 Then
                For Each objectPointer In objectPointerTable
                    Console.WriteLine("{0}: {1}", objectPointer.Key, objectPointer.Value)
                Next
            End If
        End Sub

        Private Sub readObjectPointerTable()
            Dim tablePointer As Integer = reader.ReadInt()
            reader.Pointer = tablePointer + contentOffset

            Dim objectPointerTableLen As Integer = reader.ReadInt()

            Dim key As Integer
            Dim pointer As Integer

            Dim i As UInteger = 0
            While i < objectPointerTableLen
                key = reader.ReadInt()
                pointer = reader.ReadInt()

                objectPointerTable.Add(key, pointer + contentOffset)
                i += 4 * 2
            End While
        End Sub
    End Class
End Namespace

