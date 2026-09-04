using UnityEngine;
using UnityEngine.UI;

public class AudioToggleVisual : MonoBehaviour
{
    public Toggle toggle;
    public GameObject iconOn;
    public GameObject iconOff;

    void Start()
    {
        UpdateVisual(toggle.isOn);
        toggle.onValueChanged.AddListener(UpdateVisual);
    }

    void UpdateVisual(bool isOn)
    {
        iconOn.SetActive(isOn);
        iconOff.SetActive(!isOn);
    }
}