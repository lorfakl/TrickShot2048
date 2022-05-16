using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class GridTesting : MonoBehaviour
{
    private Grid<IGaveUp> grid;
    // Start is called before the first frame update
    void Start()
    {
        grid = new Grid<IGaveUp>(2, 6, 10f, new Vector3(0,0), (Grid<IGaveUp> grid, int x, int y) => new IGaveUp(grid, x, y));
        //grid = new Grid(4, 2, 10f, new Vector3(20, -10));
    }

    // Update is called once per frame
    void Update()
    {
        //print("Mouse Position: " + Input.mousePosition);
        if(Input.GetMouseButtonUp(0))
        {
            Vector3 scrn2WrldPt = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, 1));
            print(scrn2WrldPt);
            //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //cube.transform.position = scrn2WrldPt;
            var pt = grid.GetValue(scrn2WrldPt);
            pt.AddValue(5);
            //grid.SetValue(HelperFunctions.GetMouseWorldPosition(), true);
        }

        if(Input.GetMouseButtonUp(1))
        {
            List<IGaveUp> rowZero = grid.GetRow(5);
            string content = "";
            foreach(IGaveUp thing in rowZero)
            {
                content += thing.Print() + " ";
            }

            List<IGaveUp> colZero = grid.GetColumn(1);
            string contents = "";
            foreach (IGaveUp thing in colZero)
            {
                contents += thing.Print() + " ";
            }
            //print("Out of bounds!: " + rowZero.Count);
            //print("There should be no colZero: " + colZero.Count);
            print("Stuff in rowZero: " + content);
            print("Stuff in  colZero: " + contents);
        }
    }
}

public class IGaveUp: IGrid
{
    private const int MIN = 0;
    private const int MAX = 0;

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

    public Grid<IGaveUp> Grid
    {
        get;
        private set;
    }
    public int value;

    public IGaveUp(Grid<IGaveUp> grid, int x, int y)
    {
        Grid = grid;
        X = x;
        Y = y;
        value = 0;
    }

    public void AddValue(int addVal)
    {
        value += addVal;
        Mathf.Clamp(value, MIN, MAX);
        TriggerGridObjectChanged(Grid, X, Y);
    }

    public float GetValNormalized()
    {
        return (float)value / MAX;
    }

    public override string ToString()
    {
        return value.ToString();
    }

    public void TriggerGridObjectChanged<T>(Grid<T> grid, int x, int y)
    {
        grid.TriggerGridValueChange(x, y);
    }

    public string Print()
    {
        return this.ToString();
    }
}


