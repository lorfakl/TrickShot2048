using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class AngleControl : MonoBehaviour
{
    public Slider angleSlider;

    // Start is called before the first frame update
    void Start()
    {
        angleSlider.value = 45;
        angleSlider.onValueChanged.AddListener(CubeController.UpdateVectorRotationAngle);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
