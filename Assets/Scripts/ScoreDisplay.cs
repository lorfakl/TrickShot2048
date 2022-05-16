using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Utilities;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField]
    FloatVariable currentScore;
    [SerializeField]
    TMP_Text scoreDisplayText;
    [SerializeField]
    ComboUIUpdater[] comboUIUpdaters;

    float score;
    


    /// <summary>
    /// Update Score Handler receives the UpdateScoreEvent whose data is formatted in the following way
    /// Index 0: Base score value
    /// Index 1: Any multiplier, if this block was on a column with an active combo, default let to one
    /// </summary>
    /// <param name="relevantVariables"></param>
    public void UpdateScoreHandler(List<FloatReference> relevantVariables)
    {
        HelperFunctions.Log("New score incomming base value of: " + relevantVariables[1].Value
            + " at index " + relevantVariables[0].Value);
        float baseScore = relevantVariables[1].Value;
        int comboIndex = (int)relevantVariables[0].Value;
        float multiplier = comboUIUpdaters[comboIndex].ComboMultiplier;
        score += (baseScore * multiplier);

        scoreDisplayText.text = "Score: " + score;
    }
}
