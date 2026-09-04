using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameInteraction : MonoBehaviour
{
    public static GameInteraction Instance;

    [SerializeField] Camera camera;
    [SerializeField] GameObject placePrefab;
    [SerializeField] GameObject kingPrefab;
    [SerializeField] GameObject p1soldierPrefab;
    [SerializeField] GameObject p1queenPrefab;
    [SerializeField] GameObject p2soldierPrefab;
    [SerializeField] GameObject p2queenPrefab;
    [SerializeField] GameObject dicePrefab;
    [SerializeField] GameObject diePos1;
    [SerializeField] GameObject diePos2;
    [SerializeField] GameObject diePos3;
    [SerializeField] Material pieceMaterial;
    [SerializeField] Material selectablePieceMaterial;

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
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene("MainMenu");
        }

        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if(Physics.Raycast(ray, out hit, 1000.0f))
            {
                Debug.Log("Press: " + hit.transform.name, hit.transform.gameObject);

                PieceData data = hit.transform.GetComponent<PieceData>();

                if (data != null)
                {
                    selectedPiece = data;
                    for (int i = 0; i < places.Count; ++i)
                    {
                        bool isValidPlace = data.pieceInfo.allowedPlaces.Contains(i);
                        places[i].transform.GetChild(0).gameObject.SetActive(isValidPlace);
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

    public void UpdatePieces()
    {
        HideAllModels();

        int p1soldierIndex = 0;
        int p2soldierIndex = 0;

        foreach (GameLogic.Place place in GameLogic.Instance.places)
        {
            foreach (GameLogic.Piece piece in place.pieces)
            {
                if (piece.type == GameLogic.PieceType.Soldier)
                {
                    if (piece.owner == GameLogic.PieceOwner.P1)
                    {
                        p1Soldiers[p1soldierIndex].transform.GetComponent<MeshRenderer>().material = piece.IsSelectable() ? selectablePieceMaterial : pieceMaterial;
                        p1Soldiers[p1soldierIndex].transform.position = GetScaledBoardPosition(place.x, place.y);
                        p1Soldiers[p1soldierIndex].SetActive(true);
                        p1Soldiers[p1soldierIndex].name = piece.placeIndex.ToString();
                        p1Soldiers[p1soldierIndex].GetComponent<PieceData>().pieceInfo = piece;
                        p1soldierIndex++;
                    }
                    else
                    {
                        p2Soldiers[p2soldierIndex].transform.GetComponent<MeshRenderer>().material = piece.IsSelectable() ? selectablePieceMaterial : pieceMaterial;
                        p2Soldiers[p2soldierIndex].transform.position = GetScaledBoardPosition(place.x, place.y);
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
                        p1queen.transform.position = GetScaledBoardPosition(place.x, place.y);
                        p1queen.name = piece.placeIndex.ToString();
                        p1queen.GetComponent<PieceData>().pieceInfo = piece;
                        p1queen.SetActive(true);
                    }
                    else
                    {
                        p2queen.transform.GetComponent<MeshRenderer>().material = piece.IsSelectable() ? selectablePieceMaterial : pieceMaterial;
                        p2queen.transform.position = GetScaledBoardPosition(place.x, place.y);
                        p2queen.name = piece.placeIndex.ToString();
                        p2queen.GetComponent<PieceData>().pieceInfo = piece;
                        p2queen.SetActive(true);
                    }
                }
                else if (piece.type == GameLogic.PieceType.King)
                {
                    king.transform.GetComponent<MeshRenderer>().material = piece.IsSelectable() ? selectablePieceMaterial : pieceMaterial;
                    king.transform.position = GetScaledBoardPosition(place.x, place.y);
                    king.name = piece.placeIndex.ToString();
                    king.GetComponent<PieceData>().pieceInfo = piece;
                    king.SetActive(true);
                }
            }
        }

        List<GameLogic.Piece> potentialPieces = GameLogic.Instance.GetAllPotentialPieces();
        foreach (GameLogic.Piece piece in potentialPieces)
        {
            int i = 0;
            // TODO: Show potential places from selected piece
            //places[piece.allowedPlaces[i]].transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    void HideAllModels()
    {
        foreach (GameObject go in places)
        {
            go.transform.GetChild(0).gameObject.SetActive(false);
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
