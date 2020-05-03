Imports System.Console
Imports System.Drawing
Imports System.Threading

Module Game6 ' That's My Number
    Dim KeyPenalty As Short = 2
    Dim Headstart As Single = 750 ' headstart in ms
    Dim PlayerName As String
    Dim Centre As Short = CInt(WindowWidth / 2) - 1
    Dim EscPressedIdent As Short = -1 ' value used to trap user pressing escape
    Dim Rand As New Random

    Public Function StartGame(Name As String) As Single
        ' Returns game score and if the player chose to quit

        If Name = "" Or Name = Nothing Then Name = "Anna Nimmity"
        PlayerName = Name

        Dim PenaltyTotal As Single
        Dim score As (Short, Short) = (0, 0)

        ForegroundColor = ConsoleColor.White
        BackgroundColor = ConsoleColor.Black

        ClearKeyboardBuffer()
        GameGFX()
        ClearKeyboardBuffer()

        score = PlayRound()
        If score.Item1 = EscPressedIdent Then Return vbNull

        PenaltyTotal = score.Item1 + score.Item2

        'Show score
        ShowScore(score.Item1, score.Item2)
        Return PenaltyTotal
    End Function

    Function PlayRound() As (Short, Short) ' number missed, number wrong
        Clear()

        Dim watch As New Stopwatch ' times game
        Dim WatchNextLetter As New Stopwatch ' prevents timer displaying too often and corrupting console screen when keyhighlight routine resets key in separate thread
        Dim WatchLetter As New Stopwatch ' times how long player takes to press key
        Dim LetterSpeed As Short = 2000 ' times how quickly numbers appear
        Dim SpeedRamp As Single = 0.975 ' how quickly the numbers speed up - somewhere between .95 and .99 seems to give the most challenge while remaining doable
        Dim GameLength As Single = 31 ' number of seconds the round lasts (if changing this, consider different music)

        Dim TimerPosition As New CGH.Position(10, 33)
        Dim PenaltyPosition As New CGH.Position(10, 37) ' where to display penalty text
        Dim LetterLinePosition As Short = 2 ' line number that the numbers appear on (numbers are centred)
        Dim KeyboardPosition As Short = 25 ' position of onscreen keyboard
        Dim KB As New CGH.Keyboard ' onscreen keyboard
        Dim LetterBMP As Bitmap ' stores the generated letter
        Dim CurrentLetter As Char ' represents the current letter
        Dim DoNext As Boolean = False ' True when player has pressed a key and time to generate new key

        Dim KeyPenalty As Short = 0
        Dim TimePenalty As Single = 0 ' holds total time between key presses, after headstart allowance
        Dim KeyPress As ConsoleKeyInfo
        Dim KeyLetter As String = "" ' simplifies code to prevent clutter converting to upper

        ' set up game params
        ClearKeyboardBuffer()
        watch.Reset()
        'WatchUpdate.Reset()
        WatchLetter.Reset()

        ' set sound effect & music channel(s) up
        If PlayAudio Then
            SndFX1.Open(New System.Uri(MusicPath & "Correct_01.MP3")) ' correct key
            SndFX2.Open(New System.Uri(MusicPath & "Twang.MP3")) ' incorrect key
            SndFX3.Open(New System.Uri(MusicPath & "Bell_01.MP3")) ' next number
            Music.Open(New System.Uri(MusicPath & "Sparks.mp3")) ' Background music
        End If

        ' set up screen
        KB.DrawKeybaord(KeyboardPosition, Alignment.Centre)

        SetCursorPosition(PenaltyPosition.X, PenaltyPosition.Y)
        ForegroundColor = ConsoleColor.White
        WriteLine($"Key Penalty: {KeyPenalty,-3:N0}")


        Randomize()
        If PlayAudio Then
            Music.Position = SFXPosReset
            Music.Play()
        End If

        ' start timers
        watch.Restart()
        Thread.CurrentThread.Join(10)

        Do ' main game loop
            ClearKeyboardBuffer()

            ' Generate letter bitmap & sfx
            DoNext = False ' reset Do Loop flag
            CurrentLetter = Chr(Rand.Next(65, 91))
            LetterBMP = GetLetterBMP(CurrentLetter)

            If PlayAudio Then
                SndFX3.Position = SFXPosReset
                SndFX3.Play()
            End If
            'CGH.DrawRectangle(Centre - 10, LetterLinePosition, Centre + 7, 10, ConsoleColor.Black) 'erase current letter
            CGH.BmpToConsole(0, LetterLinePosition, LetterBMP, Rand.Next(65, 91), True, ConsoleColor.Green,,,,, 0, 0, 0, 0, Alignment.Centre) ' fills with a random letter to confuse
            WatchLetter.Restart() ' restart letter timer now shown
            WatchNextLetter.Restart() ' timer for when next letter due on screen

            ' wait for key press and check
            Do
                If Console.KeyAvailable Then
                    KeyPress = ReadKey(True)
                    Thread.CurrentThread.Join(10)
                    If KeyPress.Key = ConsoleKey.Escape Then
                        watch.Stop()
                        'WatchUpdate.Stop()
                        WatchLetter.Stop()
                        If CheckQuit("Do you want to abandon the game?") Then
                            watch.Reset()
                            'WatchUpdate.Reset()
                            If PlayAudio Then StopAudio()
                            Return (EscPressedIdent, 0)
                        End If
                        watch.Start()
                        WatchLetter.Start()
                        WatchNextLetter.Start()
                    Else
                        KeyLetter = KeyPress.KeyChar.ToString.ToUpper
                        If KeyLetter >= "A" And KeyLetter <= "Z" Then
                            WatchLetter.Stop() ' stop the letter timer
                            DoNext = True
                            If KeyLetter = CurrentLetter Then ' correct key
                                KB.HighlightKey(KeyLetter, True,, ConsoleColor.Green, ConsoleColor.DarkGreen) ' show key press as correct
                                If PlayAudio Then
                                    SndFX1.Position = SFXPosReset
                                    SndFX1.Play()
                                End If
                            Else ' incorrect key press
                                KB.HighlightKey(KeyLetter, True,, ConsoleColor.Red, ConsoleColor.DarkRed) ' show key press as wrong
                                KB.HighlightKey(CurrentLetter, True,, ConsoleColor.Yellow, ConsoleColor.DarkYellow) ' highlight correct key
                                KeyPenalty += Game6.KeyPenalty
                                If PlayAudio Then
                                    SndFX2.Position = SFXPosReset
                                    SndFX2.Play()
                                End If
                                ' update penalty points
                                SetCursorPosition(PenaltyPosition.X, PenaltyPosition.Y)
                                ForegroundColor = ConsoleColor.White
                                WriteLine($"Key Penalty: {KeyPenalty,-3:N0}")
                            End If
                        End If
                    End If
                End If
            Loop Until watch.ElapsedMilliseconds >= GameLength * 1000 Or DoNext ' only come out if end of game

            ' add time to timer (penalty added above)
            WatchLetter.Stop() ' If didn't stop above due to time-out, stop now
            TimePenalty += If(WatchLetter.ElapsedMilliseconds <= Headstart, 0, WatchLetter.ElapsedMilliseconds - Headstart) ' either add 0 or watch time after headstart

            CGH.DrawRectangle(Centre - 10, LetterLinePosition, Centre + 10, LetterLinePosition + 20, ConsoleColor.Black) 'erase current letter

            Do ' loop until time for next letter
                ' do nothing
            Loop Until WatchNextLetter.ElapsedMilliseconds >= LetterSpeed Or watch.ElapsedMilliseconds >= GameLength * 1000
            KB.ResetKey(KeyLetter)
            KB.ResetKey(CurrentLetter)
            LetterSpeed *= SpeedRamp ' decrease letter speed by multiplier

        Loop Until watch.ElapsedMilliseconds >= GameLength * 1000 ' end of game
        StopAudio()
        watch.Reset()
        WatchLetter.Reset()
        WatchNextLetter.Reset()

        Return (TimePenalty / 1000, KeyPenalty)
    End Function
    Function GetLetterBMP(Letter As Char) As Bitmap
        ' generates a letter bitmap and rorates it a random position
        ' First get bitmap
        GetLetterBMP = CGH.TextToBMP(Letter, Color.Green, 120, 120, "Courier New", 12,, 20, 20)

        'GetLetterBMP.RotateFlip(RotateFlipType.Rotate180FlipNone)

        ' rotating 90,180 or 270 may prove too challenging with some letters (e.g. I & H)
        ' ToDo: Optimise this by having this method to draw the letter too, and determine the cropping factor based on rotation

        Randomize()
        Select Case Rand.Next(0, 3) ' determine rotation
            Case 0
                GetLetterBMP.RotateFlip(RotateFlipType.Rotate90FlipNone)
            Case 1
                GetLetterBMP.RotateFlip(RotateFlipType.Rotate180FlipNone)
            Case 2
                GetLetterBMP.RotateFlip(RotateFlipType.Rotate270FlipNone)
        End Select
    End Function

    Sub GameGFX()
        Clear()
        ClearKeyboardBuffer()
        CGH.BmpToConsole(0, 0, CGH.TextToBMP("Game #6:Keyboard Warrior", Color.Yellow, 100, 100), &H25CB, False,,,,,,,, 30,, Alignment.Centre)

        Dim TextPos As Short = 20

        CGH.CentreText("Your final game is another simple test of reflexes, keyboard accuracy and mental agility.", TextPos, ConsoleColor.Yellow)
        CGH.CentreText("You you be shown a letter and you must press it as quickly as possible. The only problem is, the letter will be rotated 90°, 180° or 270°.", TextPos + 2, ConsoleColor.Yellow)

        CGH.CentreText($"You will be given {(If(Headstart = 0, "no", If(Headstart = 1, "a 1 second", Format(Headstart / 1000, "0.0") & " seconds")))} headstart before the game starts timing your reaction for each letter.", TextPos + 4, ConsoleColor.Yellow)

        CGH.CentreText($"There is a {KeyPenalty} point penalty if you press the wrong letter!", TextPos + 6, ConsoleColor.Yellow)

        CGH.CentreText(String.Format("Good luck as the letters will flash up at an ever increasing speed until the end of the 30 seconds."), TextPos + 10, ConsoleColor.Yellow)

        PressKeyToStart(WindowHeight - 4, True)
        Clear()
    End Sub
    Sub ShowScore(TimeTook As Short, Penalty As Short)

        Clear()
        If PlayAudio Then
            SndFX4.Open(New System.Uri(MusicPath & "Bach Air 02.mp3")) ' Background music
            SndFX4.Position = SFXPosReset
            SndFX4.Play()
        End If

        CGH.BmpToConsole(0, 0, CGH.TextToBMP($"Time Penalty: {TimeTook,3:N0}", Color.White, 100, 100),, True, ConsoleColor.Yellow,,,,,,, 15,, Alignment.Centre)
        CGH.BmpToConsole(0, 22, CGH.TextToBMP($"Key Penalty: {Penalty,3:N0}", Color.White, 100, 100),, True, ConsoleColor.Yellow,,,,,,, 15,, Alignment.Centre)
        CGH.BmpToConsole(0, 45, CGH.TextToBMP($"Game Total: {TimeTook + Penalty,5:N2}", Color.White, 100, 100), &H2736, True, ConsoleColor.White,,,,,,, 10,, Alignment.Centre)
        Thread.Sleep(5000)
        If PlayAudio Then StopAudio()
    End Sub

End Module
