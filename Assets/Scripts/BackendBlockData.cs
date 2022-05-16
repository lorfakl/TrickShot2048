using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using UnityEngine.Events;
using System;
using System.Linq;

public class BackendBlockData : MonoBehaviour
{
    #region Inspector Fields
    [SerializeField]
    Transform[] columnBases;
    #endregion

    #region Private Fields
    Grid<BlockCtrlWrapper> blockData;
    BlockController currentCtrl;
    Dictionary<BlockController, Vector2> blocksInFlight;
    private float secondsUntilNextFlightCheck = 0.2f;
    #endregion

    #region Public Fields
    #endregion

    #region C# Events
    #endregion

    #region Unity Events
    public UnityEvent<BlockController, BlockController> ManagerBlockAddedEvent;
    public UnityEvent<BlockController, BlockController> ManagerBlockRemovedEvent;
    public UnityEvent<BlockController, BlockController> ManagerBlocksMergedEvent;
    public UnityEvent<FloatReference> BlocksMergedEvent;
    public UnityEvent<List<BlockController>> ManagerHorizaontalMatchFoundEvent;
    public UnityEvent<FloatReference> HorizaontalMatchFoundEvent;
    #endregion

    #region Public Methods
    public void BlockSpawnedEventHandler(BlockController blkCtrl)
    {
        print("Received new block from Manager");
        blocksInFlight.Add(blkCtrl, blockData.GetGridXY(blkCtrl.gameObject.transform.position));
    }

    public void BlockHitByCubeEventHandler(BlockController blkCtrl)
    {
        print("Manager detected that a block was hit by a cube");
        RemoveBlock(blkCtrl, true);
    }

    public void BlockOnGroundContactEventHandler(BlockController blkCtrl)
    {
        if (blocksInFlight.ContainsKey(blkCtrl))
        {
            //currently in flight according to this but we have receieved information about a physics contact
            if (AddToModelFromContact(blkCtrl))
            {
                blocksInFlight.Remove(blkCtrl);
                CheckForHorizontalMatch();
            }
        }
    }

    public void BlockOnBlockContactEventHandler(BlockController blkCtrl)
    {
        if(blocksInFlight.ContainsKey(blkCtrl))
        {
            //currently in flight according to this but we have receieved information about a physics contact
            if(AddToModelFromContact(blkCtrl))
            {
                blocksInFlight.Remove(blkCtrl);
                BlockCtrlWrapper rap = blockData.GetValue(blkCtrl.transform.position);
                CheckForCascade(rap);
                CheckForHorizontalMatch();
            }
        }
        
    }


    #endregion

    #region Unity Methods
    private void Awake()
    {
        blockData = new Grid<BlockCtrlWrapper>(columnBases.Length, 7, 6, columnBases[0].position,
            (Grid<BlockCtrlWrapper> grid, int x, int y) => new BlockCtrlWrapper(grid, x, y), false);
        blocksInFlight = new Dictionary<BlockController, Vector2>();
        print("Grid Width: " + blockData.Width);
    }
    #endregion

    #region Private Methods
    private void RemoveBlock(BlockController ctrl, bool wasRemovedByCube)
    {
        if(wasRemovedByCube)
        {
            var blkCtrlWrapper = blockData.GetValue(ctrl.gameObject.transform.position);
            try
            {
                BlockController blockRemoved = blkCtrlWrapper.BlockCtrl;
                if(blockRemoved != null)
                {
                    HelperFunctions.Log("We have a block its not null and its value is: " + blockRemoved.Value);
                    ManagerBlockRemovedEvent?.Invoke(blockData.GetValue(blkCtrlWrapper.X, blkCtrlWrapper.Y-1).BlockCtrl, blockRemoved);
                }
                blkCtrlWrapper.BlockCtrl = default;
            }
            catch(Exception e)
            {
                HelperFunctions.CatchException(e);
            }
        }
    }

    private void RemoveBlock(BlockController ctrl)
    {
        //block was removed by other means possibly 
        List<BlockCtrlWrapper> col = blockData.GetColumn(ctrl.ListIndex);
        int startingIndex = 0;
        for (int i = 0; i < col.Count; i++) //BlockCtrlWrapper wrper in col)
        {
            if (col[i].BlockCtrl == ctrl)
            {
                startingIndex = i;
                HelperFunctions.Log(ctrl.ToString() + " is located at (" + ctrl.ListIndex + " , " + startingIndex + ")");
            }
        }

        for (int i = startingIndex; i < col.Count; i++)
        {
            if ((i + 1) < col.Count)
            {
                int oneAhead = i + 1;
                col[i].BlockCtrl = col[oneAhead].BlockCtrl;
            }
            else if ((i + 1) == col.Count)
            {
                col[i].BlockCtrl = default;
            }
        }
    }

    private void CheckForCascade(BlockCtrlWrapper ctrlWrapper)
    {
        if(blockData.ValidXY(blockData.GetGridXY(ctrlWrapper.BlockCtrl.transform.position)))
        {
            var currentCol = ctrlWrapper.GetColumn();
            for(int i = currentCol.Count - 1; i > 0; i--)
            {
                if(MergeBlocks(currentCol[i].BlockCtrl))
                {
                    HelperFunctions.Log("Successful Merge");
                    RemoveBlock(currentCol[i].BlockCtrl);
                }
            }


        }
    }

    private bool MergeBlocks(BlockController target)
    {
        bool mergeWasPerfromed = false;

        Vector2 targetCoords = blockData.GetGridXY(target.transform.position);
        if(blockData.ValidXY(targetCoords))
        {
            BlockCtrlWrapper belowTarget = blockData.GetValue(targetCoords.x, targetCoords.y - 1);
            if (target.Value == belowTarget.BlockCtrl.Value)
            {
                belowTarget.BlockCtrl.Value = target.Value;
                mergeWasPerfromed = true;
                ManagerBlocksMergedEvent?.Invoke(belowTarget.BlockCtrl, target);
                return mergeWasPerfromed;
            }
            else
            {
                return mergeWasPerfromed;
            }
        }
        else
        {
            HelperFunctions.CatchException(new Exception("The supplied BlockController does not have a valid X Y on the Grid"));
            return mergeWasPerfromed;
        }
    }

    private void CheckForHorizontalMatch()
    {
        BlockCtrlWrapper originBlock = blockData.GetValue(0, 0);
        if(originBlock.BlockCtrl == default)
        {
            return;
        }
        else 
        {
            var currentCol = originBlock.GetColumn();
            foreach(var block in currentCol)
            {
                var currentRow = block.GetRow();
                if (currentRow.Count == blockData.Width)
                {
                    if(CheckRowEquality(currentRow))
                    {
                        List<BlockController> horizontallyMatchedBlocks = new List<BlockController>();
                        foreach(var b in currentRow)
                        {
                            horizontallyMatchedBlocks.Add(b.BlockCtrl);
                            RemoveBlock(b.BlockCtrl);
                        }
                        HelperFunctions.Log("Detected an Equal Row");
                        ManagerHorizaontalMatchFoundEvent?.Invoke(horizontallyMatchedBlocks);
                        HorizaontalMatchFoundEvent?.Invoke(new FloatReference(
                            horizontallyMatchedBlocks[0].Value * blockData.Width));
                    }
                }
            }
        }
    }
    
    private bool CheckRowEquality(List<BlockCtrlWrapper> row)
    {
        int targetValue = row[0].BlockCtrl.Value * blockData.Width;
        foreach (var rapper in row)
        {
            int valueQuotient = targetValue / rapper.BlockCtrl.Value;
            if (valueQuotient != blockData.Width)
            {
                return false;
            }
        }

        return true;
    }

    private bool AddToModelFromContact(BlockController ctrl)
    {
        bool successfulModelUpdate = false;
        Vector2 newposition = blockData.GetGridXY(ctrl.transform.position);
        if(blockData.ValidXY(newposition)) //should return true since this is from the 
        {
            BlockCtrlWrapper ctrlWrap = blockData.GetValue(newposition);
            if (ctrlWrap != null)
            {
                ctrlWrap.BlockCtrl = ctrl;
                print("Resting at: " + newposition);
                if (newposition.y > 0)
                {
                    BlockController oldBlock = blockData.GetValue(newposition.x, newposition.y - 1).BlockCtrl;
                    if(oldBlock.Value == ctrlWrap.BlockCtrl.Value)
                    {
                        successfulModelUpdate = true;
                        return successfulModelUpdate;
                    }
                    ManagerBlockAddedEvent?.Invoke(oldBlock, ctrlWrap.BlockCtrl);
                }
                successfulModelUpdate = true;
                return successfulModelUpdate;
            }
            else
            {
                HelperFunctions.CatchException(new Exception("The supplied BlockController does not have a valid X Y on the Grid"));
                return successfulModelUpdate;
            }
        }
        return successfulModelUpdate;
    }
    #endregion
}


