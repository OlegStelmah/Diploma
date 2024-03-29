﻿using UnityEngine;
using System.Collections;

public class PointPair
{
    public Point firstPoint;
    public Point secondPoint;
}

public class DGIntPairClass
{
    public int value01;
    public int value02;

    public DGIntPairClass()
    {
        value01 = 0;
        value02 = 0;
    }

    public DGIntPairClass(int v1, int v2)
    {
        value01 = v1;
        value02 = v2;
    }

    public override string ToString()
    {
        return "(" + value01.ToString() + ", " + value02.ToString() + ")";
    }
}

public class Point
{
    int x;
    int y;

    public Point()
    {
        x = 0;
        y = 0;
    }

    public Point(Point point)
    {
        x = point.GetX();
        y = point.GetY();
    }

    public int GetX()
    {
        return x;
    }

    public int GetY()
    {
        return y;
    }

    public void SetX(int value)
    {
        x = value;
    }

    public void SetY(int value)
    {
        y = value;
    }

    public Point(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public override string ToString()
    {
        return "(" + x.ToString() + ", " + y.ToString() + ")";
    }

    public float GetDistance(Point point)
    {
        return Mathf.Sqrt((point.GetX() - x) * (point.GetX() - x) + (point.GetY() - y) * (point.GetY() - y));
    }
}
