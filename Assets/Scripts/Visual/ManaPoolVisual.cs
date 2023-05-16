using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif
[ExecuteInEditMode]
public class ManaPoolVisual : MonoBehaviour
{
    public int TestFullCrystals;
    public int TestTotalCrystalsThisTurn;

    private int totalCrystals;
    public int TotalCrystals
    {
        get { return totalCrystals; }
        set
        {
            int clampedValue = Mathf.Clamp(value, 0, Crystals.Length);
            if (clampedValue != totalCrystals)
            {
                totalCrystals = clampedValue;
                UpdateCrystals();
            }
        }
    }

    private int availableCrystals;
    public int AvailableCrystals
    {
        get { return availableCrystals; }
        set
        {
            int clampedValue = Mathf.Clamp(value, 0, totalCrystals);
            if (clampedValue != availableCrystals)
            {
                availableCrystals = clampedValue;
                UpdateCrystals();
            }
        }
    }

    public Image[] Crystals;
    public Text ProgressText;

    private readonly string progressTextFormat = "{0}/{1}";

    void Update()
    {
#if UNITY_EDITOR
        if (Application.isEditor && !Application.isPlaying)
        {
            TotalCrystals = TestTotalCrystalsThisTurn;
            AvailableCrystals = TestFullCrystals;
        }
#endif
    }

    private void UpdateCrystals()
    {
        for (int i = 0; i < Crystals.Length; i++)
        {
            if (i < totalCrystals)
            {
                Crystals[i].color = i < availableCrystals ? Color.white : Color.gray;
            }
            else
            {
                Crystals[i].color = Color.clear;
            }
        }

        UpdateProgressText();
    }

    private void UpdateProgressText()
    {
        ProgressText.text = string.Format(progressTextFormat, availableCrystals, totalCrystals);
    }
}