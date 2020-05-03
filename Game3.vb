Imports System.Console
Imports System.Drawing
Imports System.Threading

Module Game3
    ' Game 3: Time guess
    Public Function StartGame(Name As String) As Single
        ' Returns game score and if the player chose to quit

        If Name = "" Or Name = Nothing Then Name = "Anna Nimmity"

        Dim Watch As New Stopwatch
        Dim Centre As Short = CInt(WindowWidth / 2) - 1
        Dim Rand As New Random
        Randomize()

        Watch.Reset() ' fixes an issue with game being re-run

        ForegroundColor = ConsoleColor.White
        BackgroundColor = ConsoleColor.Black

        ClearKeyboardBuffer()
        GameGFX()
        ClearKeyboardBuffer()


        If PlayAudio Then Music.Open(New System.Uri(MusicPath & "Back In Toon - Swords.mp3")) ' Set background music

        ' generate random time
        Dim TimePassage As Short = Rand.Next(13, 21) * 1000 ' between 13 and 20 seconds (longer than 20s will require new music)

        Clear()
        CGH.BmpToConsole(0, 0, CGH.TextToBMP(String.Format("Your Time: {0} seconds", TimePassage / 1000), Color.White, 100, 100,, 10,), &H25D8, False,,,,,,,,,, Alignment.Centre)

        CentreText("Press space to begin", 30, ConsoleColor.Yellow)
        CGH.DrawBorder(Centre - 15, 29, Centre + 15, 31, BorderType.SingleLineCurved, ConsoleColor.DarkYellow)

        ' first loop, watch for space (and escape)
        Do
            If KeyAvailable Then
                Select Case ReadKey(True).Key
                    Case ConsoleKey.Escape
                        If CheckQuit("Abandon the game?") = True Then
                            StopAudio()
                            Return vbNull  ' user quit
                        End If
                    Case ConsoleKey.Spacebar
                        If PlayAudio Then
                            Music.Position = SFXPosReset
                            Music.Play()
                        End If
                        Exit Do
                End Select
            End If
        Loop
        CGH.DrawBorder(Centre - 15, 29, Centre + 15, 31, BorderType.SingleLineCurved, ConsoleColor.Red)
        ' now start timer
        Watch.Start()
        Thread.CurrentThread.Join(10)
        CentreText("Press space again to stop ", 30, ConsoleColor.Yellow)
        Do
            ' give user 10 seconds past their alloted time and abort
            If Watch.ElapsedMilliseconds >= TimePassage + 10000 Then
                Watch.Stop()
                Exit Do
            End If
            If KeyAvailable Then
                Watch.Stop() ' pause watch
                Thread.CurrentThread.Join(10)
                Select Case ReadKey(True).Key
                    Case ConsoleKey.Escape
                        If CheckQuit("Abandon the game?") = True Then
                            StopAudio()
                            Return vbNull  ' user quit
                        End If
                    Case ConsoleKey.Spacebar
                        Exit Do
                End Select
            End If
            Watch.Start() ' continue watch
        Loop
        StopAudio()

        Clear()
        Dim Score As Single = Math.Abs(Watch.ElapsedMilliseconds - TimePassage) / 1000

        CGH.BmpToConsole(0, 0, CGH.TextToBMP("Your Score", Color.White, 120, 120,, 12,), , False,,,,,,,,,, Alignment.Centre)
        CGH.BmpToConsole(0, 30, CGH.TextToBMP(String.Format("{0:0.000}", Score), Color.White, 120, 120,, 10,), &H25D8, False,,,,,,,,,, Alignment.Centre)
        If Score < 1.5 Then
            CGH.BmpToConsole(0, 50, CGH.TextToBMP(String.Format("Amazing!", Score), Color.Yellow, 120, 120,, 12,), &H25D8, False,,,,,,,,,, Alignment.Centre)
        End If

        Thread.Sleep(3000)
        Return Score
    End Function
    Sub GameGFX()
        Clear()
        CGH.BmpToConsole(0, 0, CGH.TextToBMP("Game #3: Time Commander", Color.Yellow, 100, 100), &H25CB, False,,,,,,,, 15,, Alignment.Centre)

        ForegroundColor = ConsoleColor.Yellow

        CGH.CentreText("A simple game whereby you need to to guess the passage of time.", 19, ConsoleColor.Yellow)

        CGH.CentreText("You will be shown a number of seconds and you need to stop when you feel that time has elapsed.", 24, ConsoleColor.Yellow)
        CGH.CentreText("You will press space to begin the time and 'space' again to stop", 26, ConsoleColor.Yellow)

        CGH.CentreText("You score is based on the number of seconds you are out.", 30, ConsoleColor.Green)

        PressKeyToStart(WindowHeight - 4, True)
        Clear()
    End Sub

End Module
