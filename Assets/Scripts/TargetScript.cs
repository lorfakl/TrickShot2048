using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TargetScript : MonoBehaviour
{
    [SerializeField]
    private int scoreValue;
    [SerializeField]
    private Rigidbody rb;

    private float baseScore = 50;

    public UnityEvent<float, GameObject> TargetDestroyed;

    private void Awake()
    {
        TargetDestroyed = new UnityEvent<float, GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        print("Original Score" + scoreValue);
        scoreValue = (int)(baseScore + this.transform.position.y);
        print("Y position score" + scoreValue);
        scoreValue /= (int)this.transform.lossyScale.magnitude;
        print("Scale position" + scoreValue);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        //rb.AddForce(collision.rigidbody.velocity * Time.deltaTime);
        //rb.AddTorque(collision.rigidbody.angularVelocity * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            TargetDestroyed.Invoke(baseScore+scoreValue, this.gameObject);
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(transform.forward * 400 * Time.deltaTime);
            rb.AddTorque(transform.up * collision.rigidbody.angularVelocity.magnitude * Time.deltaTime);
        }
    }
}
