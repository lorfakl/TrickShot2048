using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectSpecificGlobals;
using TMPro;

[RequireComponent(typeof(Rigidbody))]

public class BlockController : MonoBehaviour
{
    [SerializeField]
    int multiOfTwo;

    Rigidbody rb;
    bool isSpawnFalling;
    Collision previousCollider;
    RaycastHit hit;

    public bool isTrackedInModel = false;
    public BlockContantactEvent contactEvent;
    [SerializeField]
    public TMP_Text valueText;
    
    [SerializeField]
    int listIndex;
    Material blockMaterial;
    Color startingColor;

    public int Value
    {
        get { return multiOfTwo; }
        set { UpdateValue(value); }
    }
    
    public Rigidbody Rb
    {
        get { return rb; }
    }

    public Guid GUID
    {
        get;
        private set;
    }

    public int ListIndex
    {
        get { return listIndex; }
        set { listIndex = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.maxDepenetrationVelocity = 1;
        isSpawnFalling = true;
        blockMaterial = GetComponent<Renderer>().material;
        startingColor = blockMaterial.color;
        GUID = Guid.NewGuid();
    }

    // Update is called once per frame
    void Update()
    {
        if(isSpawnFalling)
        {
            //rb.velocity -= (rb.velocity * 0.01f);
        }

        if(rb.isKinematic)
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
        else if(blockMaterial.color == Color.red)
        {
            GetComponent<Renderer>().material.color = startingColor;
        }
    }

    private void FixedUpdate()
    {

    }

    private void OnCollisionStay(Collision collision)
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision != previousCollider || collision.gameObject.tag == "Player")
        {
            previousCollider = collision;

            ContactTypes contact = ScriptableObject.CreateInstance<ContactTypes>();
            contact.GenerateContactData(this, collision);

            if (collision.gameObject.tag == "ground")
            {
                //rb.isKinematic = true;
                contact.UpdateContactType(ContactType.GroundContact);
                EvaluateChange(contact);
            }
            else if (collision.gameObject.tag == "block")
            {
                contact.UpdateContactType(ContactType.BlockContact);
                EvaluateChange(contact);
                //EvaluateChange(contact);
            }
            if (collision.gameObject.tag == "Player")
            {
                Rb.constraints = RigidbodyConstraints.None;
                contact.UpdateContactType(ContactType.CubeContact);
                EvaluateChange(contact);

            }

            isSpawnFalling = false;
        }


    }

    private void OnCollisionExit(Collision collision)
    {
        /*if (collision.gameObject.tag == "block")
        {
            if(rb.isKinematic)
            {
                rb.isKinematic = false;
            }
        }*/

    }

    private void OnTriggerEnter(Collider other)
    {
        //print("You're on top of me");
    }

    private void OnTriggerExit(Collider other)
    {
        //print("weight off my head");
    }

    private void OnDestroy()
    {
        this.enabled = false;
    }

    void EvaluateChange(ContactTypes contact)
    {
        //print(contact.CType);
        contactEvent.NotifyListeners(contact);
    }

    private void UpdateValue(int v)
    {
        multiOfTwo += v;
        valueText.text = multiOfTwo.ToString(); 
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
