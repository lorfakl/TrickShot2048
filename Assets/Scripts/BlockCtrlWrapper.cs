using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using System.Linq;

public class BlockCtrlWrapper : IGrid
{


    #region Private Fields
    BlockController internalReference; 
    #endregion

    #region Public Fields
    public Grid<BlockCtrlWrapper> Grid
    {
        get;
        private set;
    }

    public int X
    {
        get;
        private set;
    }

    public int Y
    {
        get;
        private set;
    }

    public BlockController BlockCtrl
    {
        get { return internalReference; }
        set 
        {
            internalReference = value;
            TriggerGridObjectChanged(Grid, X, Y);
        }
    }

    public bool IsEmpty
    {
        get { return BlockCtrl == null ? true : false; }
    }


    #endregion

    #region C# Events
    #endregion

    #region Public Methods

    public BlockCtrlWrapper(Grid<BlockCtrlWrapper> grid, int x, int y)
    {
        Grid = grid;
        X = x;
        Y = y;
        internalReference = default;
    }
    public string Print()
    {
        return BlockCtrl.Value.ToString();
    }

    public void TriggerGridObjectChanged<T>(Grid<T> grid, int x, int y)
    {
        Grid.TriggerGridValueChange(x, y);
    }

    public List<BlockCtrlWrapper> GetRow()
    {
        var currentRow = Grid.GetRow(Y);
        foreach (var rapper in currentRow.ToList())
        {
            if (rapper.internalReference == default)
            {
                currentRow.Remove(rapper);
            }
        }

        return currentRow;
    }

    public List<BlockCtrlWrapper> GetColumn()
    {
        var currentCol = Grid.GetColumn(X);
        foreach (var rapper in currentCol.ToList())
        {
            if (rapper.internalReference == default)
            {
                currentCol.Remove(rapper);
            }
        }

        return currentCol;
    }
    #endregion

    #region Private Methods
    #endregion
}
