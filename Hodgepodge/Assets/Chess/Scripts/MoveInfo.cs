using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Apthorpe.Chess
{
    public class MoveInfo
    {
        private int turnOfMove;
        public int TurnOfMove { get { return turnOfMove; } }

        private Piece pieceMoved;
        public Piece PieceMoved { get { return pieceMoved; } }

        private Square previousSquare;
        public Square PreviousSquare { get { return previousSquare; } }

        private Square squareMovedTo;
        public Square SquareMovedTo { get { return squareMovedTo; } }

        private Piece pieceCaptured;
        public Piece PieceCaptured { get { return pieceCaptured; } }

        private MovementType movementType;
        public MovementType MovementType { get { return movementType; } }

        // poss property: caused check
        // poss property: caused checkmate


        public MoveInfo(int _turnOfMove, Piece _pieceMoved, Square _previousSquare, Square _squareMovedTo, Piece _pieceCaptured, MovementType _movementType)
        {
            turnOfMove = _turnOfMove;
            pieceMoved = _pieceMoved;
            previousSquare = _previousSquare;
            squareMovedTo = _squareMovedTo;
            pieceCaptured = _pieceCaptured;
            movementType = _movementType;
        }
    }
}