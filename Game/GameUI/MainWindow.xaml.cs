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

            session = new GameSession(new GameState(Player.White, Board.Initial()));
            currentBoard = BoardType.Main;
            DrawBoard(currentState.Board);
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
            if (selectedHistory != null)
            {
                if (currentState.CurrentPlayer != selectedHistory.NextPlayer)
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
                    if(!session.TimelineExist(selectedHistory))
                    {
                        currentBoard = session.CreateTimeline(selectedHistory);
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
            Guid? capturedPieceId = currentState.MakeMove(move);

            if (capturedPieceId.HasValue)
            {
                session.RemovePieceFromAllBoards(capturedPieceId.Value);
            }

            DrawBoard(currentState.Board);
            UpdateMoveHistory(currentState);

            if (currentState.IsGameOver())
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
            GameOverMenu gameOverMenu = new GameOverMenu(currentState);
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

        private void RestartGame()
        {
            selectedPos = null;
            HideHighLights();
            moveCache.Clear();
            session = new GameSession(new GameState(Player.White, Board.Initial()));
            currentBoard = BoardType.Main;
            DrawBoard(currentState.Board);
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

            DrawBoard(currentState.Board);
            UpdateMoveHistory(currentState);
        }
            
        private void TimelineWhite_Click(object sender, RoutedEventArgs e)
        {
            historyState = null;
            if (session.GetBoardState(BoardType.TimelineWhite) == null)
            {
                return;
            }
            currentBoard = BoardType.TimelineWhite;

            DrawBoard(currentState.Board);
            UpdateMoveHistory(currentState);
        }

        private void TimelineBlack_Click(object sender, RoutedEventArgs e)
        {
            historyState = null;
            if (session.GetBoardState(BoardType.TimelineBlack) == null)
            {
                return;
            }
            currentBoard = BoardType.TimelineBlack;

            DrawBoard(currentState.Board);
            UpdateMoveHistory(currentState);
        }
    }
}