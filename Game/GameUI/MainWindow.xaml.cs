using ChessLogic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

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
        private GameState gameState;
        private GameState timeStateWhite;
        private GameState timeStateBlack;
        private GameState currentState;
        private GameState historyState;

        private Position selectedPos = null;

        public MainWindow()
        {
            InitializeComponent();
            InitilaizeBoard();

            gameState = new GameState(Player.White, Board.Initial());
            timeStateWhite = null;
            timeStateBlack = null;
            currentBoard = BoardType.Main;
            currentState = GetGameState(currentBoard);
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

        private bool IsCorrectPlayer()
        {
            MoveHistory selectedHistory = MoveHistoryList.SelectedItem as MoveHistory;

            if (selectedHistory == null)
            {
                return true;
            }

            return selectedHistory.NextPlayer == gameState.CurrentPlayer;
        }

        private bool CanCreateTimeline()
        {
            MoveHistory selectedHistory = MoveHistoryList.SelectedItem as MoveHistory;

            if (selectedHistory == null)
            {
                return false;
            }

            if (selectedHistory.NextPlayer == Player.White)
            {
                return timeStateWhite == null;
            }
            if (selectedHistory.NextPlayer == Player.Black)
            {
                return timeStateBlack == null;
            }

            return false;
        }

        private void CreateTimeline()
        {
            MoveHistory selectedHistory = MoveHistoryList.SelectedItem as MoveHistory;

            if (selectedHistory.NextPlayer == Player.White)
            {
                timeStateWhite = new GameState(
                    selectedHistory.NextPlayer,
                    selectedHistory.Board.Copy());
                currentBoard = BoardType.TimelineWhite;
            }
            else
            {
                timeStateBlack = new GameState(
                    selectedHistory.NextPlayer,
                    selectedHistory.Board.Copy());
                currentBoard = BoardType.TimelineBlack;
            }

            currentState = GetGameState(currentBoard);
        }

        private void OnFromPositionSelected(Position pos)
        {
            if(!IsCorrectPlayer())
            {
                return;
            }
            if (CanCreateTimeline())
            {
                CreateTimeline();
            }

            IEnumerable<Move> moves = currentState.LegalMovesForPiece(pos);

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
                if(move.Type == MoveType.PawnPromotion)
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
        // TODO: Rewrite after implementing the GameState UI
        private GameState GetGameState(BoardType boardType)
        {
            return boardType switch
            {
                BoardType.Main => gameState,
                BoardType.TimelineWhite => timeStateWhite,
                BoardType.TimelineBlack => timeStateBlack,
                _ => gameState
            };
        }

        private void HandleMove(Move move)
        {
            currentState = GetGameState(currentBoard);

            currentState.MakeMove(move);
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

            historyState = new GameState(
                selectedHistory.NextPlayer,
                selectedHistory.Board.Copy());

            currentState = historyState;
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
            timeStateWhite = null;
            timeStateBlack = null;
            gameState = new GameState(Player.White, Board.Initial());
            currentBoard = BoardType.Main;
            currentState = GetGameState(currentBoard);
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
            currentBoard = BoardType.Main;
            currentState = GetGameState(currentBoard);

            DrawBoard(currentState.Board);
            UpdateMoveHistory(currentState);
        }
            
        private void TimelineWhite_Click(object sender, RoutedEventArgs e)
        {
            if(timeStateWhite == null)
            {
                return;
            }
            currentBoard = BoardType.TimelineWhite;
            currentState = GetGameState(currentBoard);

            DrawBoard(currentState.Board);
            UpdateMoveHistory(currentState);
        }

        private void TimelineBlack_Click(object sender, RoutedEventArgs e)
        {
            if(timeStateBlack == null)
            {
                return;
            }
            currentBoard = BoardType.TimelineBlack;
            currentState = GetGameState(currentBoard);

            DrawBoard(currentState.Board);
            UpdateMoveHistory(currentState);
        }
    }
}