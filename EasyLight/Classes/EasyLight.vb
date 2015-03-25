'Imports System.Drawing.Imaging
Imports SlimDX.Direct3D9
Imports SlimDX
Imports System.Windows.Forms

Public Class EasyLight
#Region "Enums"
    Public Enum EnumMethod
        MAX
        AVERAGE
    End Enum

    Public Enum EnumLayer
        L4XH2 = 0
        L3XH2 = 1
        L3XH1 = 2
        L2XH2 = 3
    End Enum
#End Region

#Region "Properties"
    'Contient les coordonnées de chaque group

    Private DicoPos As New Dictionary(Of Integer, List(Of Integer))
    Private Proportional As Integer = 3
    Private Layer As EnumLayer = EnumLayer.L4XH2
    Private Method As EnumMethod = EnumMethod.MAX
    Private Hscan As Integer = 25
    Private Vscan As Integer = 25
    Private SkipDarkPixel As Boolean = True
    Private Const _DarkPixelLimit As Integer = 50

    Private Width As Integer = Screen.PrimaryScreen.Bounds.Width
    Private Height As Integer = Screen.PrimaryScreen.Bounds.Height
#End Region

#Region "Constructeurs"
    'Contructeur qui va créer le DicoLines
    Public Sub New(ByVal _proportional As Integer, ByVal _method As EnumMethod, ByVal _layer As EnumLayer, ByVal _hscan As Integer, ByVal _vscan As Integer, ByVal _skipdarkpixel As Boolean)
        Me.Proportional = _proportional
        Me.Method = _method
        Me.Layer = _layer
        Me.Hscan = _hscan
        Me.Vscan = _vscan
        Me.SkipDarkPixel = _skipdarkpixel
        Call MakeLines()
    End Sub
#End Region

#Region "Fonctions Principales"
    'Fonction qui genere le gabarit des lines
    Private Sub MakeLines()
        Dim surf As Surface = DxCapture.Capture()
        Dim DicoLines As New Dictionary(Of Integer, Coords)

        Select Case Me.Layer
            Case EnumLayer.L2XH2
                '-------------------------
                '|     0     |     1     |
                '-------------------------
                '|2|                   |3|
                '-------------------------
                '|4|                   |5|
                '-------------------------
                '|     6     |    7      |
                '-------------------------
                Dim heightbump As Integer = CInt(height * Me.Vscan / 100)
                Dim y0 As Integer = 0
                Dim y1 As Integer = heightbump
                Dim y3 As Integer = height - heightbump
                Dim y4 As Integer = height
                Dim y2 As Integer = CInt(y3 - ((y3 - y1) / 2))

                Dim widthbump As Integer = CInt(width * Me.Hscan / 100)
                Dim x0 As Integer = 0
                Dim x1 As Integer = widthbump
                Dim x2 As Integer = width - widthbump
                Dim x3 As Integer = width

                Dim sizeX As Integer = width \ 2

                For i As Integer = 0 To 7
                    Dim line As New Coords
                    Select Case i
                        Case 0
                            line.X1 = x0 : line.Y1 = y0
                            line.X2 = sizeX : line.Y2 = y1
                        Case 1
                            line.X1 = sizeX : line.Y1 = y0
                            line.X2 = x3 : line.Y2 = y1
                        Case 2
                            line.X1 = x0 : line.Y1 = y1
                            line.X2 = x1 : line.Y2 = y2
                        Case 3
                            line.X1 = x2 : line.Y1 = y1
                            line.X2 = x3 : line.Y2 = y2
                        Case 4
                            line.X1 = x0 : line.Y1 = y2
                            line.X2 = x1 : line.Y2 = y3
                        Case 5
                            line.X1 = x2 : line.Y1 = y2
                            line.X2 = x3 : line.Y2 = y3
                        Case 6
                            line.X1 = x0 : line.Y1 = y3
                            line.X2 = sizeX : line.Y2 = y4
                        Case 7
                            line.X1 = sizeX : line.Y1 = y3
                            line.X2 = x3 : line.Y2 = y4
                    End Select
                    DicoLines(i) = line
                Next

            Case EnumLayer.L3XH1
                '-------------------------
                '|   0   |   1   |   2   |
                '-------------------------
                '| |                   | |
                '|3|                   |4|
                '| |                   | |
                '-------------------------
                '|   5   |   6   |   7   |
                '-------------------------
                Dim heightbump As Integer = CInt(height * Me.Vscan / 100)
                Dim y0 As Integer = 0
                Dim y1 As Integer = heightbump
                Dim y2 As Integer = height - heightbump
                Dim y3 As Integer = height

                Dim widthbump As Integer = CInt(width * Me.Hscan / 100)
                Dim x0 As Integer = 0
                Dim x1 As Integer = widthbump
                Dim x2 As Integer = width - widthbump
                Dim x3 As Integer = width

                Dim sizeX As Integer = width \ 3

                For i As Integer = 0 To 7
                    Dim line As New Coords
                    Select Case i
                        Case 0
                            line.X1 = x0 : line.Y1 = y0
                            line.X2 = sizeX : line.Y2 = y1
                        Case 1
                            line.X1 = sizeX : line.Y1 = y0
                            line.X2 = (sizeX * 2) : line.Y2 = y1
                        Case 2
                            line.X1 = (sizeX * 2) : line.Y1 = y0
                            line.X2 = x3 : line.Y2 = y1
                        Case 3
                            line.X1 = x0 : line.Y1 = y1
                            line.X2 = x1 : line.Y2 = y2
                        Case 4
                            line.X1 = x2 : line.Y1 = y1
                            line.X2 = x3 : line.Y2 = y2
                        Case 5
                            line.X1 = x0 : line.Y1 = y2
                            line.X2 = sizeX : line.Y2 = y3
                        Case 6
                            line.X1 = sizeX : line.Y1 = y2
                            line.X2 = (sizeX * 2) : line.Y2 = y3
                        Case 7
                            line.X1 = (sizeX * 2) : line.Y1 = y2
                            line.X2 = x3 : line.Y2 = y3
                    End Select
                    DicoLines(i) = line
                Next

            Case EnumLayer.L3XH2
                '-------------------------
                '|   0   |   1   |   2   |
                '-------------------------
                '|3|                   |4|
                '-------------------------
                '|5|                   |6|
                '-------------------------
                '|   7   |   8   |   9   |
                '-------------------------
                Dim heightbump As Integer = CInt(height * Me.Vscan / 100)
                Dim y0 As Integer = 0
                Dim y1 As Integer = heightbump
                Dim y3 As Integer = height - heightbump
                Dim y4 As Integer = height
                Dim y2 As Integer = CInt(y3 - ((y3 - y1) / 2))

                Dim widthbump As Integer = CInt(width * Me.Hscan / 100)
                Dim x0 As Integer = 0
                Dim x1 As Integer = widthbump
                Dim x2 As Integer = width - widthbump
                Dim x3 As Integer = width

                Dim sizeX As Integer = width \ 3

                For i As Integer = 0 To 9
                    Dim line As New Coords
                    Select Case i
                        Case 0
                            line.X1 = x0 : line.Y1 = y0
                            line.X2 = sizeX : line.Y2 = y1
                        Case 1
                            line.X1 = sizeX : line.Y1 = y0
                            line.X2 = (sizeX * 2) : line.Y2 = y1
                        Case 2
                            line.X1 = (sizeX * 2) : line.Y1 = y0
                            line.X2 = x3 : line.Y2 = y1
                        Case 3
                            line.X1 = x0 : line.Y1 = y1
                            line.X2 = x1 : line.Y2 = y2
                        Case 4
                            line.X1 = x2 : line.Y1 = y1
                            line.X2 = x3 : line.Y2 = y2
                        Case 5
                            line.X1 = x0 : line.Y1 = y2
                            line.X2 = x1 : line.Y2 = y3
                        Case 6
                            line.X1 = x2 : line.Y1 = y2
                            line.X2 = x3 : line.Y2 = y3
                        Case 7
                            line.X1 = x0 : line.Y1 = y3
                            line.X2 = sizeX : line.Y2 = y4
                        Case 8
                            line.X1 = sizeX : line.Y1 = y3
                            line.X2 = (sizeX * 2) : line.Y2 = y4
                        Case 9
                            line.X1 = (sizeX * 2) : line.Y1 = y3
                            line.X2 = x3 : line.Y2 = y4
                    End Select
                    DicoLines(i) = line
                Next

            Case EnumLayer.L4XH2
                '-------------------------
                '|  0  |  1  |  2  |  3  |
                '-------------------------
                '|4|                   |5|
                '-------------------------
                '|6|                   |7|
                '-------------------------
                '|  8  |  9  | 10  | 11  |
                '-------------------------
                Dim heightbump As Integer = CInt(height * Me.Vscan / 100)
                Dim y0 As Integer = 0
                Dim y1 As Integer = heightbump
                Dim y3 As Integer = height - heightbump
                Dim y4 As Integer = height
                Dim y2 As Integer = CInt(y3 - ((y3 - y1) / 2))

                Dim widthbump As Integer = CInt(width * Me.Hscan / 100)
                Dim x0 As Integer = 0
                Dim x1 As Integer = widthbump
                Dim x3 As Integer = width - widthbump
                Dim x4 As Integer = width
                Dim x2 As Integer = CInt(x3 - ((x3 - x1) / 2))

                Dim sizeX As Integer = width \ 4

                For i As Integer = 0 To 11
                    Dim line As New Coords
                    Select Case i
                        Case 0
                            line.X1 = x0 : line.Y1 = y0
                            line.X2 = sizeX : line.Y2 = y1
                        Case 1
                            line.X1 = sizeX : line.Y1 = y0
                            line.X2 = (sizeX * 2) : line.Y2 = y1
                        Case 2
                            line.X1 = (sizeX * 2) : line.Y1 = 0
                            line.X2 = (sizeX * 3) : line.Y2 = y1
                        Case 3
                            line.X1 = (sizeX * 3) : line.Y1 = y0
                            line.X2 = x4 : line.Y2 = y1
                        Case 4
                            line.X1 = x0 : line.Y1 = y1
                            line.X2 = x1 : line.Y2 = y2
                        Case 5
                            line.X1 = x3 : line.Y1 = y1
                            line.X2 = x4 : line.Y2 = y2
                        Case 6
                            line.X1 = x0 : line.Y1 = y2
                            line.X2 = x1 : line.Y2 = y3
                        Case 7
                            line.X1 = x3 : line.Y1 = y2
                            line.X2 = x4 : line.Y2 = y3
                        Case 8
                            line.X1 = x0 : line.Y1 = y3
                            line.X2 = sizeX : line.Y2 = y4
                        Case 9
                            line.X1 = sizeX : line.Y1 = y3
                            line.X2 = (sizeX * 2) : line.Y2 = y4
                        Case 10
                            line.X1 = (sizeX * 2) : line.Y1 = y3
                            line.X2 = (sizeX * 3) : line.Y2 = y4
                        Case 11
                            line.X1 = (sizeX * 3) : line.Y1 = y3
                            line.X2 = x4 : line.Y2 = y4
                    End Select
                    DicoLines(i) = line
                Next
        End Select

        'on calcule les positions
        Const _bpp As Integer = 4
        For Each itm As KeyValuePair(Of Integer, Coords) In DicoLines

            Dim tmpList As New List(Of Integer)
            For i As Integer = CInt(itm.Value.X1) To CInt(itm.Value.X2) - 1 Step Proportional
                For j As Integer = CInt(itm.Value.Y1) To CInt(itm.Value.Y2) - 1 Step Proportional
                    Dim pos As Integer = ((j * Width) + i) * _bpp
                    tmpList.Add(pos)
                Next
            Next

            DicoPos(itm.Key) = tmpList
        Next

    End Sub


    'Fonction qui analyse les pixels
    Public Function AnalysePixels() As Dictionary(Of Integer, System.Windows.Media.Color)
        Dim s As New Stopwatch
        s.Start()

        Dim ret As New Dictionary(Of Integer, System.Windows.Media.Color)
        Dim surf As Surface = DxCapture.Capture()
        If surf IsNot Nothing Then
            Dim dr As DataRectangle = surf.LockRectangle(LockFlags.None)
            Dim gs As DataStream = dr.Data

            Dim b(3) As Byte

            Parallel.ForEach(DicoPos, Sub(itm)
                                          'on lit les datas
                                          Dim tmpListR As New List(Of Integer)
                                          Dim tmpListG As New List(Of Integer)
                                          Dim tmpListB As New List(Of Integer)
                                          Dim tmpR, tmpG, tmpB As Byte
                                          For Each pos As Integer In itm.Value
                                              gs.Position = pos
                                              gs.Read(b, 0, 4)
                                              Dim cont As Boolean = True
                                              tmpR = b(2)
                                              tmpG = b(1)
                                              tmpB = b(0)
                                              If Me.SkipDarkPixel Then
                                                  If tmpR < _DarkPixelLimit AndAlso tmpG < _DarkPixelLimit AndAlso tmpB < _DarkPixelLimit Then
                                                      cont = False
                                                  End If
                                              End If
                                              If cont Then
                                                  tmpListR.Add(tmpR)
                                                  tmpListG.Add(tmpG)
                                                  tmpListB.Add(tmpB)
                                              End If
                                          Next

                                          'on génère les stats
                                          Select Case Me.Method
                                              Case EnumMethod.AVERAGE
                                                  tmpR = CByte(tmpListR.Average)
                                                  tmpG = CByte(tmpListG.Average)
                                                  tmpB = CByte(tmpListB.Average)

                                              Case EnumMethod.MAX
                                                  tmpR = CByte((From x In tmpListR Group By x Into g = Group Order By g.Count Descending Let by = g(0) Select by Take 1)(0))
                                                  tmpG = CByte((From x In tmpListG Group By x Into g = Group Order By g.Count Descending Let by = g(0) Select by Take 1)(0))
                                                  tmpB = CByte((From x In tmpListB Group By x Into g = Group Order By g.Count Descending Let by = g(0) Select by Take 1)(0))
                                          End Select
                                          ret(itm.Key) = System.Windows.Media.Color.FromArgb(255, tmpR, tmpG, tmpB)
                                      End Sub)

            surf.UnlockRectangle()
            surf.Dispose()
        End If
        s.Stop()
        Return ret
    End Function
#End Region

#Region "Structure Coords"
    Public Structure Coords
        Dim X1 As Integer
        Dim X2 As Integer
        Dim Y1 As Integer
        Dim Y2 As Integer
    End Structure
#End Region

#Region "Classe de mappage DirectX"
    Private Class DxCapture
        Private Shared Device As Device

        Shared Sub New()
            Dim Params As New PresentParameters With {.Windowed = True, .SwapEffect = SwapEffect.Discard}
            Device = New Device(New Direct3D(), 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.SoftwareVertexProcessing, Params)
        End Sub

        Public Shared Function Capture() As Surface
            Dim s As Surface = Nothing
            Try
                s = Surface.CreateOffscreenPlain(Device, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, Format.A8R8G8B8, Pool.Scratch)
                Device.GetFrontBufferData(0, s)
            Catch ex As Direct3D9Exception
                s = Nothing
            Catch ex As Exception
                s = Nothing
            End Try
            Return s
        End Function
    End Class
#End Region

End Class
