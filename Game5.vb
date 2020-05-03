Imports System.Console
Imports System.Drawing
Imports System.Threading

Module Game5 ' That's My Number
    Dim MissPenalty As Short = 2
    Dim WrongNumPenalty As Short = 5

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

        PenaltyTotal = (score.Item1 * MissPenalty) + (score.Item2 * WrongNumPenalty)

        'Show score
        ShowScore(score.Item1, score.Item2, PenaltyTotal)
        Return PenaltyTotal
    End Function

    Function PlayRound() As (Short, Short) ' number missed, number wrong
        Clear()

        Dim watch As New Stopwatch
        Dim WatchUpdate As New Stopwatch ' prevents timer displaying too often and corrupting console screen when keyhighlight routine resets key in separate thread
        Dim WatchNumber As New Stopwatch ' times how quickly the numbers appear
        Dim NumberSpeed As Short = 1500 ' times how quickly numbers appear
        Dim SpeedRamp As Single = 0.983 ' how quickly the numbers speed up - somewhere between .98 and .99 seems to give the most challenge while remaining doable
        Dim RoundLength As Single = 60 ' number of seconds the round lasts (if changing this, consider different music)

        Dim TimerPosition As New CGH.Position(10, 33)
        Dim MissPenaltyPosition As New CGH.Position(10, 36) ' where to display penalty text
        Dim WrongPenaltyPosition As New CGH.Position(10, 37) ' where to display penalty text
        Dim NumberLinePosition As Short = 5 ' line number that the numbers appear on (numbers are centred)

        Dim NumberWrong As Short = 0
        Dim NumberMissed As Short = 0
        Dim NumberQueue As New Queue(Of Short) ' stores the numbers in the game
        Dim Nums2Generate As Short = 80 ' should be sufficient numbers to generate
        Dim PlayerNumber As Short ' the player's number
        Dim CurrentNumber As Short ' this is the new number
        Dim Pressed As Boolean ' Set to true when the user presses a button 

        ' set up game params
        ClearKeyboardBuffer()
        watch.Reset()
        WatchUpdate.Reset()
        WatchNumber.Reset()
        PlayerNumber = GetPlayerNumber() ' modular so generation routine can easily be swapped out
        GenerateNumbers(PlayerNumber, Nums2Generate, NumberQueue) ' fill number queue (passed in by reference)

        ShowPlayerNumber(PlayerNumber) ' this uses some of the same audio tracks, so use first

        ' set sound effect & music channel(s) up
        If PlayAudio Then
            SndFX1.Open(New System.Uri(MusicPath & "Correct_01.MP3")) ' correct key
            SndFX2.Open(New System.Uri(MusicPath & "Twang.MP3")) ' incorrect key
            SndFX3.Open(New System.Uri(MusicPath & "Bell_01.MP3")) ' next number
            SndFX4.Open(New System.Uri(MusicPath & "Wrong Sound.MP3")) ' Missed number
            Music.Open(New System.Uri(MusicPath & "Four Square.mp3")) ' Background music
        End If

        ' Display penalty indicators and game timer
        SetCursorPosition(TimerPosition.X, TimerPosition.Y)
        ForegroundColor = ConsoleColor.White
        WriteLine("Round Timer: ")

        SetCursorPosition(WrongPenaltyPosition.X, WrongPenaltyPosition.Y)
        ForegroundColor = ConsoleColor.White
        WriteLine($"Wrong Hits: {NumberWrong,-3:N0}")

        SetCursorPosition(MissPenaltyPosition.X, MissPenaltyPosition.Y)
        ForegroundColor = ConsoleColor.White
        WriteLine($"Missed: {NumberWrong,-3:N0}")

        watch.Start()
        WatchUpdate.Start()
        WatchNumber.Start()
        ClearKeyboardBuffer()

        If PlayAudio Then Music.Position = SFXPosReset
        If PlayAudio Then Music.Play()
        Pressed = False ' set to true when user presses space bar (used to detect if number was missed)

        Do ' main game loop
            ' ToDo: Generate another number if user fires space, or simply wait until next is generated (may be easier and more aestetically pleasing)? If former, exit Do

            ' Generate and display number
            CurrentNumber = NumberQueue.Dequeue
            CGH.DrawRectangle(Centre - 15, NumberLinePosition, Centre + 15, NumberLinePosition + 7, ConsoleColor.Black) ' fill space where number was
            CGH.BmpToConsole(0, NumberLinePosition, CGH.TextToBMP(CurrentNumber.ToString("00"), Color.Green), &H2736, False, ,,,,,,,,, Alignment.Centre)
            If PlayAudio Then SndFX3.Position = SFXPosReset
            If PlayAudio Then SndFX3.Play() ' new number effect

            Do ' inner loop that iterates updating timer & watches keyboard until time for a new number
                If KeyAvailable Then
                    Thread.CurrentThread.Join(10)
                    Select Case Console.ReadKey(True).Key
                        Case ConsoleKey.Escape
                            watch.Stop()
                            WatchUpdate.Stop()
                            WatchNumber.Stop()
                            If CheckQuit("Do you want to abandon the game?") Then
                                watch.Reset()
                                WatchUpdate.Reset()
                                If PlayAudio Then StopAudio()
                                Return (EscPressedIdent, 0)
                            End If
                            watch.Start()
                            WatchUpdate.Start()
                            WatchNumber.Start()
                        Case ConsoleKey.Spacebar
                            ' add here logic to determine if the space key was correctly pressed - e.g. based on colour (if implemented) or wildcard symbols, etc.
                            If CurrentNumber = PlayerNumber Then
                                If PlayAudio Then SndFX1.Position = SFXPosReset
                                If PlayAudio Then SndFX1.Play()
                                Pressed = True
                            Else ' was not correct
                                If PlayAudio Then SndFX2.Position = SFXPosReset
                                If PlayAudio Then SndFX2.Play()
                                NumberWrong += 1

                                ' update penalty points
                                SetCursorPosition(WrongPenaltyPosition.X, WrongPenaltyPosition.Y)
                                ForegroundColor = ConsoleColor.White
                                WriteLine($"Wrong Hits: {NumberWrong,-3:N0}")
                            End If
                    End Select
                End If
                ' check if update timer
                If WatchUpdate.ElapsedMilliseconds >= 200 Then ' update on-screen timer
                    ForegroundColor = ConsoleColor.White
                    SetCursorPosition(TimerPosition.X, TimerPosition.Y)
                    Console.Write($"Round Timer: {RoundLength - ((watch.ElapsedMilliseconds / 1000)),3:N0}") ' currently set to display whole seconds
                    WatchUpdate.Restart()
                End If
            Loop Until WatchNumber.ElapsedMilliseconds >= NumberSpeed Or watch.ElapsedMilliseconds >= RoundLength * 1000

            ' Before generate new number, check if player missed their number
            If (Not Pressed And CurrentNumber = PlayerNumber) And watch.ElapsedMilliseconds < RoundLength * 1000 Then ' they sure did
                If PlayAudio Then SndFX4.Position = SFXPosReset
                If PlayAudio Then SndFX4.Play()
                NumberMissed += 1

                ' update penalty points
                SetCursorPosition(MissPenaltyPosition.X, MissPenaltyPosition.Y)
                ForegroundColor = ConsoleColor.White
                WriteLine($"Missed: {NumberMissed,-3:N0}")
            End If

            Pressed = False
            WatchNumber.Restart()
            NumberSpeed *= SpeedRamp ' reduce number speed
        Loop Until watch.ElapsedMilliseconds >= RoundLength * 1000

        watch.Stop()
        WatchNumber.Stop()
        WatchUpdate.Stop()
        StopAudio()

        Return (NumberMissed, NumberWrong) ' only score in this game is penalty as not measuring time
    End Function
    Sub GenerateNumbers(PlayerNum As Short, NumNums As Single, ByRef q As Queue(Of Short))
        ' calls on different number generating algorithms to generate x numbers, which should be sufficient to fill the round
        ' each routine will generate up to 3 instances of the player's number and approximately 5-12 numbers (to add some element of randomness)

        Dim Routine As Short
        Dim Numbers As New List(Of Short)
        Randomize()

        Do While Numbers.Count < NumNums ' don't really care if we generate too many numbers
            Routine = Rand.Next(0, 4)
            Select Case Routine ' determine which random generating routine to call
                Case 0
                    Numbers.AddRange(GenerateRandom(PlayerNum))
                Case 1
                    Numbers.AddRange(GenerateSequence(PlayerNum))
                Case 2
                    Numbers.AddRange(GenerateCloseX(PlayerNum))
                Case 3
                    Numbers.AddRange(GeneratePlayerNum(PlayerNum))
            End Select
        Loop

        For Each n As Short In Numbers ' cannot see easy way to enqueue lists without overloading queue class
            q.Enqueue(n)
        Next

    End Sub
    Function GenerateRandom(PlayerNum As Short) As List(Of Short)
        Dim RandPlayerNum As Short = Rand.Next(0, 4) ' how many player num instances to add
        Dim MaxNum As Short = Rand.Next(6, 10) ' how many nums to generate
        Dim NumLst As New List(Of Short)
        For count As Short = 1 To MaxNum
            NumLst.Add(Rand.Next(1, 100))
        Next

        Dim Instances As Short = NumLst.Where(Function(value) value = PlayerNum).Count ' Use LinQ to see how many times the player's number was generated, so can top up

        For count As Short = 1 To RandPlayerNum - Instances ' if this goes from 1 to 0, it will skip so no need to check end value
            NumLst.Add(PlayerNum)
        Next

        ' shuffle numbers
        ShuffleList(NumLst)
        Return NumLst
    End Function
    Function GenerateSequence(PlayerNum As Short) As List(Of Short)
        ' will build a sequence up to and over number but may or may not include the player's number (generates 4-9 numbers)
        Dim NumLst As New List(Of Short)
        Dim Start, EndPoint As Short
        Randomize()
        Dim skip As Boolean = Rand.Next(0, 4) Mod 2 = 0 ' true if we generate an even number
        Start = PlayerNum - Rand.Next(2, 6) ' start sequence between 2 and 5 places before player number
        EndPoint = PlayerNum + (Rand.Next(1, 3)) ' set end point of sequence

        For Count As Short = Start To EndPoint
            If skip And Count = PlayerNum Then Count += 1 ' skip the player's number
            NumLst.Add(Count)
        Next
        Return NumLst
    End Function
    Function GenerateCloseX(PlayerNum As Short) As List(Of Short)
        ' generates a series of numbers around player's number with at only 1 instance of their number
        Dim NumLst As New List(Of Short)
        Dim Start, EndPoint, NumNums, Num As Short ' starting and end position (before/after player number)
        Randomize()
        NumNums = Rand.Next(3, 7) ' between 3 and 6 numbers
        Dim skip As Boolean = Rand.Next(0, 4) Mod 2 = 0 ' true if we generate an even number
        Start = PlayerNum - Rand.Next(1, 8) ' start sequence between 1 and 6 places before player number
        EndPoint = PlayerNum + (Rand.Next(1, 8)) ' set end point of sequence between 1 and 6 places before player number

        Do
            Num = Rand.Next(Start, EndPoint)
            If Num <> PlayerNum Then NumLst.Add(Num) ' add number if not player's number
        Loop Until NumLst.Count = NumNums

        NumLst.Add(PlayerNum)

        ShuffleList(NumLst)
        Return NumLst
    End Function
    Function GeneratePlayerNum(PlayerNum As Short) As List(Of Short)
        ' Generates 3 in 5 chance of player number with 1 in 2 chance of either number either side, or inverse of player number
        ' ToDo: Re-work so that if number (if generated) is 1 before playernum, add before, or after if 1 after

        Dim NumLst As New List(Of Short)
        Randomize()
        If Rand.Next(0, 6) Mod 2 <> 0 Then NumLst.Add(PlayerNum) ' player number added
        If Rand.Next(1, 2) Mod 2 = 0 Then
            Select Case Rand.Next(0, 3)
                Case 0
                    NumLst.Add(PlayerNum - 1)
                Case 1
                    NumLst.Add(PlayerNum + 1)
                Case 2
                    ' inversion could be done on one line, but that would not aid code speed or being used as educational tool
                    Dim Str As String = If(PlayerNum < 10, "0" & PlayerNum.ToString, PlayerNum.ToString) ' could flip <10 here, but still need to code for 10+ so no point
                    Dim NewStr = Str(1) & Str(0)
                    NumLst.Add(CInt(NewStr))
            End Select
        End If
        Return NumLst ' if list is blank, it will not crash
    End Function
    Sub ShuffleList(ByRef lst As List(Of Short))
        ' shuffle numbers using Knuth-Fisher-Yates algorithm

        Dim Temp, Count, NewPos As Short
        For Count = 0 To lst.Count - 1
            NewPos = Rand.Next(Count, lst.Count - 1)
            Temp = lst(Count)
            lst(Count) = lst(NewPos)
            lst(NewPos) = Temp
        Next
    End Sub

    Function GetPlayerNumber() As Short
        Randomize()
        Return Rand.Next(15, 92) ' number between 15 and 91 inclusive (allows number sequences to go +8 either side and stick within two digit limite
    End Function
    Sub ShowPlayerNumber(Number As Short)
        ' generate a random number and display it on screen
        Clear()

        If PlayAudio Then
            SndFX3.Open(New System.Uri(MusicPath & "starter.mp3"))
            SndFX4.Open(New System.Uri(MusicPath & "Drum roll.mp3"))
        End If
        CGH.BmpToConsole(0, 0, CGH.TextToBMP(PlayerName, Color.White, 100, 100,, 12.5), &H25CF, True, ConsoleColor.White,,,,,,,,, Alignment.Centre)
        CGH.BmpToConsole(0, 20, CGH.TextToBMP("Your Number is", Color.White, 120, 120),, True, ConsoleColor.White,,,,,,,,, Alignment.Centre)
        SndFX4.Position = SFXPosReset
        SndFX4.Play()
        Thread.Sleep(2700)
        CGH.BmpToConsole(0, 45, CGH.TextToBMP(Number.ToString, Color.Cyan, 120, 120),, True, ConsoleColor.Yellow,,,,,,,,, Alignment.Centre)
        Thread.Sleep(2000)
        SndFX4.Stop()
        SndFX3.Position = SFXPosReset
        SndFX3.Play()
        Thread.Sleep(3000)
        SndFX3.Stop()

        Clear()
    End Sub
    Sub GameGFX()
        Clear()
        ClearKeyboardBuffer()
        CGH.BmpToConsole(0, 0, CGH.TextToBMP("Game #5: Number Crunch", Color.Yellow, 100, 100), &H25CB, False,,,,,,,, 20,, Alignment.Centre)

        Dim TextPos As Short = 20

        CGH.CentreText("You will be presented with a number at the start which you need to memorise.", TextPos, ConsoleColor.Yellow)
        CGH.CentreText("You will then see a series of numbers flash up. You need to press [SPACE] when you see you number.", TextPos + 2, ConsoleColor.Yellow)

        CGH.CentreText($"There is a {MissPenalty} point penalty for each time you miss your number, and {WrongNumPenalty} points if you press when it's not your number!", TextPos + 5, ConsoleColor.Yellow)

        CGH.CentreText(String.Format("Good luck as the numbers will flash up at an ever increasing speed until the end of the game."), TextPos + 10, ConsoleColor.Yellow)

        PressKeyToStart(WindowHeight - 4, True)
        Clear()
    End Sub
    Sub ShowScore(Missed As Short, wrong As Short, score As Single) ' this sub leaves the calculations to the calling function

        Clear()

        If PlayAudio Then
            SndFX4.Open(New System.Uri(MusicPath & "A Big Start - Finale.mp3")) ' Background music
            SndFX4.Position = SFXPosReset
            SndFX4.Play()
        End If

        CGH.BmpToConsole(0, 0, CGH.TextToBMP($"Number Missed: {Missed,3:N0}", Color.White, 100, 100),, True, ConsoleColor.Yellow,,,,,,, 15,, Alignment.Centre)
        CGH.BmpToConsole(0, 22, CGH.TextToBMP($"Number Wrong: {wrong,3:N0}", Color.White, 100, 100),, True, ConsoleColor.Yellow,,,,,,, 15,, Alignment.Centre)
        CGH.BmpToConsole(0, 45, CGH.TextToBMP($"Penalty Total: {score,5:N2}", Color.White, 100, 100), &H2736, True, ConsoleColor.White,,,,,,, 10,, Alignment.Centre)
        Thread.Sleep(5000)
        If PlayAudio Then StopAudio()
    End Sub
End Module
