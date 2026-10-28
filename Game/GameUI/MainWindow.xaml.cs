using ChessLogic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GameUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Image[,] pieceImages = new Image[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];
        private readonly Dictionary<Position, Move> moveCache = new Dictionary<Position, Move>();

        private BoardType currentBoard;
        private GameSession session;
        private GameState historyState;
        private GameState currentState => session.GetBoardState(currentBoard);

        private Position selectedPos = null;

        public MainWindow()
        {
            InitializeComponent();
            InitilaizeBoard();
            UpdateBoardSelectionHighlight();

            session = new GameSession(new GameState(Player.White, Board.Initial()));
            currentBoard = BoardType.Main;
            DrawBoard(currentState.Board);

            UpdateGameInfo();
        }

        private void InitilaizeBoard()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Image image = new Image();
                    pieceImages[r, c] = image;
                    PieceGrid.Children.Add(image);

                    Rectangle hightlight = new Rectangle();
                    highlights[r, c] = hightlight;
                    HighLightGrid.Children.Add(hightlight);
                }
            }
        }

        private void DrawBoard(Board board)
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Piece piece = board[r,c];
                    pieceImages[r,c].Source = Images.GetImage(piece);
                }
            }
        }

        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMenuOnScreen())
            {
                return;
            }

            Point point = e.GetPosition(BoardGrid);
            Position pos = ToSquarePosition(point); 

            if(selectedPos == null)
            {
                OnFromPositionSelected(pos);
            }
            else
            {
                OnToPositionSelected(pos);
            }
        }

        private Position ToSquarePosition(Point point)
        {
            double squareSize = BoardGrid.ActualWidth / 8;
            int row = (int)(point.Y / squareSize);
            int col = (int)(point.X / squareSize);
            return new Position(row, col);
        }

        private void OnFromPositionSelected(Position pos)
        {
            MoveHistory selectedHistory = MoveHistoryList.SelectedItem as MoveHistory;
            bool isLastHistoryEntry = MoveHistoryList.SelectedIndex == MoveHistoryList.Items.Count - 1;
            
            if (selectedHistory == null || isLastHistoryEntry)
            {
                if (currentState.CurrentPlayer != session.CurrentPlayer)
                {
                    return;
                }
                if (!session.CanPlayOnBoard(currentBoard))
                {
                    return;
                }
            }

            if (selectedHistory != null)
            {
                if (session.CurrentPlayer != selectedHistory.NextPlayer)
                {
                    return;
                }
                if (!isLastHistoryEntry && session.TimelineExist(selectedHistory))
                {
                    return;
                }
            }
            
            GameState stateToUse = historyState ?? currentState;

            IEnumerable<Move> moves = stateToUse.LegalMovesForPiece(pos);

            if (moves.Any())
            {
                selectedPos = pos;
                CacheMoves(moves);
                ShowHighLights();
            }
        }

        private void OnToPositionSelected(Position pos)
        {
            selectedPos = null;
            HideHighLights();

            if (moveCache.TryGetValue(pos, out Move move))
            {
                MoveHistory selectedHistory = MoveHistoryList.SelectedItem as MoveHistory;
                bool isLastHistoryEntry = MoveHistoryList.SelectedIndex == MoveHistoryList.Items.Count - 1;
                
                if (selectedHistory != null && !isLastHistoryEntry)
                {
                    if (session.IsHistoryEntryBlocked(currentBoard, selectedHistory.NextPlayer, MoveHistoryList.SelectedIndex))
                    {
                        return;
                    }

                    if (!session.TimelineExist(selectedHistory))
                    {
                        currentBoard = session.CreateTimeline(selectedHistory);
                        UpdateBoardSelectionHighlight();
                        historyState = null;
                    }
                }
                
                if (move.Type == MoveType.PawnPromotion)
                {
                    HandlePromotion(move.FromPos, move.ToPos);
                }
                else
                {
                    HandleMove(move);
                }
            }
        }
        
        private void HandlePromotion(Position from, Position to)
        {
            pieceImages[to.Row, to.Column].Source = Images.GetImage(currentState.CurrentPlayer, PieceType.Pawn);
            pieceImages[from.Row, from.Column].Source = null;

            PromotionMenu promMenu = new PromotionMenu(currentState.CurrentPlayer);
            MenuContainer.Content = promMenu;

            promMenu.PieceSelected += type =>
            {
                MenuContainer.Content = null;
                Move promMove = new PawnPromotion(from, to, type);
                HandleMove(promMove);
            };
        }
        
        private void HandleMove(Move move)
        {
            Player movingPlayer = session.CurrentPlayer;
            session.UpdateBoardTimers(currentBoard, movingPlayer);

            MoveResult result = currentState.MakeMove(move);
            session.UpdateMoveCounter(result);
            session.SwitchPlayer();

            if (result.CapturedPieceId.HasValue)
            {
                session.RemovePieceFromAllBoards(result.CapturedPieceId.Value, movingPlayer);
            }

            session.CheckForGameOver();

            UpdateGameInfo();

            DrawBoard(currentState.Board);
            UpdateMoveHistory(currentState);
            UpdateBoardHighlights();
            UpdateTimerDisplay();

            if (session.IsGameOver())
            {
                ShowGameOver();
            }
        }

        private void UpdateMoveHistory(GameState currentState)
        {
            MoveHistoryList.Items.Clear();

            foreach (MoveHistory moveHistory in currentState.MoveHistory)
            {
                MoveHistoryList.Items.Add(moveHistory);
            }

            if (MoveHistoryList.Items.Count > 0)
            {
                MoveHistoryList.ScrollIntoView(
                    MoveHistoryList.Items[MoveHistoryList.Items.Count - 1]);
            }
        }

        private void MoveHistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MoveHistory selectedHistory = (MoveHistory)MoveHistoryList.SelectedItem;
            if (selectedHistory == null)
            {
                return;
            }

            bool isLastHistoryEntry = MoveHistoryList.SelectedIndex == MoveHistoryList.Items.Count - 1;
            if (isLastHistoryEntry)
            {
                historyState = null;
                DrawBoard(currentState.Board);
                return;
            }

            historyState = new GameState(
                selectedHistory.NextPlayer,
                selectedHistory.Board.Copy());

            DrawBoard(historyState.Board);
        }

        private void CacheMoves(IEnumerable<Move> moves)
        {
            moveCache.Clear();

            foreach (Move move in moves)
            {
                moveCache[move.ToPos] = move;
            }
        }

        private void ShowHighLights()
        {
            Color color = Color.FromArgb(150, 100, 220, 255);

            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = new SolidColorBrush(color);
            }
        }

        private void HideHighLights()
        {
            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = Brushes.Transparent;
            }
        }

        private bool IsMenuOnScreen()
        {
            return MenuContainer.Content != null;
        }

        private void ShowGameOver()
        {
            GameOverMenu gameOverMenu = new GameOverMenu(session);
            MenuContainer.Content = gameOverMenu;

            gameOverMenu.OptionSelected += option =>
            {
                if (option == Option.Restart)
                {
                    MenuContainer.Content = null;
                    RestartGame();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            };
        }

        private void ShowConfirmation(string question, Action<bool> onConfirmation)
        {
            ConfirmationMenu confirmationMenu = new ConfirmationMenu(question);
            MenuContainer.Content = confirmationMenu;

            confirmationMenu.ConfirmationSelected += confirmed =>
            {
                MenuContainer.Content = null;
                onConfirmation(confirmed);
            };
        }

        private void RestartGame()
        {
            selectedPos = null;
            HideHighLights();
            moveCache.Clear();
            session = new GameSession(new GameState(Player.White, Board.Initial()));
            currentBoard = BoardType.Main;
            historyState = null;
            MoveHistoryList.Items.Clear();
            DrawBoard(currentState.Board);
            UpdateGameInfo();
            UpdateTimerDisplay();
            UpdateBoardHighlights();
            UpdateBoardSelectionHighlight();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IsMenuOnScreen() && e.Key == Key.Escape)
            {
                ShowPauseMenu();
            }
        }

        private void ShowPauseMenu()
        {
            PauseMenu pauseMenu = new PauseMenu();
            MenuContainer.Content = pauseMenu;

            pauseMenu.OptionSelected += option =>
            {
                MenuContainer.Content = null;

                if (option == Option.Restart)
                {
                    RestartGame();
                }
            };
        }

        private void MainBoard_Click(object sender, RoutedEventArgs e)
        {
            historyState = null;
            currentBoard = BoardType.Main;
            UpdateBoardSelectionHighlight();

            DrawBoard(currentState.Board);
            UpdateMoveHistory(currentState);
            UpdateGameInfo();
        }
            
        private void TimelineWhite_Click(object sender, RoutedEventArgs e)
        {
            historyState = null;
            if (session.GetBoardState(BoardType.TimelineWhite) == null)
            {
                return;
            }
            currentBoard = BoardType.TimelineWhite;
            UpdateBoardSelectionHighlight();

            DrawBoard(currentState.Board);
            UpdateMoveHistory(currentState);
            UpdateGameInfo();
        }

        private void TimelineBlack_Click(object sender, RoutedEventArgs e)
        {
            historyState = null;
            if (session.GetBoardState(BoardType.TimelineBlack) == null)
            {
                return;
            }
            currentBoard = BoardType.TimelineBlack;
            UpdateBoardSelectionHighlight();

            DrawBoard(currentState.Board);
            UpdateMoveHistory(currentState);
            UpdateGameInfo();
        }

        private void UpdateBoardSelectionHighlight()
        {
            Brush normalColor = new SolidColorBrush((Color)Application.Current.Resources["NormalBoardColor"]);
            Brush selectedColor = new SolidColorBrush((Color)Application.Current.Resources["SelectedBoardColor"]);

            Brush normalTextColor = Brushes.Gray;
            Brush selectedTextColor = Brushes.White;

            MainBoardButton.BorderBrush = currentBoard == BoardType.Main
                ? selectedColor : normalColor;
            MainBoardButton.Foreground = currentBoard == BoardType.Main
                ? selectedTextColor : normalTextColor;

            TimelineWhiteButton.BorderBrush = currentBoard == BoardType.TimelineWhite
                ? selectedColor : normalColor;
            TimelineWhiteButton.Foreground = currentBoard == BoardType.TimelineWhite
                ? selectedTextColor : normalTextColor;

            TimelineBlackButton.BorderBrush = currentBoard == BoardType.TimelineBlack
                ? selectedColor : normalColor;
            TimelineBlackButton.Foreground = currentBoard == BoardType.TimelineBlack
                ? selectedTextColor : normalTextColor;
        }

        private void UpdateBoardHighlights()
        {
            CheckBoards(BoardType.Main, MainBoardButton);
            CheckBoards(BoardType.TimelineWhite, TimelineWhiteButton);
            CheckBoards(BoardType.TimelineBlack, TimelineBlackButton);
        }

        private void CheckBoards(BoardType type, Button button)
        {
            GameState state = session.GetBoardState(type);

            bool inCheck = state != null &&
                           state.Board.IsInCheck(state.CurrentPlayer);

            bool forcedBoard = session.GetTimerForcedBoard() == type;

            Color checkColor = (Color)Application.Current.Resources["CheckColor"];
            Color forcedColor = (Color)Application.Current.Resources["ForcedColor"];

            if (forcedBoard)
            {
                button.Background = new SolidColorBrush(forcedColor);
            }
            else if (inCheck)
            {
                button.Background = new SolidColorBrush(checkColor);
            }
            else
            {
                button.Background = Brushes.Transparent;
            }
        }

        private void UpdateTimerDisplay()
        {
            MainBoardButton.Content =
                $"Main\nW: {session.GetTimerCount(BoardType.Main, Player.White)} | " +
                $"B: {session.GetTimerCount(BoardType.Main, Player.Black)}";

            TimelineWhiteButton.Content =
                $"Timeline White\nW: {session.GetTimerCount(BoardType.TimelineWhite, Player.White)} | " +
                $"B: {session.GetTimerCount(BoardType.TimelineWhite, Player.Black)}";

            TimelineBlackButton.Content =
                $"Timeline Black\nW: {session.GetTimerCount(BoardType.TimelineBlack, Player.White)} | " +
                $"B: {session.GetTimerCount(BoardType.TimelineBlack, Player.Black)}";
        }
        private void UpdateGameInfo()
        {
            CurrentPlayerText.Text = session.CurrentPlayer.ToString();

            GameStatusText.Text = session.IsGameOver()
                ? "Game over"
                : "Game in progress";

            BoardPlayerText.Text = currentState.CurrentPlayer + " to move";

            bool whiteInCheck = currentState.Board.IsInCheck(Player.White);
            bool blackInCheck = currentState.Board.IsInCheck(Player.Black);
            
            if (whiteInCheck)
            {
                BoardCheckText.Text = "White is in check";
            }
            else if (blackInCheck)
            {
                BoardCheckText.Text = "Black is in check";
            }
            else
            {
                BoardCheckText.Text = "No check";
            }
        }
        private void ResignButton_Click(object sender, RoutedEventArgs e)
        {
            ShowConfirmation("Resign the match?", confirmed =>
            {
                if (confirmed)
                {
                    session.Resign();
                    UpdateGameInfo();
                    ShowGameOver();
                }
            });
        }

        private void OfferDrawButton_Click(object sender, RoutedEventArgs e)
        {
            ShowConfirmation("Offer a draw?", confirmed =>
            {
                if (!confirmed)
                    return;

                bool success = session.OfferDraw();

                if (!success)
                {
                    ShowConfirmation("Draw not possible.", _ => { });
                    return;
                }

                ShowConfirmation("Accept draw?", accepted =>
                {
                    if (accepted)
                    {
                        AcceptDraw();
                    }
                    else
                    {
                        RejectDraw();
                    }
                });
            });
        }


        private void AcceptDraw()
        {
            session.AcceptDraw();
            UpdateGameInfo();
            ShowGameOver();
        }

        private void RejectDraw()
        {
            session.RejectDraw();
            ShowInformation("DRAW REJECTED");
        }

        private void ShowInformation(string message)
        {
            InformationMenu informationMenu = new InformationMenu(message);
            MenuContainer.Content = informationMenu;

            informationMenu.OkSelected += () =>
            {
                MenuContainer.Content = null;
            };
        }
    }
}