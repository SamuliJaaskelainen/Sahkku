using UnityEngine;

public class HideInBrowser : MonoBehaviour
{
    void OnEnable()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            gameObject.SetActive(false);
        }
    }
}
