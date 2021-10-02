using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Apthorpe.Chess
{
    public class Utility
    {
        public static string ConvertPositionToChessTerms(Square square)
        {
            string pos = "";

            switch (square.Coords.x)
            {
                case 0:
                    pos += "a";
                    break;
                case 1:
                    pos += "b";
                    break;
                case 2:
                    pos += "c";
                    break;
                case 3:
                    pos += "d";
                    break;
                case 4:
                    pos += "e";
                    break;
                case 5:
                    pos += "f";
                    break;
                case 6:
                    pos += "g";
                    break;
                case 7:
                    pos += "h";
                    break;
                default:
                    pos += "?";
                    break;
            }

            pos += square.Coords.y + 1;

            return pos;
        }

        public static string ConvertTeamNumToColor(int teamNum)
        {
            if (teamNum == ChessManager.WhiteTeamNum)
            {
                return "White";
            }
            else if (teamNum == ChessManager.BlackTeamNum)
            {
                return "Black";
            }
            else
            {
                Debug.LogError($"Value of {teamNum} is unaccounted for.");
                return null;
            }
        }


        public static bool VerifyInSameRank(Square firstSquare, Square secondSquare)
        {
            if (firstSquare.Coords.y == secondSquare.Coords.y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool VerifyInSameFile(Square firstSquare, Square secondSquare)
        {
            if (firstSquare.Coords.x == secondSquare.Coords.x)
            {
                return true;
            }
            else
            {
                //Debug.Log($"Square at {firstSquare.Coords}) not in same file as square at {secondSquare.Coords}.");
                return false;
            }
        }

        public static bool VerifyDiagonal(Square firstSquare, Square secondSquare)
        {
            // if aX != bX && aY != bY && Abs(aX - bX) == Abs(aY - bY)
            Vector2Int a = firstSquare.Coords;
            Vector2Int b = secondSquare.Coords;

            if (a.x != b.x && a.y != b.y && Mathf.Abs(a.x - b.x) == Mathf.Abs(a.y - b.y))
            {
                return true;
            }

            return false;
        }

        public static bool VerifyPerpendicular(Square firstSquare, Square secondSquare)
        {
            Vector2Int a = firstSquare.Coords;
            Vector2Int b = secondSquare.Coords;

            if (a.x == b.x || a.y == b.y)
            {
                return true;
            }

            return false;
        }

        public static bool VerifyFirstSquareIsBehindSecond(Square firstSquare, Square secondSquare, int relativeToThisTeam)
        {
            if (relativeToThisTeam == ChessManager.WhiteTeamNum && firstSquare.Coords.y < secondSquare.Coords.y)
            {
                return true;
            }
            else if (relativeToThisTeam == ChessManager.BlackTeamNum && firstSquare.Coords.y > secondSquare.Coords.y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Vector2Int GetManhattanDistanceBetweenSquares(Square firstSquare, Square secondSquare)
        {
            return new Vector2Int(GetHorizontalDistanceBetweenSquares(firstSquare, secondSquare), GetVerticalDistanceBetweenSquares(firstSquare, secondSquare));
        }

        public static int GetHorizontalDistanceBetweenSquares(Square firstSquare, Square secondSquare)
        {
            return Mathf.Abs(firstSquare.Coords.x - secondSquare.Coords.x);
        }

        public static int GetVerticalDistanceBetweenSquares(Square firstSquare, Square secondSquare)
        {
            return Mathf.Abs(firstSquare.Coords.y - secondSquare.Coords.y);
        }

        public static Square GetSquareBehind(Square square, int relativeToTeam)
        {
            int oneBehind = 0;
            if (relativeToTeam == ChessManager.WhiteTeamNum)
            {
                oneBehind = -1;
            }
            else if (relativeToTeam == ChessManager.BlackTeamNum)
            {
                oneBehind = 1;
            }

            Vector2Int coords = new Vector2Int(square.Coords.x, square.Coords.y + oneBehind);
            if (CoordsWithinBounds(coords))
            {
                return ChessManager.Board[coords.x, coords.y];
            }
            else
            {
                return null;
            }
        }

        public static bool CoordsWithinBounds(Vector2Int coords)
        {
            if (coords.x >= 0 && coords.y >= 0 && coords.x < ChessManager.Board.GetLength(0) && coords.y < ChessManager.Board.GetLength(1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Square GetSquareAheadOf(Piece piece)
        {
            int oneAhead = 0;
            if (piece.Team == ChessManager.WhiteTeamNum)
            {
                oneAhead = 1;
            }
            else if (piece.Team == ChessManager.BlackTeamNum)
            {
                oneAhead = -1;
            }

            Vector2Int coords = new Vector2Int(piece.Pos.Coords.x, piece.Pos.Coords.y + oneAhead);

            if (CoordsWithinBounds(coords))
            {
                return ChessManager.Board[coords.x, coords.y];
            }
            else
            {
                return null;
            }
        }

        public static Square GetSquareXAwayHorizontallyAndYAwayVertically(Square square, int x, int y)
        {
            Vector2Int coords = new Vector2Int(square.Coords.x + x, square.Coords.y + y);
            
            if (CoordsWithinBounds(coords))
            {
                return ChessManager.Board[coords.x, coords.y];
            }
            else
            {
                return null;
            }
        }

        public static Vector2Int GetDirectionAhead(Piece piece)
        {
            if (piece.Team == ChessManager.WhiteTeamNum)
            {
                return Vector2Int.up;
            }
            else if (piece.Team == ChessManager.BlackTeamNum)
            {
                return Vector2Int.down;
            }

            throw new System.Exception();
        }

        public static bool VerifySquareIsNumAwayVertically(int num, Square square1, Square square2)
        {
            if (Mathf.Abs(square1.Coords.y - square2.Coords.y) == num)
            {
                return true;
            }

            return false;
        }

        public static bool VerifySquareIsNumAwayHorizontally(int num, Square square1, Square square2)
        {
            if (Mathf.Abs(square1.Coords.x - square2.Coords.x) == num)
            {
                return true;
            }

            return false;
        }


        public static List<Square> GetDiagonalPathBetweenSquares(Square square1, Square square2)
        {
            if (!VerifyDiagonal(square1, square2))
            {
                return null;
            }

            List<Square> path = new List<Square>();
            Vector2Int direction = GetDirectionFromFirstSquareToSecond(square1, square2);

            bool tracingPath = true;
            int ilp = 0;
            Vector2Int progression = Vector2Int.zero;
            while (tracingPath)
            {
                // note that neither sq1 nor sq2 are included in the path

                progression += direction;
                Square nextSquare = ChessManager.Board[square1.Coords.x + progression.x, square1.Coords.y + progression.y];
                if (nextSquare == square2)
                {
                    return path;
                }

                path.Add(nextSquare);

                ilp++;
                if (ilp > 100)
                {
                    Debug.LogWarning($"Tripped ILP in {System.Reflection.MethodBase.GetCurrentMethod().Name}.");
                    break;
                }
            }

            throw new System.NotImplementedException();
        }

        public static List<Square> GetAllDiagonalMovesFrom(Square square)
        {
            // going clockwise, starting from NE
            List<Square> neSquaresThreatened = GetAllMovesFromSquareInDirection(square, new Vector2Int(1, 1));
            List<Square> seSquaresThreatened = GetAllMovesFromSquareInDirection(square, new Vector2Int(1, -1));
            List<Square> swSquaresThreatened = GetAllMovesFromSquareInDirection(square, new Vector2Int(-1, -1));
            List<Square> nwSquaresThreatened = GetAllMovesFromSquareInDirection(square, new Vector2Int(-1, 1));

            List<Square> threatenedSquares = neSquaresThreatened
                .Concat(seSquaresThreatened)
                .Concat(swSquaresThreatened)
                .Concat(nwSquaresThreatened)
                .ToList();

            return threatenedSquares;
        }

        public static List<Square> GetPerpendicularPathBetweenSquares(Square square1, Square square2)
        {
            if (!VerifyPerpendicular(square1, square2))
            {
                return null;
            }

            List<Square> path = new List<Square>();
            Vector2Int direction = GetDirectionFromFirstSquareToSecond(square1, square2);

            bool tracingPath = true;
            int ilp = 0;
            Vector2Int progression = Vector2Int.zero;

            while (tracingPath)
            {
                // note that neither sq1 nor sq2 are included in the path

                progression += direction;
                Square nextSquare = ChessManager.Board[square1.Coords.x + progression.x, square1.Coords.y + progression.y];
                if (nextSquare == square2)
                {
                    return path;
                }

                path.Add(nextSquare);

                ilp++;
                if (ilp > 100)
                {
                    Debug.LogWarning($"Tripped ILP in {System.Reflection.MethodBase.GetCurrentMethod().Name}.");
                    break;
                }
            }

            throw new System.NotImplementedException();
        }

        public static List<Square> GetAllPerpendicularMovesFrom(Square square)
        {
            // going clockwise, starting from N
            List<Square> northSquaresThreatened = GetAllMovesFromSquareInDirection(square, new Vector2Int(0, 1));
            List<Square> eastSquaresThreatened = GetAllMovesFromSquareInDirection(square, new Vector2Int(1, 0));
            List<Square> southSquaresThreatened = GetAllMovesFromSquareInDirection(square, new Vector2Int(0, -1));
            List<Square> westSquaresThreatened = GetAllMovesFromSquareInDirection(square, new Vector2Int(-1, 0));

            List<Square> threatenedSquares = northSquaresThreatened
                .Concat(eastSquaresThreatened)
                .Concat(southSquaresThreatened)
                .Concat(westSquaresThreatened)
                .ToList();

            return threatenedSquares;
        }


        private static List<Square> GetAllMovesFromSquareInDirection(Square square, Vector2Int direction)
        {
            List<Square> threatenedSquares = new List<Square>();
            int ilp = 0;
            bool traveling = true;
            Vector2Int coords = square.Coords;

            while (traveling)
            {
                coords += direction;
                if (CoordsWithinBounds(coords))
                {
                    Square nextSquare = ChessManager.Board[coords.x, coords.y];
                    threatenedSquares.Add(nextSquare);

                    if (nextSquare.Occupier != null)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }

                ilp++;
                if (ilp > 100)
                {
                    throw new System.Exception("ILP hit.");
                }
            }

            return threatenedSquares;
        }


        public static Vector2Int GetDirectionFromFirstSquareToSecond(Square square1, Square square2)
        {
            Vector2Int firstCoords = square1.Coords;
            Vector2Int secondCoords = square2.Coords;

            if (firstCoords == secondCoords)
            {
                return Vector2Int.zero;
            }
            else if (firstCoords.x == secondCoords.x)
            {
                if (firstCoords.y < secondCoords.y) // N
                {
                    return new Vector2Int(0, 1);
                }
                else if (firstCoords.y > secondCoords.y) // S
                {
                    return new Vector2Int(0, -1);
                }
            }
            else if (firstCoords.y == secondCoords.y)
            {
                if (firstCoords.x < secondCoords.x) // E
                {
                    return new Vector2Int(1, 0);
                }
                else if (firstCoords.x > secondCoords.x) // W
                {
                    return new Vector2Int(-1, 0);
                }
            }
            else if (firstCoords.x < secondCoords.x)
            {
                if (firstCoords.y < secondCoords.y) // NE
                {
                    return new Vector2Int(1, 1);
                }
                else if (firstCoords.y > secondCoords.y) // SE
                {
                    return new Vector2Int(1, -1);
                }
            }
            else if (firstCoords.x > secondCoords.x)
            {
                if (firstCoords.y < secondCoords.y) // NW
                {
                    return new Vector2Int(-1, 1);
                }
                else if (firstCoords.y > secondCoords.y) // SW
                {
                    return new Vector2Int(-1, -1);
                }
            }

            Debug.LogError($"Failed to get direction.");
            throw new System.Exception();
        }

        public static bool VerifyUnoccupied(List<Square> squares)
        {
            foreach (Square square in squares)
            {
                if (square.Occupier != null)
                {
                    return false;
                }
            }

            return true;
        }

    }
}