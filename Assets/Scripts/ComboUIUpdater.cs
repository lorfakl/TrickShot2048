using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ComboUIUpdater : MonoBehaviour
{
    [SerializeField]
    TMP_Text comboLevelText;
    [SerializeField]
    int indexOfThisComboPrefab;
    [SerializeField]
    FloatReference scoreMultiplier;
    [SerializeField]
    FloatReference columnIndex;

    public UnityEvent<List<FloatReference>> ApplyScoreMultiplierEvent;

    public float ComboMultiplier
    {
        get { return comboLevel; }
    }

    float comboLevel = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreaseComboLevel(List<FloatReference> comboLevelIndex)
    {
        if(comboLevelIndex[0].Value == (float)indexOfThisComboPrefab)
        {
            comboLevel++;
            if(comboLevel >= 2)
            {
                comboLevelText.text = "Combo: " + comboLevel + "x";
                if (comboLevelText.enabled)
                {
                    return;
                }
                else
                {
                    comboLevelText.enabled = true;
                }
            }
            
        }
        else
        {
            return;
        }
        
    }

    public void RemoveComboLevel(List<FloatReference> comboLevelIndex)
    {
        if (comboLevelIndex[0].Value == (float)indexOfThisComboPrefab)
        {
            Utilities.HelperFunctions.Log("Combo Removed");
            scoreMultiplier.Variable.Value = comboLevel;
            columnIndex.Variable.Value = (float)indexOfThisComboPrefab;
            comboLevel = 1;
            if (comboLevelText.enabled)
            {
                comboLevelText.enabled = false;
            }
            ApplyScoreMultiplierEvent.Invoke(new List<FloatReference> { scoreMultiplier, columnIndex });
        }
        else
        {
            return;
        }
    }
}
