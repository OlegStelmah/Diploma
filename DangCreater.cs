using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangCreater : MonoBehaviour {

    public GameObject lineLGO;
    public GameObject lineRGO;
    public GameObject lineTGO;
    public GameObject lineBGO;
    public GameObject ICornerTLGO;
    public GameObject ICornerTRGO;
    public GameObject ICornerBLGO;
    public GameObject ICornerBRGO;
    public GameObject OCornerTLGO;
    public GameObject OCornerTRGO;
    public GameObject OCornerBLGO;
    public GameObject OCornerBRGO;
    public GameObject FloorPlate;
    public GameObject lamp;
    public GameObject torch;


    public int dSize = 12;
    public int roomSize = 6;
    public int roomSizeDelta = 3;
    public int roomsCount = 4;
    public int coridorThickness = 2;
    public float oneStepSize;
    public bool isAllowIntersection = false;
    public bool isSetIds = false;
    public int coridorsCount = 1;
    public float whProportion = 0f;
    public DGCore dgCore;

    public GameObject cube;
    public int maxCount;

    public GameObject interierFirst;
    public GameObject interierSecond;
    public GameObject interierThird;
    public GameObject interierFourth;
    List<GameObject> interiers = new List<GameObject>();

    System.Random rnd = new System.Random();
    // Use this for initialization
    void Start () {
        if (dgCore == null)
        {
            dgCore = ScriptableObject.CreateInstance<DGCore>();
        }

        interiers.AddRange(new GameObject[] { interierFirst, interierSecond, interierThird });

        dgCore.Init(dSize, roomSize, roomSizeDelta, roomsCount, isAllowIntersection, coridorThickness, oneStepSize, whProportion, coridorsCount);
        dgCore.Generate();
        dgCore.EmitGeometry(lineLGO, lineRGO, lineTGO, lineBGO, ICornerTLGO, ICornerTRGO, ICornerBLGO, ICornerBRGO, OCornerTLGO, OCornerTRGO, OCornerBLGO, OCornerBRGO, FloorPlate, oneStepSize, isSetIds);
        Debug.Log(dgCore.GetRoomsCount());

        int totalArea = 0;
        foreach(Room room in dgCore.rooms) { totalArea += room.square; }
        for(int y = 0; y < dgCore.GetRoomsCount(); y ++) {

            /*
            DGRoomClass room = dgCore.rooms[y];
            float height = room.verticalSize * oneStepSize;
            float weight = room.horizontalSize * oneStepSize;
            Vector3 vector = dgCore.GetPoints(y)[i];
            Debug.Log("x:" + vector.x + "y:" + vector.y + "z:" + vector.z);
            GameObject newCube = Instantiate(cube);
            switch (i)
            {
                case 0:
                    newCube.transform.position = new Vector3(vector.x + (weight / 4), 5f, vector.z + (height / 4));
                    break;
                case 1:
                    newCube.transform.position = new Vector3(vector.x + (weight / 4), 5f, vector.z - (height / 4));
                    break;
                case 2:
                    newCube.transform.position = new Vector3(vector.x - (weight / 4), 5f, vector.z - (height / 4));
                    break;
                case 3:
                    newCube.transform.position = new Vector3(vector.x - (weight / 4), 5f, vector.z + (height / 4));
                    break;

            }*/
            AddLampToTheRoom(y, 2, 2);
            AddInterierToRoom(y);
            AddNPCToRoom(y, totalArea);
            
        }

        for(int i= 0; i < dgCore.GetCoridorsCount(); i++)
        {
            AddTorchToTheCoridor(i);
        }


        //newCube.transform.position = new Vector3(centrVector.x + (room.horizontalSize * oneStepSize / 4), 0f, centrVector.y + (room.verticalSize * oneStepSize / 4));
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void AddInterierToRoom(int roomIndex)
    {
        Room room = dgCore.rooms[roomIndex];
        GameObject interier = null;
        ObjInfo currentObj = new ObjInfo();
        for (int count = 0; count < 10; count++)
        {
            GameObject tmpInterier = interiers[rnd.Next(0, interiers.Count)];
            currentObj = tmpInterier.GetComponent<ObjInfo>();
            int sqr = (int)(currentObj.width * currentObj.length);
            if(sqr < room.square && room.horizontalSize > currentObj.width && room.verticalSize > currentObj.length) {interier = tmpInterier; break; }
        }
        if(interier != null)
        {

            int newPosX =(int)( 1 + Mathf.RoundToInt(currentObj.length * oneStepSize) / 1.5);
            int newPosY =(int)( 1 + Mathf.RoundToInt(currentObj.width * oneStepSize) / 1.5);
            Debug.Log(newPosX + " " + newPosY);
            for (int turn = 0; turn < 10; turn++)
            {
                bool isCanBePlaced = true;
                
                Room newObj = new Room((int)dgCore.GetPoints(roomIndex)[1].x + newPosX, (int)dgCore.GetPoints(roomIndex)[1].z + newPosY, Mathf.RoundToInt(currentObj.width), Mathf.RoundToInt(currentObj.length));
                if (room.objectsInRoom.Count != 0)
                {
                    foreach (ObjInfo obj in room.objectsInRoom)
                    {
                        if (obj.obj.IsIntersect(newObj)) { isCanBePlaced = false; }
                    }
                }
                if (isCanBePlaced)
                {
                    Debug.Log("wasHere");
                    AddObjectToPoint(interier, new Vector3(dgCore.GetPoints(roomIndex)[1].x + newPosX, 1.7f, dgCore.GetPoints(roomIndex)[1].z - newPosY));
                    room.objectsInRoom.Add(new ObjInfo { obj = newObj });
                    break;
                }
                newPosX = rnd.Next(3, (int)((room.horizontalSize - (currentObj.length/2)) * oneStepSize)) + (1 + Mathf.RoundToInt(currentObj.length * oneStepSize) / 2);
                newPosY = rnd.Next(3, (int)((room.verticalSize - (currentObj.width / 2)) * oneStepSize)) + (1 + Mathf.RoundToInt(currentObj.width * oneStepSize) / 2);
            }
        }

    }

    void AddNPCToRoom(int roomIndex, int totalArea)
    {
        Room room = dgCore.rooms[roomIndex];
        ObjInfo currentObj = cube.GetComponent<ObjInfo>();
        float percent = (float)(room.square) / totalArea;
        int count = Mathf.RoundToInt(maxCount * percent);
        int newPosX= 0;
        int newPosY = 0;
        for (int i = 0; i < count; i++)
        {
            for (int turn = 0; turn < 10; turn++)
            {
                bool isCanBePlaced = true;
                newPosX = rnd.Next(3, (int)(room.horizontalSize * oneStepSize));
                newPosY = rnd.Next(3, (int)(room.verticalSize * oneStepSize));
                Room newObj = new Room((int)dgCore.GetPoints(roomIndex)[1].x + newPosX,(int)dgCore.GetPoints(roomIndex)[1].z + newPosY, Mathf.RoundToInt(currentObj.width), Mathf.RoundToInt(currentObj.length));
                if (room.objectsInRoom.Count != 0)
                {
                    foreach (ObjInfo obj in room.objectsInRoom)
                    {
                       if(obj.obj.IsIntersect(newObj)) { isCanBePlaced = false; }
                    } 
                }
                if (isCanBePlaced) {
                    AddObjectToPoint(cube, new Vector3(dgCore.GetPoints(roomIndex)[1].x + newPosX, 0.5f, dgCore.GetPoints(roomIndex)[1].z - newPosY));
                    room.objectsInRoom.Add(new ObjInfo { obj = newObj });
                    break;
                }
            }
        }
    }

    void AddTorchToTheCoridor(int coridorIndex)
    {
        Room coridor = dgCore.coridors[coridorIndex];
        float length = coridor.verticalSize;
        float widht = coridor.horizontalSize;
        if (widht <= length) { HorizontalCorridor(coridorIndex, length); }
        else { VerticallCorridor(coridorIndex, widht); }
    }
    
    void HorizontalCorridor(int coridorIndex, float widht)
    {
        Vector3 leftpoint = dgCore.GetCoridorPoints(coridorIndex)[0];
        leftpoint.y = 3;
        float bottomWallPos = leftpoint.x + 0.25f;
        float topWallPos = dgCore.GetCoridorPoints(coridorIndex)[3].x - 0.25f;
        float delta = oneStepSize * 2;
        int count = (int)(widht / 2);

        if(count != 0)
        {
            bool boo = false;
            for (int i = 0; i < count; i++)
            {
                torch.transform.rotation = new Quaternion();
                leftpoint.z += delta;
                if (boo) { leftpoint.x = topWallPos; boo = false; torch.transform.Rotate(0, -90, 0); }
                else { leftpoint.x = bottomWallPos; boo = true; torch.transform.Rotate(0, 90, 0); }
                AddObjectToPoint(torch, leftpoint);
            }
        }
    }

    void VerticallCorridor(int coridorIndex, float length)
    {
        Vector3 leftpoint = dgCore.GetCoridorPoints(coridorIndex)[0];
        float leftWallPos = leftpoint.z + 0.25f;
        leftpoint.y = 3;
        float rightWallPos = dgCore.GetCoridorPoints(coridorIndex)[1].z - 0.25f;
        float delta = oneStepSize * 2;
        int count = (int)(length / 2);

        if (count != 0)
        {
            bool boo = false;
            for (int i = 0; i < count; i++)
            {
                torch.transform.rotation = new Quaternion();
                leftpoint.x += delta;
                if (boo) { leftpoint.z = rightWallPos; boo = false; torch.transform.Rotate(0, 180, 0); }
                else { leftpoint.z = leftWallPos; boo = true; torch.transform.Rotate(0, 0, 0); }
                AddObjectToPoint(torch, leftpoint);
            }
        }
    }

    void AddLampToTheRoom(int roomIndex, int lineCount, int rowCount)
    {
        Room room = dgCore.rooms[roomIndex];
        float length = room.verticalSize * oneStepSize;
        float widht = room.horizontalSize * oneStepSize;
    
        float deltaLength = length / (lineCount + 1);
        float deltaWidth = widht / (rowCount + 1);

        Vector3 point = new Vector3(dgCore.GetPoints(roomIndex)[0].x, 5f, dgCore.GetPoints(roomIndex)[0].z);

        for (int i = 0; i < lineCount; i++)
        {
            point.z += deltaLength;
            
            for (int y = 0; y < rowCount; y++)
            {
                point.x += deltaWidth;
                AddObjectToPoint(lamp, point);
            }
            point.x = dgCore.GetPoints(roomIndex)[0].x;
        }
    }

    void AddObjectToPoint(GameObject obj, Vector3 coordinate)
    {
        GameObject newObj = Instantiate(obj);
        newObj.transform.position = coordinate;
    }
}
