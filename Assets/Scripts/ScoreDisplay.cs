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

    public static float score = 0;

    private void Update()
    {
        scoreDisplayText.text = "Score: " + score;
    }

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

    public void UpdateScoreTextHandler(List<FloatReference> relevantVariables)
    {
        HelperFunctions.Log("New score incomming value of: " + relevantVariables[0].Value);
        
        int scoreChange = (int)relevantVariables[0].Value;
        
        score += scoreChange;

        scoreDisplayText.text = "Score: " + score;
    }

}
