﻿using System;

namespace C20_Ex02
{
    public class Board
    {
        public enum eBoardDimension
        {
            Small = 6,
            Medium = 8,
            Large = 10
        }

        private readonly eBoardDimension r_Dimension;
        public Cell[,] m_Grid;

        public Board(eBoardDimension i_Dimension)
        {
            r_Dimension = i_Dimension;
            m_Grid = new Cell[(int)r_Dimension, (int)r_Dimension];
            buildBoard(ref m_Grid);
        }

        public eBoardDimension Dimension
        {
            get
            {
                return r_Dimension;
            }
        }

        private static bool bothIndexAreEvenOrOdd(int i_Index1, int i_Index2)
        {
            return (i_Index1 % 2 == 0 && i_Index2 % 2 == 0) ||
                   (i_Index1 % 2 != 0 && i_Index2 % 2 != 0);
        }

        private void buildBoard(ref Cell[,] o_Board)
        {
            for (int i = 0; i < (int)r_Dimension; i++)
            {
                for (int j = 0; j < (int)r_Dimension; j++)
                {
                    if (bothIndexAreEvenOrOdd(i, j) || i == ((int)r_Dimension / 2) - 1 || i == ((int)r_Dimension / 2))
                    {
                        o_Board[i, j].Sign = Cell.eCellSign.Empty;
                        o_Board[i, j].PawnInCell = false;
                        continue;
                    }

                    o_Board[i, j].Sign = i <= ((int)r_Dimension / 2) - 1 ? Cell.eCellSign.PawnO : Cell.eCellSign.PawnX;

                    o_Board[i, j].PawnInCell = true;
                }
            }
        }

        public int CalculateRowIndexOfCellToRemove(string i_Move)
        {
            int rowIndex;

            // In this case the source cell is lower on the board the destination cell meaning we ate forward
            if (i_Move[1] - i_Move[4] > 0)
            {
                rowIndex = i_Move[1] - 'a' - 1;
            }
            else
            { // The source cell is higher on the source than the destination - meaning we ate backwards
                rowIndex = i_Move[1] - 'a' + 1;
            }

            return rowIndex;
        }

        public int CalculateColIndexOfCellToRemove(string i_Move)
        {
            int colIndex;

            // In this case the source cell is to the left of the destination cell meaning we ate to our right
            if (i_Move[0] - i_Move[3] < 0)
            {
                colIndex = i_Move[0] - 'A' + 1;
            }
            else
            { // The source cell is to the right of destination cell - meaning we ate to our left
                colIndex = i_Move[0] - 'A' - 1;
            }

            return colIndex;
        }

        public void MovePawn(string i_Move, Player i_Player)
        {
            int sourceColIndex = i_Move[0] - 'A';
            int sourceRowIndex = i_Move[1] - 'a';
            int destinationColIndex = i_Move[3] - 'A';
            int destinationRowIndex = i_Move[4] - 'a';

            if (m_Grid[sourceRowIndex, sourceColIndex].Sign == i_Player.PawnSign)
            {
                if (destinationRowIndex == 0 || destinationRowIndex == (int)r_Dimension - 1)
                {
                    m_Grid[destinationRowIndex, destinationColIndex].Sign = i_Player.KingSign;
                    i_Player.NumOfKings++;
                    i_Player.NumOfPawns--;
                }
                else
                {
                    m_Grid[destinationRowIndex, destinationColIndex].Sign = i_Player.PawnSign;
                }
            }
            else
            {
                m_Grid[destinationRowIndex, destinationColIndex].Sign = i_Player.KingSign;
            }

            m_Grid[sourceRowIndex, sourceColIndex].PawnInCell = false;
            m_Grid[destinationRowIndex, destinationColIndex].PawnInCell = true;
        }

        public void InitializeBoard()
        {
            buildBoard(ref m_Grid);
        }

        public bool PawnIsInFirstTwoColumnFromTheLeft(int i_ColumnNumber)
        {
            return i_ColumnNumber == 0 || i_ColumnNumber == 1;
        }

        public bool PawnIsInLastTwoColumnFromTheLeft(int i_ColumnNumber)
        {
            return i_ColumnNumber == (int)r_Dimension - 1 || i_ColumnNumber == (int)r_Dimension - 2;
        }

        public bool PawnIsOnFarLeft(int i_ColumnNumber)
        {
            return i_ColumnNumber == 0;
        }

        public bool PawnIsOnFarRight(int i_ColumnNumber)
        {
            return i_ColumnNumber == (int)r_Dimension - 1;
        }
    }
}
