using ProjectSpecificGlobals;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utilities;
using DG.Tweening;

public class CubeManager : MonoBehaviour
{
    private struct GuidPairs
    {
        Guid first;
        Guid sec;
    }

    #region Private Fields
    [SerializeField]
    FloatReference playerScore;
    [SerializeField]
    FloatReference columnIndex;
    [SerializeField]
    FloatReference scoreMultiplier;

    Dictionary<Guid, Guid> observedGUIDs = new Dictionary<Guid, Guid>();
    Dictionary<Guid, SimpleBlock> currentlySpawnedCubes = new Dictionary<Guid, SimpleBlock>();
    #endregion

    #region Public Fields

    public Button respwn;
    public GameObject cubePrefab;

    public UnityEvent<List<FloatReference>> UpdateScoreEvent;
    public UnityEvent<List<FloatReference>> RemoveComboLevel;
    public UnityEvent<List<FloatReference>> IncreaseComboLevel;
    public UnityEvent<SimpleBlock> BlockOnBlockContactEvent;
    
    public static Vector3 HomePosition
    {
        get;
        set;
    }
    #endregion

    #region C# Events
    
    #endregion

    #region Public Methods
    public static int GetRandomPowerOfTwo()
    {
        return (int)Mathf.Pow(2, UnityEngine.Random.Range(1, 6));
    }

    public void SimpleMergeEventHandler(SimpleBlock b, SimpleBlock b2)
    {
        bool result = CheckIfSeenBefore(b, b2);
        HelperFunctions.Log("Seen this before?: " + result);
        
        if(result)
        {
            HelperFunctions.Log(b.Value + " + " + b2.Value + " equals " + b.Value * 2);
            b.UpdateValue();
            b2.gameObject.SetActive(false);
            ScoreDisplay.score += b.Value;
            RotateSkybox.PlayMergeEventHandler();
            //playerScore.Variable.Value += b.Value;
            //b2.gameObject.transform.DOMoveZ(25f, 0.1f);
        }
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        
      
    }

    

    void Start()
    {
        respwn.onClick.AddListener(RespawnCube);
    }

    void Update()
    {
    
    }
    #endregion

    #region Private Methods
    private void RespawnCube()
    {
        GameObject newCube = Instantiate(cubePrefab, HomePosition, Quaternion.identity);
        newCube.GetComponent<Renderer>().material.color = HelperFunctions.RandomColor();
        //Destroy(this.gameObject);
    }

    bool CheckIfSeenBefore(SimpleBlock b, SimpleBlock b2)
    {
        if (observedGUIDs.ContainsKey(b.GUID))
        {
            if (observedGUIDs[b.GUID] == b2.GUID)
            {
                return true;
            }
            else
            {
                observedGUIDs[b.GUID] = b2.GUID;
                return false;
            }
        }

        if(observedGUIDs.ContainsKey(b2.GUID))
        {
            if (observedGUIDs[b2.GUID] == b.GUID)
            {
                return true;
            }
            else
            {
                observedGUIDs[b2.GUID] = b.GUID;
                return false;
            }
        }
            
        observedGUIDs.Add(b.GUID, b2.GUID);
        observedGUIDs.Add(b2.GUID, b.GUID);
        return false;
        


    }
    void ContactEventHandler(ContactTypes contact)
    {

        /*switch (contact.CType)
        {
            case ContactType.BlockContact:
                BlockController existingBlock = contact.Collision.gameObject.GetComponent<BlockController>();
                BlockOnBlockContactEvent?.Invoke(contact.Block);
                RaiseRemoveComboEvent(contact.Block.ListIndex);
                if (contact.Block.Value == existingBlock.Value)
                {

                }
                else
                {
                    int currentStack = contact.Block.ListIndex;
                }
                break;

            case ContactType.CubeContact:
                if (!contact.Block.Rb.isKinematic)
                {
                    foreach (ContactPoint pt in contact.Collision.contacts)
                    {
                        Debug.DrawRay(pt.point, pt.normal, Color.red);
                    }

                    contact.Block.Rb.AddForce(Vector3.forward * 100000);
                    BlockHitByCubeEvent?.Invoke(contact.Block);
                    RaiseIncreaseComboEvent(contact.Block.ListIndex);
                }
                break;

            case ContactType.GroundContact:
                BlockOnGroundContactEvent?.Invoke(contact.Block);
                RaiseRemoveComboEvent(contact.Block.ListIndex);
                break;

            default:
                break;
        }*/
    }

    private void OnBoardStatusChangedEventHandler(object sender, SimpleBlock.OnBoardStatusChangedEventArgs e)
    {
        if(e.remove)
        {
            if(currentlySpawnedCubes.Remove(e.blockReference.GUID))
            {
                HelperFunctions.Log("Successfully Removed " + e.blockReference.Value + "\n" + e.blockReference.GUID);
                StartCoroutine(CountDownToDeletion(.2f, e.blockReference.gameObject));
            }
            else
            {
                HelperFunctions.Error("Unable to Removed " + e.blockReference.Value + "\n" + e.blockReference.GUID
                    + "\n" + "You should probably check that out");
            }
            
        }
        else
        {
            if(currentlySpawnedCubes.ContainsKey(e.blockReference.GUID))
            {
                return;
            }

            currentlySpawnedCubes.Add(e.blockReference.GUID, e.blockReference);
            HelperFunctions.Log("Added " + e.blockReference.Value + "\n" + e.blockReference.GUID);
        }
    }

    void RaiseRemoveComboEvent(int index)
    {
        RemoveComboLevel.Invoke(new List<FloatReference> { new FloatReference(index) });
    }

    void RaiseIncreaseComboEvent(int index)
    {
        IncreaseComboLevel.Invoke(new List<FloatReference> { new FloatReference(index) });
    }

    IEnumerator CountDownToDeletion(float seconds, GameObject g)
    {
        yield return HelperFunctions.Timer(seconds);
        Destroy(g);
    }

    IEnumerator CountDownToKinematicChangeCorou(float seconds, Rigidbody rb)
    {
        yield return HelperFunctions.Timer(seconds);
        rb.isKinematic = !rb.isKinematic;

    }
    #endregion
}
