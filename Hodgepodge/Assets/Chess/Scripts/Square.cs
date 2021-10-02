using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Apthorpe.Chess
{
    public class Square
    {
        public Vector2Int Coords { get; set; }
        //public string ChessPos { get; set; }
        public Vector2 WorldPos { get; set; }

        public Piece Occupier { get; set; }

        public Square(Vector2Int _coords)
        {
            Coords = _coords;
        }
    }
}