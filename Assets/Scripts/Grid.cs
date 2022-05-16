using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public class Grid<T>
    {
        public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
        public class OnGridValueChangedEventArgs : EventArgs
        {
            public int x;
            public int y;
        }

        private int width;
        private int height;
        private T[,] gridArray;
        //private 
        private TextMesh[,] debugArray;
        private float cellSize;
        private Vector3 origin;
        private bool showDebug;

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public Grid(int width, int height, float cellSize, Vector3 origin, Func<Grid<T>, int, int, T> referenedObj, bool showDebug = true) : this(width, height, cellSize, origin, showDebug)
        {

            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    gridArray[x, y] = referenedObj(this, x, y);
                    if (showDebug)
                    {
                        //debugArray[x, y] = HelperFunctions.CreateWorldText(gridArray[x, y]?.ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize / 2, cellSize / 2));
                    }
                }
            }
        }

        public Grid(int width, int height, float cellSize, Vector3 origin, bool showDebug = true)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.origin = origin;
            gridArray = new T[width, height];
            this.showDebug = showDebug;


            if (showDebug)
            {
                debugArray = new TextMesh[width, height];

                for (int x = 0; x < gridArray.GetLength(0); x++)
                {

                    for (int y = 0; y < gridArray.GetLength(1); y++)
                    {
                        debugArray[x, y] = HelperFunctions.CreateWorldText(gridArray[x, y]?.ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize / 2, cellSize / 2));
                        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                        SetDebugValue(x, y, x.ToString() + y.ToString());
                    }
                }

                Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.red, 100f);
                Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.red, 100f);
            }

            //SetValue(0, 1, 52);
        }


        public Vector3 GetWorldPosition(int x, int y)
        {
            Vector3 newWorldPos = new Vector3(x, y) * cellSize + origin;
            return newWorldPos;
        }

        public Vector2 GetGridXY(Vector3 worldPos)
        {
            //HelperFunctions.Log("Input variables : (" + worldPos.x + "," + worldPos.y + ")");
            //HelperFunctions.Log("Midput variables : (" + worldPos.x / cellSize + "," + worldPos.y / cellSize + ")");
            int x = Mathf.FloorToInt((worldPos - origin).x / cellSize);
            int y = Mathf.FloorToInt((worldPos - origin).y / cellSize);
            //HelperFunctions.Log("Output variables : (" + x + "," + y + ")");
            if(ValidXY(x,y))
            {
                return new Vector2(x, y);
            }

            return new Vector2(-1, -1);
            
        }

        public bool ValidXY(Vector2 coords)
        {
            return ValidXY((int)coords.x, (int)coords.y);
        }

        public bool ValidXY(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        

        public List<T> GetRow(int y)
        {
            List<T> rowObjects = new List<T>();
            for(int x = 0; x < width; x++)
            {
                if(ValidXY(x,y))
                {
                    rowObjects.Add(gridArray[x, y]);
                }
            }

            return rowObjects;
        }

        public List<T> GetColumn(int x)
        {
            List<T> colObjects = new List<T>();
            for (int y = 0; y < height; y++)
            {
                if (ValidXY(x, y))
                {
                    colObjects.Add(gridArray[x, y]);
                }
            }

            return colObjects;
        }

        public T GetValue(float x, float y)
        {
            return GetValue((int)x, (int)y);
        }

        public T GetValue(int x, int y)
        {
            if (ValidXY(x, y))
            {
                return gridArray[x, y];
            }
            else
            {
                return default(T);
            }
        }

        public T GetValue(Vector3 worldPos)
        {
            Vector2 xy = GetGridXY(worldPos);
            return GetValue((int)xy.x, (int)xy.y);
        }

        public T GetValue(Vector2 xy)
        {
            return GetValue((int)xy.x, (int)xy.y);
        }

        public void SetValue(Vector3 worldPos, T value)
        {
            Vector2 gridIndex = GetGridXY(worldPos);
            SetValue((int)gridIndex.x, (int)gridIndex.y, value);
            //HelperFunctions.Log(gridIndex.x + "," + gridIndex.y);
        }

        public void SetValue(int x, int y, T value)
        {
            if (ValidXY(x, y))
            {
                gridArray[x, y] = value;
                debugArray[x, y].text = gridArray[x, y].ToString();
                if (OnGridValueChanged != null)
                {
                    OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y });
                }
            }
        }

        public void SetDebugValue(int x, int y, string value)
        {
            if (ValidXY(x, y))
            {
                debugArray[x, y].text = value;
            }
        }

        public void TriggerGridValueChange(int x, int y)
        {
            if (showDebug)
            {
                SetDebugValue(x, y, gridArray[x, y].ToString());
            }

            if (OnGridValueChanged != null)
            {
                OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y });
            }
        }
    }

    public interface IGrid
    {
        void TriggerGridObjectChanged<T>(Grid<T> grid, int x, int y);
        
        /// <summary>
        /// The Purpose of these functions is to remove null wrapper values from the 
        /// GetRow and Column functions on the Grid
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        //List<T> GetRow<T>() where T : IGrid;
        //List<T> GetColumn<T>() where T : IGrid;
        string Print();
    }
}

