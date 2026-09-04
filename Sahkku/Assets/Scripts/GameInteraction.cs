using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInteraction : MonoBehaviour
{
    public static GameInteraction Instance;

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
        foreach (GameLogic.Place p in GameLogic.Instance.places)
        {
            places.Add(Instantiate(placePrefab, GetScaledBoardPosition(p.x, p.y), Quaternion.identity));
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
            // TODO: Open menu
        }

        if(Keyboard.current.dKey.wasPressedThisFrame)   
        {
            
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
                        p1soldierIndex++;
                    }
                    else
                    {
                        p2Soldiers[p2soldierIndex].transform.GetComponent<MeshRenderer>().material = piece.IsSelectable() ? selectablePieceMaterial : pieceMaterial;
                        p2Soldiers[p2soldierIndex].transform.position = GetScaledBoardPosition(place.x, place.y);
                        p2Soldiers[p2soldierIndex].SetActive(true);
                        p2soldierIndex++;
                    }
                }
                else if (piece.type == GameLogic.PieceType.Queen)
                {
                    if (piece.owner == GameLogic.PieceOwner.P1)
                    {
                        p1queen.transform.GetComponent<MeshRenderer>().material = piece.IsSelectable() ? selectablePieceMaterial : pieceMaterial;
                        p1queen.transform.position = GetScaledBoardPosition(place.x, place.y);
                        p1queen.SetActive(true);
                    }
                    else
                    {
                        p2queen.transform.GetComponent<MeshRenderer>().material = piece.IsSelectable() ? selectablePieceMaterial : pieceMaterial;
                        p2queen.transform.position = GetScaledBoardPosition(place.x, place.y);
                        p2queen.SetActive(true);
                    }
                }
                else if (piece.type == GameLogic.PieceType.King)
                {
                    king.transform.GetComponent<MeshRenderer>().material = piece.IsSelectable() ? selectablePieceMaterial : pieceMaterial;
                    king.transform.position = GetScaledBoardPosition(place.x, place.y);
                    king.SetActive(true);
                }
            }
        }

        List<GameLogic.Piece> potentialPieces = GameLogic.Instance.GetAllPotentialPieces();
        foreach (GameLogic.Piece piece in potentialPieces)
        {
            int i = 0;
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
