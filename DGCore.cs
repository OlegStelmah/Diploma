﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class DGCore : ScriptableObject
{
    int dSize;
    int roomSize;
    int roomSizeDelta;
    int roomsCount;
    //bool isIntersections;
    int coridorThicknes;
    float stepSize;
    int coridorsCount;
    float whProp;

    public List<Room> rooms;
    public List<Room> coridors;
    public Map dgMap;

    List<GameObject> wallModels = new List<GameObject>();
    GameObject rootObject;
    GameObject floorRoot;
    GameObject wallsRoot;

    int generationCycleLimits = 50;
    float propCoefficient = 4f;
    string rootName = "dungeonRoot";
    string floorRootName = "floor";
    string wallsRootName = "walls";

    public void Init(int _dSize, int _roomSize, int _roomSizeDelta, int _roomsCount, bool _isIntersections, int _coridorThickness, float _stepSize, float _whProp, int _coridorsCount)
    {
        dSize = _dSize;
        roomSize = _roomSize;
        roomSizeDelta = _roomSizeDelta;
        roomsCount = _roomsCount;
        coridorThicknes = _coridorThickness;
        stepSize = _stepSize;
        whProp = _whProp;
        coridorsCount = _coridorsCount;

        ClearGeneratedWalls();
    }

    public bool isCorrect()
    {
        if (rooms != null && rooms.Count > 1)
        {
            return true;
        }
        return false;
    }

    public void Generate()
    {
        int generatedRooms = 0;
        int genStep = 0;
        rooms = new List<Room>();
        coridors = new List<Room>();
        while (generatedRooms < roomsCount && genStep < generationCycleLimits)
        {
            Room newRoom = GenerateRandomRoom();
            //проверяем, пересекается ли комната с уже созданными
            if (!newRoom.IsTooSmall(coridorThicknes) && !newRoom.IsIntersect(rooms))
            {
                rooms.Add(newRoom);
                generatedRooms++;
            }
            else
            {
                genStep++;
            }
        }

        if (rooms.Count > 1)
        {//есть хотя бы две комнаты, строим коридоры
            //для каждой ищем ближайшую
            for (int i = 0; i < rooms.Count; i++)
            {
                //int j = GetRoomToRoomClosestIndex(i);
                int[] toRoomClostArray = GetRoomToRoomClosestIndexesArray(i);
                for (int j = 0; j < coridorsCount; j++)
                {
                    if (j < toRoomClostArray.Length)
                    {
                        int toRoomId = toRoomClostArray[j];
                        if (toRoomId != i && toRoomId > -1)
                        {
                            BuildCoridor(i, toRoomId);
                        }
                    }
                }
            }

            //дальше формируем матрицу с проходимостью
            PointPair minMax = GetMinMax();
            dgMap = new Map(minMax.secondPoint.GetY() - minMax.firstPoint.GetY(), minMax.secondPoint.GetX() - minMax.firstPoint.GetX(), minMax.firstPoint, minMax.secondPoint);
            foreach (Room room in rooms)
            {
                dgMap.FillByRoom(room, false);
            }
            foreach (Room coridor in coridors)
            {
                dgMap.FillByRoom(coridor, true);
            }

        }


    }

    PointPair GetMinMax()
    {
        PointPair toReturn = new PointPair();
        Point rMin = rooms[0].GetCorner(0);
        Point rMax = rooms[0].GetCorner(2);
        toReturn.firstPoint = new Point(rMin);
        toReturn.secondPoint = new Point(rMax);
        foreach (Room room in rooms)
        {
            rMin = room.GetCorner(0);
            rMax = room.GetCorner(2);
            if (rMin.GetX() < toReturn.firstPoint.GetX())
            {
                toReturn.firstPoint.SetX(rMin.GetX());
            }
            if (rMin.GetY() < toReturn.firstPoint.GetY())
            {
                toReturn.firstPoint.SetY(rMin.GetY());
            }

            if (rMax.GetX() > toReturn.secondPoint.GetX())
            {
                toReturn.secondPoint.SetX(rMax.GetX());
            }
            if (rMax.GetY() > toReturn.secondPoint.GetY())
            {
                toReturn.secondPoint.SetY(rMax.GetY());
            }
        }

        foreach (Room coridor in coridors)
        {
            rMin = coridor.GetCorner(0);
            rMax = coridor.GetCorner(2);
            if (rMin.GetX() < toReturn.firstPoint.GetX())
            {
                toReturn.firstPoint.SetX(rMin.GetX());
            }
            if (rMin.GetY() < toReturn.firstPoint.GetY())
            {
                toReturn.firstPoint.SetY(rMin.GetY());
            }

            if (rMax.GetX() > toReturn.secondPoint.GetX())
            {
                toReturn.secondPoint.SetX(rMax.GetX());
            }
            if (rMax.GetY() > toReturn.secondPoint.GetY())
            {
                toReturn.secondPoint.SetY(rMax.GetY());
            }
        }

        //Debug.Log("Minimum: " + toReturn.point01.ToString() + " Maximum: " + toReturn.point02.ToString());

        return toReturn;
    }

    void BuildCoridor(int startIndex, int endIndex)
    {
        Room startRoom = rooms[startIndex];
        Room endRoom = rooms[endIndex];

        //выбрасываем у стартовой комнаты точку на вертикали, а у конечной - на горизоантали
        int sPos = startRoom.GetRandomVertical(coridorThicknes);
        int ePos = endRoom.GetRandomHorizontal(coridorThicknes);

        //теперь через sPos надо провести горизонталь, а через ePos вертикаль
        //надо найти к точке пересечений ближайшую вертикальную сторону у стартовой комнаты и ближайшую горизонтальную у конечной
        int clostV = startRoom.GetClosestVerticalCoordinate(ePos);
        int clostH = endRoom.GetClosestHorizontalCoordinate(sPos);

        Room newCoridor01 = new Room(new Point(clostV, sPos - coridorThicknes / 2), new Point(ePos + (Sign(ePos - clostV) > 0 ? coridorThicknes - coridorThicknes / 2 : -1 * coridorThicknes / 2), sPos - coridorThicknes / 2 + coridorThicknes));
        Room newCoridor02 = new Room(new Point(ePos - coridorThicknes / 2, clostH), new Point(ePos - coridorThicknes / 2 + coridorThicknes, sPos + (Sign(sPos - clostH) > 0 ? coridorThicknes - coridorThicknes / 2 : -1 * coridorThicknes / 2)));

        coridors.Add(newCoridor01);
        coridors.Add(newCoridor02);

    }

    int Sign(int i)
    {
        if (i >= 0)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }

    float Sign(float i)
    {
        if (i >= 0)
        {
            return 1f;
        }
        else
        {
            return -1f;
        }
    }

    int GetRoomToRoomClosestIndex(int i)
    {
        float closestDistance = 2 * dSize;
        Room room = rooms[i];
        int closestIndex = -1;
        for (int j = 0; j < rooms.Count; j++)
        {
            if (i != j)
            {
                //считаем расстояние от каждой вершины комнаты до каждой вершины j-ой комнаты и берем минимальное
                float d = room.GetCornerDistance(rooms[j]);
                if (d < closestDistance)
                {
                    closestDistance = d;
                    closestIndex = j;
                }
            }
        }

        return closestIndex;
    }

    //для каждой id-ой комнаты вычисляем последовательность ближайших комнат
    int[] GetRoomToRoomClosestIndexesArray(int id)
    {
        Dictionary<int, float> roomDistanceMap = new Dictionary<int, float>();
        for (int i = 0; i < rooms.Count; i++)
        {
            if (i != id)
            {
                float d = rooms[id].GetCornerDistance(rooms[i]);//расстояние от текущей комнаты до i-ой
                roomDistanceMap.Add(i, d);
            }
        }
        //Debug.Log(id.ToString() + " room: " + dictToString(roomDistanceMap));
        //дальше выводим индексы в порядке возрастания расстояний
        int[] toReturn = new int[roomDistanceMap.Count];
        float recordMin = -1f;
        int recordMinKey = -1;
        for (int i = 0; i < toReturn.Length; i++)
        {
            float min = 0f;
            int minKey = -1;
            foreach (int k in roomDistanceMap.Keys)
            {
                if (minKey == -1)
                {
                    if (roomDistanceMap[k] >= recordMin && k != recordMinKey)
                    {
                        min = roomDistanceMap[k];
                        minKey = k;
                    }
                }
                else
                {
                    if (roomDistanceMap[k] > recordMin && roomDistanceMap[k] < min)
                    {
                        min = roomDistanceMap[k];
                        minKey = k;
                    }
                }
            }
            toReturn[i] = minKey;
            recordMin = min;
            recordMinKey = minKey;
        }
        //Debug.Log(id.ToString() + " room: " + arrayToString(toReturn));

        return toReturn;
    }

    string arrayToString(int[] array)
    {
        string str = "";
        for (int i = 0; i < array.Length; i++)
        {
            str += array[i] + " ";
        }

        return str;
    }

    string dictToString(Dictionary<int, float> dict)
    {
        string str = "";
        foreach (int k in dict.Keys)
        {
            str += k.ToString() + ": " + dict[k].ToString() + "; ";
        }
        return str;
    }

    float FilterRandom(bool isWidth, float value)
    {
        if (isWidth)
        {
            return value * Mathf.Pow(propCoefficient, whProp);
        }
        else
        {
            return value * Mathf.Pow(propCoefficient, -1 * whProp);
        }
    }

    Room GenerateRandomRoom()
    {
        float xRandom = Random.Range(-1f, 1f);
        float yRandom = Random.Range(-1f, 1f);

        int cx = (int)(Sign(xRandom) * dSize * FilterRandom(true, Mathf.Abs(xRandom)) / (2 * Mathf.Pow(propCoefficient, Mathf.Abs(whProp))));
        int cy = (int)(Sign(yRandom) * dSize * FilterRandom(false, Mathf.Abs(yRandom)) / (2 * Mathf.Pow(propCoefficient, Mathf.Abs(whProp))));

        int w = Random.Range(roomSize - roomSizeDelta, roomSize + roomSizeDelta);
        int h = Random.Range(roomSize - roomSizeDelta, roomSize + roomSizeDelta);

        return new Room(cx, cy, w, h);
    }

    public Vector3[] GetPoints(int roomIndex)
    {
        Vector3[] a = rooms[roomIndex].GetPointsArray();
        Vector3[] toReturn = new Vector3[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            toReturn[i] = a[i] * stepSize;
        }
        return toReturn;
    }

    public Vector3[] GetCoridorPoints(int coridorIndex)
    {
        Vector3[] a = coridors[coridorIndex].GetPointsArray();
        Vector3[] toReturn = new Vector3[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            toReturn[i] = a[i] * stepSize;
        }
        return toReturn;
    }

    public int GetCoridorsCount()
    {
        return coridors.Count;
    }

    public int GetRoomsCount()
    {
        return rooms.Count;
    }

    void ClearGeneratedWalls()
    {
        if (wallModels.Count > 0)
        {
            for (int i = wallModels.Count - 1; i > -1; i--)
            {
                if (wallModels[i] != null)
                {
                    DestroyImmediate(wallModels[i]);
                }
            }
        }
        if (rootObject != null)
        {
            DestroyImmediate(rootObject);
        }
        if (floorRoot != null)
        {
            DestroyImmediate(floorRoot);
        }
        if (wallsRoot != null)
        {
            DestroyImmediate(wallsRoot);
        }
    }

    void EmitElement(GameObject element, Vector3 position, bool isFloor, int id, bool isColorize)
    {
        if (element != null)
        {
            GameObject newInst = (GameObject)PrefabUtility.InstantiatePrefab(element);
            newInst.transform.position = position;
            newInst.GetComponent<Renderer>().material = new Material(newInst.GetComponent<Renderer>().sharedMaterial);
            DRId drIdComponent = newInst.GetComponent<DRId>();
            if (drIdComponent != null)
            {
                drIdComponent.id = id;
            }
            if (isColorize)
            {
                SetColor(id, newInst);
            }
            wallModels.Add(newInst);
            if (!isFloor)
            {
                newInst.transform.SetParent(wallsRoot.transform);
            }
            else
            {
                newInst.transform.SetParent(floorRoot.transform);
            }

        }
    }

    void SetColor(int id, GameObject go)
    {
        if (id <= -1)
        {
            go.GetComponent<Renderer>().sharedMaterial.color = new Color(227f / 255f, 227f / 255f, 227f / 255f);
        }
        else
        {
           // go.GetComponent<Renderer>().sharedMaterial.color = DRCustomEditor.HSVToRGB(DRCustomEditor.GetSin(id, 0f), 0.5f, 0.8f);
        }
    }

    int SetId(int id, bool isSet)
    {
        if (isSet)
        {
            return id;
        }
        else
        {
            return -1;
        }
    }

    public void EmitGeometry(GameObject lineLGO, GameObject lineRGO, GameObject lineTGO, GameObject lineBGO, GameObject ICornerTLGO, GameObject ICornerTRGO, GameObject ICornerBLGO,
        GameObject ICornerBRGO, GameObject OCornerTLGO, GameObject OCornerTRGO, GameObject OCornerBLGO, GameObject OCornerBRGO, GameObject floorPlate, float stepSize, bool isSetId)
    {
        ClearGeneratedWalls();
        wallModels = new List<GameObject>();
        rootObject = new GameObject();
        rootObject.name = rootName;
        floorRoot = new GameObject();
        floorRoot.transform.SetParent(rootObject.transform);
        floorRoot.name = floorRootName;
        wallsRoot = new GameObject();
        wallsRoot.transform.SetParent(rootObject.transform);
        wallsRoot.name = wallsRootName;


        //теперь начинаем бегать по матрице и смотреть, какие элементы стоит порождать
        if (dgMap != null)
        {
            int u = dgMap.GetRowMax();
            int v = dgMap.GetColumMax();
            for (int i = 0; i < u + 1; i++)
            {
                for (int j = 0; j < v + 1; j++)
                {
                    bool center = dgMap.GetCell(i, j);
                    bool left = dgMap.GetCell(i, j - 1);
                    bool top = dgMap.GetCell(i - 1, j);
                    bool leftTop = dgMap.GetCell(i - 1, j - 1);
                    Vector3 coordinate = stepSize * dgMap.GetPosition(i, j);
                    if (center)
                    {//открытая клетка
                        EmitElement(floorPlate, stepSize * dgMap.GetCenterPosition(i, j), true, SetId(0, isSetId), isSetId);
                        //EmitElement(floorPlate, new Vector3(coordinate.x+3, 6.05f, coordinate.z-3), true, SetId(0, isSetId), isSetId);
                        if (left && leftTop && top)
                        {

                        }
                        else if (left && leftTop && !top)
                        {
                            coordinate.x = coordinate.x + 5;
                            EmitElement(OCornerBLGO, coordinate, false, SetId(5, isSetId), isSetId);
                        }
                        else if (left && !leftTop && top)
                        {
                            coordinate.z = coordinate.z + 5;
                            EmitElement(OCornerBRGO, coordinate, false, SetId(8, isSetId), isSetId);
                        }
                        else if (left && !leftTop && !top)
                        {
                            EmitElement(lineTGO, stepSize * dgMap.GetPosition(i, j), false, SetId(10, isSetId), isSetId);
                        }
                        else if (!left && leftTop && top)
                        {
                            coordinate.x = coordinate.x - 5;
                            EmitElement(OCornerTRGO, coordinate, false, SetId(7, isSetId), isSetId);
                        }
                        else if (!left && leftTop && !top)
                        {
                            coordinate.z = coordinate.z - 5;
                            //спорный случай
                            EmitElement(ICornerTLGO, coordinate, false, SetId(2, isSetId), isSetId);
                            coordinate.z = coordinate.z + 10;
                            EmitElement(ICornerBRGO, coordinate, false, SetId(4, isSetId), isSetId);
                        }
                        else if (!left && !leftTop && top)
                        {
                            EmitElement(lineLGO, stepSize * dgMap.GetPosition(i, j), false, SetId(9, isSetId), isSetId);
                        }
                        else if (!left && !leftTop && !top)
                        {
                            coordinate.z = coordinate.z - 5;
                            EmitElement(ICornerTLGO, coordinate, false, SetId(2, isSetId), isSetId);
                        }
                    }
                    else
                    {//сама клетка закрыта
                        if (left && leftTop && top)
                        {
                            coordinate.z = coordinate.z - 5;
                            EmitElement(OCornerTLGO, coordinate, false, SetId(6, isSetId), isSetId);
                        }
                        else if (left && leftTop && !top)
                        {
                            EmitElement(lineRGO, stepSize * dgMap.GetPosition(i, j), false, SetId(11, isSetId), isSetId);
                        }
                        else if (left && !leftTop && top)
                        {
                            coordinate.z = coordinate.z + 5;
                            //спорный случай
                            EmitElement(OCornerBRGO, coordinate, false, SetId(8, isSetId), isSetId);
                            coordinate.z = coordinate.z - 10;
                            EmitElement(OCornerTLGO, coordinate, false, SetId(6, isSetId), isSetId);
                        }
                        else if (left && !leftTop && !top)
                        {
                            coordinate.x = coordinate.x - 5;
                            EmitElement(ICornerTRGO, coordinate, false, SetId(3, isSetId), isSetId);
                        }
                        else if (!left && leftTop && top)
                        {
                            EmitElement(lineBGO, stepSize * dgMap.GetPosition(i, j), false, SetId(12, isSetId), isSetId);
                        }
                        else if (!left && leftTop && !top)
                        {
                            coordinate.z = coordinate.z + 5;
                            EmitElement(ICornerBRGO, coordinate, false, SetId(4, isSetId), isSetId);
                        }
                        else if (!left && !leftTop && top)
                        {
                            coordinate.x = coordinate.x + 5;
                            EmitElement(ICornerBLGO, coordinate, false, SetId(1, isSetId), isSetId);
                        }
                        else if (!left && !leftTop && !top)
                        {

                        }
                    }
                }
            }
        }
    }


}

