' Console Graphics Helper (CGH)
' L Minett 2020
' See independent solution for full source code

Imports System.Drawing
Imports System.Runtime.InteropServices ' used for console maximisation
Imports System.Text
Imports System.Threading

Module CGH
    Public Enum BorderType
        SingleLine
        SingleLineCurved
        SingleThick
        SingleDashed
        SingleThickDashed
        DoubleLine
        DoubleSideSingleTop
        SingleSideDoubleTop
        Solid
        Circle
    End Enum
    Public Enum Alignment
        None
        Left
        Centre
        Right
    End Enum
    Dim CurrForeground As ConsoleColor ' stores current fore and back colour for restoring
    Dim CurrBackground As ConsoleColor
    Public Class Win32Native
        ' class handles the Win32 pointers to control the console window as no native support in .NET for this
        Private Const SWP_NOZORDER As Integer = &H4
        Private Const SWP_NOACTIVATE As Integer = &H10
        Private Const SW_MAXIMIZE As Integer = &H3

        <StructLayout(LayoutKind.Sequential)>
        Private Structure RECT
            Public Left As Integer
            Public Top As Integer
            Public Right As Integer
            Public Bottom As Integer
        End Structure

        <DllImport("kernel32")>
        Private Shared Function GetConsoleWindow() As IntPtr
        End Function

        <DllImport("user32")>
        Private Shared Function SetWindowPos(hWnd As IntPtr, hWndInsertAfter As IntPtr,
        x As Integer, y As Integer, cx As Integer, cy As Integer, flags As Integer) As Boolean
        End Function

        <DllImport("user32.dll")>
        Private Shared Function GetWindowRect(ByVal hWnd As IntPtr, ByRef lpRect As RECT) As Boolean
        End Function

        <DllImport("user32.dll")>
        Public Shared Function ShowWindow(hWnd As IntPtr, cmdShow As Integer) As Boolean
        End Function

        Public Shared Sub SetConsoleWindowPosition(x As Integer, y As Integer)
            Dim r As RECT
            GetWindowRect(GetConsoleWindow(), r)

            SetWindowPos(GetConsoleWindow(), IntPtr.Zero,
                     x, y,
                     r.Left + r.Right, r.Top + r.Bottom,
                     SWP_NOZORDER Or SWP_NOACTIVATE)
        End Sub

        Public Shared Sub MaximizeConsoleWindow()
            Dim p As Process = Process.GetCurrentProcess()
            ShowWindow(p.MainWindowHandle, SW_MAXIMIZE)
        End Sub
    End Class
    Public Class Position
        Private _PosX As Short
        Private _PosY As Short
        Public Sub New(X As Short, Y As Short)
            _PosX = X
            _PosY = Y
        End Sub
        Public Property X() As Short
            Get
                Return _PosX
            End Get
            Set(value As Short)
                _PosX = value
            End Set
        End Property
        Public Property Y() As Short
            Get
                Return _PosY
            End Get
            Set(value As Short)
                _PosY = value
            End Set
        End Property
    End Class

    Public Class Keyboard
        'KB V1.10

        'ToDo: Implement l/c and u/c keyboard (should be quite easy): Declare 'Shift' as global and raise ToUpper or ToLower
        ' if implement above, consider a ShiftKeyboard method, so that all the borders do not have to be re-drawn
#Region "Declatations"
        ' creates an onscreen keyboard
        Private Const _KeyHeight As Short = 3 ' Chars high (including key boarder shape)
        Private Const _KeyWidth As Short = 5 ' Chars wide (including key boarder shape)
        Private Const _KeyXSpacing As Short = 2 ' Vertical spacing between keys
        Private Const _KeyYSpacing As Short = 2 ' horizontal spacing between keyboard rows
        Private Const _KeyboardWidth As Short = (10 * _KeyWidth) + (_KeyXSpacing * 9) ' taken from top row: (10 keys * 5 cols) + ([key spacing] * 9)
        Private Const _MiddleRowIndent As Short = 3 ' number of cols to offset middle row from X Pos
        Private Const _BottomRowIndent As Short = 8 ' number of cols to offset bottom row from X Pos

        ' keyboard colours
        Private Const _KeyOutlineColour As ConsoleColor = ConsoleColor.White
        Private Const _KeyOutlineBackColour As ConsoleColor = ConsoleColor.Black
        Private Const _KeyLetterColour As ConsoleColor = ConsoleColor.Yellow
        Private Const _KeyLetterBackColour As ConsoleColor = ConsoleColor.Black
        Private Const _KeyHighlightOutlineColour As ConsoleColor = ConsoleColor.Cyan
        Private Const _KeyHighlightOutlineBackColour As ConsoleColor = ConsoleColor.Black
        Private Const _KeyHighlightLetterColour As ConsoleColor = ConsoleColor.Yellow
        Private Const _KeyHighlightLetterBackColour As ConsoleColor = ConsoleColor.Black
        Private Const _KeyOverlayColour As ConsoleColor = ConsoleColor.Gray
        Private Const _KeyOverlayBackColour As ConsoleColor = ConsoleColor.Black

        'General variables
        Private _Y_Pos As Short = -1 ' where keyboard will appear on screen (some routines will check for -1 and throw error if DrawKeyboard not called first
        Private _X_Pos As Short ' determined by the alignment
        Private _KeyboardHeight As Short = (3 * _KeyHeight) + (_KeyYSpacing * 2)
        Private _KeyboardAlignment As Alignment = Alignment.Left ' set from draw keyboard
        Private Shared _Keys As String = "QWERTYUIOPASDFGHJKLZXCVBNM"
        Private _KeyRemap As String
        Private _KeyboardVisible As Boolean = False

        Private _Overlay As Boolean = False ' if True, original key is shown in upper right corner
        ' NOTE: If overlay position is within the key area, additional work needs to be done to erase when hidden, as this area may not get drawn over by key draw
        Private _OverlayPosition As New Point(2, 0) ' original key will set in upper border

        Dim CentrePoint As Short = CInt(Console.WindowWidth / 2) - 1
#End Region
#Region "Constructor"
        ''' <summary>
        ''' Create a new keyboard with default QWERTY mappings
        ''' </summary>
        Sub New()
            InitialiseKeyboard(_Keys) ' default standard keyboard
        End Sub
        ''' <summary>
        ''' Create a new keyboard
        ''' </summary>
        ''' <param name="KeyRemap">New map of keys with each row as a string indicating new key position. Keys positions must map to e.g "SDFGHJKLA" would map 1 place left: S to A, D to S, etc.</param>
        Sub New(KeyRemap As String)
            InitialiseKeyboard(KeyRemap)
        End Sub
        ''' <summary>
        ''' Create a new keyboard
        ''' </summary>
        ''' <param name="TopRow">New map of top row QWERTY keyboard keys with each char indicating new key position. Requires 10 chars </param>
        ''' <param name="MiddleRow">New map of middle row QWERTY keyboard keys with each char indicating new key position. Requires 9 chars</param>
        ''' <param name="BottomRow">New map of bottom row QWERTY keyboard keys with each char indicating new key position. Requires 7 chars</param>
        Sub New(TopRow As String, MiddleRow As String, BottomRow As String)
            InitialiseKeyboard(TopRow & MiddleRow & BottomRow)
        End Sub
#End Region
        ''' <summary>
        ''' Determines if the keyboard will overlay the physical key in addition to the mapped one
        ''' </summary>
        ''' <returns></returns>
        Public Property Overlay
            Get
                Return _Overlay
            End Get
            Set(value)
                If _Overlay <> value Then
                    _Overlay = value
                    ' setting has changed, so update graphics
                    If value Then
                        ' show overlay
                        OverlayKeys()
                    Else
                        ' draw keyboaard without overlay
                        DrawKeybaord(_Y_Pos, _KeyboardAlignment)
                    End If
                End If

            End Set
        End Property
        Private Sub InitialiseKeyboard(KeyboardMap As String)
            ' called from Sub New
            If KeyboardMap.Length <> 26 Then
                Throw New Exception("Invalid keyboard map given. Ensure there are 26 chars given in the mapped string")
            End If
            KeyboardMap = KeyboardMap.ToUpper
            _KeyRemap = KeyboardMap
            Console.CursorVisible = False
            _Overlay = False
        End Sub
        ''' <summary>
        ''' Draws the keyboard on screen from the given y coordinate. This must be called before highlight key can be used.
        ''' </summary>
        ''' <param name="Pos_Y">The top position of the keyboard</param>
        ''' <param name="Align">Indicated the position of the keyboard on the Y axis</param>
        Sub DrawKeybaord(Pos_Y As Short, Optional Align As Alignment = Alignment.Left)
            ' draw the keyboard on screen at given Y coordinate using the default or supplied key mappings

            ' First check if keyboard has already been drawn, as may be moved
            If _Y_Pos >= 0 Then
                HideKeyboard() ' paint it out in current position
            End If

            'check params are valid
            _Y_Pos = If(Pos_Y < 0, 0, If(Pos_Y > Console.WindowHeight - _KeyboardHeight, Console.WindowHeight - _KeyboardHeight, Pos_Y)) ' do not allow off screen
            Select Case Align
                Case Alignment.Centre
                    _X_Pos = CentrePoint - (_KeyboardWidth / 2)
                Case Alignment.Right
                    _X_Pos = Console.WindowWidth - 1 - _KeyboardWidth
                Case Else
                    _X_Pos = 0
            End Select
            _KeyboardAlignment = Align

            DrawKB(_Y_Pos, _KeyboardAlignment)
        End Sub
        Private Sub DrawKB(Pos_Y As Short, Align As Alignment)
            ' draw the keyboard on screen at given Y coordinate using the default or supplied key mappings

            Dim NextXPos As Short = _X_Pos
            Dim NextYPos As Short = _Y_Pos
            Dim KeyIndex As Short = 0
            For TopRow As Short = 1 To 10
                DrawBorder(NextXPos, NextYPos, NextXPos + (_KeyWidth - 1), NextYPos + (_KeyHeight - 1), BorderType.SingleLineCurved, _KeyOutlineColour, _KeyOutlineBackColour)
                Console.SetCursorPosition(NextXPos + (_KeyWidth \ 2), NextYPos + (_KeyHeight \ 2))
                Console.ForegroundColor = _KeyLetterColour
                Console.BackgroundColor = _KeyLetterBackColour
                Console.Write(_KeyRemap(KeyIndex))
                KeyIndex += 1
                NextXPos += _KeyWidth + _KeyXSpacing
            Next

            NextXPos = _X_Pos + _MiddleRowIndent
            NextYPos += _KeyHeight + _KeyYSpacing
            For MiddleRow As Short = 1 To 9
                DrawBorder(NextXPos, NextYPos, NextXPos + (_KeyWidth - 1), NextYPos + (_KeyHeight - 1), BorderType.SingleLineCurved, _KeyOutlineColour, _KeyOutlineBackColour)
                Console.SetCursorPosition(NextXPos + (_KeyWidth \ 2), NextYPos + (_KeyHeight \ 2))
                Console.ForegroundColor = _KeyLetterColour
                Console.BackgroundColor = _KeyLetterBackColour
                Console.Write(_KeyRemap(KeyIndex))
                KeyIndex += 1
                NextXPos += _KeyWidth + _KeyXSpacing
            Next

            NextXPos = _X_Pos + _BottomRowIndent
            NextYPos += _KeyHeight + _KeyYSpacing
            For BottomRow As Short = 1 To 7
                DrawBorder(NextXPos, NextYPos, NextXPos + (_KeyWidth - 1), NextYPos + (_KeyHeight - 1), BorderType.SingleLineCurved, _KeyOutlineColour, _KeyOutlineBackColour)
                Console.SetCursorPosition(NextXPos + (_KeyWidth \ 2), NextYPos + (_KeyHeight \ 2))
                Console.ForegroundColor = _KeyLetterColour
                Console.BackgroundColor = _KeyLetterBackColour
                Console.Write(_KeyRemap(KeyIndex))
                KeyIndex += 1
                NextXPos += _KeyWidth + _KeyXSpacing
            Next
            If _Overlay Then OverlayKeys()
            _KeyboardVisible = True
        End Sub
        Private Sub OverlayKeys()
            ' chosen to separate out this logic as overlay can be applied without needing to draw entire keyboard
            If _Overlay = False Then Exit Sub
            If _Y_Pos = -1 Then Exit Sub ' keybaoard has not yet been drawn

            Dim NextXPos As Short = _X_Pos
            Dim NextYPos As Short = _Y_Pos
            Dim KeyIndex As Short = 0
            For TopRow As Short = 1 To 10
                Console.SetCursorPosition(NextXPos + _OverlayPosition.X, NextYPos + _OverlayPosition.Y)
                Console.ForegroundColor = _KeyOverlayColour
                Console.BackgroundColor = _KeyOverlayBackColour
                Console.Write(_Keys(KeyIndex).ToString.ToLower)
                KeyIndex += 1
                NextXPos += _KeyWidth + _KeyXSpacing
            Next

            NextXPos = _X_Pos + _MiddleRowIndent
            NextYPos += _KeyHeight + _KeyYSpacing
            For MiddleRow As Short = 1 To 9
                Console.SetCursorPosition(NextXPos + _OverlayPosition.X, NextYPos + _OverlayPosition.Y)
                Console.ForegroundColor = _KeyOverlayColour
                Console.BackgroundColor = _KeyOverlayBackColour
                Console.Write(_Keys(KeyIndex).ToString.ToLower)
                KeyIndex += 1
                NextXPos += _KeyWidth + _KeyXSpacing
            Next

            NextXPos = _X_Pos + _BottomRowIndent
            NextYPos += _KeyHeight + _KeyYSpacing
            For BottomRow As Short = 1 To 7
                Console.SetCursorPosition(NextXPos + _OverlayPosition.X, NextYPos + _OverlayPosition.Y)
                Console.ForegroundColor = _KeyOverlayColour
                Console.BackgroundColor = _KeyOverlayBackColour
                Console.Write(_Keys(KeyIndex).ToString.ToLower)
                KeyIndex += 1
                NextXPos += _KeyWidth + _KeyXSpacing
            Next


        End Sub
        Private Sub DrawKey(Key As Char, Highlight As Boolean,
                            Optional HighlightTextColour As ConsoleColor = _KeyHighlightLetterColour, Optional HighlightOutlineColour As ConsoleColor = _KeyHighlightOutlineColour)
            ' Private helper sub that will draw a key on the screen (used for highlighting, not for main drawing)

            ' cannot draw key if do not know where to draw it
            If _Y_Pos = -1 Then Throw New Exception("DrawKeyboard must be used first") ' in case this sub is made public

            Key = Key.ToString.ToUpper

            ' calculate logical row the key is on
            Dim IndexPos As Short = _KeyRemap.IndexOf(Key)
            If IndexPos < 0 Then Exit Sub ' key not valid

            Dim Row, KeyNum, KeyX, KeyY, Indent As Short
            If IndexPos < 10 Then
                Row = 0
                KeyNum = IndexPos
                Indent = 0
            ElseIf IndexPos < 19 Then
                Row = 1
                KeyNum = IndexPos - 10
                Indent = _MiddleRowIndent
            Else
                Row = 2
                KeyNum = IndexPos - 19
                Indent = _BottomRowIndent
            End If

            ' Relative X and Y coordinate can now be calculated for the key
            KeyX = _X_Pos + Indent + ((_KeyXSpacing + _KeyWidth) * KeyNum)
            KeyY = _Y_Pos + ((_KeyYSpacing + _KeyHeight) * Row)

            If Highlight Then ' No overlay when key is highlightd as it's already been pressed
                DrawBorder(KeyX, KeyY, KeyX + (_KeyWidth - 1), KeyY + (_KeyHeight - 1), BorderType.SingleLineCurved, HighlightOutlineColour, _KeyHighlightOutlineBackColour)
                Console.ForegroundColor = HighlightTextColour
                Console.BackgroundColor = _KeyHighlightLetterBackColour
                Console.SetCursorPosition(KeyX + (_KeyWidth \ 2), KeyY + (_KeyHeight \ 2))
                Console.Write(Key)
            Else
                DrawBorder(KeyX, KeyY, KeyX + (_KeyWidth - 1), KeyY + (_KeyHeight - 1), BorderType.SingleLineCurved, _KeyOutlineColour, _KeyOutlineBackColour)
                Console.ForegroundColor = _KeyLetterColour
                Console.BackgroundColor = _KeyLetterBackColour
                Console.SetCursorPosition(KeyX + (_KeyWidth \ 2), KeyY + (_KeyHeight \ 2))
                Console.Write(Key)

                ' now draw overlay if required
                If _Overlay Then
                    Console.SetCursorPosition(_OverlayPosition.X + KeyX, _OverlayPosition.Y + KeyY)
                    Console.ForegroundColor = _KeyOverlayColour
                    Console.BackgroundColor = _KeyOverlayBackColour
                    Console.Write(_Keys(IndexPos).ToString.ToLower)
                End If

            End If

        End Sub

        ''' <summary>
        ''' Highlights key on screen.  Will not work if DrawKeyboard has not first been called or keyboard hidden
        ''' </summary>
        ''' <param name="Key">Char equivalent of key to highlight. Will highlight on mapped keyboard if applicable. Use MappedToStandard if you need to do this the other way around</param>
        ''' <param name="Persist">Highlight will remain if persist is on. Use ResetKey to unhighlight key</param>
        ''' <param name="Duration">The duration (in ms) that the key will highlight.  Ignored if Persist is set to true</param>
        Sub HighlightKey(Key As Char, Optional Persist As Boolean = False, Optional Duration As Short = 200,
                         Optional HighlightTextColour As ConsoleColor = _KeyHighlightLetterColour, Optional HighlightOutlineColour As ConsoleColor = _KeyHighlightOutlineColour)
            If _Y_Pos = -1 Or Not _KeyboardVisible Then Exit Sub ' keyboard not yet drawn
            If Persist Then
                ' call HighlightKeyThread to automatically de-hilight after a given time
                DrawKey(Key, True, HighlightTextColour, HighlightOutlineColour)
            Else
                DrawKey(Key, True, HighlightTextColour, HighlightOutlineColour)
                Dim Reset As New Threading.Thread(Sub() HighlightKeyThread(Key, Duration))
                Reset.Start()
            End If
        End Sub
        ''' <summary>
        ''' Paints over where the keyboard has been and sets keyboard to invisible. Can only be called once DrawKeyboard has been used.
        ''' </summary>
        ''' <param name="Background">Background colour to paint space with</param>
        Public Sub HideKeyboard(Optional Background As ConsoleColor = ConsoleColor.Black)
            ' will only paint over where keyboard has been drawn
            If _Y_Pos = -1 Then Exit Sub ' keyboard not yet drawn

            _KeyboardVisible = False
            For line As Short = _Y_Pos To _Y_Pos + _KeyboardHeight
                Console.BackgroundColor = Background
                Console.SetCursorPosition(_X_Pos, line)
                Console.WriteLine(StrDup(_KeyboardWidth, " "))
            Next
        End Sub
        ''' <summary>
        ''' Re-displays the keyboard (if hidden) without needing to call DrawKeyboard again.  Can also be used to repaint keyboard
        ''' </summary>
        Public Sub ShowKeyboard()
            ' can call if already shown, to allow re-paining
            If _Y_Pos = -1 Then Exit Sub ' keyboard not yet drawn
            _KeyboardVisible = True
            DrawKB(_Y_Pos, _KeyboardAlignment)
        End Sub
        ' called asynchronously in a new thread
        Private Sub HighlightKeyThread(key As Char, duration As Short)
            Dim watch As New Stopwatch
            watch.Start()

            Do While watch.ElapsedMilliseconds < duration
            Loop
            watch.Reset()
            ResetKey(key)
            Thread.CurrentThread.Abort()
        End Sub
        Sub ResetKey(key As Char)
            If _Y_Pos = -1 Then Exit Sub ' keyboard not yet drawn
            DrawKey(key, False)
        End Sub
        Function MappedToStandard(key As Char) As Char
            ' find position of mapped key in keys and return that char
            key = key.ToString.ToUpper
            Dim MappedIndex As Short = _KeyRemap.IndexOf(key)
            If MappedIndex < 0 Then
                Return Nothing
            Else
                Return _Keys(MappedIndex)
            End If

        End Function
        Function StandardToMapped(key As Char) As Char
            ' find position of standard key in keys and return the mapped equivalent char
            key = key.ToString.ToUpper
            Dim MappedIndex As Short = _Keys.IndexOf(key)
            If MappedIndex < 0 Then
                Return Nothing
            Else
                Return _KeyRemap(MappedIndex)
            End If
        End Function
        ''' <summary>
        ''' Will shift keyboard right (+ve) or left (-ve) given spaces (keep to max 7 for logic purposes). Each row is shifted individually the given value, wrapping at end of row
        ''' E.g. +1 will Map A key to S, W to E and P to Q. -1 will map Q to P, W to Q, A to L, etc.
        ''' </summary>
        ''' <param name="Shift"></param>
        ''' <returns>String</returns>
        Shared Function GenerateQwertyMapping(Shift As Short) As String
            Shift = Shift * -1 ' reverses logic based on shift as -1 actually shifts right 1
            Dim NewStr As New StringBuilder

            ' takes a substring (e.g. QWERTY...ASFFG...ZXC.. and shifts left or right using ShiftLetters function - wraps around (per individual row) if too great a number selected
            NewStr.Append(ShiftLetters(Shift, _Keys.Substring(0, 10)))
            NewStr.Append(ShiftLetters(Shift, _Keys.Substring(10, 9)))
            NewStr.Append(ShiftLetters(Shift, _Keys.Substring(19, 7)))
            Return NewStr.ToString
        End Function
        Private Shared Function ShiftLetters(Position As Short, Str As String) As String
            ' takes a given string and shifts places and returns the new string

            Dim Newstring As New StringBuilder
            Dim StartPos As Short = (Str.Length + Position) Mod Str.Length
            For x As Short = 0 To Str.Length - 1
                ' go through each char and build new string
                Newstring.Append(Str(StartPos))
                StartPos = If(StartPos >= Str.Length - 1, 0, StartPos + 1)
            Next
            Return Newstring.ToString
        End Function
        ''' <summary>
        ''' Will shift keyboard forward (+ve) or backward (-ve) alphabetically (max 26). E.g. +1 will map key A to B, B to C, Z to A. -1 will map A to Z, B to A, I to H etc.
        ''' </summary>
        ''' <param name="Shift"></param>
        ''' <returns>String</returns>
        Shared Function GenerateAlphabetMapping(Shift As Short) As String
            ' could also achieve this by creating an array of chars and cycling through alphabet (using ASCII and MOD + position shift) and then finding the index position of that new char 
            ' to know where to map it to using  _KEYS index position

            ' This could be done on fewer lines, without using a variable, but process is broken down for educational purposes
            Dim NewString As New StringBuilder
            Shift = Shift Mod 26
            Dim Index As Short
            For count As Short = 0 To _Keys.Length - 1
                Index = Asc(_Keys(count)) - 65 ' get ascii value of keyboard key and subtract 65, meaning A starts at 0 (so can mod)
                Index = (Index + Shift) Mod 26 ' get relative 
                Index += 65 ' turn back into ASCII letter
                NewString.Append(Chr(Index))
            Next
            Return NewString.ToString
        End Function
        ''' <summary>
        ''' Will return a completely random keyboard mapping
        ''' </summary>
        ''' <returns>String</returns>
        Shared Function GenerateRandomMapping() As String
            Dim rnd As New Random
            Dim temp As Char
            Dim pos As Short
            Dim TempCharArray As Char() = _Keys.ToCharArray
            Randomize()
            For count As Integer = 0 To TempCharArray.Length - 1
                pos = rnd.Next(0, TempCharArray.Length)
                temp = TempCharArray(count)
                TempCharArray(count) = TempCharArray(pos)
                TempCharArray(pos) = temp
            Next
            Return New String(TempCharArray)
        End Function

    End Class

#Region "Border"
    ''' <summary>
    ''' Draws a border between x1,y1 and x2,y2. Requires console to have output encoding set to UTF-8.
    ''' </summary>
    ''' <param name="X1">Starting X position</param>
    ''' <param name="Y1">Starting Y position</param>
    ''' <param name="X2">Finishing X position</param>
    ''' <param name="Y2">Finishing Y position</param>
    ''' <param name="BorderStyle">Style of border</param>
    ''' <param name="LineColour">Console colour of the border</param>
    ''' <param name="BackColour">Background colour of the border</param>
    ''' 
    Public Sub DrawBorder(X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer, Optional BorderStyle As BorderType = BorderType.DoubleLine,
             Optional LineColour As ConsoleColor = ConsoleColor.White, Optional BackColour As ConsoleColor = ConsoleColor.Black)
        Dim TopLeft, TopRight, BottomLeft, BottomRight As Short ' corner symbols
        CurrBackground = Console.BackgroundColor
        CurrForeground = Console.ForegroundColor
        Select Case BorderStyle ' set corner shapes
            Case BorderType.SingleLine
                TopLeft = &H250C
                TopRight = &H2510
                BottomLeft = &H2514
                BottomRight = &H2518
            Case BorderType.SingleLineCurved
                TopLeft = &H256D
                TopRight = &H256E
                BottomLeft = &H2570
                BottomRight = &H256F
            Case BorderType.DoubleLine
                TopLeft = &H2554
                TopRight = &H2557
                BottomLeft = &H255A
                BottomRight = &H255D
            Case BorderType.DoubleSideSingleTop
                TopLeft = &H2553
                TopRight = &H2556
                BottomLeft = &H2559
                BottomRight = &H255C
            Case BorderType.SingleDashed
                TopLeft = &H250C
                TopRight = &H2510
                BottomLeft = &H2514
                BottomRight = &H2518
            Case BorderType.SingleSideDoubleTop
                TopLeft = &H2552
                TopRight = &H2555
                BottomLeft = &H2558
                BottomRight = &H255B
            Case BorderType.SingleThick
                TopLeft = &H250F
                TopRight = &H2513
                BottomLeft = &H2517
                BottomRight = &H251B
            Case BorderType.SingleThickDashed
                TopLeft = &H250F
                TopRight = &H2513
                BottomLeft = &H2517
                BottomRight = &H251B
            Case BorderType.Circle
                TopLeft = &H25CF
                TopRight = &H25CF
                BottomLeft = &H25CF
                BottomRight = &H25CF
            Case BorderType.Solid
                TopLeft = &H2588
                TopRight = &H2588
                BottomLeft = &H2588
                BottomRight = &H2588
        End Select

        ' paint specific lines - line shapes are determined by the DrawLine
        'top 
        DrawLine(X1, Y1, X2, Y1, BorderStyle, LineColour, BackColour)
        ' sides
        DrawLine(X1, Y1, X1, Y2, BorderStyle, LineColour, BackColour)
        DrawLine(X2, Y1, X2, Y2, BorderStyle, LineColour, BackColour)
        ' bottom
        DrawLine(X1, Y2, X2, Y2, BorderStyle, LineColour, BackColour)

        ' now paint corners C1-4
        Console.ForegroundColor = LineColour
        Console.BackgroundColor = BackColour

        Console.SetCursorPosition(X1, Y1)
        Console.Write(ChrW(TopLeft))
        Console.SetCursorPosition(X2, Y1)
        Console.Write(ChrW(TopRight))
        Console.SetCursorPosition(X1, Y2)
        Console.Write(ChrW(BottomLeft))
        Console.SetCursorPosition(X2, Y2)
        Console.Write(ChrW(BottomRight))

        'default colours back 
        Console.ForegroundColor = CurrForeground
        Console.BackgroundColor = CurrBackground

    End Sub
#End Region

#Region "Custom Border"
    ''' <summary>
    ''' Draws a border between x1,y1 and x2,y2 using specified unicode character
    ''' </summary>
    ''' <param name="X1">Starting X position</param>
    ''' <param name="Y1">Starting Y position</param>
    ''' <param name="X2">Finishing X position</param>
    ''' <param name="Y2">Finishing Y position</param>
    ''' <param name="BorderSymbol">Specifies a char to be drawn</param>
    Public Sub DrawCustomBorder(X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer, BorderSymbol As Char,
             Optional LineColour As ConsoleColor = ConsoleColor.White, Optional BackColour As ConsoleColor = ConsoleColor.Black)
        ' will call the custom line draw to paint the symbol on the screen

        'top 
        DrawCustomLine(X1, Y1, X2, Y1, BorderSymbol, LineColour, BackColour)
        ' sides
        DrawCustomLine(X1, Y1, X1, Y2, BorderSymbol, LineColour, BackColour)
        DrawCustomLine(X2, Y1, X2, Y2, BorderSymbol, LineColour, BackColour)

        ' bottom
        DrawCustomLine(X1, Y2, X2, Y2, BorderSymbol, LineColour, BackColour)
    End Sub
    ''' <summary>
    ''' Draws a border between x1,y1 and x2,y2 using specified unicode character
    ''' </summary>
    ''' <param name="X1">Starting X position</param>
    ''' <param name="Y1">Starting Y position</param>
    ''' <param name="X2">Finishing X position</param>
    ''' <param name="Y2">Finishing Y position</param>
    ''' <param name="UnicodeChar">Specifies a char to be drawn</param>
    Public Sub DrawCustomBorder(X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer, UnicodeChar As Integer,
             Optional LineColour As ConsoleColor = ConsoleColor.White, Optional BackColour As ConsoleColor = ConsoleColor.Black)
        DrawCustomBorder(X1, Y1, X2, Y2, ChrW(UnicodeChar), LineColour, BackColour)
    End Sub
#End Region

#Region "Line"
    ''' <summary>
    ''' Draws a vertical or horizontal line. Default style is SingleLine
    ''' </summary>
    ''' <param name="X1">Starting coordinate</param>
    ''' <param name="Y1">Starting coordinate</param>
    ''' <param name="X2">Finishing coordinate</param>
    ''' <param name="Y2">Finishing coordinate</param>
    Public Sub DrawLine(X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer)
        DrawLine(X1, Y1, X2, Y2, BorderType.SingleLine, ConsoleColor.White, ConsoleColor.Black)
    End Sub
    ''' <summary>
    ''' Draws a vertical or horizontal line. Default style is SingleLine
    ''' </summary>
    ''' <param name="X1">Starting coordinate</param>
    ''' <param name="Y1">Starting coordinate</param>
    ''' <param name="X2">Finishing coordinate</param>
    ''' <param name="Y2">Finishing coordinate</param>
    ''' <param name="LineStyle">Specifies which style to draw the line</param>
    ''' <param name="LineColour">The console colour of the line</param>
    ''' <param name="BackColour">The console colour of the background</param>
    Public Sub DrawLine(X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer, LineStyle As BorderType,
             Optional LineColour As ConsoleColor = ConsoleColor.White, Optional BackColour As ConsoleColor = ConsoleColor.Black)
        ' called custom line to draw shape
        Dim Horizontal As Boolean = If(Y1 = Y2, True, False) ' is line horizontal or vertical
        Dim Symb As Integer
        Select Case LineStyle
            Case BorderType.SingleLine
                ' call custom line with correct symbol
                Symb = If(Horizontal, &H2500, &H2502)
            Case BorderType.SingleLineCurved
                Symb = If(Horizontal, &H2500, &H2502)
            Case BorderType.Circle
                Symb = &H25CF
            Case BorderType.DoubleLine
                Symb = If(Horizontal, &H2550, &H2551)
            Case BorderType.DoubleSideSingleTop
                Symb = If(Horizontal, &H2500, &H2551)
            Case BorderType.SingleDashed
                Symb = If(Horizontal, &H2504, &H2506)
            Case BorderType.SingleSideDoubleTop
                Symb = If(Horizontal, &H2550, &H2502)
            Case BorderType.SingleThick
                Symb = If(Horizontal, &H2501, &H2503)
            Case BorderType.Solid
                Symb = &H2588
            Case BorderType.SingleThickDashed
                Symb = If(Horizontal, &H2505, &H2507)
        End Select
        DrawCustomLine(X1, Y1, X2, Y2, Symb, LineColour, BackColour)
    End Sub

#End Region
#Region "Custom Line"
    ''' <summary>
    ''' Draws a horizontal or vertical line on the screen
    ''' </summary>
    ''' <param name="X1"></param>
    ''' <param name="Y1"></param>
    ''' <param name="X2"></param>
    ''' <param name="Y2"></param>
    ''' <param name="UnicodeChar">The unicode value of the symbol used to paint the line</param>
    ''' <param name="LineColour">The console colour of the line</param>
    ''' <param name="BackColour">The console colour of the background</param>
    Sub DrawCustomLine(X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer, UnicodeChar As Integer,
                   Optional LineColour As ConsoleColor = ConsoleColor.White, Optional BackColour As ConsoleColor = ConsoleColor.Black)
        DrawCustomLine(X1, Y1, X2, Y2, ChrW(UnicodeChar), LineColour, BackColour) ' call other overload and pass in integer converted to char
    End Sub
    ''' <summary>
    ''' Draws a horizontal or vertical line on the screen
    ''' </summary>
    ''' <param name="X1"></param>
    ''' <param name="Y1"></param>
    ''' <param name="X2"></param>
    ''' <param name="Y2"></param>
    ''' <param name="LineSymbol">The char with which to draw the line</param>
    ''' <param name="LineColour">The console colour of the line</param>
    ''' <param name="BackColour">The console colour of the background</param>
    Sub DrawCustomLine(X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer, LineSymbol As Char,
                   Optional LineColour As ConsoleColor = ConsoleColor.White, Optional BackColour As ConsoleColor = ConsoleColor.Black)
        ' draw a line from X1,Y1 to X2,Y2 using the supplied symbol
        ' ToDo: With more time, implement Bresenham's line algorithm, or even Wu's
        ' If X and Y are both different (i.e. diagonal, then horizontal assumed until diagonal line functionality supported

        Dim Horizontal As Boolean = If(Y1 = Y2, True, False) ' is line horizontal or vertical
        CurrBackground = Console.BackgroundColor
        CurrForeground = Console.ForegroundColor

        Console.ForegroundColor = LineColour
        Console.BackgroundColor = BackColour

        If Horizontal Then
            For cols = X1 To X2
                Console.SetCursorPosition(cols, Y1)
                Console.Write(LineSymbol)
            Next
        Else
            For rows = Y1 To Y2
                Console.SetCursorPosition(X1, rows)
                Console.Write(LineSymbol)
            Next
        End If
        'default colours back 
        Console.ForegroundColor = CurrForeground
        Console.BackgroundColor = CurrBackground
    End Sub

#End Region
    ''' <summary>
    ''' Draws a filled rectangle on the console screen
    ''' </summary>
    ''' <param name="X1"></param>
    ''' <param name="Y1"></param>
    ''' <param name="X2"></param>
    ''' <param name="Y2"></param>
    ''' <param name="FillColour"></param>
    Public Sub DrawRectangle(X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer, FillColour As ConsoleColor)
        DrawRectangle(X1, Y1, X2, Y2, FillColour, &H2588) ' default as a block
    End Sub
    ''' <summary>
    ''' Draws a filled rectangle on the console screen
    ''' </summary>
    ''' <param name="X1"></param>
    ''' <param name="Y1"></param>
    ''' <param name="X2"></param>
    ''' <param name="Y2"></param>
    ''' <param name="FillColour"></param>
    ''' <param name="FillChar">The character with which to fill the rectangle</param>
    Public Sub DrawRectangle(X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer, FillColour As ConsoleColor, FillChar As Char)
        ' uses the drawcustomline to fill solid block
        If Y1 > Y2 Then Throw New Exception("Y2 cannot be less than Y1")
        If X1 > X2 Then Throw New Exception("X2 cannot be less than X1")

        For row As Integer = Y1 To Y2
            DrawCustomLine(X1, row, X2, row, FillChar, FillColour)
        Next
    End Sub
    ''' <summary>
    '''  Draws a filled rectangle on the console screen
    ''' </summary>
    ''' <param name="X1"></param>
    ''' <param name="Y1"></param>
    ''' <param name="X2"></param>
    ''' <param name="Y2"></param>
    ''' <param name="FillColour"></param>
    ''' <param name="UnicodeChar">The unicode value with which to fill the rectangle</param>
    Public Sub DrawRectangle(X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer, FillColour As ConsoleColor, UnicodeChar As Short)
        DrawRectangle(X1, Y1, X2, Y2, FillColour, ChrW(UnicodeChar))
    End Sub

    ''' <summary>
    ''' Will display given text centred on a given line
    ''' </summary>
    ''' <param name="LineText"></param>
    ''' <param name="Pos_Y"></param>
    Sub CentreText(LineText As String, Pos_Y As Short, Optional TextColour As ConsoleColor = ConsoleColor.White, Optional BackColour As ConsoleColor = ConsoleColor.Black)
        CurrBackground = Console.BackgroundColor
        CurrForeground = Console.ForegroundColor

        Console.ForegroundColor = TextColour
        Console.BackgroundColor = BackColour
        Dim Pos_X As Short = CInt(Console.WindowWidth / 2) - CInt((LineText.Length / 2))
        Console.SetCursorPosition(Pos_X, Pos_Y)
        Console.WriteLine(LineText)

        Console.ForegroundColor = CurrForeground
        Console.BackgroundColor = CurrBackground
    End Sub

    ''' <summary>
    ''' Will take a string of text and render this to a bitmap image
    ''' </summary>
    ''' <param name="text">The text to rasterise</param>
    ''' <param name="TextColour">What colour should the text be</param>
    ''' <param name="DPIx">The image DPI for x axis</param>
    ''' <param name="DPIy">The image DPI for y axis</param>
    ''' <param name="fnt">The name of the font family to use (default Consolas)</param>
    ''' <param name="Sze">The font size in points (default 12)</param>
    ''' <param name="BackgroundClr">The background colour to pain the canvas</param>
    ''' <param name="Width">Allow the specification of the bitmap witdth (in pixels) if automatic width value is cropping</param>
    ''' <param name="Height">Allow the specification of the bitmap witdth (in pixels) if automatic height value is cropping</param>
    ''' <returns></returns>
    Public Function TextToBMP(text As String, TextColour As Color, Optional DPIx As Single = 120, Optional DPIy As Single = 120, Optional fnt As String = "Consolas",
                       Optional Sze As Single = 12, Optional BackgroundClr As Drawing.Color = Nothing, Optional Width As Integer = vbNull, Optional Height As Integer = vbNull) As Bitmap
        If BackgroundClr = Nothing Then ' if no background colour supplied, assume black
            BackgroundClr = Color.Black
        End If

        ' Makes an attempt to calculate image width and height based on text length (assumes 6px x 12px @72dpi with standard kerning)
        'ToDo: Still needs work on width. E.g. 20 char 110dpi Ariel at 10pt is 60 pixels wider (font and kerning are big issues that may not be resolvable quickly)
        Dim imgWidth As Short = If(Width <> vbNull, Width, text.Length * ((DPIx * (Sze * 0.64)) / 72))
        Dim imgHeight As Short = If(Height <> vbNull, Height, ((DPIy * Sze) / 72) + 1)


        Dim Bmp As New Bitmap(imgWidth, imgHeight, Imaging.PixelFormat.Format32bppPArgb) ' define new graphics object

        'Set the resolution to 150 DPI
        Bmp.SetResolution(DPIx, DPIy)
        'Create a graphics object from the bitmap
        Using G = Graphics.FromImage(Bmp)
            'Paint the canvas white
            G.Clear(BackgroundClr)
            'Set various modes to higher quality
            G.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            G.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            G.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
            'Create a font
            Using F As New Font(fnt, Sze)
                'Create a brush
                Using B As New SolidBrush(TextColour)
                    'Draw some text
                    G.DrawString(text, F, B, 0, 0)
                End Using
            End Using
        End Using
        Return Bmp
    End Function
    ''' <summary>
    ''' Will display a console text representation of a bitmap graphic
    ''' </summary>
    ''' <param name="PosX">The starting console x position of the render</param>
    ''' <param name="PosY">The starting console y position of the render</param>
    ''' <param name="image">The bitmap to render</param>
    ''' <param name="ShapeIndex">The unicode value of the shape to be used. Block is default</param>
    ''' <param name="SolidColour">Any non-background coloured pixel will be painted a single colour specified by [DefaultClr]</param>
    ''' <param name="DefaultClr">If using solid colour, this is the console colour to use for any non-transparent pixel</param>
    ''' <param name="OrigBkgClr">Specifies that this colour is treated as a background colour. Default is black.</param>
    ''' <param name="NewBkgClr">Specifies what console colour the background should be painted, if nothing specified, uses black</param>
    ''' <param name="ScaleX">If set, will skip the indicated number of colss to reduce the x axis. e.g. 1=1:1 scale</param>
    ''' <param name="ScaleY">If set, will skip the indicated number of rows to reduce the y axis. e.g. 1=1:1 scale</param>
    ''' <param name="IgnoreX1">X coordinate of first x pixel to draw (acts like crop)</param>
    ''' <param name="IgnoreY1">Y coordinate of first y pixel to draw (acts like crop)</param>
    ''' <param name="IgnoreX2">Specifies the last X coordinate of bitmap that will be drawn</param>
    ''' <param name="IgnoreY2">Specifies the last Y coordinate of bitmap that will be drawn</param>
    ''' <param name="BMPAlign">If specified, ignores X co-ordinate and positions accordingly</param>
    Public Sub BmpToConsole(PosX As Short, PosY As Short, image As Bitmap, Optional ShapeIndex As Short = &H2588, Optional SolidColour As Boolean = True,
                Optional DefaultClr As ConsoleColor = ConsoleColor.White, Optional OrigBkgClr As Color = Nothing, Optional NewBkgClr As ConsoleColor = ConsoleColor.Black,
                Optional ScaleX As Short = 1, Optional ScaleY As Short = 1,
                Optional IgnoreX1 As Short = 2, Optional IgnoreY1 As Short = 2, Optional IgnoreX2 As Short = 0, Optional IgnoreY2 As Short = 0,
                            Optional BMPAlign As Alignment = Alignment.None)
        ' will convert a bitmap image to console pixels

        CurrBackground = Console.BackgroundColor
        CurrForeground = Console.ForegroundColor

        If OrigBkgClr = Nothing Then ' default parameter values must be constants, which color is not 
            OrigBkgClr = Color.Black
        End If
        ScaleY = If(ScaleY < 1, 1, ScaleY) ' check not less than 1

        ' If SolidColour is true, then no determination will be made of the actual colour and will use the the Clr parameter
        ' IgnoreX/IgnoreY will ignore first x pixels in image (padding). Saves having to crop bitmap (different size fonts have different padding)

        ' As the intention here is to display a bitmap, the code ignores the background colour

        If BMPAlign = Alignment.Left Then
            PosX = 0
        ElseIf BMPAlign = Alignment.Centre Then
            PosX = CInt(Console.WindowWidth / 2) - CInt(((image.Width - (IgnoreX1 + IgnoreX2)) / 2) / ScaleX)
        ElseIf BMPAlign = Alignment.Right Then
            PosX = (Console.WindowWidth - 1) - CInt(Math.Ceiling((image.Width - (IgnoreX1 + IgnoreX2)) / ScaleX))
        End If

        Dim Row As Short = PosY
        Dim Col As Short = PosX

        For y As Integer = IgnoreY1 To (image.Height - 1) - IgnoreY2 Step ScaleY

            For x As Integer = IgnoreX1 To (image.Width - 1) - IgnoreX2 Step ScaleX
                'Console.SetCursorPosition((x - IgnoreX) + PosX, (y - IgnoreY) + PosY)
                Console.SetCursorPosition(Col, Row)
                If SolidColour Then
                    ' paint console pixel as solid colour
                    If image.GetPixel(x, y).ToArgb = OrigBkgClr.ToArgb Then
                        Console.ForegroundColor = NewBkgClr ' convert new background colour to console colour
                    Else
                        Console.ForegroundColor = DefaultClr ' paint using the requested colour
                    End If
                Else
                    Console.ForegroundColor = GetConsoleColours(image.GetPixel(x, y).ToArgb).Item1 ' get consolecolour equivalent of current pixel
                End If
                Console.Write(ChrW(ShapeIndex)) 'write same shape incase justification issue with console font
                Col += 1
            Next
            Col = PosX ' reset column count
            Row += 1
        Next
        Console.ForegroundColor = CurrForeground
        Console.BackgroundColor = CurrBackground
    End Sub

    ''' <summary>
    ''' Converts a given ARGB integer to Returns equivalent foreground colour and contrasting background
    ''' </summary>
    ''' <param name="Clr">ARGB integer of system.color</param>
    ''' <returns>Tuple of consoleforeground and consolebackground colours</returns>
    Public Function GetConsoleColours(ByVal Clr As Integer) As (ConsoleColor, ConsoleColor)
        'Dim argb As Integer = Integer.Parse(Hex.Replace("#", ""), NumberStyles.HexNumber)
        Dim c As Color = Color.FromArgb(Clr)

        ' Counting the perceptive luminance - human eye favors green color... 
        Dim a As Double = 1 - (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255
        Dim index As Integer = If(c.R > 128 Or c.G > 128 Or c.B > 128, 8, 0) ' Bright bit
        index = index Or If(c.R > 64, 4, 0) ' Red bit
        index = index Or If(c.G > 64, 2, 0) ' Green bit
        index = index Or If(c.B > 64, 1, 0) ' Blue bit
        Dim foregroundColor As ConsoleColor = CType(index, System.ConsoleColor)
        Dim backgroundColor As ConsoleColor = If(a < 0.5, ConsoleColor.Black, ConsoleColor.White)
        Return (foregroundColor, backgroundColor)
    End Function

End Module
