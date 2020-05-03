'#####################################################################
'#                Skill Testing Game: L Minett 2020                  #
'# ----------------------------------------------------------------- #
'# Ideas for improvment:                                             #
'#  1) add more challenges (possibly random)                         #
'#  2) add subscores to high-score table, so players can become      #
'#     competitive                                                   #                           #
'#  3) Load games at random. Would need to remove number references  #
'#                                                                   #
'#                                                                   #
'#                                                                   #
'#                                                                   #
'#####################################################################

' Side note: Game is able to play two sounds simultaneously by making use of Windows Media Player
' To do this in another project, add a project reference to PresentationCore and WindowsBase
' Then numerous reference variables to MediaPlayer can be made to play their own independent sounds
' E.g.
'   Dim Snd1 As New Windows.Media.MediaPlayer
'   Snd1.Open(New System.Uri(path & "Back In Toon - Swords.MP3"))        
'   Snd1.Play()

' ToDo: GetPlayerName needs some work due to letters being chopped off when generated (due to auto cropping or font kerning), generate single letters?

Imports System.Drawing
Imports System.Console
Imports System.IO
Imports System.Text
Imports System.Threading
' Each game is contained in a separate module
Structure StrGameLog ' used across all game modules
    Dim PlayerName As String
    Dim TotalScore As Single
    Dim Game1Score As Single ' ToDo: Make into an array (only intended on having 3 games originally)
    Dim Game2Score As Single
    Dim Game3Score As Single
    Dim Game4Score As Single
    Dim Game5Score As Single
    Dim Game6Score As Single
End Structure
Module Main
    Structure StrHighScore
        Dim Score As Single
        Dim Name As String
        Sub New(Sc As Single, Player As String)
            Score = Sc
            Name = If(Player.Length = 0, "Sum Ting Wong", Player) 'default name if nothing presented
        End Sub
    End Structure

#Region "Globals"
    Public FilePath As String = Directory.GetCurrentDirectory & "\" ' ToDoL persist into my.resources
    Public MusicPath As String = FilePath & "..\..\My Project\Music\" ' ToDo: possibly change this when game is packaged or at least test on a different machine
    Public SFXPosReset As New TimeSpan(0)
    Dim HighScoreFile As String = FilePath & "score.dat"
    Dim HighScore As New List(Of StrHighScore)
    Public PlayAudio As Boolean
    Public SndFX1 As New Windows.Media.MediaPlayer ' Can pre-load up sound effects in game and play when needed
    Public SndFX2 As New Windows.Media.MediaPlayer ' These are public (global), so as to not have too many instances in memory across the different games
    Public SndFX3 As New Windows.Media.MediaPlayer
    Public SndFX4 As New Windows.Media.MediaPlayer
    Public Music As New Windows.Media.MediaPlayer
#End Region

    Sub Main()

        ' Set up console environment
        CGH.Win32Native.SetConsoleWindowPosition(0, 0)
        CGH.Win32Native.MaximizeConsoleWindow()
        OutputEncoding = Encoding.UTF8
        CursorVisible = False

        LoadHighScoreFile()

        'CreateDefaultHighScore() ' ToDo: clear this line once testing is complete

        PlayAudio = My.Computer.FileSystem.FileExists(MusicPath & "Twang.mp3") ' can we see the audio files? If not, don't play audio

        Do ' main loop
            Clear()

            ForegroundColor = ConsoleColor.White

            DisplayHighScore()  ' Y position of where high score graphics can be placed

            PlayGames()

        Loop
    End Sub
    Sub PlayGames()
        Dim CurrentGame As StrGameLog
        ' Each game will return a score or vbNull if the player exits out of the game.
        ClearKeyboardBuffer()
        CurrentGame.PlayerName = GetPlayerName()

        If IsNothing(CurrentGame.PlayerName) Then Exit Sub ' User pressed 'esc' on name enter, so flow back to intro screen

        ' ToDo: Randomise game sequence by shuffling a list of game numbers (internally each game is linked, but then pass actual sequence number to each rount to display)
        ' use a Select Case to determine which function is triggered

        'load game 1
        Dim Game = Game1.StartGame(CurrentGame.PlayerName)
        If Game = vbNull Then Exit Sub
        CurrentGame.Game1Score = Game
        CurrentGame.TotalScore += Game

        ' Load game 2
        Game = Game2.StartGame(CurrentGame.PlayerName)
        If Game = vbNull Then Exit Sub
        CurrentGame.Game2Score = Game
        CurrentGame.TotalScore += Game

        ' Load game 3
        Game = Game3.StartGame(CurrentGame.PlayerName)
        If Game = vbNull Then Exit Sub
        CurrentGame.Game3Score = Game
        CurrentGame.TotalScore += Game

        ' Load game 4
        Game = Game4.StartGame(CurrentGame.PlayerName)
        If Game = vbNull Then Exit Sub
        CurrentGame.Game4Score = Game
        CurrentGame.TotalScore += Game

        ' Load game 5
        Game = Game5.StartGame(CurrentGame.PlayerName)
        If Game = vbNull Then Exit Sub
        CurrentGame.Game5Score = Game
        CurrentGame.TotalScore += Game

        ' Load game 6
        Game = Game6.StartGame(CurrentGame.PlayerName)
        If Game = vbNull Then Exit Sub
        CurrentGame.Game6Score = Game
        CurrentGame.TotalScore += Game

        ' Create score card of all games
        ShowGameCard(CurrentGame)

        AddHighScore(CurrentGame.TotalScore, CurrentGame.PlayerName) ' new high score?


    End Sub
    Sub ShowGameCard(card As StrGameLog)
        ' show individual scores for each game

        Clear()
        Dim GameDelay As Short = 600
        SndFX1.Open(New System.Uri(MusicPath & "Clang1.mp3"))
        SndFX2.Open(New System.Uri(MusicPath & "Clang2.mp3"))

        CGH.BmpToConsole(0, 0, CGH.TextToBMP($"Game 1: {card.Game1Score,3:N3}", Color.White, 100, 100,, 10),, True, ConsoleColor.Cyan,,,,,, 4, 5,, Alignment.Centre)
        If PlayAudio Then
            SndFX1.Position = SFXPosReset
            SndFX1.Play()
        End If
        Thread.Sleep(GameDelay)

        CGH.BmpToConsole(0, 10, CGH.TextToBMP($"Game 2: {card.Game2Score,3:N3}", Color.White, 100, 100,, 10),, True, ConsoleColor.Blue,,,,,, 4, 5,, Alignment.Centre)
        If PlayAudio Then
            SndFX1.Position = SFXPosReset
            SndFX1.Play()
        End If
        Thread.Sleep(GameDelay)

        CGH.BmpToConsole(0, 20, CGH.TextToBMP($"Game 3: {card.Game3Score,3:N3}", Color.White, 100, 100,, 10),, True, ConsoleColor.Cyan,,,,,, 4, 5,, Alignment.Centre)
        If PlayAudio Then
            SndFX1.Position = SFXPosReset
            SndFX1.Play()
        End If
        Thread.Sleep(GameDelay)

        CGH.BmpToConsole(0, 30, CGH.TextToBMP($"Game 4: {card.Game4Score,3:N3}", Color.White, 100, 100,, 10),, True, ConsoleColor.Blue,,,,,, 4, 5,, Alignment.Centre)
        If PlayAudio Then
            SndFX1.Position = SFXPosReset
            SndFX1.Play()
        End If
        Thread.Sleep(GameDelay)

        CGH.BmpToConsole(0, 40, CGH.TextToBMP($"Game 5: {card.Game5Score,3:N3}", Color.White, 100, 100,, 10),, True, ConsoleColor.Cyan,,,,,, 4, 5,, Alignment.Centre)
        If PlayAudio Then
            SndFX1.Position = SFXPosReset
            SndFX1.Play()
        End If
        Thread.Sleep(GameDelay)

        CGH.BmpToConsole(0, 50, CGH.TextToBMP($"Game 6: {card.Game6Score,3:N3}", Color.White, 100, 100,, 10),, True, ConsoleColor.Blue,,,,,, 4, 5,, Alignment.Centre)
        If PlayAudio Then
            SndFX1.Position = SFXPosReset
            SndFX1.Play()
        End If
        Thread.Sleep(GameDelay)

        CGH.BmpToConsole(0, 60, CGH.TextToBMP($"Total: {card.TotalScore,3:N3}", Color.White, 100, 100,, 10),, True, ConsoleColor.Yellow,,,,,, 4, 5,, Alignment.Centre)
        If PlayAudio Then
            SndFX2.Position = SFXPosReset
            SndFX2.Play()
        End If
        Thread.Sleep(1000)

        If PlayAudio Then
            Music.Open(New System.Uri(MusicPath & "Bach Air 01.mp3")) ' Background music
            Music.Position = SFXPosReset
            Music.Play()
        End If

        Thread.Sleep(4000)
        If PlayAudio Then StopAudio()

    End Sub
    Public Function CheckQuit(QuitMsg As String) As Boolean
        Dim Result As MsgBoxResult = MsgBox(QuitMsg, MsgBoxStyle.YesNoCancel, "Leaving already?")
        Select Case Result
            Case MsgBoxResult.Yes
                Return True
            Case Else
                Return False
        End Select
    End Function
    Sub QuitGame()
        ' called when user wants to quit
        SaveHighScoreFile() ' save the high score back to game
        End ' close application
    End Sub
    Function GetPlayerName() As String
        ' allows the user to enter their name
        ' Two options here, generate a single bitmap for each char and position on the screen, or generate new complete bitmaps each time char is entered
        ' option two is best for code efficiency (not memory)

        ClearKeyboardBuffer() ' dispose of any leftover key presses

        Dim NameString As New StringBuilder ' stores the name
        NameString.Append(" ") ' cannot generate a blank bitmap - strip space out at end
        Dim NameGraphic As Bitmap = CGH.TextToBMP(NameString.ToString, Color.White, 120, 120, "Ariel", 10) ' generate initial bmp
        Dim Finished As Boolean = False
        Dim KeyPress As ConsoleKeyInfo

        Clear()

        CGH.BmpToConsole(0, 0, CGH.TextToBMP("Please enter your player name", Color.White, 110, 110, "Ariel", 8), , False,,,,,,,, 55,, Alignment.Centre)
        CGH.CentreText("Press [Delete] or [Backspace] to delete, [Enter] to confirm or [Esc] to return to abort game", 60, ConsoleColor.White, ConsoleColor.DarkRed)

        Do
            If KeyAvailable Then
                KeyPress = ReadKey(True) ' store key press - we may need it later
                Select Case KeyPress.Key
                    Case ConsoleKey.Spacebar And NameString.Length < 20
                        NameString.Append(" ")
                    Case ConsoleKey.Enter
                        Finished = True
                    Case ConsoleKey.Escape
                        Return Nothing

                    Case ConsoleKey.Backspace, ConsoleKey.Delete
                        If NameString.Length > 1 Then
                            NameString.Remove(NameString.Length - 1, 1) ' keep initial space
                            CGH.BmpToConsole(0, 20, NameGraphic, &H25CF, True, ConsoleColor.Black) ' display as black to erase previous
                        End If
                End Select

                ' handle A-Z, 0-9 separately as key enums are a PITA
                ' add more statements to handle other types of chars
                If KeyPress.KeyChar.ToString.ToUpper >= "A" And KeyPress.KeyChar.ToString.ToUpper <= "Z" And NameString.Length < 21 Then
                    NameString.Append(KeyPress.KeyChar.ToString)
                ElseIf KeyPress.KeyChar.ToString >= "0" And KeyPress.KeyChar.ToString.ToUpper <= "9" And NameString.Length < 21 Then
                    NameString.Append(KeyPress.KeyChar.ToString)
                End If

                'refresh graphic
                NameGraphic = CGH.TextToBMP(NameString.ToString, Color.White, 120, 120, "Ariel", 10)
                CGH.BmpToConsole(0, 20, NameGraphic, &H25CF, True, ConsoleColor.Blue)
            End If
        Loop Until Finished
        Return NameString.ToString.Trim
    End Function
    Sub ClearKeyboardBuffer()
        ' This will cycle through any keys in the buffer and dispose of them
        ' used so that the player cannot accidentially skip through too many screens
        Do While KeyAvailable
            ReadKey()
        Loop
    End Sub

    Sub PressKeyToStart(Pos_Y As Short, Optional SpaceKey As Boolean = True)

        ' displays flasing 'Press 'Space'//'Enter' to start
        Console.BackgroundColor = ConsoleColor.Black

        Dim PressStartClrs As New List(Of Short)({0, 8, 7, 15, 7, 8}) ' rotate colours
        Dim ClrIndex As Short = 0 ' which colour idnex in PressStartClrs is in use
        Dim KeyStopwatch As New Stopwatch ' times 'press space to start'
        KeyStopwatch.Start()

        Dim HotKey As ConsoleKey = If(SpaceKey, ConsoleKey.Spacebar, ConsoleKey.Enter)

        Do
            If KeyStopwatch.ElapsedMilliseconds >= 300 Then
                Console.SetCursorPosition(CInt(WindowWidth / 2) - 10, Pos_Y)
                Console.ForegroundColor = PressStartClrs.Item(ClrIndex)
                Console.WriteLine("Press {0} to start", If(SpaceKey, "SPACE", "ENTER"))
                ClrIndex = If(ClrIndex = PressStartClrs.Count - 1, 0, ClrIndex + 1)
                KeyStopwatch.Restart()
            End If
            If KeyAvailable Then
                If ReadKey(True).Key = HotKey Then
                    KeyStopwatch.Reset()
                    Exit Do
                End If
            End If
        Loop
    End Sub
    Sub GameHelp()
        Clear()
        CGH.BmpToConsole(0, 0, CGH.TextToBMP("Skillz: Help", Color.Yellow, 100, 100), &H25CB, False,,,,,,,, 5,, Alignment.Centre)

        ForegroundColor = ConsoleColor.Yellow

        Dim Pos As Short = 20

        CGH.CentreText("Skillz is a series of games to test your keyboard and reaction skills.", Pos, ConsoleColor.Yellow)

        CGH.CentreText("Luck doesn't come into it! What determines your score is your agility and dexterity.", Pos + 4, ConsoleColor.Yellow)
        CGH.CentreText("There are 6 games of skill, each lasting around 1 minute.", Pos + 6, ConsoleColor.Yellow)

        CGH.CentreText("The aim of the game is simple: Score as close to 0 as you can!", Pos + 8, ConsoleColor.Green)

        CGH.CentreText("Press any key to return to main screen", Pos + 14, ConsoleColor.White)

        ClearKeyboardBuffer()
        Console.ReadKey(True)
    End Sub

#Region "High Score"
    Sub CreateDefaultHighScore()
        ' will generate a high score table if one is not present or reset
        HighScore.Clear() ' clear if there are contents
        HighScore.Add(New StrHighScore(30, "Vic Torius"))
        HighScore.Add(New StrHighScore(35, "Hugh Jeego"))
        HighScore.Add(New StrHighScore(40, "Major Kickbutt"))
        HighScore.Add(New StrHighScore(45, "Mike Hockertz"))
        HighScore.Add(New StrHighScore(50, "Barb Dwyer"))
        HighScore.Add(New StrHighScore(55, "Al Pacca"))
        HighScore.Add(New StrHighScore(60, "Sue Perb"))
        HighScore.Add(New StrHighScore(70, "Tom Foolery"))
        HighScore.Add(New StrHighScore(80, "Betty Dident"))
        HighScore.Add(New StrHighScore(90, "Frank Furter"))

        ' ensure table is sorted
        HighScore = HighScore.OrderBy(Function(x) x.Score).ToList ' uses a lambda expression to define the property by which to order
    End Sub
    Sub AddHighScore(Score As Single, Player As String)
        Dim Position As Short = -1
        If Score < HighScore.Min.Score Then ' made top of the board
            SndFX1.Open(New System.Uri(MusicPath & "trumpets.mp3")) ' top of leaderboard
        ElseIf Score < HighScore.Max.Score Then ' made it into the board
            SndFX1.Open(New System.Uri(MusicPath & "fanfare.mp3")) ' into leaderboard
        Else
            Exit Sub ' they're crap, kick 'em out
        End If

        For Each Entry As StrHighScore In HighScore
            If Score <= Entry.Score Then ' if smaller, insert before current list entry
                HighScore.Insert(HighScore.IndexOf(Entry), New StrHighScore(Score, If(Player.Trim.Length > 20, Left(Player, 20), Player.Trim)))
                HighScore.Remove(HighScore.Last) ' take last item out of list as it's dropped out of table
                Position = HighScore.IndexOf(Entry)
                Exit For
            End If
        Next
        If Position <> -1 Then ' defensive code, this should always trigger
            Clear()
            SndFX1.Position = SFXPosReset
            SndFX1.Play()
            CGH.BmpToConsole(34, 0, CGH.TextToBMP("Congratulations!", Color.Blue, 120, 120, "Ariel", 12), &H2666, False)
            CGH.BmpToConsole(37, 20, CGH.TextToBMP("New High Score", Color.Blue, 120, 120, "Ariel", 12), &H2666, False)
            CGH.BmpToConsole(85, 40, CGH.TextToBMP("#" & Position, Color.Yellow, 150, 150, "Ariel", 14), &H2666, False)
            Thread.Sleep(3000)
            SndFX1.Stop()
        End If
    End Sub
    Public Sub StopAudio()
        ' can be called from anywhere in the game to stop all music
        If Not PlayAudio Then Exit Sub
        Music.Stop()
        SndFX1.Stop()
        SndFX2.Stop()
        SndFX3.Stop()
        SndFX4.Stop()
        Music.Stop()
    End Sub
    Sub DisplayHighScore()
        ' Displays the high score page, the graphics and passes control back once a key has been pressed
        ' There is a limit to what can be achieved with the console buffer memory and with time, doublebuffering could be achieved

        Dim NamePad As Char = "." ' symbol used to pad out high score names
        Dim TopNameGraphic As Bitmap = CGH.TextToBMP(HighScore(0).Name, Color.White, 100, 100, "Consolas", 10, Color.Black) ' rasterize top score name
        Dim ScoreTablePos_X As Short = CInt(WindowWidth / 2) - 16 ' position for score table
        Dim ScoreTablePos_Y As Short = WindowHeight - 12 ' position for score table
        Dim pos As Short = 31 ' position where high score graphics load (after 'Skills' logo)

        Dim Jiggle_Y1, Jiggle_X1, Jiggle_X2, Jiggle_Y2 As Short ' bounds in whicih top player can jiggle
        Jiggle_X1 = 15
        Jiggle_X2 = WindowWidth - (TopNameGraphic.Width + 1)
        Jiggle_Y1 = pos + 2
        Jiggle_Y2 = ScoreTablePos_Y - TopNameGraphic.Height - 1

        Dim PressStartClrs As New List(Of Short)({0, 8, 7, 15, 7, 8}) ' this relates to the console enum values for the 'press key to start (black, dark grey, light grey, white and back to black
        Dim ClrIndex As Short = 0 ' which colour idnex in PressStartClrs is in use
        Dim HighScoreClrs As New List(Of Short)({1, 9, 11, 10, 14, 12, 5, 13}) ' this relates to the console enum values for the 'press key to start (black, dark grey, light grey, white and back to black
        Dim HighScoreClrIndex As Short = 0 ' which colour idnex in PressStartClrs is in use
        Dim HighScoreTopClr As Short = 0 ' this enables the smooth scrolling

        Dim Ran_X, Ran_Y, RanClr As Short
        Dim Rand As New Random
        Randomize()
        Do
            ' set up BGM
            If PlayAudio Then
                Music.Open(New System.Uri(MusicPath & "closer.mp3")) ' Background music
                Music.Position = SFXPosReset
                Music.Play() ' will not loop
            End If


            ' Display intiial logo
            Clear()
            CGH.BmpToConsole(0, 0, CGH.TextToBMP("Skillz", Color.Blue, 120, 120,, 25), &H263B, False,,,,,, 5, 8, 10,, Alignment.Centre)

            Dim graphic As Bitmap = CGH.TextToBMP("High Scores", Color.Yellow, 100, 100, "Ariel", 9, Color.Black)
            graphic.RotateFlip(RotateFlipType.Rotate270FlipNone)
            CGH.BmpToConsole(0, 0, graphic, &H25CF, False, ConsoleColor.Cyan,, ConsoleColor.Red,,,, 17,, 2)

            CGH.CentreText("TODAY'S TOP SCORER", pos)

            ' Display F1 Help (not animated)
            CGH.DrawBorder(WindowWidth - 26, 1, WindowWidth - 6, 3, BorderType.SingleLineCurved, ConsoleColor.Blue)
            Console.SetCursorPosition(WindowWidth - 25, 2)
            Console.ForegroundColor = ConsoleColor.Yellow
            Console.WriteLine("Press [F1] for help")

            ' draw score table border
            ForegroundColor = ConsoleColor.White
            DrawBorder(ScoreTablePos_X - 2, ScoreTablePos_Y - 1, (CInt(WindowWidth / 2) - ScoreTablePos_X) + CInt(WindowWidth / 2) + 2, ScoreTablePos_Y + 10, BorderType.SingleLineCurved, ConsoleColor.Red)
            For Counter = 0 To HighScore.Count - 1
                SetCursorPosition(ScoreTablePos_X, ScoreTablePos_Y + Counter)
                WriteLine($"{(Counter + 1).ToString,2}  {HighScore(Counter).Name.PadRight(20, NamePad),-20}  {HighScore(Counter).Score,6:N3}") ' -ve value is left align, +ve is right align
            Next

            ' set up threading for score and 'key to start' animation 
            ' parameter passing is tricky with threads, so create anonymous sub and pass in reference to parameters
            'Dim anim As New Threading.Thread(Sub() AnimateHighScore(ScoreTablePos_X, ScoreTablePos_Y, NamePad)) ' NOT USED UNTIL RESOLVE CONSOLE BUFFER LOCKING

            ' loop flashing top player
            Dim NameStopwatch As New Stopwatch ' times name flash
            NameStopwatch.Start()
            Dim KeyStopwatch As New Stopwatch ' times 'press space to start'
            KeyStopwatch.Start()
            Dim ScoreStopwatch As New Stopwatch ' times highscore colour cascade
            ScoreStopwatch.Start()

            Ran_X = Rand.Next(Jiggle_X1, Jiggle_X2)
            Ran_Y = Rand.Next(Jiggle_Y1, Jiggle_Y2)
            RanClr = Rand.Next(1, 16)
            CGH.BmpToConsole(Ran_X, Ran_Y, TopNameGraphic, &H263A, True, RanClr)

            Do ' text animation loop
                ' key press
                If KeyAvailable Then
                    Select Case ReadKey(True).Key
                        Case ConsoleKey.Spacebar
                            StopAudio()
                            Exit Sub
                        Case ConsoleKey.F1
                            GameHelp()
                            Exit Do ' triggers highscore refresh
                        Case ConsoleKey.Escape
                            If CheckQuit("Exit the game?") = True Then QuitGame()
                    End Select
                End If

                If NameStopwatch.ElapsedMilliseconds >= 2000 Then
                    Thread.CurrentThread.Join(10)
                    CGH.BmpToConsole(Ran_X, Ran_Y, TopNameGraphic, &H263A, True, ConsoleColor.Black) ' wipe over
                    Ran_X = Rand.Next(Jiggle_X1, Jiggle_X2)
                    Ran_Y = Rand.Next(Jiggle_Y1, Jiggle_Y2)
                    RanClr = Rand.Next(1, 16)
                    CGH.BmpToConsole(Ran_X, Ran_Y, TopNameGraphic, &H263A, True, RanClr)
                    NameStopwatch.Restart()
                End If
                If KeyStopwatch.ElapsedMilliseconds >= 300 Then
                    Console.SetCursorPosition((WindowWidth / 2) - 11, ScoreTablePos_Y - 2)
                    Console.ForegroundColor = PressStartClrs.Item(ClrIndex)
                    Console.WriteLine("Press [SPACE] to start")
                    ClrIndex = If(ClrIndex = PressStartClrs.Count - 1, 0, ClrIndex + 1)
                    KeyStopwatch.Restart()
                End If
                If ScoreStopwatch.ElapsedMilliseconds >= 500 Then
                    HighScoreTopClr = HighScoreClrIndex
                    HighScoreClrIndex = If(HighScoreClrIndex = HighScoreClrs.Count - 1, 0, HighScoreClrIndex + 1)
                    For Counter = 0 To HighScore.Count - 1
                        Console.ForegroundColor = HighScoreClrs.Item(HighScoreTopClr)
                        HighScoreTopClr = If(HighScoreTopClr = HighScoreClrs.Count - 1, 0, HighScoreTopClr + 1)
                        SetCursorPosition(ScoreTablePos_X, ScoreTablePos_Y + Counter)
                        WriteLine($"{(Counter + 1).ToString,2}  {HighScore(Counter).Name.PadRight(20, NamePad),-20}  {HighScore(Counter).Score,6:N3}") ' -ve value is left align, +ve is right align
                    Next
                    ScoreStopwatch.Restart()
                End If
            Loop
        Loop
    End Sub

    Sub LoadHighScoreFile()

        If File.Exists(FilePath & "Score.dat") Then
            Try
                HighScore.Clear()
                Using Reader As New IO.StreamReader(HighScoreFile)
                    Do Until Reader.EndOfStream
                        HighScore.Add(New StrHighScore(CSng(Reader.ReadLine), Reader.ReadLine))
                    Loop
                End Using

            Catch ex As Exception
                ' highscore not available, or error reading data so generate default
                MsgBox($"Error {ex.ToString}{vbCrLf}{vbCrLf}Loading default score table!", MsgBoxStyle.OkOnly, "Error readig score table")
                CreateDefaultHighScore()
            End Try

        Else
            ' doesn't exist so initialise new score table and save it
            CreateDefaultHighScore()
            SaveHighScoreFile()
        End If
    End Sub
    Sub SaveHighScoreFile()
        Try
            ' no need to check if folder exists, let it fail
            Using Writer As New StreamWriter(HighScoreFile, False)
                For Each entry As StrHighScore In HighScore
                    Writer.WriteLine(entry.Score)
                    Writer.WriteLine(entry.Name)
                Next
            End Using

        Catch ex As Exception
            ' Cannot save high score table, could be a permissions issue, or app loaded from readonly location
            MsgBox($"Error {ex.ToString}{vbCrLf}{vbCrLf}Could ot save score table!", MsgBoxStyle.OkOnly, "Error Saving")
            CreateDefaultHighScore()
        End Try
    End Sub
#End Region

End Module

