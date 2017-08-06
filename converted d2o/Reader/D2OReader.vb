
Imports System.Linq
Imports System.Text
Imports System.IO

Namespace D2oReader
    Public Class D2OReader
        Implements IDisposable
        Private reader As BinaryReader

        Public Sub New(input As Stream)
            reader = New BinaryReader(input)
        End Sub

        Public Property Pointer() As Integer
            Get
                Return CInt(reader.BaseStream.Position)
            End Get
            Set
                reader.BaseStream.Position = Value
            End Set
        End Property

        Public ReadOnly Property Length() As Integer
            Get
                Return CInt(reader.BaseStream.Length)
            End Get
        End Property

        Public ReadOnly Property BytesAvailable() As Long
            Get
                Return reader.BaseStream.Length - reader.BaseStream.Position
            End Get
        End Property

        Public Function ReadBool() As Boolean
            Return reader.ReadBoolean()
        End Function
        Public Function ReadInt() As Integer
            Dim int32 As Byte() = reader.ReadBytes(4)

            int32 = int32.Reverse().ToArray()

            Return BitConverter.ToInt32(int32, 0)
        End Function

        Public Function ReadUInt() As UInteger
            Dim uint32 As Byte() = reader.ReadBytes(4)

            uint32 = uint32.Reverse().ToArray()

            Return BitConverter.ToUInt32(uint32, 0)
        End Function

        Public Function ReadShort() As Short
            Dim [short] As Byte() = reader.ReadBytes(2)

            [short] = [short].Reverse().ToArray()

            Return BitConverter.ToInt16([short], 0)
        End Function

        Public Function ReadUShort() As UShort
            Dim [ushort] As Byte() = reader.ReadBytes(2)

            [ushort] = [ushort].Reverse().ToArray()

            Return BitConverter.ToUInt16([ushort], 0)
        End Function

        Public Function ReadBytes(bytesAmount As Integer) As Byte()
            Return reader.ReadBytes(bytesAmount)
        End Function

        Public Function ReadSByte() As SByte
            Return reader.ReadSByte()
        End Function

        Public Function ReadDouble() As Double
            Dim [double] As Byte() = reader.ReadBytes(8)

            [double] = [double].Reverse().ToArray()

            Return BitConverter.ToDouble([double], 0)
        End Function

        Public Function ReadFloat() As Single
            Dim float As Byte() = reader.ReadBytes(4)

            float = float.Reverse().ToArray()

            Return BitConverter.ToSingle(float, 0)
        End Function

        Public Function ReadAscii(bytesAmount As Integer) As String
            Dim buffer As Byte() = reader.ReadBytes(bytesAmount)

            Return Encoding.ASCII.GetString(buffer)
        End Function

        Public Function ReadUtf8() As String
            Dim buffer As Byte()

            Dim len As UShort = ReadUShort()

            buffer = reader.ReadBytes(len)

            Return Encoding.UTF8.GetString(buffer)
        End Function

        Public Sub Dispose()
            reader.Dispose()
        End Sub

        Private Sub IDisposable_Dispose() Implements IDisposable.Dispose
            Throw New NotImplementedException()
        End Sub
    End Class
End Namespace

