using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using ProjectSpecificGlobals;
using TMPro;
using UnityEngine.Events;
using Utilities;
using System.Collections;

public class BlockManager : MonoBehaviour
{
    [SerializeField]
    bool canSpawnBlock = true;
    [SerializeField]
    bool areMatchesPossible = false;
    [SerializeField]
    bool enableMultipleColumns = true;
    [SerializeField]
    bool isUsingRandomVals = true;
    [SerializeField]
    GameObject blockPrefab;
    [SerializeField]
    Transform[] columnBases; //these transforms are from left to right in the scene
    [SerializeField]
    float secondsUntilSpeedIncrease = 0;
    [SerializeField]
    float secondsUntilBlockSpawn = 0;

    //public static UnityEvent<BlockController> contactMade = new UnityEvent<BlockController>();
    public BlockContantactEvent blockContactEventPublisher;

    public float secondsBetweenSpeedIncrease  = 90;
    public float secondsBetweenBlockSpawn = 1.0f;
    public float gravityIncreasePercentage = 0.05f;
    public float deleteInSeconds = 1.5f;

    [SerializeField]
    FloatReference playerScore;
    [SerializeField]
    FloatReference columnIndex;
    [SerializeField]
    FloatReference scoreMultiplier;

    public UnityEvent<List<FloatReference>> UpdateScoreEvent;
    public UnityEvent<List<FloatReference>> RemoveComboLevel;
    public UnityEvent<List<FloatReference>> IncreaseComboLevel;

    public BlockManager Instance { get; private set; }

    float secondUntilObjectDelete = 5f;

    Dictionary<int, List<GameObject>> createdObjects = new Dictionary<int, List<GameObject>>();
    public Queue<GameObject> deletetionQueue = new Queue<GameObject>();
    int lastUsedIndex = 0;
    int lastUsedValue = 0;
    int blocksCreated = 0;
    float spawnHeight = 50;

    int blocksAddedToModel = 0;
    Dictionary<Guid, int> observedGUIDs = new Dictionary<Guid, int>();

    int[] possibleBlockValues = { 2, 4, 8, 16, 32, 64 };

    //[SerializeField]

    #region public methods

    /// <summary>
    /// Update Score Handler receives the UpdateScoreEvent whose data is formatted in the following way
    /// Index 0: score multiplier
    /// Index 1: index on which the bonus should be applied
    /// </summary>
    /// <param name="floatReferences"></param>
    public void ApplyScoreMultiplierHandler(List<FloatReference> floatReferences)
    {
        scoreMultiplier.Variable.Value = floatReferences[0];
        HelperFunctions.Log("Apply Score Multiplier Handler handled");
        columnIndex.Variable.Value = floatReferences[1];
    }

    #endregion

    private void Awake()
    {
        playerScore.Variable.Value = 0;
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            print("copy destroyed");
        }
        else
        {
            Instance = this;
        }

        if (columnBases == null || blockPrefab == null)
        {
            throw new System.Exception("Attach the references for Block PRefab and BlockColumnsBases");
        }

        secondsUntilSpeedIncrease = secondsBetweenSpeedIncrease;
        secondsUntilBlockSpawn = secondsBetweenBlockSpawn;
        secondUntilObjectDelete = deleteInSeconds;
    }

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < columnBases.Length; i++)
        {
            createdObjects.Add(i, new List<GameObject>());
        }

        blockContactEventPublisher.ContactEvent.AddListener(ContactEventHandler);

        SpawnBlock();
    }

    // Update is called once per frame
    void Update()
    {
        secondsUntilSpeedIncrease -= Time.deltaTime;
        secondsUntilBlockSpawn -= Time.deltaTime;
        secondUntilObjectDelete -= Time.deltaTime;

        if (secondsUntilSpeedIncrease < 0)
        {
            secondsUntilSpeedIncrease = secondsBetweenSpeedIncrease;
            InceaseSpeed();
        }

        if(secondsUntilBlockSpawn < 0)
        {
            SpawnBlock();
            secondsUntilBlockSpawn = secondsBetweenBlockSpawn - (Time.fixedDeltaTime * blocksCreated);
        }

        if(secondUntilObjectDelete < 0)
        {
            secondUntilObjectDelete = deleteInSeconds;
            if(deletetionQueue.Count > 0)
            {
                Destroy(deletetionQueue.Dequeue());
            }
            
        }
    }

    void InceaseSpeed()
    {
        Physics.gravity += (Physics.gravity * gravityIncreasePercentage);
    }

    void SpawnBlock()
    {
        if(canSpawnBlock)
        {
            int newIndex = 0;
            if (enableMultipleColumns)
            {
                newIndex = GenerateRandomIndex(ref lastUsedIndex, columnBases.Length);
            }
            
            
            Transform parent = columnBases[newIndex];
            Vector3 newPosition = parent.position;
            newPosition.y += spawnHeight;

            GameObject block = GameObject.Instantiate(blockPrefab, newPosition, Quaternion.identity, parent);
            //TMP_Text numberText = block.transform.Find("Canvas/Number").GetComponent<TMP_Text>();
           
            BlockController blockController = block.GetComponent<BlockController>();
            blockController.ListIndex = newIndex;
            blockController.Value = GetRandomBlockValue(areMatchesPossible);
            //numberText.text = blockController.Value.ToString();
            //createdObjects[newIndex].Add(block);
            //blocksCreated++;

            //print("Spawning Block of Value: " + blockController.Value);
        }
    }

    void ContactEventHandler(ContactTypes contact)
    {

        switch(contact.CType)
        {
            case ContactType.BlockContact:
                BlockController existingBlock = contact.Collision.gameObject.GetComponent<BlockController>();

                if (contact.Block.Value == existingBlock.Value)
                {
                    MergeBlocks(existingBlock, contact.Block);
                }
                else
                {
                    int currentStack = contact.Block.ListIndex;
                    UpdateBlockModel(contact.Block, contact.Block.ListIndex, false);
                }
                RaiseRemoveComboEvent(existingBlock.ListIndex);
                break;

            case ContactType.CubeContact:

                if(!contact.Block.Rb.isKinematic)
                {
                    foreach (ContactPoint pt in contact.Collision.contacts)
                    {
                        Debug.DrawRay(pt.point, pt.normal, Color.red);
                    }

                    contact.Block.Rb.AddForce(Vector3.forward * 100000);
                    UpdateBlockModel(contact.Block, contact.Block.ListIndex, true);
                }
                RaiseIncreaseComboEvent(contact.Block.ListIndex);
                break;

            case ContactType.GroundContact:
                //contact.Block.Rb.isKinematic = true;
                UpdateBlockModel(contact.Block, contact.Block.ListIndex, false);
                break;

            default:
                break;
        }
        
    }

    int GetRandomBlockValue(bool canBlocksMatch = true)
    {
        if(canBlocksMatch && isUsingRandomVals)
        {
           return possibleBlockValues[GenerateRandomIndex(ref lastUsedValue, possibleBlockValues.Length)];
        }
        else if(isUsingRandomVals)
        {
            return UnityEngine.Random.Range(-2147483, 2147483);
        }
        else
        {
            return 2;
        }
    }
    
    int GenerateRandomIndex(ref int lastUsed, int length)
    {
        int index;

        if (UnityEngine.Random.value > 0.72)
        {
            index = UnityEngine.Random.Range(0, length);
        }
        else
        {
            index = UnityEngine.Random.Range(0, length);
            while (index == lastUsed)
            {
                index = UnityEngine.Random.Range(0, length);
            }
        }

        lastUsed = index;
        return index;
    }

    void ResetPosition(GameObject g)
    {
        float currentYPosition = g.transform.localPosition.y;
        g.transform.localPosition = new Vector3(0, currentYPosition + 5, 0);
        g.transform.localRotation = Quaternion.identity;
    }

    void UpdateBlockModel(BlockController b, int index, bool removeOperation)
    {
        int listCount = createdObjects[index].Count;
        if (!observedGUIDs.ContainsKey(b.GUID))
        {
            observedGUIDs.Add(b.GUID, 1);
            b.isTrackedInModel = true;
            if (listCount - 1 >= 0)
            {
                GameObject lastBlock = createdObjects[index][listCount - 1];
                lastBlock.GetComponent<Rigidbody>().isKinematic = true;
            }
            createdObjects[index].Add(b.gameObject);
            //HelperFunctions.Log("Added block to model");
            blocksAddedToModel++;
            HelperFunctions.Log("Added " + blocksAddedToModel + " blocks to the model so far");
        }
        else if(removeOperation)
        {
            b.isTrackedInModel = false;
            //print("Doing a Remove operation");
            createdObjects[index].RemoveAt(listCount - 1);
            int newCount = createdObjects[index].Count;
            if(newCount > 0)
            {
                ChangeKinematic(createdObjects[index][newCount - 1].GetComponent<Rigidbody>());
                //createdObjects[index][newCount - 1].GetComponent<Rigidbody>().isKinematic = false;
            }
            
            HelperFunctions.Log("Removed block in column " + index + " with value of " + b.Value);
        }
        VerifyBlockModel();
        //HelperFunctions.Log(createdObjects[index].Count + " block are at index " + index);
    }

    void VerifyBlockModel()
    {
        //what turns purple should be everything that is on the board
        //HelperFunctions.Log("Printing Block Model before Verify Block Model");
        //PrintBlockModel(createdObjects);
        //HelperFunctions.Log("Starting Verify Block Model ===============");
        for (int i = 0; i < columnBases.Length; i++)
        {
            for(int j = 0;  j < createdObjects[i].Count; j++)
            {
                if(createdObjects[i][j] == null)
                {
                    Debug.LogWarning("For some reason we're trying to access something that should have been deleted");
                    HelperFunctions.LogListContent(createdObjects[i]);
                }
                else
                {
                    createdObjects[i][j].GetComponent<Renderer>().material.color = Color.magenta;
                }
                
                if(j > 0)
                {
                    BlockController b = createdObjects[i][j - 1].GetComponent<BlockController>();
                    BlockController b2 = createdObjects[i][j].GetComponent<BlockController>();
                    if (b.Value == b2.Value)
                    {
                        //Casscading
                        MergeBlocks(b, b2);
                        UpdateBlockModel(b2, i, true);
                        //DeleteBlock(b2);
                    }
                }

            }
        }

        List<int> rowIndices = CheckForHorizontalMatches(createdObjects);
        if(rowIndices.Count > 0)
        {
            ProcessEqualRows(rowIndices, createdObjects);
        }

        //HelperFunctions.Log("Exiting Verify Block Model ===============");
    }

    void PrintBlockModel(Dictionary<int, List<GameObject>> model)
    {
        foreach(KeyValuePair<int, List<GameObject>> entry in model)
        {
            if(model[entry.Key].Count > 0)
            {
                HelperFunctions.Log("Block values at index: " + entry.Key.ToString());
            }
            foreach(GameObject g in model[entry.Key])
            {
                HelperFunctions.Log(g.GetComponent<BlockController>().Value.ToString());
            }
        }
    }

    List<int> CheckForHorizontalMatches(Dictionary<int, List<GameObject>> model)
    {
        int maxHeight = GetTallestStackCount();
        List<int> matchingRowIndices = new List<int>();

        for(int i = 0; i < maxHeight; i++)
        {
            int columnValueTotal = 0;
            int targetValue = 0;
            bool wasExitedEarly = false;
            for(int col = 0; col < columnBases.Length; col++)
            {
                //HelperFunctions.Log("Current i: " + i + " Current stack height: " + model[col].Count);
                if (model[col].Count > i) //if current stack is higher than the i
                {
                    //HelperFunctions.Log("Col: " + col + " has a height greater than " + i);
                    targetValue = model[col][i].GetComponent<BlockController>().Value;
                    columnValueTotal += targetValue;
                }
                else //if current stack is not taller than i exit this inner most loop
                {
                    //HelperFunctions.Log("Col: " + col + " does not have a height greater than " + i);
                    wasExitedEarly = true;
                    break;
                }
            }

            if(!wasExitedEarly)
            {
                if ((columnValueTotal / targetValue) == 5)
                {
                    HelperFunctions.Log("The row of " + i + "are all the same value");
                    matchingRowIndices.Add(i);
                }
            }
        }

        return matchingRowIndices;
    }

    void ProcessEqualRows(List<int> rowIndices, Dictionary<int, List<GameObject>> model)
    {
        foreach(int index in rowIndices)
        {
            foreach (KeyValuePair<int, List<GameObject>> entry in model)
            {
                model[entry.Key][index].GetComponent<Renderer>().material.color = Color.cyan;
                BlockController b = model[entry.Key][index].GetComponent<BlockController>();
                b.Rb.constraints = RigidbodyConstraints.None;
                b.Rb.AddForce(Vector3.right * 100000);
                UpdateBlockModel(b, entry.Key, true);
            }
        }
    }

    int GetTallestStackCount()
    {
        int stackCount = 0;

        foreach (KeyValuePair<int, List<GameObject>> entry in createdObjects)
        {
            if(createdObjects[entry.Key].Count > stackCount)
            {
                stackCount = createdObjects[entry.Key].Count;
            }
        }
        HelperFunctions.Log("Tallest Stack: " + stackCount);
        return stackCount;
    }

    void ChangeKinematic(Rigidbody rb)
    {
        StartCoroutine(CountDownToKinematicChangeCorou(.5f, rb));
    }

    void MergeBlocks(BlockController existingBlock, BlockController blockOnTop)
    {
        existingBlock.gameObject.GetComponent<Renderer>().material.color = Color.green;
        blockOnTop.gameObject.transform.DOMoveY(transform.position.y - 10, .5f);
        blockOnTop.gameObject.GetComponent<Renderer>().material.color = Color.blue; //contact.Block.gameObject is the newest block in column we want to move this one

        existingBlock.Value = blockOnTop.Value;
        playerScore.Variable.Value += existingBlock.Value;
        HelperFunctions.Log("I really hope the score multiplier event has been handled at this point");
        RaiseUpdateScoreEvent(existingBlock.Value, scoreMultiplier);
        VerifyBlockModel();
        //DeleteBlock(blockOnTop.gameObject);
    }

    void RaiseUpdateScoreEvent(int score, float scoreMultiplier)
    {
        UpdateScoreEvent.Invoke(new List<FloatReference> { new FloatReference(score), new FloatReference(scoreMultiplier) }); 
    }

    void RaiseRemoveComboEvent(int index)
    {
        RemoveComboLevel.Invoke(new List<FloatReference> { new FloatReference(index) });
    }

    void RaiseIncreaseComboEvent(int index)
    {
        IncreaseComboLevel.Invoke(new List<FloatReference> { new FloatReference(index) });
    }

    void DeleteBlock(BlockController g)
    {
        if(createdObjects[g.ListIndex].Remove(g.gameObject))
        {
            StartCoroutine(CountDownToDeletion(1.5f, g.gameObject));
        }
        else if(observedGUIDs.ContainsKey(g.GUID))
        {
            
            //throw new Exception("Yo The removal did not complete hmmmmmmmmmmmmmmmmmmmmmm");
        }
        else
        {
            StartCoroutine(CountDownToDeletion(.2f, g.gameObject));
        }
        
    }

    IEnumerator CountDownToDeletion(float seconds, GameObject g)
    {
        yield return HelperFunctions.Timer(seconds);
        Destroy(g);
    }

    IEnumerator CountDownToKinematicChangeCorou(float seconds, Rigidbody rb)
    {
        yield return HelperFunctions.Timer(seconds);
        rb.isKinematic = false;
     
    }
}
