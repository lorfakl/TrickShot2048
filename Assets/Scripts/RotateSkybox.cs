using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSkybox : MonoBehaviour
{
    #region Private Fields
    #endregion

    #region Public Fields
    public float rotateSpeed = 5f;
    public static AudioSource mergeSound;
    #endregion

    #region C# Events
    #endregion

    #region Public Methods
    public static void PlayMergeEventHandler()
    {
        if(mergeSound != null)
        {
            mergeSound.Play();
        }       
    }

    #endregion

    #region Unity Methods
    void Start()
  {
        mergeSound = gameObject.GetComponent<AudioSource>();
  }

  void Update()
  {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotateSpeed);
  }
  #endregion

  #region Private Methods
  #endregion
}
