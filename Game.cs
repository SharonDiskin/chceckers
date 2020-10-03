using System;
using System.Collections.Generic;

namespace C20_Ex02
{
    public class Game
    {
        private const int k_HumanPlayer = 2;
        private readonly Board r_GameBoard;
        private Player m_Player1;
        private Player m_Player2;

        public Game()
        {
            m_Player1 = initializePlayerX();
            r_GameBoard = initializeBoard();
            m_Player2 = initializePlayerO();
        }

        private static void calculateSumOfPointsForPlayers(ref Player io_Player1, ref Player io_Player2)
        {
            io_Player1.NumOfPoints += io_Player1.NumOfPawns + (4 * io_Player1.NumOfKings);
            io_Player2.NumOfPoints += io_Player2.NumOfPawns + (4 * io_Player2.NumOfKings);
        }

        private static Player initializePlayerX()
        {
            string userName = UI.AskUserName();
            UI.WelcomePlayer(userName);

            return new Player(userName, 0, Cell.eCellSign.PawnX, Cell.eCellSign.KingX);
        }

        private static Player initializePlayerO()
        {
            string userName;
            int opponent = UI.AskForOpponent();
            if (opponent == k_HumanPlayer)
            {
                userName = UI.AskOpponentUserName();
                UI.WelcomePlayer(userName);
            }
            else
            {
                userName = "PC";
            }

            return new Player(userName, 0, Cell.eCellSign.PawnO, Cell.eCellSign.KingO);
        }

        private static Board initializeBoard()
        {
            Board.eBoardDimension boardDimension = UI.AskBoardDimension();
            return new Board(boardDimension);
        }

        private static bool playerHasPawnInCell(Player i_Player, Cell i_CellInBoard)
        {
            return i_Player.PawnSign == i_CellInBoard.Sign ||
                   i_Player.KingSign == i_CellInBoard.Sign;
        }

        private static bool gameOver(int i_NumOfPawnsForPlayer1, int i_NumOfPawnsForPlayer2)
        {
            return i_NumOfPawnsForPlayer1 == 0 || i_NumOfPawnsForPlayer2 == 0;
        }

        public void Play()
        {
            bool userWantsAnotherGameWithSameOpponent;
            do
            {
               userWantsAnotherGameWithSameOpponent = playGame();
            } 
            while (userWantsAnotherGameWithSameOpponent);

            UI.GoodBye();
        }

        private bool playGame()
        {
            r_GameBoard.InitializeBoard();
            distributePawnsToPlayers(r_GameBoard.Dimension);
            m_Player1.Turn = true;
            string lastMove = string.Empty;  // We initialize it to be empty for the first iteration
            bool inEatingSequence = false;

            while (!gameOver(m_Player1.NumOfPawns + m_Player1.NumOfKings, m_Player2.NumOfPawns + m_Player2.NumOfKings))
            {
                bool playerExecutedEat = false;
                UI.PrintGameState(m_Player1, m_Player2, r_GameBoard, lastMove, inEatingSequence);

                if ((m_Player1.Turn && !m_Player1.HasLegalMoves(r_GameBoard)) || (m_Player2.Turn && !m_Player2.HasLegalMoves(r_GameBoard)))
                {
                    break;
                }
                else if (m_Player1.Turn)
                {
                    lastMove = giveTurnTo(ref m_Player1, ref m_Player2, ref playerExecutedEat);

                        if (m_Player1.WantsToQuit(lastMove))
                        {
                            m_Player1.NumOfPawns = m_Player1.NumOfKings = 0;
                        }

                        // This check meant to check if there is more to eat in a row
                        m_Player1.Turn = playerExecutedEat && m_Player1.CanEatMore(r_GameBoard, lastMove);
                        m_Player2.Turn = !m_Player1.Turn;
                        inEatingSequence = m_Player1.Turn;
                }
                else
                {
                    lastMove = giveTurnTo(ref m_Player2, ref m_Player1, ref playerExecutedEat);

                    if (m_Player2.WantsToQuit(lastMove))
                    {
                        m_Player2.NumOfPawns = m_Player2.NumOfKings = 0;
                    }

                    // This check meant to check if there is more to eat in a row
                    m_Player2.Turn = playerExecutedEat && m_Player2.CanEatMore(r_GameBoard, lastMove); 
                    m_Player1.Turn = !m_Player2.Turn;
                    inEatingSequence = m_Player2.Turn;
                }
            }

            calculateSumOfPointsForPlayers(ref m_Player1, ref m_Player2);
            UI.PrintGameState(m_Player1, m_Player2, r_GameBoard, lastMove, inEatingSequence);
            UI.DisplayScore(m_Player1, m_Player2);
            bool wantsAnotherGame = UI.AskIfUserWantsAnotherGame();

            return wantsAnotherGame;
        }

        private void distributePawnsToPlayers(Board.eBoardDimension i_BoardDimension)
        {
            m_Player1.NumOfPawns = m_Player2.NumOfPawns = ((int)i_BoardDimension / 2) * (((int)i_BoardDimension - 2) / 2);
            m_Player1.NumOfKings = m_Player2.NumOfKings = 0;
        }

        private bool moveDoNotCrossBorder(string i_Move)
        {
            bool validCoordinate = i_Move.Length == 5 && i_Move.Contains(">");
            for (int i = 0; i < i_Move.Length; i++)
            {
                if (i == 2 && i_Move[i] != '>')
                {
                    validCoordinate = false;
                    break;
                }

                if (i == 0 || i == 3)
                {
                    if (i_Move[i] < 'A' || i_Move[i] > 'A' + (int)r_GameBoard.Dimension - 1)
                    {
                        validCoordinate = false;
                        break;
                    }
                }
                else if (i == 1 || i == 4)
                {
                    if (i_Move[i] < 'a' || i_Move[i] > 'a' + (int)r_GameBoard.Dimension - 1)
                    {
                        validCoordinate = false;
                        break;
                    }
                }
            }

            return validCoordinate;
        }

        private bool legalMove(Player i_Player, ref string io_Move)
        {
            int sourceColIndex = io_Move[0] - 'A';
            int sourceRowIndex = io_Move[1] - 'a';
            bool moveIsLegal;

            bool sourceCoordinateHavePlayerPawn = playerHasPawnInCell(i_Player, r_GameBoard.m_Grid[sourceRowIndex, sourceColIndex]);

            List<string> possibleEatingMoves = i_Player.PossibleEatingMoves(r_GameBoard);
            List<string> possibleNonEatingMoves = i_Player.PossibleNonEatingMoves(r_GameBoard);

            if (possibleEatingMoves.Count > 0)
            {
                moveIsLegal = possibleEatingMoves.Contains(io_Move);
            }
            else if(possibleNonEatingMoves.Count > 0)
            {
                moveIsLegal = possibleNonEatingMoves.Contains(io_Move);
            }
            else
            {
                io_Move = "Q";
                moveIsLegal = true;
            }

            return sourceCoordinateHavePlayerPawn && moveIsLegal;
        }

        private bool validMove(Player i_Player, ref string io_Move)
        {
            bool moveIsValid = 
                (moveDoNotCrossBorder(io_Move) &&
                legalMove(i_Player, ref io_Move)) || io_Move == "Q";

            if (!moveIsValid)
            {
                UI.PrintInvalidMove();
            }

            return moveIsValid;
        }

        private string choosePcMove(Player i_Player)
        {
            List<string> possibleEatingMoves = i_Player.PossibleEatingMoves(r_GameBoard);
            List<string> possibleNonEatingMoves = i_Player.PossibleNonEatingMoves(r_GameBoard);
            Random randomGenerator = new Random();

            string pcMove;
            int indexInList;

            if (possibleEatingMoves.Count > 0)
            {
                indexInList = randomGenerator.Next(possibleEatingMoves.Count);
                pcMove = possibleEatingMoves[indexInList];
            }
            else if (possibleNonEatingMoves.Count > 0)
            {
                indexInList = randomGenerator.Next(possibleNonEatingMoves.Count);
                pcMove = possibleNonEatingMoves[indexInList];
            }
            else
            {
                pcMove = "Q";
            }

            return pcMove;
        }

        private string giveTurnTo(ref Player io_Player, ref Player io_Opponent, ref bool io_PlayerExecutedEat)
        {
            string move;

            if (io_Player.Name != "PC")
            {
                do
                {
                    UI.PromptTurn(io_Player.Name);
                    move = Console.ReadLine();
                }
                while (!validMove(io_Player, ref move));
            }
            else
            {
                move = choosePcMove(io_Player);
            }

            if (!io_Player.WantsToQuit(move))
            {
                r_GameBoard.MovePawn(move, io_Player);

                if (io_Player.AteOpponent(move))
                {
                    int colIndexToRemove = r_GameBoard.CalculateColIndexOfCellToRemove(move);
                    int rowIndexToRemove = r_GameBoard.CalculateRowIndexOfCellToRemove(move);

                    if (r_GameBoard.m_Grid[rowIndexToRemove, colIndexToRemove].Sign == io_Opponent.PawnSign)
                    {
                        io_Opponent.NumOfPawns--;
                    }
                    else
                    {
                        io_Opponent.NumOfKings--;
                    }

                    r_GameBoard.m_Grid[rowIndexToRemove, colIndexToRemove].PawnInCell = false;
                    io_PlayerExecutedEat = true;
                }
                else
                {
                    io_PlayerExecutedEat = false;
                }
            }

            return move;
        }
    }
}