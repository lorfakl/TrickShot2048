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
    bool placeEvenly = true;
    [SerializeField]
    bool isUsingRandomVals = true;
    [SerializeField]
    GameObject blockPrefab;
    [SerializeField]
    GameObject gameOverUiPrefab;
    [SerializeField]
    Transform[] columnBases; //these transforms are from left to right in the scene
    [SerializeField]
    float secondsUntilSpeedIncrease = 0;
    [SerializeField]
    float secondsUntilBlockSpawn = 0;

    //public static UnityEvent<BlockController> contactMade = new UnityEvent<BlockController>();
    public BlockContantactEvent blockContactEventPublisher;

    [Range(0.1f, 1f)]
    public float gravityControl;

    public float secondsBetweenSpeedIncrease  = 90;
    public float secondsBetweenBlockSpawn = 1.0f;
    public float gravityIncreasePercentage = 0.05f;
    public float deleteInSeconds = 1.5f;

    float defaultGravity = -9.8f;

    [SerializeField]
    FloatReference playerScore;
    [SerializeField]
    FloatReference columnIndex;
    [SerializeField]
    FloatReference scoreMultiplier;

    public UnityEvent<List<FloatReference>> UpdateScoreEvent;
    public UnityEvent<List<FloatReference>> RemoveComboLevel;
    public UnityEvent<List<FloatReference>> IncreaseComboLevel;
    public UnityEvent<BlockController> BlockSpawnedEvent;
    public UnityEvent<BlockController> BlockHitByCubeEvent;
    public UnityEvent<BlockController> BlockOnBlockContactEvent;
    public UnityEvent<BlockController> BlockOnGroundContactEvent;
    public UnityEvent GameOverEvent;

    #region C# Events

    public event EventHandler<BlockSpawnedEventArgs> OnBlockSpawmed;
    public class BlockSpawnedEventArgs : EventArgs
    {
        public BlockController block;
    }
    #endregion

    public BlockManager Instance { get; private set; }

    float secondUntilObjectDelete = 5f;

    Dictionary<int, List<GameObject>> createdObjects = new Dictionary<int, List<GameObject>>();
    public Queue<GameObject> deletetionQueue = new Queue<GameObject>();
    int lastUsedIndex = 0;
    int lastUsedValue = 0;
    int blocksCreated = 0;
    float spawnHeight = 80;

    Dictionary<Guid, int> observedGUIDs = new Dictionary<Guid, int>();

    int[] possibleBlockValues = { 2, 4, 8, 16, 32, 64 };

    //[SerializeField]

    #region public methods

    public void BlocksMergedEventHandler(BlockController existingBlock, BlockController blockOnTop)
    {
        existingBlock.gameObject.GetComponent<Renderer>().material.color = Color.green;
        blockOnTop.gameObject.transform.DOMoveY(transform.position.y - 10, .5f);
        blockOnTop.gameObject.GetComponent<Renderer>().material.color = Color.blue; //contact.Block.gameObject is the newest block in column we want to move this one
        
        if(existingBlock.Rb.isKinematic)
        {
            ChangeKinematic(existingBlock.Rb);
        }
        
        RaiseUpdateScoreEvent(existingBlock.ListIndex, existingBlock.Value);
        StartCoroutine(CountDownToDeletion(0.75f, blockOnTop.gameObject));
    }

    public void ManagerBlockRemovedEventHandler(BlockController existingBlock, BlockController blockOnTop)
    {
        ChangeKinematic(existingBlock.Rb);
        StartCoroutine(CountDownToDeletion(1, blockOnTop.gameObject));
    }

    public void ManagerBlockAddedEventHandler(BlockController existingBlock, BlockController blockOnTop)
    {
        ChangeKinematic(existingBlock.Rb);
    }

    public void ManagerHorizaontalMatchFoundEventHandler(List<BlockController> matchedBlocks)
    {
        foreach(var b in matchedBlocks)
        {
            b.Rb.isKinematic = true;
            b.GetComponent<Renderer>().material.color = Color.cyan;
            b.transform.DOMoveY(b.transform.position.y - 50, 0.5f);
        }

        foreach(var b in matchedBlocks)
        {
            StartCoroutine(CountDownToDeletion(0.75f, b.gameObject));
        }
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
        HelperFunctions.Log("Current gravity: " + Physics.gravity);
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

        Physics.gravity = new Vector3(0, defaultGravity * gravityControl, 0);
    }

    void InceaseSpeed()
    {
        //Physics.gravity += (Physics.gravity * gravityIncreasePercentage);
    }

    void SpawnBlock()
    {
        if(canSpawnBlock)
        {
            int newIndex = 0;
            if (enableMultipleColumns)
            {
                if(placeEvenly)
                {
                    newIndex = GenerateSequenticalIndex(columnBases.Length);
                }
                else
                {
                    newIndex = GenerateRandomIndex(ref lastUsedIndex, columnBases.Length);
                } 
            }
            
            Transform parent = columnBases[newIndex];
            
            Vector3 newPosition = parent.position;
            newPosition.y += spawnHeight;

            GameObject block = GameObject.Instantiate(blockPrefab, newPosition, Quaternion.identity, parent);

            if (parent.childCount > 7)
            {
                ExecuteGameOverActions();
            }

            BlockController blockController = block.GetComponent<BlockController>();
            blockController.ListIndex = newIndex;
            blockController.Value = GetRandomBlockValue(areMatchesPossible);
            BlockSpawnedEvent?.Invoke(blockController);

        }
    }

    void ContactEventHandler(ContactTypes contact)
    {

        switch(contact.CType)
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
                if(!contact.Block.Rb.isKinematic)
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

    int GenerateSequenticalIndex(int length)
    {
        int indexToUse = lastUsedValue++;
        if(lastUsedIndex >= length)
        {
            indexToUse = 0;
        }

        lastUsedIndex = indexToUse;
        //print("Index to Use: " + indexToUse);
        return indexToUse;
    }

    void ExecuteGameOverActions()
    {
        canSpawnBlock = false;
        GameOverEvent?.Invoke();
        Transform parentCanvas = GameObject.FindGameObjectWithTag("canvas").transform;
        GameObject.Instantiate(gameOverUiPrefab, parentCanvas);
    }

    void ChangeKinematic(Rigidbody rb)
    {
        StartCoroutine(CountDownToKinematicChangeCorou(.5f, rb));
    }

    void RaiseUpdateScoreEvent(int index, int score)
    {
        UpdateScoreEvent.Invoke(new List<FloatReference> { new FloatReference(index), new FloatReference(score) }); 
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
}
