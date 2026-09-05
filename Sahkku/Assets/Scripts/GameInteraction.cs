using System.Collections.Generic;
using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static GameLogic;

public class GameInteraction : MonoBehaviour
{
    public static GameInteraction Instance;

    [SerializeField] Camera camera;
    [SerializeField] LayerMask p1mask;
    [SerializeField] LayerMask p2mask;
    [SerializeField] GameObject placePrefab;
    [SerializeField] GameObject kingPrefab;
    [SerializeField] GameObject p1soldierPrefab;
    [SerializeField] GameObject p1queenPrefab;
    [SerializeField] GameObject p2soldierPrefab;
    [SerializeField] GameObject p2queenPrefab;
    [SerializeField] GameObject dicePrefab;
    [SerializeField] GameObject p1capturesPos;
    [SerializeField] GameObject p2capturesPos;
    [SerializeField] GameObject diePos1;
    [SerializeField] GameObject diePos2;
    [SerializeField] GameObject diePos3;
    [SerializeField] Material pieceMaterial;
    [SerializeField] Material selectablePieceMaterial;
    [SerializeField] TextMeshProUGUI gameStatus;
    [SerializeField] GameObject rollDiceButton;
    [SerializeField] GameObject dieHighlight1;
    [SerializeField] GameObject dieHighlight2;
    [SerializeField] GameObject dieHighlight3;

    List<GameObject> places = new List<GameObject>();
    List<GameObject> p1Soldiers = new List<GameObject>();
    List<GameObject> p2Soldiers = new List<GameObject>();
    GameObject p1queen;
    GameObject p2queen;
    GameObject king;
    List<GameObject> dice = new List<GameObject>();
    PieceData selectedPiece;

    public Vector2 boardScalar = new Vector2(1.0f, 1.0f);

    Vector3 GetScaledBoardPosition(int x, int y)
    {
        return new Vector3(x * boardScalar.x, 0.0f, y * boardScalar.y);
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    { 
        int y = 0;
        for (int x = 0; x < GameLogic.BOARD_SIZE_X; ++x)
        {
            GameObject newPlace = Instantiate(placePrefab, GetScaledBoardPosition(x, y), Quaternion.identity);
            newPlace.name = places.Count.ToString();
            places.Add(newPlace);
        }

        y = 1;
        for (int x = GameLogic.BOARD_SIZE_X - 1; x >= 0; --x)
        {
            GameObject newPlace = Instantiate(placePrefab, GetScaledBoardPosition(x, y), Quaternion.identity);
            newPlace.name = places.Count.ToString();
            places.Add(newPlace);
        }

        y = 2;
        for (int x = 0; x < GameLogic.BOARD_SIZE_X; ++x)
        {
            GameObject newPlace = Instantiate(placePrefab, GetScaledBoardPosition(x, y), Quaternion.identity);
            newPlace.name = places.Count.ToString();
            places.Add(newPlace);
        }

        for (int i = 0; i < GameLogic.BOARD_SIZE_X; ++i)
        {
            p1Soldiers.Add(Instantiate(p1soldierPrefab));
            p2Soldiers.Add(Instantiate(p2soldierPrefab));
        }

        p1queen = Instantiate(p1queenPrefab);
        p2queen = Instantiate(p2queenPrefab);
        king = Instantiate(kingPrefab);

        HideAllModels();

        dice.Add(Instantiate(dicePrefab, diePos1.transform.position, Quaternion.identity));
        dice.Add(Instantiate(dicePrefab, diePos2.transform.position, Quaternion.identity));
        dice.Add(Instantiate(dicePrefab, diePos3.transform.position, Quaternion.identity));

        UpdatePieces();
    }

    void Update()
    {
        if(GameLogic.Instance.gameOver)
        {
            if(GameLogic.Instance.winner == GameSettings.Player.One)
            {
                if(GameLogic.Instance.p1captures == GameLogic.BOARD_SIZE_X)
                {
                    gameStatus.text = "Player One WINS! All soldiers captured.";
                }
                else
                {
                    gameStatus.text = "Player One WINS! Queen captured.";
                }
            }
            else
            {
                if (GameLogic.Instance.p2captures == GameLogic.BOARD_SIZE_X)
                {
                    gameStatus.text = "Player One WINS! All soldiers captured.";
                }
                else
                {
                    gameStatus.text = "Player One WINS! Queen captured.";
                }
            }
        }
        else
        {
            switch (GameLogic.Instance.turnPhase)
            {
                case GameLogic.TurnPhase.P1roll:
                    gameStatus.text = "Player One Turn: Roll the dice";
                    break;

                case GameLogic.TurnPhase.P1move:
                    gameStatus.text = "Player One Turn: Move pieces";
                    break;

                case GameLogic.TurnPhase.P2roll:
                    gameStatus.text = "Player Two Turn: Roll the dice";
                    break;

                case GameLogic.TurnPhase.P2move:
                    gameStatus.text = "Player Two Turn: Move pieces";
                    break;
            }
        }

        if ((GameLogic.Instance.turnPhase == GameLogic.TurnPhase.P1move || GameLogic.Instance.turnPhase == GameLogic.TurnPhase.P2move) && !GameLogic.Instance.gameOver)
        {
            dieHighlight1.SetActive(GameLogic.Instance.currentActiveDie == 0);
            dieHighlight2.SetActive(GameLogic.Instance.currentActiveDie == 1);
            dieHighlight3.SetActive(GameLogic.Instance.currentActiveDie == 2);
        }
        else
        {
            dieHighlight1.SetActive(false);
            dieHighlight2.SetActive(false);
            dieHighlight3.SetActive(false);
        }

        rollDiceButton.SetActive(false);
        if (GameLogic.Instance.turnPhase == GameLogic.TurnPhase.P1roll || GameLogic.Instance.turnPhase == GameLogic.TurnPhase.P2roll || GameLogic.Instance.CanReroll())
        {
            if (!(GameSettings.singlePlayer && GameLogic.Instance.GetCurrentPlayer() == GameLogic.PieceOwner.P2))
            {
                if (!GameLogic.Instance.gameOver)
                {
                    rollDiceButton.SetActive(true);
                }
            }
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            BackToMainMenu();
        }

        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if(Physics.Raycast(ray, out hit, 1000.0f, GameLogic.Instance.GetCurrentPlayer() == GameLogic.PieceOwner.P1 ? p1mask : p2mask))
            {
                Debug.Log("Press: " + hit.transform.name, hit.transform.gameObject);

                PieceData data = hit.transform.GetComponent<PieceData>();

                if (data != null)
                {
                    selectedPiece = data;
                    for (int i = 0; i < places.Count; ++i)
                    {
                        bool isValidPlace = data.pieceInfo.allowedPlaces.Contains(i);
                        places[i].SetActive(isValidPlace);
                    }
                }
                else if(hit.transform.tag == "Place")
                {
                    if (selectedPiece != null)
                    {
                        GameLogic.Instance.MovePiece(selectedPiece.pieceInfo, int.Parse(hit.transform.name));
                        selectedPiece = null;
                    }
                }
            }
        }
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void UpdatePieces()
    {
        HideAllModels();

        int p1soldierIndex = 0;
        int p2soldierIndex = 0;

        foreach (GameLogic.Place place in GameLogic.Instance.places)
        {
            int pieceCountInPlace = place.pieces.Count;
            float centerOffset = (pieceCountInPlace - 1) * 0.5f;
            int currentPiece = 0;
            foreach (GameLogic.Piece piece in place.pieces)
            {
                Vector3 offset = Vector3.back * (currentPiece - centerOffset);
                if (piece.type == GameLogic.PieceType.Soldier)
                {
                    if (piece.owner == GameLogic.PieceOwner.P1)
                    {
                        p1Soldiers[p1soldierIndex].transform.GetComponent<MeshRenderer>().material = piece.IsSelectable() ? selectablePieceMaterial : pieceMaterial;
                        p1Soldiers[p1soldierIndex].transform.position = GetScaledBoardPosition(place.x, place.y) + offset;
                        p1Soldiers[p1soldierIndex].SetActive(true);
                        p1Soldiers[p1soldierIndex].name = piece.placeIndex.ToString();
                        p1Soldiers[p1soldierIndex].GetComponent<PieceData>().pieceInfo = piece;
                        p1soldierIndex++;
                    }
                    else
                    {
                        p2Soldiers[p2soldierIndex].transform.GetComponent<MeshRenderer>().material = piece.IsSelectable() ? selectablePieceMaterial : pieceMaterial;
                        p2Soldiers[p2soldierIndex].transform.position = GetScaledBoardPosition(place.x, place.y) + offset;
                        p2Soldiers[p2soldierIndex].SetActive(true);
                        p2Soldiers[p2soldierIndex].name = piece.placeIndex.ToString();
                        p2Soldiers[p2soldierIndex].GetComponent<PieceData>().pieceInfo = piece;
                        p2soldierIndex++;
                    }
                }
                else if (piece.type == GameLogic.PieceType.Queen)
                {
                    if (piece.owner == GameLogic.PieceOwner.P1)
                    {
                        p1queen.transform.GetComponent<MeshRenderer>().material = piece.IsSelectable() ? selectablePieceMaterial : pieceMaterial;
                        p1queen.transform.position = GetScaledBoardPosition(place.x, place.y) + offset;
                        p1queen.name = piece.placeIndex.ToString();
                        p1queen.GetComponent<PieceData>().pieceInfo = piece;
                        p1queen.SetActive(true);
                    }
                    else
                    {
                        p2queen.transform.GetComponent<MeshRenderer>().material = piece.IsSelectable() ? selectablePieceMaterial : pieceMaterial;
                        p2queen.transform.position = GetScaledBoardPosition(place.x, place.y) + offset;
                        p2queen.name = piece.placeIndex.ToString();
                        p2queen.GetComponent<PieceData>().pieceInfo = piece;
                        p2queen.SetActive(true);
                    }
                }
                else if (piece.type == GameLogic.PieceType.King)
                {
                    king.transform.GetComponent<MeshRenderer>().material = piece.IsSelectable() ? selectablePieceMaterial : pieceMaterial;
                    king.transform.position = GetScaledBoardPosition(place.x, place.y) + offset;
                    king.name = piece.placeIndex.ToString();
                    king.GetComponent<PieceData>().pieceInfo = piece;
                    king.SetActive(true);
                    king.layer = LayerMask.NameToLayer(piece.owner == GameLogic.PieceOwner.P1 ? "P1" : "P2");
                }
                currentPiece++;
            }
        }

        for(int i = 0; i < GameLogic.Instance.p2captures; ++i)
        {
            if (p1soldierIndex < p1Soldiers.Count)
            {
                p1Soldiers[p1soldierIndex].transform.GetComponent<MeshRenderer>().material = pieceMaterial;
                p1Soldiers[p1soldierIndex].transform.position = p2capturesPos.transform.position + Vector3.right * i;
                p1Soldiers[p1soldierIndex].SetActive(true);
                p1Soldiers[p1soldierIndex].name = "captured";
                p1soldierIndex++;
            }
        }

        for (int i = 0; i < GameLogic.Instance.p1captures; ++i)
        {
            if (p2soldierIndex < p2Soldiers.Count)
            {
                p2Soldiers[p2soldierIndex].transform.GetComponent<MeshRenderer>().material = pieceMaterial;
                p2Soldiers[p2soldierIndex].transform.position = p1capturesPos.transform.position - Vector3.right * i;
                p2Soldiers[p2soldierIndex].SetActive(true);
                p2Soldiers[p2soldierIndex].name = "captured";
                p2soldierIndex++;
            }
        }
    }

    void HideAllModels()
    {
        foreach (GameObject go in places)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in p1Soldiers) go.SetActive(false);
        foreach (GameObject go in p2Soldiers) go.SetActive(false);
        p1queen.SetActive(false);
        p2queen.SetActive(false);
        king.SetActive(false);
    }

    public void RollDice(int index)
    {
        dice[index].transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        dice[index].GetComponentInChildren<Animator>().StopPlayback();
        dice[index].GetComponentInChildren<Animator>().SetTrigger("Throw");
    }

    public void DisplayDiceResults()
    {
        for(int i = 0; i < GameLogic.Instance.dice.Count; ++i)
        {
            if(GameLogic.Instance.dice[i] == GameLogic.D4.Zero)
            {
                dice[i].transform.localEulerAngles = new Vector3(0.0f, 0.0f, 180.0f);
            }
            else if (GameLogic.Instance.dice[i] == GameLogic.D4.Two)
            {
                dice[i].transform.localEulerAngles = new Vector3(0.0f, 0.0f, 90.0f);
            }
            else if (GameLogic.Instance.dice[i] == GameLogic.D4.Three)
            {
                dice[i].transform.localEulerAngles = new Vector3(0.0f, 0.0f, 270.0f);
            }
            else
            {
                dice[i].transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }
    }
}
