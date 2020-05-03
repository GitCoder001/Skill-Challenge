Imports System.Console
Imports System.Drawing
Imports System.Threading

Module Game2
    Dim HeadStart As Short ' different for each game
    Dim KeyboardPreview As Short = 10
    Dim KeyPenalty As Short = 2
    Dim Centre As Short = CInt(WindowWidth / 2) - 1
    Dim EscPressedIdent As Short = -1 ' value used to trap user pressing escape

    Public Function StartGame(Name As String) As Single
        ' Returns game score and if the player chose to quit

        If Name = "" Or Name = Nothing Then Name = "Anna Nimmity"

        'ToDo: Re-write instruction screens to have a demo word and keyboard, showing how to complete with keyboard passed into routine, so it works on actual keyboard

        ' set up sound effects
        SndFX1.Open(New System.Uri(MusicPath & "Wrong Sound.mp3"))
        SndFX2.Open(New System.Uri(MusicPath & "Correct_01.mp3"))
        SndFX3.Open(New System.Uri(MusicPath & "SCORE1.mp3"))

        Dim ScoreTotal, PenaltyTotal As Single
        Dim score As (Single, Single)
        Dim Rand As New Random
        Randomize()

        ForegroundColor = ConsoleColor.White
        BackgroundColor = ConsoleColor.Black

        ' Round 1
        HeadStart = 5

        ClearKeyboardBuffer()
        GameGFXRound1()
        ClearKeyboardBuffer()

        Dim Round1KB As New Keyboard(Keyboard.GenerateQwertyMapping(1))
        If PlayAudio Then Music.Open(New System.Uri(MusicPath & "Mary France 1.mp3")) ' Set background music
        score = PlayRound(Round1KB, True, False)
        If score.Item1 = EscPressedIdent Then Return vbNull
        ScoreTotal = score.Item1
        PenaltyTotal = score.Item2

        ' Round 2
        HeadStart = 7

        ClearKeyboardBuffer()
        GameGFXRound2()
        ClearKeyboardBuffer()

        Dim Round2KB As New Keyboard(Keyboard.GenerateAlphabetMapping(-1)) 'logical shift is actually backwards 1 place, not forward
        If PlayAudio Then Music.Open(New System.Uri(MusicPath & "Mary France 2.mp3")) ' Set background music
        score = PlayRound(Round2KB, False, False)
        If score.Item1 = EscPressedIdent Then Return vbNull
        ScoreTotal += score.Item1
        PenaltyTotal += score.Item2


        ' Round 3
        HeadStart = 10

        ClearKeyboardBuffer()
        GameGFXRound3()
        ClearKeyboardBuffer()

        Dim Round3KB As New Keyboard(Keyboard.GenerateRandomMapping)
        If PlayAudio Then Music.Open(New System.Uri(MusicPath & "Mary France 3.mp3")) ' Set background music
        score = PlayRound(Round3KB, True, True)
        If score.Item1 = EscPressedIdent Then Return vbNull
        ScoreTotal += score.Item1
        PenaltyTotal += score.Item2

        ShowScore(ScoreTotal, PenaltyTotal)
        Return ScoreTotal + PenaltyTotal

    End Function

    Function PlayRound(kb As Keyboard, KBVisible As Boolean, KBPreview As Boolean) As (Single, Single)
        ' takes a given keyboard (from the graphics helper module) and uses the mapping to determine if user is correct
        ' this method handles all rounds, no matter the mappings

        Dim watch As New Stopwatch
        Dim WatchUpdate As New Stopwatch ' prevents timer displaying too often and corrupting console screen when keyhighlight routine resets key in separate thread
        Dim WordYPosition As Short = 10
        Dim KeyboardPosition As Short = 40
        Dim TimerPosition As New CGH.Position(10, 33)
        Dim PenaltyPosition As New CGH.Position(10, 36)
        Dim PenaltyPoints As Short = 0
        Dim Correct As Boolean = False
        Dim Timeout As Short = 40 ' times out after x seconds

        Dim WordString As String = Words.Get3Letter & Words.Get4Letter & Words.Get3Letter ' if this words, remove need to declare 3 different word variables
        Dim XPositionOffset As Short = Centre - 100 ' adjustment for the positions below to enable centring
        Dim XPositions As New List(Of Short)({0, 15, 30, 70, 85, 100, 115, 155, 170, 185}) ' X coordinate of each word on screen
        Dim WordBitmaps As New List(Of Bitmap)

        Dim KeyPress As ConsoleKeyInfo
        Dim KeyLetter As String ' simplifies code to prevent clutter converting to upper

        ' display keyboard & preview if necessary
        Clear()
        If KBVisible Then kb.DrawKeybaord(KeyboardPosition, Alignment.Centre)
        If KBPreview = True Then
            If PlayAudio Then
                SndFX4.Open(New System.Uri(MusicPath & "Glass 01.mp3")) ' Background music
                SndFX4.Position = SFXPosReset
                SndFX4.Play()
            End If
            watch.Restart()
            WatchUpdate.Restart()
            Do
                If WatchUpdate.ElapsedMilliseconds >= 100 Then ' updating console too often causes ContextSwitchDeadlock
                    SetCursorPosition(Centre - 16, KeyboardPosition - 2)
                    ForegroundColor = ConsoleColor.White
                    Write($"Keyboard Preview Ends in: {((KeyboardPreview * 1000) - watch.ElapsedMilliseconds) / 1000,4:N1}")
                    WatchUpdate.Restart()
                End If
                If KeyAvailable Then
                    Select Case Console.ReadKey(True).Key
                        Case ConsoleKey.Escape
                            watch.Stop()
                            If CheckQuit("Do you want to abandon the game?") Then
                                watch.Reset()
                                WatchUpdate.Reset()
                                If PlayAudio Then StopAudio()
                                Return (EscPressedIdent, vbNull)
                            End If
                            watch.Start()
                        Case ConsoleKey.Spacebar, ConsoleKey.Enter
                            Exit Do
                    End Select
                End If
            Loop Until watch.ElapsedMilliseconds >= KeyboardPreview * 1000
            SetCursorPosition(0, KeyboardPosition - 2)
            WriteLine(StrDup(WindowWidth - 1, " ")) ' erase message
            If PlayAudio Then StopAudio()
        End If

        watch.Reset()
        WatchUpdate.Reset()

        ' generate letter bitmaps from each word
        For Each ch As Char In WordString
            WordBitmaps.Add(CGH.TextToBMP(ch.ToString, Color.White, 120, 120, "consolas", 12))
        Next

        ' display bitmaps on the screen
        Dim index As Short = 0
        For Each bmp As Bitmap In WordBitmaps
            CGH.BmpToConsole(XPositions.Item(index) + XPositionOffset, WordYPosition, bmp, Asc(WordString(index)), True, ConsoleColor.Gray) ' use actual letter as shape index
            index += 1
        Next

        ' display timer & penalty placeholder
        SetCursorPosition(TimerPosition.X, TimerPosition.Y - 1)
        ForegroundColor = ConsoleColor.White
        WriteLine("Game Timer")
        SetCursorPosition(PenaltyPosition.X, PenaltyPosition.Y)
        ForegroundColor = ConsoleColor.White
        WriteLine($"Penalty Points: {PenaltyPoints,-3:N0}")

        '  MUSIC
        If PlayAudio Then
            Music.Position = SFXPosReset
            Music.Play()
        End If

        'main loop, exit out on esc or all correct
        index = 0 ' Used to keep track of current letter (while guessing)
        watch.Start()
        WatchUpdate.Start()

        Do ' ToDo: Spamming the keyboard can cause corruption, so maybe introduce key delay?
            ' display timer
            If WatchUpdate.ElapsedMilliseconds >= 100 Then
                ForegroundColor = ConsoleColor.White
                SetCursorPosition(TimerPosition.X, TimerPosition.Y)
                Console.Write($"{((watch.ElapsedMilliseconds) / 1000) - HeadStart,6:N2}")
                WatchUpdate.Restart()
            End If

            If Console.KeyAvailable Then
                KeyPress = ReadKey(True)
                Thread.CurrentThread.Join(10)
                If KeyPress.Key = ConsoleKey.Escape Then
                    watch.Stop()
                    WatchUpdate.Stop()
                    If CheckQuit("Do you want to abandon the game?") Then
                        watch.Reset()
                        WatchUpdate.Reset()
                        If PlayAudio Then StopAudio()
                        Return (EscPressedIdent, 0)
                    End If
                    watch.Start()
                    WatchUpdate.Start()

                Else
                    KeyLetter = KeyPress.KeyChar.ToString.ToUpper
                    If KeyLetter >= "A" And KeyLetter <= "Z" Then
                        ' trigger key
                        If KBVisible Then kb.HighlightKey(kb.StandardToMapped(KeyLetter))
                        If kb.StandardToMapped(KeyLetter) = WordString(index) Then ' correct letter was typed
                            ' display green letter and move to next
                            CGH.BmpToConsole(XPositions.Item(index) + XPositionOffset, WordYPosition, WordBitmaps(index), Asc(WordString(index)), True, ConsoleColor.Green)
                            If index = 2 Or index = 6 Then ' completed a word
                                SndFX3.Play()
                                SndFX3.Position = SFXPosReset
                            Else
                                SndFX2.Play()
                                SndFX2.Position = SFXPosReset
                            End If
                            index += 1
                            If index = WordString.Length Then
                                watch.Stop()
                                Correct = True
                                If PlayAudio Then
                                    Music.Stop()
                                    SndFX4.Open(New System.Uri(MusicPath & "small harp arpeggio.mp3")) ' Background music
                                    SndFX4.Position = SFXPosReset
                                    SndFX4.Play()
                                    Thread.Sleep(2000)
                                    SndFX4.Stop()
                                End If
                            End If
                        Else ' incorrect key pressed
                            PenaltyPoints += KeyPenalty
                            SetCursorPosition(PenaltyPosition.X, PenaltyPosition.Y)
                            ForegroundColor = ConsoleColor.White
                            WriteLine($"Penalty Points: {PenaltyPoints,-3:N0}")
                            SndFX1.Play()
                            SndFX1.Position = SFXPosReset
                            If PenaltyPoints >= 10 And (Not KBVisible Or kb.Overlay = False) Then
                                watch.Stop()
                                KBVisible = True
                                kb.DrawKeybaord(KeyboardPosition, Alignment.Centre)
                                kb.Overlay = True
                                watch.Start()
                            End If
                        End If
                    End If
                End If
            End If
        Loop Until Correct Or (watch.ElapsedMilliseconds) >= (Timeout + HeadStart) * 1000
        If Not Correct Then
            SetCursorPosition(TimerPosition.X, TimerPosition.Y)
            Console.Write($"{((watch.ElapsedMilliseconds) / 1000) - HeadStart,6:N2}")
            CGH.BmpToConsole(0, WordYPosition, CGH.TextToBMP("Out of time!", Color.LightSalmon, 120, 120, , 16),, True, ConsoleColor.Red,,,,,,, 0, 2, Alignment.Centre)

            If PlayAudio Then
                SndFX4.Open(New System.Uri(MusicPath & "Sad Trombones.mp3")) ' Background music
                SndFX4.Position = SFXPosReset
                SndFX4.Play()
                Thread.Sleep(4000)
                SndFX4.Stop()
            End If
        End If

        Dim score As Single = Math.Round(watch.ElapsedMilliseconds / 1000, 2) ' get basic score time to 2dp.  Cannot add penalty as -1.23s with 2 penalty point should not equal score of 1.23
        If PlayAudio Then StopAudio()
        Return (If(score < 0, 0, score), PenaltyPoints) ' cancel out headstart time and add penalty points
    End Function
    Sub ShowScore(Score As Single, penalty As Single)
        Clear()

        If PlayAudio Then
            SndFX3.Open(New System.Uri(MusicPath & "A Big Start - Finale.mp3")) ' Background music
            SndFX3.Position = SFXPosReset
            SndFX3.Play()
        End If

        CGH.BmpToConsole(0, 0, CGH.TextToBMP($"Score: {Score,5:N2}", Color.White, 100, 100),, True, ConsoleColor.Yellow,,,,,,,,, Alignment.Centre)
        Thread.Sleep(800)
        CGH.BmpToConsole(0, 20, CGH.TextToBMP($"Penalty Points: {penalty,3:N0}", Color.White, 100, 100),, True, ConsoleColor.Yellow,,,,,,, 20,, Alignment.Centre)
        Thread.Sleep(700)
        CGH.BmpToConsole(0, 45, CGH.TextToBMP($"Total Points: {penalty + Score,5:N2}", Color.White, 100, 100), &H2736, True, ConsoleColor.White,,,,,,, 20,, Alignment.Centre)
        Thread.Sleep(3000)
        If PlayAudio Then StopAudio()
    End Sub
    Sub GameGFXRound1()
        Clear()
        ClearKeyboardBuffer()
        CGH.BmpToConsole(0, 0, CGH.TextToBMP("Game #2: Keyboard Shift", Color.Yellow, 100, 100), &H25CB, False,,,,,,,, 20,, Alignment.Centre)
        CGH.BmpToConsole(0, 18, CGH.TextToBMP("Part A: Right Shift", Color.Yellow, 100, 100), &H25D8, False,,,,,,,, 15,, Alignment.Centre)

        Dim TextPos As Short = 35

        CGH.CentreText("Oh, dear! The keyboard mappings have shifted. This game will test your keyboard skill like never before!", TextPos, ConsoleColor.Yellow)
        CGH.CentreText("You will only be shown a keyboard for Part A!", TextPos + 2, ConsoleColor.Red)

        CGH.CentreText("Part A: Each letter key has been shifted one place to the right.", TextPos + 5, ConsoleColor.Yellow)

        CGH.CentreText("e.g. For A you press S, for R you press T, etc., with end letters wrapping back around (e.g. L -> A, P -> Q). For 'DOG' you would press keys: FPH.", TextPos + 7, ConsoleColor.Yellow)
        CGH.CentreText($"All you have to do is type 3 random words without mistake. There is a {KeyPenalty} point penalty for each wrong key press!", TextPos + 10, ConsoleColor.Yellow)

        CGH.CentreText(String.Format($"Your time is your score. You are given a {HeadStart} second head start before the timer starts for each round."), TextPos + 13, ConsoleColor.Green)

        PressKeyToStart(WindowHeight - 4, True)
        Clear()
    End Sub
    Sub GameGFXRound2()
        Clear()
        ClearKeyboardBuffer()
        CGH.BmpToConsole(0, 0, CGH.TextToBMP("Game #2: Keyboard Shift", Color.Yellow, 100, 100), &H25CB, False,,,,,,,, 20,, Alignment.Centre)
        CGH.BmpToConsole(0, 18, CGH.TextToBMP("Part B: Alpha shift", Color.Yellow, 100, 100), &H25D8, False,,,,,,,, 15,, Alignment.Centre)

        Dim TextPos As Short = 35

        CGH.CentreText("Oh, dear! The keyboard mappings have shifted. This game will test your keyboard skill like never before!", TextPos, ConsoleColor.Yellow)
        CGH.CentreText("You will not be shown a keyboard for this round (unless you get a number of keys wrong)!", TextPos + 2, ConsoleColor.Red)
        CGH.CentreText("Part B: Each letter key has shifted one place in the ALPHABET.", TextPos + 5, ConsoleColor.Cyan)


        CGH.CentreText("e.g. For A you press B, for B you press C, etc., with Z wrapping back to A. For 'ABC' you press keys: BCD.", TextPos + 8, ConsoleColor.Yellow)
        CGH.CentreText($"All you have to do is type 3 more random words without mistake. There is a {KeyPenalty} point penalty for each wrong key press!", TextPos + 10, ConsoleColor.Yellow)

        CGH.CentreText(String.Format($"Your time is your score. You are given a {HeadStart} second head start before the timer starts for each round."), TextPos + 13, ConsoleColor.Green)

        PressKeyToStart(WindowHeight - 4, True)
        Clear()
    End Sub
    Sub GameGFXRound3()
        Clear()
        ClearKeyboardBuffer()
        CGH.BmpToConsole(0, 0, CGH.TextToBMP("Game #2: Keyboard Shift", Color.Yellow, 100, 100), &H25CB, False,,,,,,,, 20,, Alignment.Centre)
        CGH.BmpToConsole(0, 18, CGH.TextToBMP("Part C: Totally screwed", Color.Yellow, 100, 100), &H25D8, False,,,,,,,, 15,, Alignment.Centre)

        Dim TextPos As Short = 35

        CGH.CentreText("Oh, dear! The keyboard mappings have shifted. This game will test your keyboard skill like never before!", TextPos, ConsoleColor.Yellow)
        CGH.CentreText("You will be shown a keyboard for this round", TextPos + 2, ConsoleColor.Red)
        CGH.CentreText("Part C: Each letter key has shifted to a random place in the keyboard", TextPos + 5, ConsoleColor.Cyan)


        CGH.CentreText($"You will get {KeyboardPreview} seconds to preview the new keyboard before the timer starts.  You can press [space] to start earlier", TextPos + 8, ConsoleColor.Yellow)
        CGH.CentreText($"All you have to do is type 3 more random words without mistake. There is a {KeyPenalty} point penalty for each wrong key press!", TextPos + 10, ConsoleColor.Yellow)

        CGH.CentreText(String.Format($"Your time is your score. You are given a {HeadStart} second head start before the timer starts for each round."), TextPos + 13, ConsoleColor.Green)

        PressKeyToStart(WindowHeight - 4, True)
        Clear()
    End Sub
End Module
