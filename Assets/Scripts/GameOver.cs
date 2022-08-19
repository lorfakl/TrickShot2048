using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    #region Private Fields
    [SerializeField]
    GameObject gameOverPanel;

    [SerializeField]
    GameObject settingsButton;

    [SerializeField]
    GameObject respwnBtn;

    [SerializeField]
    Button startOverBtn;

    Collider m_Collider;
    Vector3 m_Center;
    Vector3 m_Size, m_Min, m_Max;

    float totalTimeEntered = 0;
    #endregion

    #region Public Fields
    #endregion

    #region C# Events
    #endregion

    #region Public Methods

    #endregion

    #region Unity Methods
    void Start()
    {
        print("Game Over Start method");
        startOverBtn.onClick.AddListener(ReloadScene);

        //Fetch the Collider from the GameObject
        m_Collider = GetComponent<Collider>();
        //Fetch the center of the Collider volume
        m_Center = m_Collider.bounds.center;
        //Fetch the size of the Collider volume
        m_Size = m_Collider.bounds.size;
        //Fetch the minimum and maximum bounds of the Collider volume
        m_Min = m_Collider.bounds.min;
        m_Max = m_Collider.bounds.max;
        //Output this data into the console
        OutputData();
    }

    void Update()
    {
    
    }

    private void OnTriggerExit(Collider other)
    {
        totalTimeEntered = 0;
    }

    private void OnTriggerStay(Collider other)
    {
        CubeController cubeInTrigger = other.GetComponent<CubeController>();
        if(cubeInTrigger == null)
        {
            return;
        }
        else
        {
            print("Something is in the triggger");
            totalTimeEntered += Time.deltaTime;
            if(totalTimeEntered >= 1f)
            {
                if (cubeInTrigger.enabled || cubeInTrigger.IsLaunched)
                {
                    gameOverPanel.SetActive(true);
                    settingsButton.SetActive(false);
                    respwnBtn.SetActive(false);

                    totalTimeEntered = 0;
                }
            }
        }
    }
    #endregion

    #region Private Methods
    private void ReloadScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    void OutputData()
    {
        //Output to the console the center and size of the Collider volume
        Debug.Log("Collider Center : " + m_Center);
        Debug.Log("Collider Size : " + m_Size);
        Debug.Log("Collider bound Minimum : " + m_Min);
        Debug.Log("Collider bound Maximum : " + m_Max);
    }
    #endregion

    

    

    
}
