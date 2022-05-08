using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField]
    FloatVariable currentScore;
    [SerializeField]
    TMP_Text scoreDisplayText;

    float score;
 


    /// <summary>
    /// Update Score Handler receives the UpdateScoreEvent whose data is formatted in the following way
    /// Index 0: Base score value
    /// Index 1: Any multiplier, if this block was on a column with an active combo, default let to one
    /// </summary>
    /// <param name="relevantVariables"></param>
    public void UpdateScoreHandler(List<FloatReference> relevantVariables)
    {
        
        score += (relevantVariables[0].Value * relevantVariables[1]);

        scoreDisplayText.text = "Score: " + score;
    }
}
