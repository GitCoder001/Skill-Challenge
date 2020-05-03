Imports System.Console
Imports System.Drawing
Imports System.Threading
Public Enum Mode
    AllKeys ' press all keys (any order) but double-press releases all keys held down
    AtoZ ' type A to Z as quickly as possible
    KeyBomb ' All keys with bombs placed around
    AtoZBomb ' A to Z mode with some bombs placed
End Enum
Module Game4
    Dim KeyPenalty As Short = 2
    Dim BombPenalty As Short = 5
    Dim HeadStart As Short = 5 ' number of seconds headstart given before timer
    Dim ForceRestart As Boolean = True ' determines if player has to start from A if they go wrong (doesn't count bombs)
    Dim GameMode As Mode ' determines current mode
    Dim RemoveBomb As Boolean = False ' if true, bomb is removed once hit (only if force restart is enabled)
    Dim BombReset As Boolean = True ' if True, hitting a bomb resets the rount (irrespective of ForceRestart)
    Dim MusicPosition As New TimeSpan ' determines where the music will start from
    Dim KeysPressed, BombKeys As New List(Of Char) ' lazy programming as these should really be passed between helper methods

    Dim Centre As Short = CInt(WindowWidth / 2) - 1
    Dim EscPressedIdent As Short = -1 ' value used to trap user pressing escape
    Dim Rand As New Random

    Dim SndFX5 As New Windows.Media.MediaPlayer ' as this game uses lots of effects which are pre-set early on, so cannot overwrite any

    Public Function StartGame(Name As String) As Single
        ' Returns game score and if the player chose to quit

        If Name = "" Or Name = Nothing Then Name = "Anna Nimmity"

        Dim ScoreTotal, PenaltyTotal As Single
        Dim score As (Single, Single) = (0, 0)

        ' sequence running order of games.  List contains game mode, head start in seconds, restart on wrong key, restart on bomb hit
        Dim GameOrder As New List(Of (Mode, Short, Boolean, Boolean)) From {{(Mode.AllKeys, 4, True, True)}, {(Mode.AtoZ, 8, False, True)}, {(Mode.AtoZBomb, 12, True, True)}, {(Mode.KeyBomb, 8, True, True)}}

        ForegroundColor = ConsoleColor.White
        BackgroundColor = ConsoleColor.Black


        ' Play rounds
        For Each GameTuple In GameOrder
            GameMode = GameTuple.Item1
            HeadStart = GameTuple.Item2
            ForceRestart = GameTuple.Item3
            BombReset = GameTuple.Item4

            ClearKeyboardBuffer()
            GameGFX()
            ClearKeyboardBuffer()

            score = PlayRound()
            If score.Item1 = EscPressedIdent Then Return vbNull
            ScoreTotal += score.Item1
            PenaltyTotal += score.Item2
            ShowRoundScore(score.Item1, score.Item2)
        Next

        'Show score
        ShowScore(ScoreTotal, PenaltyTotal)
        Return ScoreTotal + PenaltyTotal

    End Function
    Function PlayRound() As (Single, Single) ' returns time points and penalty points
        Clear()

        Dim watch As New Stopwatch
        Dim WatchUpdate As New Stopwatch ' prevents timer displaying too often and corrupting console screen when keyhighlight routine resets key in separate thread
        Dim KeyboardPosition As Short = 15 ' where to display the on-screen keyboard
        Dim AlphabetPosition As Short = 65 ' uses ASCII to determine next value in alphabet

        Dim ShowTimer As Boolean = False ' not showing the timer will leave more suspense
        Dim TimerPosition As New CGH.Position(10, 33)
        Dim PenaltyPosition As New CGH.Position(10, 36) ' where to display penalty text
        Dim PenaltyPoints As Short = 0
        Dim Timeout As Short = 30 ' times out after x seconds
        Dim StartingKey As Short = 65 ' the starting key

        Dim KeyPress As ConsoleKeyInfo
        Dim KeyLetter As String ' simplifies code to prevent clutter converting to upper
        Dim kb As New Keyboard()

        ' These params are used to determine the game logic
        Dim Correct As Boolean = False 'gets out of main game loop based on player completing the round's requirements
        Dim SequenceMode As Boolean = True ' if true, player must do keys in sequence - set by CASE
        Dim BombMode As Boolean = True ' if true, will check for bombs - set by CASE
        Dim CorrectKey As Boolean = False ' Was the key press correct.  Prevents duplicate logic based on different game modes
        Dim HitBomb As Boolean = False ' True if bomb was hit

        kb.DrawKeybaord(KeyboardPosition, Alignment.Centre)

        ' set up game params
        ' game headstart is set from calling routine
        ClearKeyboardBuffer()
        KeysPressed.Clear()
        BombKeys.Clear()
        watch.Reset()

        ' set sound effect channels up
        If PlayAudio Then
            SndFX1.Open(New System.Uri(MusicPath & "Bell_01.MP3")) ' correct key
            SndFX2.Open(New System.Uri(MusicPath & "Wrong 1.MP3")) ' incorrect key
            SndFX3.Open(New System.Uri(MusicPath & "bomb.MP3")) ' bomb hit
            SndFX4.Open(New System.Uri(MusicPath & "twang.MP3")) ' reset board
            Music.Open(New System.Uri(MusicPath & "Hocus Pocus 02.mp3")) ' Background music
        End If

        Select Case GameMode
            Case Mode.AllKeys
                SequenceMode = False
                BombMode = False
                ' set music timespan
                MusicPosition = New TimeSpan(0, 0, 0)
            Case Mode.AtoZ
                SequenceMode = True
                BombMode = False
                StartingKey = 65 ' A
                ' set music timespan
                MusicPosition = New TimeSpan(0, 0, 0, 32, 1)
            Case Mode.AtoZBomb
                SequenceMode = True
                BombMode = True
                StartingKey = 65 ' A
                GenerateBombsList(5) ' set 5 key bombs
                SetBombsKeyboard(kb)
                ' set music timespan
                MusicPosition = New TimeSpan(0, 0, 1, 18, 5)
            Case Mode.KeyBomb
                SequenceMode = False
                BombMode = True
                GenerateBombsList(9) ' set 9 key bombs
                SetBombsKeyboard(kb)
                ' set music timespan
                MusicPosition = New TimeSpan(0, 0, 1, 42, 59)
        End Select

        Thread.CurrentThread.Join(10) ' prevents thread execution timeout issue with musicplayer

        ' Music
        Music.Position = MusicPosition
        Music.Play()

        ' Initial graphics
        SetCursorPosition(PenaltyPosition.X, PenaltyPosition.Y)
        ForegroundColor = ConsoleColor.White
        WriteLine($"Penalty Points: {PenaltyPoints,-3:N0}")

        watch.Start()
        ' Display timer?
        If ShowTimer Then
            SetCursorPosition(TimerPosition.X, TimerPosition.Y - 1)
            ForegroundColor = ConsoleColor.White
            WriteLine("Game Timer")
            WatchUpdate.Start() ' if we're showing the timer, start the stopwatch that controls when the time is screen refreshed
        End If

        Do ' main logic loop
            If ShowTimer And WatchUpdate.ElapsedMilliseconds >= 100 Then ' update on-screen timer if set to be shown
                ForegroundColor = ConsoleColor.White
                SetCursorPosition(TimerPosition.X, TimerPosition.Y)
                Console.Write($"{((watch.ElapsedMilliseconds) / 1000) - HeadStart,6:N2}")
                WatchUpdate.Restart()
            End If

            If Console.KeyAvailable Then ' only do logic checks if user presses a key
                KeyPress = ReadKey(True)
                Thread.CurrentThread.Join(10)
                If KeyPress.Key = ConsoleKey.Escape Then ' Check if player wants to quit
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
                    KeyLetter = KeyPress.KeyChar.ToString.ToUpper ' Get U/Case Char of current key press (reduces code)
                    If KeyLetter >= "A" And KeyLetter <= "Z" Then ' We only care about the A to Z keys for this game
                        watch.Stop() ' stop timer while check if key is correct
                        ' First, determine if key press is valid
                        If SequenceMode Then ' Keys must be typed in specific sequence
                            If KeyLetter = Chr(AlphabetPosition) Then ' correct letter pressed
                                CorrectKey = True
                                AlphabetPosition = NextPosition(AlphabetPosition) ' what is the next key press to check (don't care about -1 as check keypresses separately @ end)
                            Else
                                CorrectKey = False
                            End If
                        Else ' Key order does not matter
                            If KeysPressed.Contains(KeyLetter) Then ' key was already pressed
                                CorrectKey = False
                            Else
                                CorrectKey = True

                            End If
                        End If
                        ' Check if bomb was hit (if bomb mode is enabled)
                        If BombMode Then
                            If BombKeys.Contains(KeyLetter) Then
                                CorrectKey = False
                                HitBomb = True
                            Else
                                HitBomb = False
                                ' do not set key to correct here, as it still may not be
                            End If
                        End If

                        ' Now act on logic from above
                        If CorrectKey = True Then
                            ' handle true press
                            KeysPressed.Add(KeyLetter)
                            kb.HighlightKey(KeyLetter, True,, ConsoleColor.DarkGray, ConsoleColor.DarkGray)
                            SndFX1.Play()
                            SndFX1.Position = SFXPosReset

                        Else ' wrong key - determine next action
                            If HitBomb Then
                                PenaltyPoints += BombPenalty
                                SndFX3.Play()
                                SndFX3.Position = SFXPosReset
                                Thread.Sleep(300)
                                If RemoveBomb And (ForceRestart Or BombReset) Then BombKeys.Remove(KeyLetter) ' remove bomb from the list
                            Else
                                PenaltyPoints += KeyPenalty
                                SndFX2.Play()
                                SndFX2.Position = SFXPosReset
                                Thread.Sleep(300)
                            End If

                            ' update penalty points
                            SetCursorPosition(PenaltyPosition.X, PenaltyPosition.Y)
                            ForegroundColor = ConsoleColor.White
                            WriteLine($"Penalty Points: {PenaltyPoints,-3:N0}")

                            If ForceRestart Or (HitBomb And BombReset) Then
                                ' play sound effect
                                SndFX4.Play()
                                SndFX4.Position = SFXPosReset

                                KeysPressed.Clear()
                                If BombMode Then KeysPressed.AddRange(BombKeys) ' add bombs back in
                                kb.ShowKeyboard() ' refresh keyboard
                                SetBombsKeyboard(kb)

                                If SequenceMode Then AlphabetPosition = StartingKey ' reset back to initial letter of sequence
                                watch.Start()
                            Else ' still add key to keys pressed list (if necessary)
                                If Not SequenceMode Then KeysPressed.Add(KeyLetter) ' do not add letter if sequence mode as stil need to form chain
                                kb.HighlightKey(KeyLetter, True,, ConsoleColor.DarkRed, ConsoleColor.White) ' accent incorrect key press
                            End If

                        End If

                        ' Finally, check if player has won the round (as may not require reset)
                        If KeysPressed.Count = 26 Then ' player has won the round
                            Correct = True
                        Else
                            watch.Start() ' restart watch
                            ClearKeyboardBuffer() ' prevent errant keys messing up game, or player spamming keyboard
                        End If
                    End If
                End If
            End If
        Loop Until Correct Or (watch.ElapsedMilliseconds) >= (Timeout + HeadStart) * 1000
        Music.Stop() ' stop background music

        If Not Correct Then
            SetCursorPosition(TimerPosition.X, TimerPosition.Y)
            Console.Write($"{((watch.ElapsedMilliseconds) / 1000) - HeadStart,6:N2}")
            CGH.BmpToConsole(0, 40, CGH.TextToBMP("Out of time!", Color.LightSalmon, 120, 120, , 16),, True, ConsoleColor.Red,,,,,,, 15, 2, Alignment.Centre)
            If PlayAudio Then
                SndFX5.Open(New System.Uri(MusicPath & "Sad Trombones.mp3")) ' Background music
                SndFX5.Position = SFXPosReset
                SndFX5.Play()
                Thread.Sleep(4000)
                SndFX5.Stop()
            End If
        End If

        Dim score As Single = Math.Round(watch.ElapsedMilliseconds / 1000, 2) ' get basic score time to 2dp.  Cannot add penalty as -1.23s with 2 penalty point should not equal score of 1.23
        If PlayAudio Then StopAudio()

        Return (If(score < 0, 0, score), PenaltyPoints) ' cancel out headstart time and add penalty points

    End Function
    Function NextPosition(CurrentSequence As Short) As Short
        ' takes the current alphabet position and returns the next that needs to be selected
        ' returns -1 if this is the final key in the sequence
        ' this enables a change in logic to go from ZtoA if the game needs to be made harder

        NextPosition = CurrentSequence + 1 ' start at current place
        Do Until Not BombKeys.Contains(Chr(NextPosition))
            NextPosition += 1
        Loop
        If NextPosition > 90 Then NextPosition = -1 ' 90 = 'Z' in ASCII
    End Function
    Sub GenerateBombsList(Number As Short)
        ' will populate the BombKeys list and keys list
        Randomize()
        Dim RandChar As Char
        For count = 1 To Number
            Do
                RandChar = Chr(Rand.Next(0, 26) + 65) ' generate a random character
            Loop While BombKeys.Contains(RandChar)
            BombKeys.Add(RandChar)
            KeysPressed.Add(RandChar) ' add this to player's total as they cannot press these
        Next

    End Sub
    Sub SetBombsKeyboard(ByRef kb As Keyboard)
        ' iterates through bombkey list and sets keys accordingly
        ' no bombs, no keys set, so safe to call from any mode
        For Each ch In BombKeys
            kb.HighlightKey(ch, True,, ConsoleColor.Red, ConsoleColor.DarkRed)
        Next
    End Sub
    Sub ShowRoundScore(score As Single, penalty As Single)
        Clear()

        If PlayAudio Then
            SndFX5.Open(New System.Uri(MusicPath & "Bach Air 01.mp3")) ' Background music
            SndFX5.Position = SFXPosReset
        End If

        CGH.BmpToConsole(0, 0, CGH.TextToBMP($"Score: {score,5:N2}", Color.White, 100, 100),, True, ConsoleColor.Yellow,,,,,,,,, Alignment.Centre)
        CGH.BmpToConsole(0, 20, CGH.TextToBMP($"Penalty Points: {penalty,3:N0}", Color.White, 100, 100),, True, ConsoleColor.Yellow,,,,,,, 20,, Alignment.Centre)
        CGH.BmpToConsole(0, 45, CGH.TextToBMP($"Round Points: {penalty + score,5:N2}", Color.White, 100, 100), &H2736, True, ConsoleColor.White,,,,,,, 20,, Alignment.Centre)

        If PlayAudio Then SndFX5.Play()
        Thread.Sleep(4000)
        SndFX5.Stop()

    End Sub
    Sub ShowScore(Score As Single, penalty As Single)
        Clear()
        If PlayAudio Then
            SndFX5.Open(New System.Uri(MusicPath & "Fool's Errand.mp3")) ' Background music
            SndFX5.Position = SFXPosReset
            SndFX5.Play()
        End If

        CGH.BmpToConsole(0, 0, CGH.TextToBMP($"Total Score: {Score,5:N2}", Color.White, 100, 100),, True, ConsoleColor.Yellow,,,,,,,,, Alignment.Centre)
        Thread.Sleep(1400)
        CGH.BmpToConsole(0, 20, CGH.TextToBMP($"Total Penalties: {penalty,3:N0}", Color.White, 100, 100),, True, ConsoleColor.Yellow,,,,,,, 15,, Alignment.Centre)
        Thread.Sleep(1400)
        CGH.BmpToConsole(0, 45, CGH.TextToBMP($"Game Total: {penalty + Score,5:N2}", Color.White, 100, 100), &H2736, True, ConsoleColor.White,,,,,,, 10,, Alignment.Centre)
        Thread.Sleep(3000)
        If PlayAudio Then SndFX5.Stop()
    End Sub

    Sub GameGFX()
        Clear()
        ClearKeyboardBuffer()
        Dim Title, Instruction1, Instruction2 As String
        Title = ""
        Instruction1 = ""
        Instruction2 = ""

        Select Case GameMode
            Case Mode.AllKeys
                Title = "Part A: Mashup!"
                Instruction1 = "Simple one this, press all of the alphabet keys (any order) as quickly as possible but each key only once or penalty."
            Case Mode.AtoZ
                Title = "Part B: Alphabet Chase!"
                Instruction1 = "Another keyboard skill game.  You need to type the alphabet as quickly as possible (from A to Z)"
            Case Mode.AtoZBomb
                Title = "Part C: Alphabet...Boom!"
                Instruction1 = "Back To the alphabet. You need To type the alphabet As quickly As possible (from A To Z)"
                Instruction2 = $"However, some keys have bombs laid out (in red), so you MUST avoid these or receive a {BombPenalty} point penalty"
            Case Mode.KeyBomb
                Title = "Part D: Mashup...Boom!"
                Instruction1 = "Finally, back to mashing the keyboard. Same rulez apply: any order but each key can ONLY be pressed once. However..."
                Instruction2 = $"there are more bombs, so you MUST avoid these or receive a {BombPenalty} point penalty"
        End Select

        CGH.BmpToConsole(0, 0, CGH.TextToBMP("Game #4: Alphabet Chase", Color.Yellow, 100, 100), &H25CB, False,,,,,,,, 20,, Alignment.Centre)
        CGH.BmpToConsole(0, 18, CGH.TextToBMP(Title, Color.Yellow, 100, 100,, 10), &H25D8, False,,,,,,,, 15,, Alignment.Centre)

        Dim TextPos As Short = 35

        CGH.CentreText(Instruction1, TextPos, ConsoleColor.Yellow)
        CGH.CentreText(Instruction2, TextPos + 2, ConsoleColor.Yellow)

        CGH.CentreText(String.Format("If you make a mistake, you will{0} need to go back to the beginning", If(ForceRestart, "", " NOT")), TextPos + 5, ConsoleColor.Yellow)

        CGH.CentreText($"There is a {KeyPenalty} point penalty for an incorrect key press.", TextPos + 7, ConsoleColor.Yellow)

        CGH.CentreText(String.Format($"Your time is your score. You are given a {HeadStart} second head start before the timer starts."), TextPos + 10, ConsoleColor.Green)

        PressKeyToStart(WindowHeight - 4, True)
        Clear()
    End Sub
End Module
