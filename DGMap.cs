using UnityEngine;
using System.Collections;

public class Map
{
    bool[,] map;
    Point minPoint;
    Point maxPoint;
    int sizeRow;
    int sizeColum;

    public Map(int row, int colum, Point min, Point max)
    {
        map = new bool[row, colum];
        minPoint = new Point(min);
        maxPoint = new Point(max);

        sizeRow = row;
        sizeColum = colum;
    }


    public void SetCell(int posRow, int posColum, bool value)
    {
        map[posRow, posColum] = value;
    }

    public bool GetCell(int posRow, int posColum)
    {
        if (posRow < sizeRow && posColum < sizeColum && posRow > -1 && posColum > -1)
        {
            return map[posRow, posColum];
        }
        else
        {
            return false;
        }

    }

    public void FillByRoom(Room room, bool isCoridor)
    {
        Point min = room.GetCorner(0);
        Point max = room.GetCorner(2);

        for (int i = maxPoint.GetY() - max.GetY(); i < maxPoint.GetY() - min.GetY(); i++)
        {
            for (int j = min.GetX() - minPoint.GetX(); j < max.GetX() - minPoint.GetX(); j++)
            {
                /*if(isCoridor)
                {
                    Debug.Log("Add to " + i.ToString() + " " + j.ToString());
                }*/
                map[i, j] = true;
            }
        }
    }

   /* public void DrawInConsole()
    {

        for (int i = 0; i < sizeU; i++)
        {
            string line = "";
            for (int j = 0; j < sizeV; j++)
            {
                if (map[i, j])
                {
                    line += "*";
                }
                else
                {
                    line += "0";
                }
            }
            Debug.Log(line);
        }
    }*/

    public int GetRowMax()
    {
        return sizeRow;
    }

    public int GetColumMax()
    {
        return sizeColum;
    }

    public Vector3 GetPosition(int row, int colum)
    {
        float xLength = ((float)(maxPoint.GetX() - minPoint.GetX())) / (float)(sizeColum);
        float yLength = ((float)(maxPoint.GetY() - minPoint.GetY())) / (float)(sizeRow);
        return new Vector3(colum * xLength + (float)minPoint.GetX(), 0f, (float)maxPoint.GetY() - row * yLength);
    }

    public Vector3 GetCenterPosition(int row, int colum)
    {
        float xLength = ((float)(maxPoint.GetX() - minPoint.GetX())) / (float)(sizeColum);
        float yLength = ((float)(maxPoint.GetY() - minPoint.GetY())) / (float)(sizeRow);
        return new Vector3(colum * xLength + (float)minPoint.GetX() + xLength / 2f, 0f, (float)maxPoint.GetY() - row * yLength - yLength / 2f);
    }

}
