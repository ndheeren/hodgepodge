using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Apthorpe.Chess
{
    public class Piece
    {
        //protected PieceType pieceType;
        //public PieceType PieceType { get { return pieceType; } }

        //public bool Captive { get; set; }

        protected int team;
        public int Team { get { return team; } }

        public int OpposingTeam { 
            get 
            {
                return (team == ChessManager.WhiteTeamNum) ? ChessManager.BlackTeamNum : ChessManager.WhiteTeamNum;
            } 
        }

        protected GameObject pieceGO;
        public GameObject PieceGO { get { return pieceGO; } }

        public Square Pos { get; set; }

        protected readonly Square originalPlacementPos;

        public List<MoveInfo> MovesMade { get; set; }

        public Square PreviousSquare {
            get 
            { 
                if (MovesMade.Count > 0)
                {
                    if (Pos == null) // i.e., the piece is captive -- should probably refactor this to make the code more clearly communicative (perhaps add a bool property named Captive which it uses?)
                    {
                        return MovesMade.Last().SquareMovedTo;
                    }
                    else
                    {
                        return MovesMade.Last().PreviousSquare;
                    }
                }
                else
                {
                    return originalPlacementPos;
                }
            } 
        }

        public virtual List<Square> GetSquaresThreatened() { throw new System.Exception("This method should never be called from its base class."); }

        public Piece(int _team, GameObject _pieceGO, Square _pos)
        {
            //pieceType = _pieceType;
            team = _team;
            pieceGO = _pieceGO;
            originalPlacementPos = _pos;
            Pos = _pos;

            Pos.Occupier = this;

            MovesMade = new List<MoveInfo>();
        }
    }

    public class Pawn : Piece
    {
        public bool CanStillTwoStepAdvance { get; set; }
        public int TurnOfTwoStepAdvance { get; set; }


        //public void PromotePawnTo(PieceType pieceTypeToPromoteTo)
        //{
        //    if (pieceType == PieceType.Pawn)
        //    {
        //        pieceType = pieceTypeToPromoteTo;

                
        //    }

        //    throw new System.NotImplementedException();
        //}

        public override List<Square> GetSquaresThreatened()
        {
            List<Square> squaresThreatened = new List<Square>();

            Vector2Int ahead = Utility.GetDirectionAhead(this);
            Vector2Int oneLeftDiagonal = Pos.Coords + ahead + Vector2Int.left;
            //Debug.Log($"oneLeftDiagonal = {oneLeftDiagonal}");
            Vector2Int oneRightDiagonal = Pos.Coords + ahead + Vector2Int.right;
            //Debug.Log($"oneRightDiagonal = {oneRightDiagonal}");

            if (Utility.CoordsWithinBounds(oneLeftDiagonal))
            {
                squaresThreatened.Add(ChessManager.Board[oneLeftDiagonal.x, oneLeftDiagonal.y]);
            }
            if (Utility.CoordsWithinBounds(oneRightDiagonal))
            {
                squaresThreatened.Add(ChessManager.Board[oneRightDiagonal.x, oneRightDiagonal.y]);
            }

            //string threatMsg = $"{pieceGO.name} threatens: ";
            //foreach(Square square in squaresThreatened)
            //{
            //    threatMsg += $"{Utility.ConvertPositionToChessTerms(square)} | ";
            //}
            //Debug.Log(threatMsg);

            return squaresThreatened;
        }


        public Pawn(int _team, GameObject _pieceGO, Square _pos) : base(_team, _pieceGO, _pos)
        {
            CanStillTwoStepAdvance = true;
            TurnOfTwoStepAdvance = -1;
        }
    }

    public class Knight : Piece
    {
        public override List<Square> GetSquaresThreatened()
        {
            List<Square> squaresThreatened = GetAllKnightMoves();

            //string threatMsg = $"{pieceGO.name} threatens: ";
            //foreach (Square square in squaresThreatened)
            //{
            //    threatMsg += $"{Utility.ConvertPositionToChessTerms(square)} | ";
            //}
            //Debug.Log(threatMsg);

            return squaresThreatened;
        }

        private List<Square> GetAllKnightMoves()
        {
            List<Square> squaresThreatened = new List<Square>();

            // mocking clockwise, starting at NE-most move
            Square oneRightTwoUp = Utility.GetSquareXAwayHorizontallyAndYAwayVertically(Pos, 1, 2);
            if (oneRightTwoUp != null) squaresThreatened.Add(oneRightTwoUp);

            Square twoRightOneUp = Utility.GetSquareXAwayHorizontallyAndYAwayVertically(Pos, 2, 1);
            if (twoRightOneUp != null) squaresThreatened.Add(twoRightOneUp);

            Square twoRightOneDown = Utility.GetSquareXAwayHorizontallyAndYAwayVertically(Pos, 2, -1);
            if (twoRightOneDown != null) squaresThreatened.Add(twoRightOneDown);

            Square oneRightTwoDown = Utility.GetSquareXAwayHorizontallyAndYAwayVertically(Pos, 1, -2);
            if (oneRightTwoDown != null) squaresThreatened.Add(oneRightTwoDown);

            Square oneLeftTwoDown = Utility.GetSquareXAwayHorizontallyAndYAwayVertically(Pos, -1, -2);
            if (oneLeftTwoDown != null) squaresThreatened.Add(oneLeftTwoDown);

            Square twoLeftOneDown = Utility.GetSquareXAwayHorizontallyAndYAwayVertically(Pos, -2, -1);
            if (twoLeftOneDown != null) squaresThreatened.Add(twoLeftOneDown);

            Square twoLeftOneUp = Utility.GetSquareXAwayHorizontallyAndYAwayVertically(Pos, -2, 1);
            if (twoLeftOneUp != null) squaresThreatened.Add(twoLeftOneUp);

            Square oneLeftTwoUp = Utility.GetSquareXAwayHorizontallyAndYAwayVertically(Pos, -1, 2);
            if (oneLeftTwoUp != null) squaresThreatened.Add(oneLeftTwoUp);

            return squaresThreatened;
        }

        
        public Knight(int _team, GameObject _pieceGO, Square _pos) : base(_team, _pieceGO, _pos) { }
    }

    public class Bishop : Piece
    {
        public override List<Square> GetSquaresThreatened()
        {
            List<Square> squaresThreatened = Utility.GetAllDiagonalMovesFrom(Pos);

            //string threatMsg = $"{pieceGO.name} threatens: ";
            //foreach (Square square in squaresThreatened)
            //{
            //    threatMsg += $"{Utility.ConvertPositionToChessTerms(square)} | ";
            //}
            //Debug.Log(threatMsg);

            return squaresThreatened;
        }

        public Bishop(int _team, GameObject _pieceGO, Square _pos) : base(_team, _pieceGO, _pos) { }
    }

    public class Rook : Piece
    {
        public bool HasMoved { get; set; }


        public override List<Square> GetSquaresThreatened()
        {
            List<Square> squaresThreatened = Utility.GetAllPerpendicularMovesFrom(Pos);

            //string threatMsg = $"{pieceGO.name} threatens: ";
            //foreach (Square square in squaresThreatened)
            //{
            //    threatMsg += $"{Utility.ConvertPositionToChessTerms(square)} | ";
            //}
            //Debug.Log(threatMsg);

            return squaresThreatened;
        }


        public Rook(int _team, GameObject _pieceGO, Square _pos) : base(_team, _pieceGO, _pos) 
        {
            HasMoved = false;
        }
    }

    public class Queen : Piece
    {
        public override List<Square> GetSquaresThreatened()
        {
            List<Square> perpendicularSquaresThreatened = Utility.GetAllPerpendicularMovesFrom(Pos);
            List<Square> diagonalSquaresThreatened = Utility.GetAllDiagonalMovesFrom(Pos);

            List<Square> squaresThreatened = perpendicularSquaresThreatened.Concat(diagonalSquaresThreatened).ToList();

            //string threatMsg = $"{pieceGO.name} threatens: ";
            //foreach (Square square in squaresThreatened)
            //{
            //    threatMsg += $"{Utility.ConvertPositionToChessTerms(square)} | ";
            //}
            //Debug.Log(threatMsg);

            return squaresThreatened;
        }


        public Queen(int _team, GameObject _pieceGO, Square _pos) : base(_team, _pieceGO, _pos) { }
    }

    public class King : Piece
    {
        public bool CanStillCastle { get; set; }


        public override List<Square> GetSquaresThreatened()
        {
            List<Square> squaresThreatened = new List<Square>();

            // check clockwise around self, starting N
            Vector2Int[] coordsToCheck = {
                Pos.Coords + Vector2Int.up, // N
                Pos.Coords + Vector2Int.up + Vector2Int.right, // NE
                Pos.Coords + Vector2Int.right, // E
                Pos.Coords + Vector2Int.down + Vector2Int.right, // SE
                Pos.Coords + Vector2Int.down, // S
                Pos.Coords + Vector2Int.down + Vector2Int.left, // SW
                Pos.Coords + Vector2Int.left, // W
                Pos.Coords + Vector2Int.up + Vector2Int.left, // NW
            };

            foreach(Vector2Int coords in coordsToCheck)
            {
                if (Utility.CoordsWithinBounds(coords))
                {
                    squaresThreatened.Add(ChessManager.Board[coords.x, coords.y]);
                }
            }

            //string threatMsg = $"{pieceGO.name} threatens: ";
            //foreach (Square square in squaresThreatened)
            //{
            //    threatMsg += $"{Utility.ConvertPositionToChessTerms(square)} | ";
            //}
            //Debug.Log(threatMsg);

            return squaresThreatened;
        }


        public King(int _team, GameObject _pieceGO, Square _pos) : base(_team, _pieceGO, _pos)
        {
            CanStillCastle = true;

            if (_team == ChessManager.WhiteTeamNum)
            {
                ChessManager.WhiteKing = this;
            }
            else if (_team == ChessManager.BlackTeamNum)
            {
                ChessManager.BlackKing = this;
            }
        }
    }
}
