using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Apthorpe.Chess
{
    public class PlayerController : MonoBehaviour
    {
        private Piece selectedPiece;

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                Square square = GetSquareAtMouseCoords();
                if (square != null)
                {
                    HandleOneSelectionAt(square);
                }
            }
            else if (Input.GetKeyUp(KeyCode.Mouse1) && selectedPiece != null && ChessManager.Instance.TurnNumber % 2 == selectedPiece.Team % 2)
            {
                Square destination = GetSquareAtMouseCoords();

                if (destination != null)
                {
                    ChessManager.Instance.TryMove(selectedPiece, destination);
                }
            }
        }

        private Square GetSquareAtMouseCoords()
        {
            Vector3 mousePos = Input.mousePosition; // get the mouse position
            mousePos = CamController.Instance.Cam.ScreenToWorldPoint(mousePos); // convert the mouse position from pixel coordinates to world point
            //Debug.Log($"{nameof(mousePos)} is {mousePos}.");
            Vector2Int mouseCoords = GetCoordsAtPos(mousePos.x, mousePos.y);

            Square square = null;

            // check if the coords fall within the board's dimensions
            if (mouseCoords.x >= 0 && mouseCoords.y >= 0 && mouseCoords.x < ChessManager.SizeX && mouseCoords.y < ChessManager.SizeY)
            {
                square = ChessManager.Board[mouseCoords.x, mouseCoords.y];
                //Debug.Log($"Got square with coords {square.Coords}");
            }

            return square;
        }

        private Vector2Int GetCoordsAtPos(float x, float y)
        {
            Vector2Int coords = new Vector2Int(-1, -1);

            // round the world space point to integer (always rounding down, since we're working with coordinates determined from bottom-left corner of GS)
            coords.x = Mathf.FloorToInt(x + ChessManager.StandardOffset);
            coords.y = Mathf.FloorToInt(y + ChessManager.StandardOffset);

            //Debug.Log($"{nameof(coords)} are {coords}.");

            return coords;
        }


        private void HandleOneSelectionAt(Square square)
        {
            if (square != null)
            {
                if (square.Occupier != null)
                {
                    Select(square.Occupier);
                }
                else
                {
                    DeselectAll();
                }
            }
        }

        private void Select(Piece piece)
        {
            selectedPiece = piece;
            Debug.Log($"Selected {selectedPiece.PieceGO.name}.");
        }

        private void DeselectAll()
        {
            selectedPiece = null;
            //Debug.Log("Deselected all.");
        }
    }
}