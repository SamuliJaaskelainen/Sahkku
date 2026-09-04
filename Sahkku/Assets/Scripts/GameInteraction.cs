using UnityEngine;
using UnityEngine.InputSystem;

public class GameInteraction : MonoBehaviour
{
    [SerializeField] GameObject kingPrefab;
    [SerializeField] GameObject p1soldierPrefab;
    [SerializeField] GameObject p1queenPrefab;
    [SerializeField] GameObject p2soldierPrefab;
    [SerializeField] GameObject p2queenPrefab;
    [SerializeField] GameObject dicePrefab;

    [SerializeField] GameObject d4;

    void Start()
    {
        
    }

    void Update()
    {
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // TODO: Open menu
        }

        if(Keyboard.current.dKey.wasPressedThisFrame)
        {
            d4.GetComponent<Animator>().SetTrigger("Throw");
        }
    }
}
