using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    #region Private Fields

    #endregion

    #region Public Fields
  #endregion

    #region C# Events
  #endregion

    #region Public Methods
    public void ReloadScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
    #endregion

    #region Unity Methods
  void Start()
  {
    
  }

  void Update()
  {
    
  }
  #endregion

    #region Private Methods
  #endregion
}
