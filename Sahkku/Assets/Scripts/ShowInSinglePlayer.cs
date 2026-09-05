using UnityEngine;

public class ShowInSinglePlayer : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(GameSettings.singlePlayer);
    }
}
