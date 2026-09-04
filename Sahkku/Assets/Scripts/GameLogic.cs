using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameLogic;

public class GameLogic : MonoBehaviour
{
    public static GameLogic Instance;

    public const int BOARD_SIZE_X = 15;
    public const int BOARD_SIZE_Y = 3;
    public const int BOARD_QUEEN_DISTANCE = 4;

    public enum PieceType
    {
        Soldier,
        King,
        Queen
    }

    public enum PieceOwner
    {
        None,
        P1,
        P2
    }

    [Serializable]
    public class Piece
    {
        public int placeIndex;
        public PieceType type;
        public PieceOwner owner;
        public bool isActive;
        public bool canBeActivated;
        public List<int> allowedPlaces;

        public Piece(int placeIndex, PieceType type = PieceType.Soldier, PieceOwner owner = PieceOwner.P1, bool isActive = false)
        {
            this.placeIndex = placeIndex;
            this.type = type;
            this.owner = owner;
            this.isActive = isActive;
            canBeActivated = false;
            allowedPlaces = new List<int>();
        }

        public bool IsSelectable()
        {
            return allowedPlaces.Count > 0;
        }
    }

    [Serializable]
    public class Place
    {
        public int x, y;
        public List<Piece> pieces = new List<Piece>();

        public Place(int x, int y)
        {
            this.x = x;
            this.y = y;
            pieces = new List<Piece>();
        }
    }

    public enum D4
    {
        Sahhku,
        Three,
        Two,
        Zero
    }

    public enum TurnPhase
    {
        P1roll,
        P1move,
        P2roll,
        P2move
    }

    public List<Place> places;
    public List<D4> dice;
    TurnPhase turnPhase;
    int currentActiveDie;
    bool gameOver;
    int p1captures;
    int p2captures;
    float aiTimer = 0.0f;
    float aiSpeed = 0.33f;
    bool aiCanAct = false;
    int playerPieceIndex = 0;

    void Awake()
    {
        Instance = this;
        InitGame();
    }

    void Update()
    {
        UpdateBoard();

        if (gameOver)
        {
            ClearAllAllowedPlaces();
            return;
        }

        aiCanAct = false;
        if (Time.time > aiTimer)
        {
            aiTimer = Time.time + aiSpeed;
            aiCanAct = true;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame || (GameSettings.singlePlayer && GetCurrentPlayer() == PieceOwner.P2 && aiCanAct))
        {
            if (turnPhase == TurnPhase.P1roll || turnPhase == TurnPhase.P2roll || CanReroll())
            {
                if(turnPhase == TurnPhase.P1roll || turnPhase == TurnPhase.P2roll)
                {
                    ThrowAllDice();
                    OrderDie();
                    currentActiveDie = 0;
                    turnPhase++;
                }
                else
                {
                    RerollSingleDie();
                    OrderDie();
                }

                if (!CheckAllowedPieceMovement())
                {
                    NextPlayerTurn();
                }
            }
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame) playerPieceIndex = 0;
        if (Keyboard.current.digit2Key.wasPressedThisFrame) playerPieceIndex = 1;
        if (Keyboard.current.digit3Key.wasPressedThisFrame) playerPieceIndex = 2;
        if (Keyboard.current.digit4Key.wasPressedThisFrame) playerPieceIndex = 3;
        if (Keyboard.current.digit5Key.wasPressedThisFrame) playerPieceIndex = 4;
        if (Keyboard.current.digit6Key.wasPressedThisFrame) playerPieceIndex = 5;
        if (Keyboard.current.digit7Key.wasPressedThisFrame) playerPieceIndex = 6;
        if (Keyboard.current.digit8Key.wasPressedThisFrame) playerPieceIndex = 7;

        if (Keyboard.current.enterKey.wasPressedThisFrame || (GameSettings.singlePlayer && GetCurrentPlayer() == PieceOwner.P2 && aiCanAct))
        {
            if (turnPhase == TurnPhase.P1move || turnPhase == TurnPhase.P2move)
            {
                List<Piece> potentialPieces = GetAllPotentialPieces();

                // Move selected piece to random direction
                int pieceIndex = Mathf.Clamp(playerPieceIndex, 0, potentialPieces.Count - 1); ;

                // Randomize ai piece selection
                if (GameSettings.singlePlayer && GetCurrentPlayer() == PieceOwner.P2)
                {
                    pieceIndex = UnityEngine.Random.Range(0, potentialPieces.Count);
                }

                bool pieceMoved = false;
                foreach(Place place in places)
                {
                    foreach(Piece piece in place.pieces)
                    {
                        if(piece.allowedPlaces.Count > 0)
                        {
                            if (piece == potentialPieces[pieceIndex])
                            {
                                MovePiece(piece, UnityEngine.Random.Range(0, 4));
                                pieceMoved = true;
                            }
                        }

                        if (pieceMoved) break;
                    }
                    if (pieceMoved) break;
                }

                if(!pieceMoved)
                {
                    Debug.Log("No piece to move, continue to next turn");
                    currentActiveDie = dice.Count;
                }

                if(gameOver)
                {
                    return;
                }

                currentActiveDie++;
                if (currentActiveDie >= dice.Count)
                {
                    NextPlayerTurn();
                }
                else
                {
                    if (!CheckAllowedPieceMovement())
                    {
                        Debug.Log("No piece to move, continue to next turn");
                        NextPlayerTurn();
                    }
                }
            }
        }
    }

    void NextPlayerTurn()
    {
        ClearAllAllowedPlaces();
        currentActiveDie = 0;

        if (GetCurrentPlayer() == PieceOwner.P2)
        {
            Debug.Log("P1 turn");
            turnPhase = TurnPhase.P1roll;
        }
        else
        {
            Debug.Log("P2 turn");
            turnPhase = TurnPhase.P2roll;
        }
    }

    PieceOwner GetCurrentPlayer()
    {
        if((int)turnPhase < 2)
        {
            return PieceOwner.P1;
        }
        return PieceOwner.P2;
    }

    public void InitGame()
    {
        Debug.Log("Init game");
        gameOver = false;
        p1captures = 0;
        p2captures = 0;

        // Create the board and connect places in S-shape
        places = new List<Place>();
        int y = 0;
        for (int x = 0; x < BOARD_SIZE_X; ++x)
        {
            Debug.Log("Create place " + x + ", " + y);
            Place place = new Place(x, y);
            places.Add(place);
        }

        y = 1;
        for (int x = BOARD_SIZE_X -1; x >= 0; --x)
        {
            Debug.Log("Create place " + x + ", " + y);
            Place place = new Place(x, y);
            places.Add(place);
        }

        y = 2;
        for (int x = 0; x < BOARD_SIZE_X; ++x)
        {
            Debug.Log("Create place " + x + ", " + y);
            Place place = new Place(x, y);
            places.Add(place);
        }

        // Add pieces to their initial places
        for (int i = 0; i < places.Count; ++i)
        {
            if (places[i].y == 0)
            {
                places[i].pieces.Add(new Piece(i, PieceType.Soldier, PieceOwner.P1, false));
            }

            if (places[i].y == 2)
            {
                places[i].pieces.Add(new Piece(i, PieceType.Soldier, PieceOwner.P2, false));
            }

            if (places[i].y == 1 && places[i].x == BOARD_QUEEN_DISTANCE - 1)
            {
                places[i].pieces.Add(new Piece(i, PieceType.Queen, PieceOwner.P2, false));
            }

            if (places[i].y == 1 && places[i].x == Mathf.Floor(BOARD_SIZE_X / 2))
            {
                places[i].pieces.Add(new Piece(i, PieceType.King, PieceOwner.None, false));
            }

            if (places[i].y == 1 && places[i].x == BOARD_SIZE_X - BOARD_QUEEN_DISTANCE)
            {
                places[i].pieces.Add(new Piece(i, PieceType.Queen, PieceOwner.P1, false));
            }
        }

        // Set up piece activations for the beginning of the game
        Debug.Log("Even odds: " + GameSettings.evenOdds);
        if (GameSettings.evenOdds)
        {
            places[GetPlaceIndexFromCoordinates(BOARD_SIZE_X - 1, 0)].pieces[0].isActive = true;
            places[GetPlaceIndexFromCoordinates(0, BOARD_SIZE_Y - 1)].pieces[0].isActive = true;
            places[GetPlaceIndexFromCoordinates(BOARD_SIZE_X - 1, 1)].pieces[0].isActive = true;
            places[GetPlaceIndexFromCoordinates(1, BOARD_SIZE_Y - 1)].pieces[0].isActive = true;
            places[GetPlaceIndexFromCoordinates(BOARD_SIZE_X - 1, 2)].pieces[0].isActive = true;
            places[GetPlaceIndexFromCoordinates(2, BOARD_SIZE_Y - 1)].pieces[0].isActive = true;

            places[GetPlaceIndexFromCoordinates(BOARD_SIZE_X - 1, 3)].pieces[0].canBeActivated = true;
            places[GetPlaceIndexFromCoordinates(3, BOARD_SIZE_Y - 1)].pieces[0].canBeActivated = true;
        }
        else
        {
            places[GetPlaceIndexFromCoordinates(BOARD_SIZE_X - 1, 0)].pieces[0].canBeActivated = true;
            places[GetPlaceIndexFromCoordinates(0, BOARD_SIZE_Y - 1)].pieces[0].canBeActivated = true;
        }

        places[GetPlaceIndexFromCoordinates(BOARD_SIZE_X - BOARD_QUEEN_DISTANCE, 1)].pieces[0].canBeActivated = true;
        places[GetPlaceIndexFromCoordinates(BOARD_QUEEN_DISTANCE - 1, 1)].pieces[0].canBeActivated = true;

        // Initialize die
        dice = new List<D4>();
        dice.Add(D4.Zero);
        dice.Add(D4.Zero);
        dice.Add(D4.Zero);

        // Set the starting player
        if(GameSettings.startingPlayer == GameSettings.Player.One)
        {
            turnPhase = TurnPhase.P1roll;
            Debug.Log("P1 starts");
        }
        else
        {
            turnPhase = TurnPhase.P2roll;
            Debug.Log("P2 starts");
        }
    }

    void UpdateBoard()
    {
        float yOffset = 0.2f;
        foreach (Place p in places)
        {
            if (p.pieces.Count > 0)
            {
                for (int i = 0; i < p.pieces.Count; ++i)
                {
                    if (p.pieces[i].owner == PieceOwner.None)
                    {
                        Debug.DrawRay(new Vector3(p.x, 0.0f, p.y + yOffset * i), Vector3.up, Color.yellow);
                    }
                    else if (p.pieces[i].owner == PieceOwner.P1)
                    {
                        if (p.pieces[i].IsSelectable())
                        {
                            Debug.DrawRay(new Vector3(p.x, 0.0f, p.y + yOffset * i), Vector3.down, Color.purple);
                        }

                        if (p.pieces[i].isActive)
                        {
                            Debug.DrawRay(new Vector3(p.x, 0.0f, p.y + yOffset * i), Vector3.up, Color.blue);
                        }
                        else if (p.pieces[i].canBeActivated)
                        {
                            Debug.DrawRay(new Vector3(p.x, 0.0f, p.y + yOffset * i), Vector3.up, Color.cyan);
                        }
                        else
                        {
                            Debug.DrawRay(new Vector3(p.x, 0.0f, p.y + yOffset * i), Vector3.up, Color.darkSlateBlue);
                        }
                    }
                    else if (p.pieces[0].owner == PieceOwner.P2)
                    {
                        if (p.pieces[i].IsSelectable())
                        {
                            Debug.DrawRay(new Vector3(p.x, 0.0f, p.y + yOffset * i), Vector3.down, Color.purple);
                        }

                        if (p.pieces[i].isActive)
                        {
                            Debug.DrawRay(new Vector3(p.x, 0.0f, p.y + yOffset * i), Vector3.up, Color.red);
                        }
                        else if (p.pieces[i].canBeActivated)
                        {
                            Debug.DrawRay(new Vector3(p.x, 0.0f, p.y + yOffset * i), Vector3.up, Color.hotPink);
                        }
                        else
                        {
                            Debug.DrawRay(new Vector3(p.x, 0.0f, p.y + yOffset * i), Vector3.up, Color.crimson);
                        }
                    }
                }
            }
            else
            {
                Debug.DrawRay(new Vector3(p.x, 0.0f, p.y), Vector3.up, Color.white);
            }
        }
    }

    void RerollSingleDie()
    {
        Debug.Log("Throw single die");
        dice[0] = RandomThrow();
        GameInteraction.Instance.RollDice(0);
    }

    void ThrowAllDice()
    {
        Debug.Log("Throw all dice");
        for (int i = 0; i < dice.Count; ++i)
        {
            dice[i] = RandomThrow();
            GameInteraction.Instance.RollDice(i);
        }
    }

    D4 RandomThrow()
    {
        int r = UnityEngine.Random.Range(0, 4);
        return (D4)r;
    }

    void OrderDie()
    {
        dice.Sort();
    }

    bool CanReroll()
    {
        bool hasActivePiece = false;
        foreach (Place place in places)
        {
            foreach (Piece piece in place.pieces)
            {
                if(piece.owner == GetCurrentPlayer() && piece.isActive)
                {
                    hasActivePiece = true;
                }
            }
        }

        return GetCurrentDice() == D4.Sahhku && hasActivePiece;
    }

    D4 GetCurrentDice()
    {
        return dice[currentActiveDie];
    }

    bool CheckAllowedPieceMovement()
    {
        bool anyAllowedPlaces = false;
        foreach (Place place in places)
        {
            foreach (Piece piece in place.pieces)
            {
                if (piece.owner == GetCurrentPlayer())
                {
                    piece.allowedPlaces = GetAllowedPlaces(piece);

                    if (piece.IsSelectable())
                    {
                        anyAllowedPlaces = true;
                    }
                }
                else
                {
                    piece.allowedPlaces.Clear();
                }
            }
        }
        Debug.Log("Allowed places: " + anyAllowedPlaces);
        GameInteraction.Instance.UpdatePieces();
        return anyAllowedPlaces;
    }

    void ClearAllAllowedPlaces()
    {
        foreach (Place place in places)
        {
            foreach (Piece piece in place.pieces)
            {
                piece.allowedPlaces.Clear();
            }
        }
        GameInteraction.Instance.UpdatePieces();
    }

    List<int> GetAllowedPlaces(Piece p)
    {
        List<int> allowedPlaces = new List<int>();

        int direction = GetCurrentPlayer() == PieceOwner.P1 ? 1 : -1;
        int movementAmount = 0;
        if(GetCurrentDice() == D4.Sahhku)
        {
            movementAmount = 1;
        }
        else if(GetCurrentDice() == D4.Three)
        {
            movementAmount = 3;
        }
        else if (GetCurrentDice() == D4.Two)
        {
            movementAmount = 2;
        }

        if (movementAmount == 0)
        {
            return allowedPlaces;
        }

        direction *= movementAmount;

        if(p.isActive || (p.canBeActivated && GetCurrentDice() == D4.Sahhku))
        {
            
            Debug.Log("Found potential piece " + p.type + "(" + p.placeIndex + ")");

            // All pieces can try to move forward
            int indexTarget = p.placeIndex + direction;
            //Debug.Log(p.placeIndex + "+" + direction);
            //Debug.Log(indexTarget + "<" + places.Count + "&&" + indexTarget + ">=0"); 
            if (indexTarget < places.Count && indexTarget >= 0)
            {
                TryAddAllowedPlace(indexTarget, ref allowedPlaces);
            }

            // Queens and kings can move to any direction
            if(p.type != PieceType.Soldier)
            {
                direction = -direction;
                indexTarget = p.placeIndex + direction;
                TryAddAllowedPlace(indexTarget, ref allowedPlaces);

                indexTarget = GetPlaceIndexFromCoordinates(places[p.placeIndex].x, places[p.placeIndex].y + movementAmount);
                TryAddAllowedPlace(indexTarget, ref allowedPlaces);

                indexTarget = GetPlaceIndexFromCoordinates(places[p.placeIndex].x, places[p.placeIndex].y - movementAmount);
                TryAddAllowedPlace(indexTarget, ref allowedPlaces);
            }
        }

        return allowedPlaces;
    }

    void TryAddAllowedPlace(int indexTarget, ref List<int> allowedPlaces)
    {
        if(indexTarget < 0 || indexTarget > places.Count - 1)
            return;

        if (places[indexTarget].pieces.Count == 0)
        {
            Debug.Log("Allowed to move to an empty place " + indexTarget);
            allowedPlaces.Add(indexTarget);
        }
        else if (places[indexTarget].pieces[0].isActive
            && !(places[indexTarget].pieces[0].type == PieceType.Queen && places[indexTarget].pieces[0].owner == GetCurrentPlayer()))
        {
            Debug.Log("Allowed to move to an occupied place " + indexTarget);
            allowedPlaces.Add(indexTarget);
        }
    }

    public List<Piece> GetAllPotentialPieces()
    {
        List<Piece> potentialPieces = new List<Piece>();

        foreach (Place place in places)
        {
            foreach (Piece piece in place.pieces)
            {
                if (piece.IsSelectable())
                {
                    potentialPieces.Add(piece);
                }
            }
        }

        return potentialPieces;
    }

    // Returns -1 when unsuccessful
    int GetPlaceIndexFromCoordinates(int x, int y)
    {
        for (int i = 0; i < places.Count; ++i)
        {
            if (places[i].x == x && places[i].y == y)
            {
                return i;
            }
        }
        return -1;
    }

    void MovePiece(Piece piece, int moveIndex = 0)
    {
        if(piece.allowedPlaces.Count == 0)
        {
            Debug.LogWarning("Trying to move piece without valid places!", gameObject);
            return;
        }

        moveIndex = Mathf.Clamp(moveIndex, 0, piece.allowedPlaces.Count - 1);
        int placeIndex = piece.allowedPlaces[moveIndex];

        Debug.Log("Move piece (" + piece.type + ") to place: " + places[placeIndex].x + ", " + places[placeIndex].y + " (" + placeIndex + ")");

        List<int> pieceIndiciesToRemove = new List<int>();
        for(int i = 0; i < places[placeIndex].pieces.Count; ++i)
        {
            if (places[placeIndex].pieces[i].owner == GetCurrentPlayer())
            {

            }
            else
            { 
                if (places[placeIndex].pieces[i].type == PieceType.Soldier)
                {
                    Debug.Log("Captered a piece!");
                    pieceIndiciesToRemove.Add(i);

                    if (GetCurrentPlayer() == PieceOwner.P1)
                    {
                        p1captures++;
                        Debug.Log("P1 captures: " + p1captures);
                    }
                    else
                    {
                        p2captures++;
                        Debug.Log("P2 captures: " + p2captures);
                    }

                    if (p1captures == BOARD_SIZE_X)
                    {
                        Debug.Log("Game over!");
                        Debug.Log("P1 WON!");
                    }
                    else if (p2captures == BOARD_SIZE_X)
                    {
                        Debug.Log("Game over!");
                        Debug.Log("P2 WON!");
                    }
                }
                else if (places[placeIndex].pieces[i].type == PieceType.King)
                {
                    Debug.Log("Captered the king!");
                    places[placeIndex].pieces[i].owner = GetCurrentPlayer();
                }
                else if (places[placeIndex].pieces[i].type == PieceType.Queen)
                {
                    Debug.Log("Captered the queen!");
                    //pieceIndiciesToRemove.Add(i);
                    gameOver = true;

                    if (GetCurrentPlayer() == PieceOwner.P1)
                    {
                        Debug.Log("Game over!");
                        Debug.Log("P1 WON!");
                    }
                    else
                    {
                        Debug.Log("Game over!");
                        Debug.Log("P2 WON!");
                    }
                }
            }
        }

        // Remove all captured pieces
        int offset = 0;
        for (int i = pieceIndiciesToRemove.Count - 1; i >= 0; --i)
        {
            places[placeIndex].pieces.RemoveAt(pieceIndiciesToRemove[i + offset]);
            offset--;
        }

        // Activate next soldier, if possible
        if(!piece.isActive && piece.type == PieceType.Soldier)
        {
            int nextSoldierIndex = piece.placeIndex + (GetCurrentPlayer() == PieceOwner.P1 ? -1 : 1);

            Debug.Log(nextSoldierIndex);
            if (nextSoldierIndex >= 0 && nextSoldierIndex < places.Count)
            {
                Debug.Log("Allow activation of next soldier");
                places[nextSoldierIndex].pieces[0].canBeActivated = true;
            }
        }

        places[piece.placeIndex].pieces.Remove(piece);
        piece.isActive = true;
        piece.placeIndex = placeIndex;
        places[placeIndex].pieces.Add(piece);

        // If king is not active and soldier moves to enemy territory, activate and capture the king
        if (piece.type == PieceType.Soldier)
        {
            if (GetCurrentPlayer() == PieceOwner.P1)
            {
                if (placeIndex >= places.Count - BOARD_SIZE_X)
                {
                    ActivateKing();
                }
            }
            else
            {
                if (placeIndex < BOARD_SIZE_X)
                {
                    ActivateKing();
                }
            }
        }

        GameInteraction.Instance.UpdatePieces();
    }

    void ActivateKing()
    {
        foreach (Place place in places)
        {
            foreach (Piece p in place.pieces)
            {
                if (p.type == PieceType.King && !p.isActive)
                {
                    Debug.Log("King activated! " + GetCurrentPlayer());
                    places[p.placeIndex].pieces[0].isActive = true;
                    places[p.placeIndex].pieces[0].owner = GetCurrentPlayer();
                }
            }
        }
    }
}
