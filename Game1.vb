Imports System.Drawing
Imports System.Console
Imports System.Threading
Imports System.IO

Module Game1
    ' Stop the ball
    ' Part 1: Stop the ball within the box
    ' Part 2: Stop the ball as soon as it drops

    Dim Centre As Short = CInt(WindowWidth / 2) - 1
    Dim TakeBest As Boolean = True ' if true, only the best score from part 2 (part 1 is too easy to get 0 if also included) otherwise takes average
    Dim BallSymbol As Short = &H25CF
    Dim Watch As New Stopwatch ' shared stopwatch for entire game
    Dim PenaltyScore As Short = 20 ' score if press space too soon before drop
    Dim EscPressedIdent As Short = -1 ' value used to trap user pressing escape

    Public Function StartGame(Name As String) As Single
        ' Returns tuple of game score and if the player chose to quit
        If Name = "" Or Name = Nothing Then Name = "Anna Nimmity"
        Watch.Reset() ' fixes an issue with game being re-run

        ForegroundColor = ConsoleColor.White
        BackgroundColor = ConsoleColor.Black

        Dim TotalScore As Short = 0
        ClearKeyboardBuffer()
        DisplayPartAGfx()
        Dim RetValue As Short

        ' ToDo: could add flexibility to this and define a list of tuples (each containing the three data items) and iterate through the list?
        ' This would allow for extra games to be added easily without adding repeating code

        If PlayAudio Then SndFX1.Open(New System.Uri(MusicPath & "Startle.mp3")) ' Audio used for all games

        ' ##### PART A #####
        ' Round one (dropspeed in ms, box opening size, box y location)

        If PlayAudio Then Music.Open(New System.Uri(MusicPath & "Clue1.mp3")) ' Set background music
        RetValue = CatchInBox(40, 5, 60)
        Thread.CurrentThread.Join(10)
        If RetValue = EscPressedIdent Then ' user pressed escape
            If PlayAudio Then StopAudio()
            Return vbNull
        Else
            TotalScore += RetValue
        End If
        ClearKeyboardBuffer()
        If PlayAudio Then StopAudio()

        ' Round two
        If PlayAudio Then Music.Open(New System.Uri(MusicPath & "Clue2.mp3")) ' Set background music
        RetValue = CatchInBox(30, 3, 50)
        Thread.CurrentThread.Join(10)
        If RetValue = EscPressedIdent Then ' user pressed escape
            If PlayAudio Then StopAudio()
            Return vbNull
        Else
            TotalScore += RetValue
        End If
        ClearKeyboardBuffer()
        If PlayAudio Then StopAudio()

        ' Round three
        If PlayAudio Then Music.Open(New System.Uri(MusicPath & "Clue3.mp3")) ' Set background music
        RetValue = CatchInBox(20, 1, 30)
        Thread.CurrentThread.Join(10)
        If RetValue = EscPressedIdent Then ' user pressed escape
            If PlayAudio Then StopAudio()
            Return vbNull
        Else
            TotalScore += RetValue
        End If
        ClearKeyboardBuffer()
        If PlayAudio Then StopAudio()

        ' ##### PART B #####
        DisplayPartBGfx()
        Dim Scores As New List(Of Single) ' futureproof score return if move to fractions
        Thread.CurrentThread.Join(10)

        ' Round One
        If PlayAudio Then Music.Open(New System.Uri(MusicPath & "Tension Build 1.mp3")) ' Set background music
        RetValue = StopWhenFalling(20)
        If RetValue = EscPressedIdent Then ' user pressed escape
            If PlayAudio Then StopAudio()
            Return vbNull
        Else
            Scores.Add(RetValue)
        End If
        If PlayAudio Then StopAudio()

        ' Round Two
        If PlayAudio Then Music.Open(New System.Uri(MusicPath & "Tension Build 1.mp3")) ' Set background music
        RetValue = StopWhenFalling(20)
        If RetValue = EscPressedIdent Then ' user pressed escape
            If PlayAudio Then StopAudio()
            Return vbNull
        Else
            Scores.Add(RetValue)
        End If
        If PlayAudio Then StopAudio()

        ' Round three
        If PlayAudio Then Music.Open(New System.Uri(MusicPath & "Tension Build 2.mp3")) ' Set background music
        RetValue = StopWhenFalling(20)
        If RetValue = EscPressedIdent Then ' user pressed escape
            If PlayAudio Then StopAudio()
            Return vbNull
        Else
            Scores.Add(RetValue)
        End If

        If TakeBest Then
            TotalScore += Scores.Min
        Else
            TotalScore += Scores.Sum
        End If
        If PlayAudio Then StopAudio()

        Clear()
        CGH.BmpToConsole(0, 0, CGH.TextToBMP("Game 1: Total Score", Color.Yellow,,, "Ariel", 12), , False,,,,,,,, 30, 0, Alignment.Centre)
        CGH.BmpToConsole(0, 30, CGH.TextToBMP(String.Format($"{TotalScore}"), Color.Yellow,,, "Ariel", 12), , False,,,,,,,, 0, 0, Alignment.Centre)
        Thread.Sleep(3000)

        Return TotalScore

    End Function
    Function CatchInBox(DropSpeed As Short, BoxOpening As Short, BoxYTop As Short) As Single
        ' runs the ball drop animation and returns offset from box
        ' DropSpeed as ms, BoxOpening in console pixels^2

        Dim BallPos_Y As Short = 0
        Dim BoxXTop As Short = Centre - (BoxOpening \ 2) - 2 ' cannot easily half odd numbrs and display correct size in draw border sub (-1 for actual border symbol)

        Dim rand As New Random
        Randomize()
        ' Music
        If PlayAudio Then
            Music.Position = SFXPosReset
            Music.Play()
        End If

        Dim Wait As Short = (rand.Next(3, 12)) * 1000 ' to get wait time in ms

        Clear()
        DrawTrapClosed(BallPos_Y)

        ' draw box - adjusting for width proportion 
        CGH.DrawBorder(BoxXTop, BoxYTop, BoxXTop + BoxOpening * 1.5 + 1, BoxYTop + BoxOpening + 1, BorderType.SingleThick, ConsoleColor.Blue)

        Select Case RandomOpenTrap(Wait) ' returns 0 in normal operation
            Case PenaltyScore
                Return PenaltyScore
            Case EscPressedIdent
                Return EscPressedIdent
        End Select

        DrawTrapOpen(BallPos_Y)
        If PlayAudio Then
            Music.Stop()
            SndFX1.Position = SFXPosReset
            SndFX1.Play()
        End If

        'Animate ball drop (detect when over box and don't show ball)
        BallPos_Y = DropBall(New List(Of Short)({BoxYTop, BoxYTop + BoxOpening + 1}), BallPos_Y, WindowHeight - 1, DropSpeed)
        If PlayAudio Then StopAudio()

        ' calculate ball position from box - ball only has to be inside the box, or +1 for every line outside
        ' to make calcs simple, assume middle point of box is 0 and calc: |ball|<= abs(box height/2)
        BallPos_Y = Math.Abs(BallPos_Y - (BoxYTop + Math.Ceiling(BoxOpening / 2))) ' move ball relative to 0 box midpoint
        Dim Score = If(BallPos_Y <= Math.Floor(BoxOpening / 2), 0, BallPos_Y - Math.Floor(BoxOpening / 2))

        DisplayScore(Score)
        Return Score
    End Function
    Function StopWhenFalling(DropSpeed As Short) As Single
        ' player needs to stop ball as quickly as possible
        ' DropSpeed as ms

        Dim BallPos_Y As Short = 0
        Dim BallPosOrigin As Short = BallPos_Y

        'ToDo: Could add penalty bar which if ball hits, adds x points

        Dim rand As New Random
        Randomize()

        Dim Wait As Short = (rand.Next(1, 18)) * 1000 ' to get wait time in ms

        ' Music
        If PlayAudio Then
            Music.Position = SFXPosReset
            Music.Play()
        End If

        Clear()
        DrawTrapClosed(BallPos_Y)

        Select Case RandomOpenTrap(Wait)
            Case PenaltyScore
                Return PenaltyScore
            Case EscPressedIdent
                Return EscPressedIdent
        End Select

        DrawTrapOpen(BallPos_Y)

        If PlayAudio Then
            Music.Stop()
            SndFX1.Position = SFXPosReset
            SndFX1.Play()
        End If

        Thread.CurrentThread.Join(10)
        'Animate ball drop (detect when over box and don't show ball)
        BallPos_Y = DropBall(New List(Of Short)(), BallPos_Y, WindowHeight - 1, DropSpeed)
        If PlayAudio Then StopAudio()

        ' calculate ball position from box - from current position to starting position
        Dim Score = BallPos_Y - BallPosOrigin

        DisplayScore(Score)
        Return Score
    End Function
    Sub DisplayPartAGfx()
        Clear()
        If PlayAudio Then
            SndFX2.Open(New System.Uri(MusicPath & "Clue Intro.mp3")) ' Background music
            SndFX2.Position = SFXPosReset
            SndFX2.Play()
        End If

        CGH.BmpToConsole(0, 0, CGH.TextToBMP("Game #1: Stop The Ball", Color.Yellow, 100, 100), &H25CB, False,,,,,,,, 15,, Alignment.Centre)

        CGH.BmpToConsole(0, 18, CGH.TextToBMP("Part A: The Box", Color.Yellow, 100, 100), &H25D8, False,,,,,,,, 15,, Alignment.Centre)

        CGH.CentreText("Three (3) balls will drop (after a random pause) And you need To press 'space' to stop the ball in the box", 32, ConsoleColor.Yellow)

        CGH.CentreText("The ball will drop quicker with each consecutive round", 34, ConsoleColor.Yellow)

        CGH.CentreText("You score zero for getting the ball IN the box.  You score more the further you stop from the box", 37, ConsoleColor.Yellow)

        PressKeyToStart(WindowHeight - 4)
        If PlayAudio Then SndFX2.Stop()
        Clear()
    End Sub
    Sub DisplayPartBGfx()
        Clear()
        If PlayAudio Then
            SndFX2.Open(New System.Uri(MusicPath & "Clue Intro.mp3")) ' Background music but does not loop
            SndFX2.Position = SFXPosReset
            SndFX2.Play()
        End If

        CGH.BmpToConsole(0, 0, CGH.TextToBMP("Game #1: Stop The Ball", Color.Yellow, 100, 100), &H25CB, False,,,,,,,, 15,, Alignment.Centre)

        CGH.BmpToConsole(0, 18, CGH.TextToBMP("Part B: The Trap", Color.Pink, 100, 100), &H25D8, False,,,,,,,, 15,, Alignment.Centre)

        CGH.CentreText("Three more balls will drop and you need to press 'space' to stop the ball as soon as the trap opens", 32, ConsoleColor.Yellow)

        CGH.CentreText("The ball will drop quicker with each consecutive round", 34, ConsoleColor.Yellow)

        CGH.CentreText("You score more the further the ball stops from the starting position", 36, ConsoleColor.Yellow)

        If TakeBest Then
            CGH.CentreText("Your best score will be taken from the three attempts", 40, ConsoleColor.Blue)
        Else
            CGH.CentreText("Your average score will be taken from the three attempts", 42, ConsoleColor.Blue)
        End If

        PressKeyToStart(WindowHeight - 4)
        If PlayAudio Then SndFX2.Stop()
        Clear()

    End Sub
    Sub DisplayScore(score As Single) 'should only be an integer value
        CGH.BmpToConsole(5, 10, CGH.TextToBMP(score.ToString, Color.Green, 150, 150,, 12), If(score > 9, &H221E, 8320 + score), False)
        If PlayAudio Then
            SndFX3.Open(New System.Uri(MusicPath & "Bach Air 02.mp3")) ' Background music
            SndFX3.Position = SFXPosReset
            SndFX3.Play()
            Thread.Sleep(3000)
            SndFX3.Stop()
        End If

    End Sub
    Sub DrawTrapClosed(BallPosY As Short)
        'draw trap closed with ball
        ForegroundColor = ConsoleColor.White
        SetCursorPosition(Centre - 2, BallPosY + 1)
        WriteLine($"{ChrW(&H25CB)}{StrDup(4, ChrW(&H2580))}") ' trap
        SetCursorPosition(Centre, BallPosY)
        Write(ChrW(BallSymbol)) 'ball

    End Sub
    Sub DrawTrapOpen(BallPosY As Short)
        ' doesn't bother redrawing the left circle of the trap
        CGH.DrawCustomLine(Centre - 1, BallPosY + 1, Centre + 3, BallPosY + 1, &H25CB, ConsoleColor.Black)
        SetCursorPosition(Centre - 2, 1) ' Trap in down position
        CGH.DrawCustomLine(Centre - 2, BallPosY + 2, Centre - 2, BallPosY + 3, &H2590, ConsoleColor.White, ConsoleColor.Black)

    End Sub
    Function RandomOpenTrap(WaitTime As Short) As Short
        ' waits a time passed in (allows different subgames to time trap differently

        Watch.Start()
        Do While Watch.ElapsedMilliseconds < WaitTime
            If KeyAvailable Then
                Thread.CurrentThread.Join(10)
                Select Case ReadKey(True).Key
                    Case ConsoleKey.Escape
                        If CheckQuit("Do you want to abandon the game?") Then
                            Watch.Reset()
                            Return EscPressedIdent
                        End If
                    Case ConsoleKey.Spacebar
                        'penalty: too soon
                        CGH.BmpToConsole(0, 40, CGH.TextToBMP("Penalty!", Color.Red, 100, 100),, False,,,,,,,, 5,, Alignment.Centre)
                        Watch.Reset()
                        If PlayAudio Then StopAudio()
                        DisplayScore(PenaltyScore)
                        Return PenaltyScore
                End Select
            End If
        Loop
        Watch.Restart() ' ready for next set of timings
        Return 0 ' dummy value that has no meaning when normal time passes
    End Function
    Function DropBall(NoShowList As List(Of Short), BallPosYStart As Short, BallEndPos As Short, DropSpeed As Short) As Short
        ' animates dropping the ball
        ' If current ball position is listed in NoShowList, then the ball is not shown for that frame
        ' Ends when the user presses space or when BallEndPos is reached
        ' Returns current line when the user pressed 'space'
        Dim finished As Boolean = False

        Do
            If Watch.ElapsedMilliseconds > DropSpeed Then
                ' draw ball in new position
                SetCursorPosition(Centre, BallPosYStart)
                ForegroundColor = ConsoleColor.Black
                If Not (NoShowList.Contains(BallPosYStart)) Then ' Do not blank ball over any shapes
                    Write(ChrW(BallSymbol))
                End If

                If BallPosYStart >= BallEndPos Then
                    finished = True
                Else
                    BallPosYStart += 1
                End If

                SetCursorPosition(Centre, BallPosYStart)
                ForegroundColor = ConsoleColor.White
                If Not (NoShowList.Contains(BallPosYStart)) Then ' Do not draw ball over any shapes
                    Write(ChrW(BallSymbol))
                End If

                Watch.Restart()
            End If
            If KeyAvailable Then ' chosen not to allow escape here as could be used for cheating
                If ReadKey(True).Key = ConsoleKey.Spacebar Then
                    Watch.Reset()
                    finished = True
                End If
            End If
        Loop Until finished
        Return BallPosYStart
    End Function
End Module
