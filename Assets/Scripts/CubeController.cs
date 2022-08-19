using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpecificGlobals;
using UnityEngine.Events;
using UnityEngine.UI;
using Utilities;

public class CubeController : MonoBehaviour
{
    public Rigidbody rb;
    public Camera cam;
    public float acceleration = 10;
    public static float dynamicMultiplierMax;
    public static float slideThreshold;

    public UnityEvent<bool> LaunchStatus = new UnityEvent<bool>();
    public UnityEvent<bool> ComboState = new UnityEvent<bool>();

    [SerializeField]
    private float slideStrength = 150;

    [SerializeField]
    private float dragStrength;
    
    [SerializeField]
    private float turnSensitivity;

    private LineRenderControler lineRenCtrl;

    private float multiplier;

    private Vector3 homePosition;
    private Vector3 initialVelocity;

    private Vector3 startingCubeScreenPosition;

    private bool isInitSet = false;
    private bool isLauched = false;
    private float timeToTurn = 0.75f;
    private float currentTurnTime = 0;
    private static float cubeLaunchAngle = 45;


    #region Experimental

    public Vector2 startPos;
    public Vector2 direction;

    public Text m_Text;
    string message;

    void Touch()
    {
        //Update the Text on the screen depending on current TouchPhase, and the current direction vector
        //m_Text.text = "Touch : " + "at position " + direction;

        // Track a single touch as a direction control.
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Handle finger movements based on TouchPhase
            switch (touch.phase)
            {
                //When a touch has first been detected, change the message and record the starting position
                case TouchPhase.Began:
                    // Record initial touch position.
                    startPos = touch.position;
                    message = "Begun ";
                    break;

                //Determine if the touch is a moving touch
                case TouchPhase.Moved:
                    // Determine direction by comparing the current touch position with the initial one
                    direction = touch.position;
                    //dragStrength = Vector2.Distance(startPos, direction);
                    //UpdateLaunchRotationAngle(touch.position.y);
                    message = "Moving ";
                    break;

                case TouchPhase.Ended:
                    // Report that the touch has ended when it ends
                    message = "Ending ";
                    break;
            }

            //print(m_Text.text);
            
        }
    }

    #endregion

    public bool IsLaunched
    {
        get { return isLauched; }
    }

    public Vector3 InitialVelocity
    {
        get { return initialVelocity; }
    }

    public Vector3 InitialPosition
    {
        get { return homePosition; }
    }

    public Vector3 CurrentPosition
    {
        get { return gameObject.transform.position; }
    }

    #region Unity Callbacks

    void Awake()
    {
        dragStrength = 0;
        homePosition = gameObject.transform.position;
        lineRenCtrl = gameObject.GetComponent<LineRenderControler>();
        cam = Camera.main;

        if (SystemInfo.deviceType != DeviceType.Handheld)
        {
            dynamicMultiplierMax = 25;
        }
        else
        {
            dynamicMultiplierMax = 5;
        }
        slideThreshold = 15;
    }

    void Start()
    {
        startingCubeScreenPosition = cam.WorldToScreenPoint(gameObject.transform.position);
        if (!this.gameObject.name.Contains("Clone"))
        {
            //GetComponent<Renderer>().material.color = Color.red;
        }
        CubeManager.HomePosition = gameObject.transform.position;
        //respwn.onClick.AddListener(RespawnCube);
    }

    // Update is called once per frame
    void Update()
    {
        /*if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            Touch();
        }*/

        currentTurnTime += Time.fixedDeltaTime;
        if (currentTurnTime > timeToTurn)
        {
            currentTurnTime = 0;
        }
    }

    private void FixedUpdate()
    {
        if(isLauched && (dragStrength > slideThreshold))
        {
            rb.AddTorque(transform.right * dragStrength * Time.fixedDeltaTime);
            //this.enabled = false;
            //print("Velocity: " + rb.velocity); 
        }
      
        if(dragStrength > slideThreshold)
        {
            initialVelocity = HelperFunctions.RotateVector(transform.forward, cubeLaunchAngle).normalized * dragStrength * multiplier * Time.fixedDeltaTime;
        }
        else
        {
            initialVelocity = transform.forward * slideStrength * multiplier * Time.fixedDeltaTime;
        }
        
        //print("Fixed update Initial Velocity: " + InitialVelocity);
    }

    private void OnMouseDrag()
    {
        if (!isInitSet)
        {
            isInitSet = true;
            currentTurnTime = 0;
        }

        if (SystemInfo.deviceType != DeviceType.Handheld)
        {
            //float mouseDeltaY = initialPointerY - Input.mousePosition.y;

            dragStrength = GetDragStrength(Input.mousePosition.y);
            TranslateWithPointer(Input.mousePosition.x);
            UpdateLaunchRotationAngle(Input.mousePosition.y);
            multiplier = CalculateDynamicMultiplier(dragStrength);
            //HelperFunctions.Log("Mulitiplioer: " + multiplier);
        }
        else
        {
            //float pointerDeltaY = initialPointerY - Input.GetTouch(0).position.y;
            dragStrength = GetDragStrength(Input.GetTouch(0).position.y);

            TranslateWithPointer(Input.GetTouch(0).position.x);
            UpdateLaunchRotationAngle(Input.GetTouch(0).position.y);
            multiplier = CalculateDynamicMultiplier(Input.GetTouch(0).position.y);
        }
        //HelperFunctions.Log("Multiplier: " + multiplier);
        //HelperFunctions.Log("Final Drag Strength: " + dragStrength * multiplier);
        lineRenCtrl.CalculateProjectilePath();
    }

    private void OnCollisionEnter(Collision collision)
    {
        isLauched = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag != "ground")
        {
            
        }
            
    }

    private void OnMouseUp()
    {
        rb.constraints = RigidbodyConstraints.None;
        isLauched = true;
        LaunchStatus?.Invoke(isLauched);
        isInitSet = false;
        Vector3 direction = HelperFunctions.RotateVector(transform.forward, cubeLaunchAngle);

        //initialVelocity = direction.normalized * dragStrength * multiplier * Time.deltaTime;
        //print("OnMouseUp Initial Velocity: " + InitialVelocity);
        lineRenCtrl.SetLineRenVisibility(false);
        rb.AddForce(initialVelocity, ForceMode.VelocityChange);
        Debug.DrawLine(transform.position, direction * dragStrength * multiplier, Color.green, 2, false);
        gameObject.GetComponent<SimpleBlock>().HasBeenLaunched = true;
        
    }

    private void OnMouseDown()
    {
        if(isLauched)
        {
            //print("Cube Reset, starting combo");
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = homePosition;
            isLauched = false;
        }
    }

    #endregion Unity Callbacks


    

    private void TranslateWithPointer(float pointerX)
    {
        //print("Raw Pointer value: " + pointerX);
        float percentTurned = currentTurnTime / timeToTurn;
        //print("Percent Lerped: " + percentTurned);

        //float reversedPointerX = Mathf.Abs(pointerX - Screen.width); //use this for inversed controls

        float delta = HelperFunctions.Map(-7, 7, Screen.width * 0.2f,
            HelperFunctions.GetPercentageOf(20, Screen.width), pointerX);

        Vector3 newPosition = new Vector3(ClampedCubeRotation(delta), gameObject.transform.position.y,
            gameObject.transform.position.z);

        transform.position = Vector3.Lerp(newPosition, transform.position, percentTurned); //Quaternion.LookRotation(cam.ScreenToWorldPoint(Input.mousePosition));
    }

    private float ClampedCubeRotation(float value)
    {
        return Mathf.Clamp(value, -32f, 32f);
    }

    private void UpdateLaunchRotationAngle(float pointerPositionY)
    {
        //this is to show the position as a higher number the closer it is to the bottom of the screen
        float reversePointerPosition = Mathf.Abs(pointerPositionY - Screen.height);
        float reverseCubePositionY = Mathf.Abs(startingCubeScreenPosition.y - Screen.height);
        float endOfScreenY = Screen.height;

        //HelperFunctions.LogListContent(new List<string>{"Reserve Pointer Pos: " + reversePointerPosition,
            //"Cube Position: " + reverseCubePositionY, "End of screen: " + endOfScreenY});

        cubeLaunchAngle = HelperFunctions.Map(30, 69, reverseCubePositionY, endOfScreenY, reversePointerPosition);
        //HelperFunctions.Log("Launch Angle: " + cubeLaunchAngle);
    }

    private float CalculateDynamicMultiplier(float dragStr)
    {
        if (SystemInfo.deviceType != DeviceType.Handheld)
        {
            float reverseCubePositionY = Mathf.Abs(startingCubeScreenPosition.y - Screen.height);
            return HelperFunctions.Map(10, dynamicMultiplierMax, reverseCubePositionY, Screen.height, dragStr);
        }
        else
        {
            /*float reverseCubePositionY = Mathf.Abs(startingCubeScreenPosition.y - Screen.height);
            HelperFunctions.Log("Multiplier: " + HelperFunctions.Map(2, 5, 0, startingCubeScreenPosition.y, dragStr));
            return HelperFunctions.Map(2, dynamicMultiplierMax, reverseCubePositionY, Screen.height, dragStr);*/
            return dynamicMultiplierMax;
        }
    }

    private float GetDragStrength(float pointerPositionY)
    {
        float cubePositionY = startingCubeScreenPosition.y;
        //HelperFunctions.Log("Normal Position: " + pointerPositionY);

        float dragStrength = cubePositionY - pointerPositionY;
        if(dragStrength < 0)
        {
            dragStrength = 0;
        }

        HelperFunctions.Log("Drag Strength: " + dragStrength);
        
        if(SystemInfo.deviceType == DeviceType.Handheld)
        {
            return dragStrength;
        }

        return dragStrength;
    }

    public static void UpdateVectorRotationAngle(float angle)
    {
        cubeLaunchAngle = angle;
    }

    void DragValue(float mouseY)
    {

    }

}
