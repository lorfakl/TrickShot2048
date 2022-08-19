using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using ProjectSpecificGlobals;
using UnityEngine.Events;

public class SimpleBlock : MonoBehaviour
{
    enum PossibleStarts { two = 2 , four = 4, ate =8, sixTeen = 16, three2 = 32 };

    #region Private Fields
    [SerializeField]
    int multiOfTwo;
    Rigidbody rb;
    Collision previousCollider;

    Material blockMaterial;
    Color startingColor;

    [SerializeField]
    private Texture[] blockTextures;

    [SerializeField]
    bool useRandomValue;

    [SerializeField]
    PossibleStarts startingValue = new PossibleStarts();

    CubeController contrlrInstance; 
    #endregion

    #region Public Fields
    //public TMP_Text valueText;
    public BlockContantactEvent contactEvent;
     
    public int Value
    {
        get { return multiOfTwo; }
    }

    public Rigidbody Rb
    {
        get { return rb; }
    }

    public bool HasBeenLaunched
    {
        get;
        set;
    }

    public Guid GUID
    {
        get;
        private set;
    }
    #endregion

    #region C# Events
    public UnityEvent<SimpleBlock, SimpleBlock> BlockMergeEvent;

    public static event EventHandler<OnBoardStatusChangedEventArgs> OnBoardStatusChanged;

    public class OnBoardStatusChangedEventArgs : EventArgs
    {
        public readonly bool remove;
        public readonly SimpleBlock blockReference;

        public OnBoardStatusChangedEventArgs(bool shouldRemove, SimpleBlock blockReference)
        {
            remove = shouldRemove;
            this.blockReference = blockReference;
        }
    }
    #endregion

    #region Public Methods
    public void UpdateValue()
    {
        multiOfTwo *= 2;
        
        Utilities.HelperFunctions.Log("Log of " + multiOfTwo + " is " + Mathf.Log(multiOfTwo, 2));
        int textTureIndex = (int)Mathf.Log(multiOfTwo, 2) - 1;
        blockMaterial.mainTexture = blockTextures[textTureIndex];
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        
        blockMaterial = GetComponent<Renderer>().material;
        rb = GetComponent<Rigidbody>();
        contrlrInstance = GetComponent<CubeController>();
        HasBeenLaunched = false;
    }

    void Start()
    {
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.maxDepenetrationVelocity = 1;
        int textureIndex = 0;
        if (useRandomValue)
        {
            textureIndex = UnityEngine.Random.Range(0, 6);
        }
        else
        {
            textureIndex = (int)Mathf.Log((float)startingValue, 2) - 1;
        }

        multiOfTwo = (int)Mathf.Pow(2, (textureIndex + 1));
        Utilities.HelperFunctions.Log("Texture Index: " + textureIndex + "\n" + "Multiple of 2: " + multiOfTwo);
        blockMaterial.mainTexture = blockTextures[textureIndex];

        GUID = Guid.NewGuid();

        if(contrlrInstance == null)
        {
            OnBoardStatusChanged?.Invoke(this, new OnBoardStatusChangedEventArgs(false, this));
        }
        
    }
    private void OnCollisionStay(Collision collision)
    {
        MergeCheck(collision);
    }

    private void OnCollisionEnter(Collision collision)
    {
        MergeCheck(collision);
    }

    private void OnDisable()
    {
        if(this.enabled)
        {
            Utilities.HelperFunctions.Log("Is enabled: " + this.enabled);
            OnBoardStatusChanged?.Invoke(this, new OnBoardStatusChangedEventArgs(true, this));
        }
        
    }

    #endregion

    #region Private Methods
    void EvaluateChange(ContactTypes contact)
    {
        //print(contact.CType);
        contactEvent.NotifyListeners(contact);
    }

    private void MergeCheck(Collision collision)
    {
        SimpleBlock blkComp = collision.gameObject.GetComponent<SimpleBlock>();
        if (blkComp != null)
        {
            //Utilities.HelperFunctions.Log("Is a simpleblock");
            if (blkComp.Value == this.Value)
            {
                //Utilities.HelperFunctions.Log("Let's Merge!: " + this.Value + " and " + blkComp.Value);
                BlockMergeEvent.Invoke(this, blkComp);

                if (contrlrInstance != null)
                {
                    contrlrInstance.enabled = false;
                    OnBoardStatusChanged?.Invoke(this, new OnBoardStatusChangedEventArgs(false, this));
                }
            }
            else
            {
                return;
            }
        }
        else
        {
            //Utilities.HelperFunctions.Log("NOT a block");
            return;
        }

        if (HasBeenLaunched)
        {
            contrlrInstance.enabled = false;
            OnBoardStatusChanged?.Invoke(this, new OnBoardStatusChangedEventArgs(false, this));
        }
    }

    public override string ToString()
    {
        return Value.ToString();
    }


  
    #endregion
}
