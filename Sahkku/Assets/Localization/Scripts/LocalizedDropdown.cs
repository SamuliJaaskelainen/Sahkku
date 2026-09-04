using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using static GameSettings;

[Serializable]
public struct PieceModelOption
{
    public PieceModel model;
    public LocalizedString localizedName;
}

public class LocalizedDropdown : MonoBehaviour
{
    public enum Target
    {
        Player1,
        Player2,
        King
    }

    public TMP_Dropdown dropdown;
    public List<PieceModelOption> options;

    [Tooltip("Which static setting this dropdown controls")]
    public Target target;

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        RefreshOptions();
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        RefreshOptions();
    }

    private void RefreshOptions()
    {
        List<string> localizedOptions = new List<string>();
        foreach (var option in options)
        {
            localizedOptions.Add(option.localizedName.GetLocalizedString());
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(localizedOptions);

        // Sync dropdown selection to whatever the current setting is
        int currentIndex = options.FindIndex(o => o.model == GetCurrentModel());
        dropdown.value = currentIndex >= 0 ? currentIndex : 0;

        dropdown.RefreshShownValue();
    }

    private void OnDropdownValueChanged(int index)
    {
        if (index >= 0 && index < options.Count)
        {
            SetCurrentModel(options[index].model);
        }
    }

    private PieceModel GetCurrentModel()
    {
        switch (target)
        {
            case Target.Player1: return p1Model;
            case Target.Player2: return p2Model;
            case Target.King: return kingModel;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private void SetCurrentModel(PieceModel model)
    {
        switch (target)
        {
            case Target.Player1: p1Model = model; break;
            case Target.Player2: p2Model = model; break;
            case Target.King: kingModel = model; break;
        }
    }
}