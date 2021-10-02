using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Apthorpe.Chess {

    public enum PieceType { Bishop, King, Knight, Pawn, Rook, Queen}

    public enum MovementType {Normal, Castling, EnPassant, TwoStepAdvance}

    public class ChessManager : MonoBehaviour
    {
        public static ChessManager Instance { get; set; }

        private Square[,] board = new Square[SizeX, SizeY];
        public static Square[,] Board { get { return Instance.board; } }

        public const int SizeX = 8;
        public const int SizeY = 8;
        public const float StandardOffset = 0.5f;

        public const int WhiteTeamNum = 1;
        public const int BlackTeamNum = 2;

        public List<Piece> WhiteActivePieces { get; set; }
        public List<Piece> WhiteCaptivePieces { get; set; }

        public List<Piece> BlackActivePieces { get; set; }
        public List<Piece> BlackCaptivePieces { get; set; }

        [Range(1, int.MaxValue)]
        private int turnNumber = 1; // odd is white team's turn | even is black team's turn
        public int TurnNumber { get { return turnNumber; } }

        List<MoveInfo> moves;
        List<MoveInfo> Moves { get { return moves; } }

        public readonly Vector2Int whiteLeftCorner = new Vector2Int(0,0);
        public readonly Vector2Int whiteRightCorner = new Vector2Int(7,0);
        public readonly Vector2Int blackLeftCorner = new Vector2Int(0, 7); // oriented from white side
        public readonly Vector2Int blackRightCorner = new Vector2Int(7, 7); // oriented from white side

        public static King WhiteKing { get; set; }
        public static King BlackKing { get; set; }


        private void Awake()
        {
            UpholdSingleton();

            if (Instance == this)
            {
                Initialize();
            }
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                GoBackOneTurn();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                ReadGameState();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Bishop blackBishop2;
                foreach (Piece piece in BlackActivePieces)
                {
                    if (piece is Bishop && piece.PieceGO.name.Contains("2"))
                    {
                        blackBishop2 = (Bishop)piece;
                        Debug.Log($"bb2 square occupier: {blackBishop2.Pos.Occupier.PieceGO.name}");
                        break;
                    }
                }

                //Knight blackKnight1;
                //foreach (Piece piece in BlackActivePieces)
                //{
                //    if (piece is Knight && piece.PieceGO.name.Contains("1"))
                //    {
                //        blackKnight1 = (Knight)piece;
                //        Debug.Log($"bk1 square occupier: {blackKnight1.Pos.Occupier.PieceGO.name}");
                //        break;
                //    }
                //}
            }
        }

        private bool PlacePieceOn(Piece piece, Square square)
        {
            if (square.Occupier != null)
            {
                Debug.LogError("Can't place a piece on a square when there's already a piece there!");
                return false;
            }
            else if (piece.Pos != null)
            {
                Debug.LogError($"Canceling placement. Shouldn't place a piece which currently has a position!");
            }

            square.Occupier = piece;
            piece.Pos = square;
            return true;
        }

        private bool RemovePieceFrom(Piece piece, Square square)
        {
            if (square.Occupier == null)
            {
                Debug.LogError("Canceling removal. This piece wasn't occupying the square!");
                return false;
            }

            piece.Pos = null;
            square.Occupier = null;
            return true;
        }


        private void MovePieceVisually(Piece piece)
        {
            piece.PieceGO.transform.position = new Vector3Int(piece.Pos.Coords.x, piece.Pos.Coords.y, 0);
            Debug.Log($"{piece.PieceGO.name} to {Utility.ConvertPositionToChessTerms(piece.Pos)}.");
        }

        private void MovePieceVirtually(Piece piece, Square destination, Piece pieceToTake, MovementType movementType) // TODO: REMOVE PIECE & DESTINATION PARAMS
        {
            if (pieceToTake != null)
            {
                CaptureVirtually(pieceToTake);
            }

            Square prevSquare = piece.Pos; // need to cache it here b/c the piece's own tracker won't be accurate until the move gets added to its tracker
            
            // below was replaced by RemovePieceFrom and PlacePieceOn
            //piece.Pos.Occupier = null;
            //piece.Pos = destination;
            //destination.Occupier = piece;

            RemovePieceFrom(piece, piece.Pos);
            PlacePieceOn(piece, destination);

            MoveInfo move = new MoveInfo(turnNumber, piece, prevSquare, destination, pieceToTake, movementType);

            moves.Add(move);
            piece.MovesMade.Add(move);

            //piece.GetSquaresThreatened();

            //Debug.Log($"prev square: {piece.PreviousSquare.Coords}");
            //Debug.Log($"cur square: {piece.Pos.Coords}");
        }

        private void GoBackOneTurn()
        {
            if (turnNumber >= 2)
            {
                ReverseMove(true);
                
                turnNumber--;

                Debug.Log($"Went back one turn. Now on turn {turnNumber}.");
            }
        }

        private void ReverseMove(bool includingVisually = false)
        {
            if (moves.Count > 0)
            {
                MoveInfo move = moves.Last();
                Piece piece = move.PieceMoved;

                // below was replaced by RemovePieceFrom and PlacePieceOn
                //piece.Pos.Occupier = null;
                //piece.Pos = move.PreviousSquare;
                //move.PreviousSquare.Occupier = piece;

                RemovePieceFrom(piece, piece.Pos);
                PlacePieceOn(piece, move.PreviousSquare);

                switch (piece) // ref: https://stackoverflow.com/questions/7252186/switch-case-on-type-c-sharp
                {
                    case Pawn pawn:
                        if (pawn.MovesMade.Count <= 1)
                        {
                            pawn.CanStillTwoStepAdvance = true;
                        }
                        break;
                    case Rook rook:
                        if (rook.MovesMade.Count <= 1)
                        {
                            rook.HasMoved = false;
                        }
                        break;
                    case King king:
                        if (king.MovesMade.Count <= 1)
                        {
                            king.CanStillCastle = true;
                        }
                        break;
                    default:
                        break;
                }

                //switch(move.MovementType)
                //{
                //    case MovementType.Normal:
                //        break;
                //    case MovementType.Castling:
                //        if (move.PieceMoved is King)
                //        {
                //            King king = (King)move.PieceMoved;
                //            king.CanStillCastle = true;
                //        }
                //        else if (move.PieceMoved is Rook)
                //        {
                //            Rook rook = (Rook)move.PieceMoved;
                //            rook.HasMoved = false;
                //        }
                //        break;
                //    case MovementType.EnPassant:
                //        break;
                //    case MovementType.TwoStepAdvance:
                //        if (move.PieceMoved is Pawn)
                //        {
                //            Pawn pawn = (Pawn)move.PieceMoved;
                //            pawn.CanStillTwoStepAdvance = true;
                //        }
                //        break;
                //    default:
                //        throw new System.Exception();
                //}

                moves.Remove(move);
                piece.MovesMade.Remove(move);

                if (move.PieceCaptured != null)
                {
                    ReverseCaptureOf(move.PieceCaptured);
                }

                // if castling occurred, this reverses the other piece's move
                if (moves.Count > 0 && moves.Last().TurnOfMove == move.TurnOfMove)
                {
                    Debug.Log($"Reversing castling for {Utility.ConvertTeamNumToColor(piece.Team)}.");
                    ReverseMove(includingVisually);
                }

                if (includingVisually)
                {
                    MovePieceVisually(piece);
                    if (move.PieceCaptured != null)
                    {
                        MovePieceVisually(move.PieceCaptured);
                    }
                }

                //Debug.Log($"While on turn {turnNumber}, reversed move. The piece reversed was {piece.PieceGO.name}. Its reversed move was from {move.TurnOfMove}.");
            }
        }

        private void CaptureVisually(Piece piece)
        {
            piece.PieceGO.transform.position = new Vector3Int(-5, -5, 0);
            Debug.Log($"{Utility.ConvertTeamNumToColor(piece.Team)} {piece.PieceGO.name} was captured.");
        }

        private void CaptureVirtually(Piece piece)
        {
            // below was replaced by RemovePieceFrom
            //piece.Pos.Occupier = null;
            //piece.Pos = null;

            RemovePieceFrom(piece, piece.Pos);
            

            if (piece.Team == WhiteTeamNum)
            {
                WhiteActivePieces.Remove(piece);
                WhiteCaptivePieces.Add(piece);
            }
            else if (piece.Team == BlackTeamNum)
            {
                BlackActivePieces.Remove(piece);
                BlackCaptivePieces.Add(piece);
            }
        }

        private void ReverseCaptureOf(Piece piece)
        {
            // reverse the capture and place the formerly captured piece on the square it last occupied

            // below was replaced by PlacePieceOn
            //piece.Pos = piece.PreviousSquare;
            //piece.Pos.Occupier = piece;

            PlacePieceOn(piece, piece.PreviousSquare);

            if (piece.Team == WhiteTeamNum)
            {
                WhiteCaptivePieces.Remove(piece);
                WhiteActivePieces.Add(piece);
            }
            else if (piece.Team == BlackTeamNum)
            {
                BlackCaptivePieces.Remove(piece);
                BlackActivePieces.Add(piece);
            }
        }

        public bool TryMove(Piece piece, Square destination, bool actuallyAttemptMove = true)
        {
            bool movePossible = false;
            MovementType movementType = MovementType.Normal; // default
            Piece pieceToTake = null;
            Rook rookMovingAsPartOfCastle = null;

            switch(piece) // ref: https://stackoverflow.com/questions/7252186/switch-case-on-type-c-sharp
            {
                case Pawn pawn:
                    movePossible = TryPawnMove(pawn, destination, ref pieceToTake, ref movementType);
                    break;
                case Knight knight:
                    movePossible = TryKnightMove(knight, destination, ref pieceToTake);
                    break;
                case Bishop bishop:
                    movePossible = TryBishopMove(bishop, destination, ref pieceToTake);
                    break;
                case Rook rook:
                    movePossible = TryRookMove(rook, destination, ref pieceToTake);
                    break;
                case Queen queen:
                    movePossible = TryQueenMove(queen, destination, ref pieceToTake);
                    break;
                case King king:
                    movePossible = TryKingMove(king, destination, ref pieceToTake, ref rookMovingAsPartOfCastle, ref movementType);
                    break;
                default:
                    Debug.LogError($"Failed to account for piece type!");
                    break;
            }

            // virtual movement
            if (movePossible)
            {
                //MoveInfo move = new MoveInfo(turnNumber, piece, piece.PreviousSquare, destination, pieceToTake, movementType);

                MovePieceVirtually(piece, destination, pieceToTake, movementType);

                switch(piece)
                {
                    case Pawn pawn:
                        if (pawn.CanStillTwoStepAdvance)
                        {
                            pawn.CanStillTwoStepAdvance = false;
                        }
                        break;
                    case Rook rook:
                        if (!rook.HasMoved)
                        {
                            rook.HasMoved = true;
                        }
                        break;
                    case King king:
                        if (king.CanStillCastle)
                        {
                            king.CanStillCastle = false;
                        }
                        if (rookMovingAsPartOfCastle != null)
                        {
                            rookMovingAsPartOfCastle.HasMoved = true;
                            int rookMoveMod = rookMovingAsPartOfCastle.Pos.Coords.x == 0 ? 1 : -1; // for moving the rook one left/right of the castling king
                            MovePieceVirtually(rookMovingAsPartOfCastle, board[king.Pos.Coords.x + rookMoveMod, king.Pos.Coords.y], null, MovementType.Castling);
                        }
                        break;
                    default:
                        break;
                }

                // check if the virtual movement has incurred check; if so, reverse the move (keep in mind that in situations where the only viable moves would incur check, this causes a draw)
                // if I decide not to implement automatic checking yet, I will want to include a button of 'Request draw' or something (wherein the other player must approve for a draw to be declared)
                // checking for a draw would entail checking for all of the current team's legal moves; if it has no legal moves, it's a draw

                //King alliedKing = (piece.Team == WhiteTeamNum) ? WhiteKing : BlackKing;
                if (actuallyAttemptMove && !CheckForCheckAgainst(piece.Team)) // !CheckForThreatAtSquareAgainst(alliedKing.Pos, piece.Team)
                {
                    if (pieceToTake != null)
                    {
                        CaptureVisually(pieceToTake);
                    }

                    MovePieceVisually(piece);

                    if (rookMovingAsPartOfCastle != null)
                    {
                        MovePieceVisually(rookMovingAsPartOfCastle);
                    }

                    bool advanceTurn = true;

                    if (advanceTurn)
                    {
                        turnNumber++;
                    }

                    //King opposingKing = (piece.Team == WhiteTeamNum) ? BlackKing : WhiteKing;
                    if (CheckForCheckAgainst(piece.OpposingTeam)) // CheckForThreatAtSquareAgainst(opposingKing.Pos, opposingKing.Team)
                    {
                        // TODO: give notification to opposing team that they are in check
                        // TODO: check for checkmate
                        if (CheckForCheckmateAgainst(piece.OpposingTeam))
                        {
                            Debug.Log($"CHECKMATE. {Utility.ConvertTeamNumToColor(piece.Team)} is victorious!");
                            // TODO: implement end of game
                        }

                    }
                    else
                    {
                        if (CheckForStalemate())
                        {
                            Debug.Log($"STALEMATE. There is no victor this day...");
                        }
                    }
                    
                }
                else if (!actuallyAttemptMove && !CheckForCheckAgainst(piece.Team))
                {
                    ReverseMove();
                }
                else
                {
                    ReverseMove();
                    movePossible = false;
                }
            }

            return movePossible;
        }


        private bool CheckForCheckAgainst(int teamNum)
        {
            if (teamNum == WhiteTeamNum)
            {
                return CheckForThreatAtSquareAgainst(WhiteKing.Pos, WhiteTeamNum);
            }
            else
            {
                return CheckForThreatAtSquareAgainst(BlackKing.Pos, BlackTeamNum);
            }
        }

        private bool CheckForThreatAtSquareAgainst(Square square, int teamNum)
        {
            List<Piece> piecesPotentiallyThreatening = (teamNum == WhiteTeamNum) ? BlackActivePieces : WhiteActivePieces;
            //Square kingSquare = (teamNum == WhiteTeamNum) ? WhiteKing.Pos : BlackKing.Pos;

            foreach(Piece piece in piecesPotentiallyThreatening)
            {
                List<Square> threatenedSquares = piece.GetSquaresThreatened();
                if (threatenedSquares.Contains(square))
                {
                    if (square.Occupier != null)
                    {
                        if (square.Occupier is King)
                        {
                            Debug.Log($"{square.Occupier.PieceGO.name} in check from {piece.PieceGO.name}.");
                        }
                        else
                        {
                            Debug.Log($"{square.Occupier.PieceGO.name} threatened by {piece.PieceGO.name}.");
                        }
                    }
                    
                    return true;
                }
            }

            return false;
        }

        private bool CheckForCheckmateAgainst(int teamNum)
        {
            if (!CheckForCheckAgainst(teamNum))
            {
                Debug.Log($"Checkmate is not possible while the {Utility.ConvertTeamNumToColor(teamNum)} king is not in check.");
                return false;
            }
            
            List<Piece> activePieces = (teamNum == WhiteTeamNum) ? WhiteActivePieces : BlackActivePieces;
            bool checkmate = true; // default
            
            foreach (Piece piece in activePieces)
            {
                if (SeeIfAnyMoveResultsInNonCheck(piece))
                {
                    Debug.Log($"Not checkmate, thanks at least to {piece.PieceGO.name}.");
                    checkmate = false;
                    break;
                }
            }

            return checkmate;
        }

        private bool SeeIfAnyMoveResultsInNonCheck(Piece piece)
        {
            // loop thru the board, trying a move for the piece at each square
            for (int y = 0; y < board.GetLength(1); y++)
            {
                for (int x = 0; x < board.GetLength(0); x++)
                {
                    if (TryMove(piece, board[x, y], false))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CheckForStalemate()
        {
            int teamNumOfMovingTeam = (turnNumber % 2 == 0) ? BlackTeamNum : WhiteTeamNum;  // odd is white team's turn | even is black team's turn

            if (CheckForCheckAgainst(teamNumOfMovingTeam))
            {
                Debug.Log($"Stalemate is not possible while the the king of the moving team is in check.");
                return false;
            }

            List<Piece> activePieces = (teamNumOfMovingTeam == WhiteTeamNum) ? WhiteActivePieces : BlackActivePieces;
            bool stalemate = true; // default

            foreach (Piece piece in activePieces)
            {
                if (SeeIfAnyMovePossible(piece))
                {
                    Debug.Log($"Not stalemate, thanks at least to {piece.PieceGO.name}.");
                    stalemate = false;
                    break;
                }
            }

            return stalemate;
        }

        private bool SeeIfAnyMovePossible(Piece piece)
        {
            // loop thru the board, trying a move for the piece at each square
            for (int y = 0; y < board.GetLength(1); y++)
            {
                for (int x = 0; x < board.GetLength(0); x++)
                {
                    if (TryMove(piece, board[x, y], false))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        private bool TryPawnMove(Pawn pawn, Square destination, ref Piece pieceToTake, ref MovementType movementType)
        {
            //pawnMovementType = PawnMovementType.Normal;

            if (!Utility.VerifyFirstSquareIsBehindSecond(pawn.Pos, destination, pawn.Team)) // since pawns can never move laterally or backward
            {
                //Debug.Log("Pawns cannot move laterally or backward.");
                return false;
            }
            
            // vertical move
            if (Utility.VerifyInSameFile(pawn.Pos, destination))
            {
                int verticalDistance = Utility.GetVerticalDistanceBetweenSquares(pawn.Pos, destination);

                if (verticalDistance <= 2) // if space is one or two directly ahead
                {
                    if (verticalDistance == 1 && destination.Occupier == null)
                    {
                        return true;
                    }
                    else if (verticalDistance == 2 && pawn.CanStillTwoStepAdvance && destination.Occupier == null) // if two spaces away, pawn can still two-step advance, and destination is unoccupied
                    {
                        // if a piece is not in the square directly ahead of the pawn, allow
                        Square squareOneAhead = Utility.GetSquareAheadOf(pawn);
                        if (squareOneAhead != null && squareOneAhead.Occupier == null) // if piece not in the way
                        {
                            movementType = MovementType.TwoStepAdvance;
                            pawn.TurnOfTwoStepAdvance = turnNumber;
                            return true;
                        }
                        else
                        {
                            //Debug.Log("Can the pawn still two-step advance? Or is there a piece blocking the pawn's move?");
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            // diagonal move
            else if (Utility.VerifyDiagonal(pawn.Pos, destination) && Utility.GetManhattanDistanceBetweenSquares(pawn.Pos, destination) == Vector2Int.one) // if space is diagonal and one square away
            {
                if ((destination.Occupier != null && destination.Occupier.Team != pawn.Team)) // check that enemy piece is there
                {
                    pieceToTake = destination.Occupier;
                    return true;
                }
                else if (QualifiesForEnPassant(pawn, destination, ref pieceToTake)) // check for en passant
                {
                    movementType = MovementType.EnPassant;
                    //Capture(pieceToTake);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool TryKnightMove(Knight knight, Square destination, ref Piece pieceToTake)
        {
            // if destination is two away horizontally/vertically and one away vertically/horizontally
            // AND if destination is not occupied by friendly piece

            if ((Utility.VerifySquareIsNumAwayVertically(2, knight.Pos, destination) && Utility.VerifySquareIsNumAwayHorizontally(1, knight.Pos, destination))
                || 
                (Utility.VerifySquareIsNumAwayHorizontally(2, knight.Pos, destination) && Utility.VerifySquareIsNumAwayVertically(1, knight.Pos, destination)))
            {
                if (destination.Occupier == null)
                {
                    return true;
                }
                else if (destination.Occupier != null && destination.Occupier.Team != knight.Team)
                {
                    pieceToTake = destination.Occupier;
                    return true;
                }
            }

            return false;
        }

        private bool TryBishopMove(Bishop bishop, Square destination, ref Piece pieceToTake)
        {
            // if destination is diagonal
            // AND if every square on path except for origin & destination is unoccupied
            // AND if destination is not occupied by friendly piece

            List<Square> path = Utility.GetDiagonalPathBetweenSquares(bishop.Pos, destination);

            if (path != null && Utility.VerifyUnoccupied(path))
            {
                if (destination.Occupier == null)
                {
                    return true;
                }
                else if (destination.Occupier != null && destination.Occupier.Team != bishop.Team)
                {
                    pieceToTake = destination.Occupier;
                    return true;
                }
            }

            return false;
        }

        private bool TryRookMove(Rook rook, Square destination, ref Piece pieceToTake)
        {
            // if destination is horizontal/vertical
            // AND if every square on path except for origin & destination is unoccupied
            // AND if destination is not occupied by friendly piece

            List<Square> path = Utility.GetPerpendicularPathBetweenSquares(rook.Pos, destination);

            if (path != null && Utility.VerifyUnoccupied(path))
            {
                if (destination.Occupier == null)
                {
                    return true;
                }
                else if (destination.Occupier != null && destination.Occupier.Team != rook.Team)
                {
                    pieceToTake = destination.Occupier;
                    return true;
                }
            }

            return false;
        }

        private bool TryQueenMove(Queen queen, Square destination, ref Piece pieceToTake)
        {
            // if destination is perpendicular or diagonal
            // AND if every square on path except for origin & destination is unoccupied
            // AND if destination is not occupied by friendly piece

            List<Square> path = Utility.GetPerpendicularPathBetweenSquares(queen.Pos, destination);
            if (path == null)
            {
                path = Utility.GetDiagonalPathBetweenSquares(queen.Pos, destination);
            }

            if (path != null && Utility.VerifyUnoccupied(path))
            {
                if (destination.Occupier == null)
                {
                    return true;
                }
                else if (destination.Occupier != null && destination.Occupier.Team != queen.Team)
                {
                    pieceToTake = destination.Occupier;
                    return true;
                }
            }

            return false;
        }

        private bool TryKingMove(King king, Square destination, ref Piece pieceToTake, ref Rook rookMovingAsPartOfCastle, ref MovementType movementType)
        {
            // if destination is within one square and destination is not occupied by friendly piece
            if (Mathf.Abs(king.Pos.Coords.x - destination.Coords.x) <= 1 && Mathf.Abs(king.Pos.Coords.y - destination.Coords.y) <= 1)
            {
                if (destination.Occupier == null)
                {
                    return true;
                }
                else if (destination.Occupier != null && destination.Occupier.Team != king.Team)
                {
                    pieceToTake = destination.Occupier;
                    return true;
                }
            }

            // if above fails, check for castling
            if (CheckForCastling(king, destination, ref rookMovingAsPartOfCastle))
            {
                Debug.Log("Castling approved.");
                movementType = MovementType.Castling;
                return true;
            }

            return false;
        }
        

        private bool QualifiesForEnPassant(Pawn pawn, Square destination, ref Piece pieceToTake)
        {
            //Debug.Log("Checking for en passant.");
            
            // if the square behind your destination has an enemy pawn, that pawn two-step advanced in the previous turn, that enemy pawn is in the same rank as you and the same file as the destination square
            Square squareBehindDestination = Utility.GetSquareBehind(destination, pawn.Team);
            //Debug.Log($"squareBehindDestination determined to be the square at {squareBehindDestination.Coords}");

            if (squareBehindDestination != null && squareBehindDestination.Occupier != null && squareBehindDestination.Occupier is Pawn)
            {
                Pawn target = (Pawn)squareBehindDestination.Occupier;
                if (target.TurnOfTwoStepAdvance == turnNumber - 1 && Utility.VerifyInSameRank(pawn.Pos, target.Pos) && Utility.VerifyInSameFile(target.Pos, destination))
                {
                    pieceToTake = target;
                    return true;
                }
            }

            //Debug.Log("Did not qualify for en passant.");
            //pieceToTake = null;
            return false;
        }

        private bool CheckForCastling(King king, Square destination, ref Rook rookMovingAsPartOfCastle)
        {
            // if king can still castle, king is not currently in check, destination is two away horizontally, and destination is unoccupied
            // check for allied rook at corner in that direction; if it can still castle, and that and every space in between the king and the rook is empty

            Rook candidateRook = null;

            if (king.CanStillCastle && !CheckForCheckAgainst(king.Team) && Utility.VerifySquareIsNumAwayHorizontally(2, king.Pos, destination) && destination.Occupier == null)
            {
                Vector2Int dir = Utility.GetDirectionFromFirstSquareToSecond(king.Pos, destination);
                Vector2Int coordsToCheckForRook = new Vector2Int(-1, -1);

                if (dir.x < 0) // look for rook in left corner
                {
                    if (king.Team == WhiteTeamNum)
                    {
                        coordsToCheckForRook = whiteLeftCorner;
                    }
                    else if (king.Team == BlackTeamNum)
                    {
                        coordsToCheckForRook = blackLeftCorner;
                    }
                }
                else if (dir.x > 0) // look for rook in right corner
                {
                    if (king.Team == WhiteTeamNum)
                    {
                        coordsToCheckForRook = whiteRightCorner;
                    }
                    else if (king.Team == BlackTeamNum)
                    {
                        coordsToCheckForRook = blackRightCorner;
                    }
                }

                Square squareToCheckForRook = board[coordsToCheckForRook.x, coordsToCheckForRook.y];
                if (squareToCheckForRook.Occupier != null && squareToCheckForRook.Occupier is Rook)
                {
                    candidateRook = (Rook)squareToCheckForRook.Occupier;
                    if (!candidateRook.HasMoved)
                    {
                        List<Square> path = Utility.GetPerpendicularPathBetweenSquares(king.Pos, candidateRook.Pos);

                        if (path != null && Utility.VerifyUnoccupied(path))
                        {
                            // TODO: verify that none of the squares the KING passes are threatened by an enemy piece
                            //nota bene: "Castling is still permitted if the rook is under attack, or if the rook crosses an attacked square."

                            List<Square> kingPath = new List<Square>();
                            if (candidateRook.Pos.Coords.x < king.Pos.Coords.x) // king's path will be two to the left
                            {
                                kingPath.Add(board[king.Pos.Coords.x - 1, king.Pos.Coords.y]);
                                kingPath.Add(board[king.Pos.Coords.x - 2, king.Pos.Coords.y]);
                            }
                            else // king's path will be two to the right
                            {
                                kingPath.Add(board[king.Pos.Coords.x + 1, king.Pos.Coords.y]);
                                kingPath.Add(board[king.Pos.Coords.x + 2, king.Pos.Coords.y]);
                            }
                            
                            foreach (Square square in kingPath)
                            {
                                if (CheckForThreatAtSquareAgainst(square, king.Team))
                                {
                                    Debug.Log($"{king.PieceGO.name} cannot castle. A square the king would traverse is threatened.");
                                    return false;
                                }
                            }

                            rookMovingAsPartOfCastle = candidateRook;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void ReadGameState()
        {
            string whiteActivePiecesMessage = "WHITE ACTIVE PIECES: ";
            foreach(Piece piece in WhiteActivePieces)
            {
                whiteActivePiecesMessage += $"{piece.PieceGO.name}: {Utility.ConvertPositionToChessTerms(piece.Pos)} | ";
            }
            Debug.Log(whiteActivePiecesMessage);
            string whiteCaptivePiecesMessage = "WHITE CAPTIVE PIECES";
            foreach (Piece piece in WhiteCaptivePieces)
            {
                whiteCaptivePiecesMessage += $"{piece.PieceGO.name}: {Utility.ConvertPositionToChessTerms(piece.Pos)} | ";
            }
            Debug.Log(whiteCaptivePiecesMessage);

            string blackActivePiecesMessage = "BLACK ACTIVE PIECES: ";
            foreach (Piece piece in BlackActivePieces)
            {
                blackActivePiecesMessage += $"{piece.PieceGO.name}: {Utility.ConvertPositionToChessTerms(piece.Pos)} | ";
            }
            Debug.Log(blackActivePiecesMessage);
            string blackCaptivePiecesMessage = "BLACK CAPTIVE PIECES";
            foreach (Piece piece in BlackCaptivePieces)
            {
                blackCaptivePiecesMessage += $"{piece.PieceGO.name}: {Utility.ConvertPositionToChessTerms(piece.Pos)} | ";
            }
            Debug.Log(blackCaptivePiecesMessage);

            if (CheckForCheckAgainst(WhiteTeamNum))
            {
                Debug.Log("White king in check.");
            }
            if (CheckForCheckAgainst(BlackTeamNum))
            {
                Debug.Log("Black king in check.");
            }
        }

        private void ReadBoard()
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    Debug.Log($"Square at {Utility.ConvertPositionToChessTerms(board[x, y])} (coords: {board[x,y].Coords}).");
                }
            }
        }

        private void ReadPieces()
        {
            Debug.Log($"WhiteActivePieces count = {WhiteActivePieces.Count}");
            foreach(Piece piece in WhiteActivePieces)
            {
                Debug.Log($"White {piece.GetType()} at {Utility.ConvertPositionToChessTerms(piece.Pos)} (coords: {piece.Pos.Coords}).");
            }

            Debug.Log($"BlackActivePieces count = {BlackActivePieces.Count}");
            foreach (Piece piece in BlackActivePieces)
            {
                Debug.Log($"Black {piece.GetType()} at {Utility.ConvertPositionToChessTerms(piece.Pos)} (coords: {piece.Pos.Coords}).");
            }
        }

        


        private void SetUpPieces()
        {
            SetUpTeam(WhiteTeamNum);
            SetUpTeam(BlackTeamNum);
        }

        private void SetUpTeam(int teamNum)
        {
            string color = "";
            int firstRank = -1;
            int secondRank = -1;
            int knightX = 1;
            int bishopX = 2;
            int rookX = 0;
            List<Piece> activePieces = null;

            if (teamNum == WhiteTeamNum)
            {
                color = "White";
                firstRank = 1;
                secondRank = 0;
                activePieces = WhiteActivePieces;
            }
            else if (teamNum == BlackTeamNum)
            {
                color = "Black";
                firstRank = 6;
                secondRank = 7;
                activePieces = BlackActivePieces;
            }
            
            GameObject piecesGO = GameObject.Find($"{color}Pieces");

            for (int i = 1; i <= 8; i++)
            {
                Transform pawnTransform = piecesGO.transform.Find($"{color}Pawn{i}");
                Piece pawn = new Pawn(teamNum, pawnTransform.gameObject, board[i - 1, firstRank]);
                activePieces.Add(pawn);
            }

            for (int i = 1; i <= 2; i++)
            {
                Transform knightTransform = piecesGO.transform.Find($"{color}Knight{i}");
                Piece knight = new Knight(teamNum, knightTransform.gameObject, board[knightX, secondRank]);
                activePieces.Add(knight);
                knightX = 6;
            }

            for (int i = 1; i <= 2; i++)
            {
                Transform bishopTransform = piecesGO.transform.Find($"{color}Bishop{i}");
                Piece bishop = new Bishop(teamNum, bishopTransform.gameObject, board[bishopX, secondRank]);
                activePieces.Add(bishop);
                bishopX = 5;
            }

            for (int i = 1; i <= 2; i++)
            {
                Transform rookTransform = piecesGO.transform.Find($"{color}Rook{i}");
                Piece rook = new Rook(teamNum, rookTransform.gameObject, board[rookX, secondRank]);
                activePieces.Add(rook);
                rookX = 7;
            }

            Transform queenTransform = piecesGO.transform.Find($"{color}Queen");
            Piece queen = new Queen(teamNum, queenTransform.gameObject, board[3, secondRank]);
            activePieces.Add(queen);

            Transform kingTransform = piecesGO.transform.Find($"{color}King");
            Piece king = new King(teamNum, kingTransform.gameObject, board[4, secondRank]);
            activePieces.Add(king);
        }


        private void UpholdSingleton()
        {
            if (Instance == null)
            {
                //DontDestroyOnLoad(gameObject);
                Instance = this;
            }
            else if (Instance != this)
            {
                Debug.Log($"{GetType().Name} instance already exists! Destroying duplicate.");
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            WhiteActivePieces = new List<Piece>();
            WhiteCaptivePieces = new List<Piece>();
            BlackActivePieces = new List<Piece>();
            BlackCaptivePieces = new List<Piece>();

            moves = new List<MoveInfo>();

            // create the grid
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    board[x, y] = new Square(new Vector2Int(x, y));
                }
            }
            //ReadBoard();

            SetUpPieces();
            //ReadPieces();
        }
    }
}