using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utilities;

public class CubeController : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject cubePrefab;
    public Button respwn;
    public Camera cam;
    public float acceleration = 10;

    public UnityEvent<bool> LaunchStatus = new UnityEvent<bool>();
    public UnityEvent<bool> ComboState = new UnityEvent<bool>();

    [SerializeField]
    private float dragStrength;
    [SerializeField]
    private float multiplier;
    [SerializeField]
    private float turnSensitivity;

    private LineRenderControler lineRenCtrl;

    private Vector3 homePosition;
    private Vector3 initialVelocity;

    private Vector3 startingCubeScreenPosition;

    private bool isInitSet = false;
    private bool isLauched = false;
    private float initialPointerY;
    private float prevPointerX;
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
        m_Text.text = "Touch : " + "at position " + direction;

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
                    prevPointerX = touch.position.x;
                    break;
            }

            //print(m_Text.text);
            
        }
    }

    #endregion


    public Vector3 InitialVelocity
    {
        get { return initialVelocity; }
    }

    public Vector3 InitialPosition
    {
        get { return homePosition; }
    }

    #region Unity Callbacks

    void Awake()
    {
        dragStrength = 0;
        homePosition = gameObject.transform.position;
        lineRenCtrl = gameObject.GetComponent<LineRenderControler>();

        
    }

    void Start()
    {
        startingCubeScreenPosition = cam.WorldToScreenPoint(gameObject.transform.position);
        if (!this.gameObject.name.Contains("Clone"))
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
        
        respwn.onClick.AddListener(RespawnCube);
    }

    // Update is called once per frame
    void Update()
    {
        //print("Mos Post" + Input.mousePosition);
        if(Input.GetKeyUp(KeyCode.Space))
        {
            GameObject newCube = Instantiate(cubePrefab, homePosition, Quaternion.identity);
            newCube.GetComponent<Renderer>().material.color = HelperFunctions.RandomColor();
            Destroy(this.gameObject);

        }

        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            Touch();
        }

        currentTurnTime += Time.fixedDeltaTime;
        if (currentTurnTime > timeToTurn)
        {
            currentTurnTime = 0;
        }

        
        //print("Mouse Position: " + Distance(Input.mousePosition, transform.position));
        //print("Cube position" + transform.localPosition);
    }

    private void FixedUpdate()
    {
        if(isLauched)
        {
            rb.AddTorque(transform.right * dragStrength * Time.fixedDeltaTime);
            //print("Velocity: " + rb.velocity); 
        }
        else
        {

        }

        initialVelocity = HelperFunctions.RotateVector(transform.forward, cubeLaunchAngle).normalized * dragStrength * multiplier * Time.fixedDeltaTime;
        //print("Fixed update Initial Velocity: " + InitialVelocity);
    }

    private void OnMouseDrag()
    {
        if (!isInitSet)
        {
            isInitSet = true;
            
            if(Input.touchCount > 0)
            {
                prevPointerX = Input.GetTouch(0).position.x;
                initialPointerY = Input.GetTouch(0).position.y;
            }
            else
            {
                prevPointerX = Input.mousePosition.x;
                initialPointerY = Input.mousePosition.y;
            }
            currentTurnTime = 0;
        }

        if (SystemInfo.deviceType != DeviceType.Handheld)
        {
            //float mouseDeltaY = initialPointerY - Input.mousePosition.y;

            dragStrength = GetDragStrength(Input.mousePosition.y);
            RotateWithPointer(Input.mousePosition.x);
            UpdateLaunchRotationAngle(Input.mousePosition.y);
            multiplier = CalculateDynamicMultiplier(dragStrength);
        }
        else
        {
            //float pointerDeltaY = initialPointerY - Input.GetTouch(0).position.y;
            dragStrength = GetDragStrength(Input.GetTouch(0).position.y);

            RotateWithPointer(Input.GetTouch(0).position.x);
            UpdateLaunchRotationAngle(Input.GetTouch(0).position.y);
            multiplier = CalculateDynamicMultiplier(dragStrength);
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
            RespawnCube();
        }
            
    }

    private void OnMouseUp()
    {
        isLauched = true;
        LaunchStatus?.Invoke(isLauched);
        isInitSet = false;
        Vector3 direction = HelperFunctions.RotateVector(transform.forward, cubeLaunchAngle);

        //initialVelocity = direction.normalized * dragStrength * multiplier * Time.deltaTime;
        //print("OnMouseUp Initial Velocity: " + InitialVelocity);
        lineRenCtrl.SetLineRenVisibility(false);
        rb.AddForce(initialVelocity, ForceMode.VelocityChange);
        Debug.DrawLine(transform.position, direction * dragStrength * multiplier, Color.green, 2, false);
        //rb.AddExplosionForce(5, transform.position, 1, 2, ForceMode.Acceleration);
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


    private void RespawnCube()
    {
        GameObject newCube = Instantiate(cubePrefab, homePosition, Quaternion.identity);
        newCube.GetComponent<Renderer>().material.color = HelperFunctions.RandomColor();
        Destroy(this.gameObject);
    }

    private void RotateWithPointer(float pointerX)
    {
        //print("Raw Pointer value: " + pointerX);
        float percentTurned = currentTurnTime / timeToTurn;
        //print("Percent Lerped: " + percentTurned);

        float reversedPointerX = Mathf.Abs(pointerX - Screen.width); //use this for inversed controls

        float delta = HelperFunctions.Map(-32, 32, Screen.width * 0.2f, 
            HelperFunctions.GetPercentageOf(20, Screen.width), reversedPointerX);

        Quaternion newRotation = Quaternion.Euler(0, ClampedCubeRotation(delta), 0);
        transform.rotation = Quaternion.Slerp(newRotation, 
            transform.rotation, percentTurned); //Quaternion.LookRotation(cam.ScreenToWorldPoint(Input.mousePosition));
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
        //    "Cube Position: " + reverseCubePositionY, "End of screen: " + endOfScreenY});

        cubeLaunchAngle = HelperFunctions.Map(15, 69, reverseCubePositionY, endOfScreenY, reversePointerPosition);
    }

    private float CalculateDynamicMultiplier(float dragStr)
    {
        if (SystemInfo.deviceType != DeviceType.Handheld)
        {
            return HelperFunctions.Map(25, 100, 0, startingCubeScreenPosition.y, dragStr);
        }
        else
        {
            return HelperFunctions.Map(25, 75, 0, startingCubeScreenPosition.y, dragStr);
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

        //HelperFunctions.Log("Drag Strength: " + dragStrength);
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
