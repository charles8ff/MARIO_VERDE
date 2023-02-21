using UnityEngine;
using System.Collections;
#if UNITY_EDITOR

using UnityEditor.AI;
#endif


public class FloorGenerator : MonoBehaviour
{
    public int maxRooms; // Maximum numbers of rooms allowed
    public int mapSize; // Size of the matrix that will attempt fo fit all rooms
    public GameObject[] rooms; // References to prefab rooms
    int[,] layout = new int[10, 10]; // Layout attempted to fit into the matrix of size = mapSize (default 10,10)
    int roomNum = 0; // Room number per floor
    int failsCount = 0; // Decides if this layout can or cannot accept more rooms

    /// Getters and Setters
    public int RoomNum { get => roomNum; set => roomNum = value; }
    public int[,] Layout { get => layout; set => layout = value; }
    
    /// <summary>
    /// Empties the whole floor
    /// </summary>
    public void emptyFloor()
    {
        for (int i = 0; i < layout.GetLength(0); i++)
        {
            for (int j = 0; j < layout.GetLength(1); j++)
            {
                layout[i, j] = 0;
            }

        }
    }
    /// <summary>
    /// Attempts to insert a room in the floor randomly
    /// </summary>
    /// <param name="room"> Prefab Room to insert </param>
    public void InsertRoom(Room room)
    {
        Room auxRoom = new Room(room.Id); // Aux room that gets saved in case we need 2 flip
        int coordX = 0; // Position in X-axis of the matrix
        int coordY = 0; // Position in Y-axis of the matrix
        bool validPosFlag = false; // Flag to check if we can place a room in that position
        int tries = 0; // Tries to insert

        while (!validPosFlag && tries < 10) // Will try until is valid or 10 tries
        {
            // Find a door (-1 cell in the matrix ) 
            coordX = Random.Range(0, mapSize);
            coordY = Random.Range(0, mapSize);
            validPosFlag = true;
            while (layout[coordX, coordY] != -1)
            {
                coordX = Random.Range(0, mapSize);
                coordY = Random.Range(0, mapSize);
            }
            // Checks if room has same index than next room
            if (coordY + 1 <= mapSize - 1)
                if (layout[coordX, coordY + 1] == room.Id)
                    validPosFlag = false;
            if (coordY - 1 >= mapSize - 1)
                if (layout[coordX , coordY - 1] == room.Id)
                    validPosFlag = false;
            if (coordX + 1 <= mapSize - 1)
                if (layout[coordX + 1, coordY] == room.Id)
                    validPosFlag = false;
            if (coordX - 1 >= mapSize - 1)
                if (layout[coordX - 1, coordY] == room.Id)
                    validPosFlag = false;
            tries++;
        }

        if (tries < 10)
        {

            int[,] aux = layout.Clone() as int[,]; // Copy of matrix in case of missposition
            bool error = false; // 
            bool validFlag = false; // 
            int flipsCount = 0;
            int[] ValidPosotions;
            int triesCount = 0;
            // While it could not place the room and it has flipped less than 2 times
            while (!validFlag && flipsCount < 2)
            {
                GameObject rooms = null;
                ValidPosotions = room.buildRoomMatrix(true); // First one to insert
                validFlag = true;
                while ((triesCount <= 5 && !error) && ValidPosotions[0] != 99)
                {
                    // We place the room checking for the other tiles that may be filled
                    int i = coordX + ValidPosotions[0];
                    int j = coordY + ValidPosotions[1];
                    if (i < layout.GetLength(0) && i > 0 && j < layout.GetLength(1) && j > 0)
                    {
                        if (layout[i, j] <= 0)
                        {
                            error = false;
                            if (layout[i, j] < 0)
                            {
                                if (i + 1 <= mapSize - 1)
                                    if (layout[i + 1, j] == room.Id)
                                        error = true;
                                if (i - 1 >= 0)
                                    if (layout[i - 1, j] == room.Id)
                                        error = true;
                                if (j + 1 <= mapSize - 1)
                                    if (layout[i, j + 1] == room.Id)
                                        error = true;
                                if (j - 1 >= 0)
                                    if (layout[i, j - 1] == room.Id)
                                        error = true;
                            }
                            if (rooms == null && ValidPosotions[4] == 1) // si ha conseguido insertar todas las casillas en la matriz, crea la sala en el juego en dicha posición
                            {
                                rooms = Instantiate(this.rooms[room.Id ], new Vector3(room.OffsetX[flipsCount] + (11 * i), room.OffsetY[flipsCount] + (11 * -j), 0), Quaternion.identity); // Hay que fundirse este offset
                                rooms.name = "Room " + room.Id;
                                rooms.transform.parent = gameObject.transform;
                            }
                            layout[i, j] = room.Id;
                            ValidPosotions = room.buildRoomMatrix(false, new int[] { ValidPosotions[2], ValidPosotions[3] });

                            validFlag = true;
                        }
                        else
                        {
                            error = true;
                        }

                    }
                    else
                    {
                        error = true;
                    }
                    if (error) // Si hay error, destruye el el objeto, devuelve la matriz a su estado anterior, aumenta los intentos y la sala auxiliar
                    {
                        Destroy(rooms);
                        layout = aux.Clone() as int[,];
                        validFlag = false;
                        triesCount++;
                        room.Shape = auxRoom.Shape.Clone() as int[,];
                    }
                }
                room.Shape = auxRoom.Shape.Clone() as int[,];
                triesCount = 0;
                error = false;
                room.flipRoom(); // Flips Room
                auxRoom.flipRoom(); // Flips Aux Room
                flipsCount+=2;
            }
            if (validFlag) // Si consigue colocar la sala, aumenta el contador total y resetea los fallos
            {
                roomNum++;
                failsCount = 0;
            }
            else
                failsCount++;
        }
    }
    /// <summary>
    /// Inserts first room (Room0Start) into the matrix
    /// Works like InsertRoom()
    /// </summary>
    /// <param name="room"> Room Prefab to insert </param>
    public void InsertFirstRoom(Room room)
    {
        roomNum++;
        int[,] firstRoom = room.Shape.Clone() as int[,];
        int posicionX = Random.Range(0, mapSize);
        int posicionY = Random.Range(0, mapSize);
        int[,] aux = layout.Clone() as int[,];
        int i = posicionX - 1;
        int j = posicionY - 1;
        bool error = false;
        bool placedSuccess = false;
        
        while (!placedSuccess)
        {
            placedSuccess = true;
            while (i < (posicionX + firstRoom.GetLength(0) - 1) && !error)
            {
                while (j < (posicionY + firstRoom.GetLength(1) - 1) && !error)
                {
                    if (i >= 0 && i < layout.GetLength(0) && j >= 0 && j < layout.GetLength(1))
                    {
                        if (layout[i, j] <= 0)
                        {

                            layout[i, j] = firstRoom[i - posicionX + 1, j - posicionY + 1];

                            if (firstRoom[i - posicionX + 1, j - posicionY + 1] > 0)
                            {
                                GameObject newRoom = Instantiate(rooms[0], new Vector3(0, 0, 0), Quaternion.identity); // First room on 0,0
                                newRoom.name = "StartingRoom";
                                newRoom.transform.parent = gameObject.transform;
                            }
                        }
                        else if (firstRoom[i - posicionX + 1, j - posicionY + 1] > 0)
                        {
                            error = true;
                            layout = aux.Clone() as int[,];
                            placedSuccess = false;
                        }
                    }
                    else
                    {
                        if (firstRoom[i - posicionX + 1, j - posicionY + 1] > 0)
                        {
                            error = true;
                            layout = aux.Clone() as int[,];
                            placedSuccess = false;
                        }
                    }
                    j++;
                }
                j = posicionY - 1;
                i++;
            }
        }
    }

    /// <summary>
    /// Clears the negatives that the matrix uses to build doors
    /// </summary>
    public void ClearNegatives()
    {
        for (int i = 0; i < layout.GetLength(0); i++)
        {
            for (int j = 0; j < layout.GetLength(1); j++)
            {
                if (layout[i, j] < 0)
                    layout[i, j] = 0;
            }
        }
    }

    /// <summary>
    /// Builds a mesh to link doors between rooms
    /// </summary>
    public void BuildDoorMesh()
    {
        for (int i = 0; i < layout.GetLength(0); i++)
        {
            for (int j = 0; j < layout.GetLength(1); j++)
            {
                if (layout[i, j] > 0)
                {
                    if (i - 1 >= 0 && layout[i - 1, j] <= 0)
                        layout[i - 1, j] += -1;
                    if (i + 1 < layout.GetLength(0) && layout[i + 1, j] <= 0)
                        layout[i + 1, j] += -1;
                    if (j - 1 >= 0 && layout[i, j - 1] <= 0)
                        layout[i, j - 1] += -1;
                    if (j + 1 < layout.GetLength(1) && layout[i, j + 1] <= 0)
                        layout[i, j + 1] += -1;
                }
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        roomNum = 0;
        failsCount = 0; // Fail counter for this matrix
        layout = new int[mapSize, mapSize]; // Space available ingame
        while (RoomNum < maxRooms)
        {
            roomNum = 0;
            failsCount = 0;
            emptyFloor(); // Empties Floor in case is some leftover
            Room startRoom = new Room(1); // Inserts first room // this works because room0 and room1 have same shape
            InsertFirstRoom(startRoom);
            Room elseRooms; // Remaining rooms to insert
            while (RoomNum < maxRooms && failsCount < 50) // Does 50 tries
            {
                if (roomNum < 2)
                {
                    elseRooms = new Room(Random.Range(2, 6));
                }
                else if (roomNum == maxRooms - 1)
                {
                    elseRooms = new Room(6);
                }
                else
                {
                    elseRooms = new Room(Random.Range(2, 6));
                }
                InsertRoom(elseRooms);
                // ClearNegatives();
                // BuildDoorMesh();
            }
            if (failsCount == 50) // When 50 tries fail, Delete all rooms
            {
                foreach (Transform child in transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }
        // StartCoroutine(loadAll()); // Covers Camera for 2s while loading everything
    }
    /// <summary>
    /// Covers Camera for 2s while loading everything
    /// </summary>
    /// <returns></returns>
    IEnumerator loadAll()
    {
        GameObject.Find("Player").transform.Find("MainCamera").GetComponent<Camera>().enabled = false;
        GameObject.Find("HUD").GetComponent<Canvas>().enabled = false;
        yield return new WaitForSeconds(2);
        GameObject.Find("Player").transform.Find("MainCamera").GetComponent<Camera>().enabled = true;
        GameObject.Find("HUD").GetComponent<Canvas>().enabled = true;
    }

}


/// <summary>
/// Room class
/// </summary>
public class Room
{
    private int id;         // Room Id, this links with one of the Prefab Model
    private int[,] shape;   // Shape of the room: square, L-shape, and so on
    private int[] offsetX;  // Offset in X-axis
    private int[] offsetY;  // Offset in Y-axis

    /// <summary>
    /// Getters and Setters
    /// </summary>
    public int[,] Shape { get => shape; set => shape = value; }
    public int Id { get => id; set => id = value; }
    public int[] OffsetX { get => offsetX; set => offsetX = value; }
    public int[] OffsetY { get => offsetY; set => offsetY = value; }

    /// <summary>
    /// Room Constructor, depending of Room ID will build a different room.
    /// </summary>
    /// <param name="id"> Room ID </param>
    public Room(int id)
    {
        this.id = id;

        // This chooses
        switch (id)
        {
            case 0: // Room0Start
                {
                    shape = new int[,] {    {   0,  0,  0,  0,  0   },
                                            {   0,  id, id, id, 0   },
                                            {   -1, id, id, id, -1  },
                                            {   0,  id, id, id, 0   },
                                            {   0,  0,  -1, 0,  0   }
                    };
                    offsetX = new int[] { 0, 0, 0, 0 };
                    offsetY = new int[] { 0, 0, 0, 0 };
                    break;
                }
            case 1: //  Room1 & Room5
            case 5:
                {
                    shape = new int[,] {    {   0,  0,  -1,  0,  0   },
                                            {   0,  id, id, id, 0   },
                                            {   -1, id, id, id, -1  },
                                            {   0,  id, id, id, 0   },
                                            {   0,  0,  -1, 0,  0   }
                    };
                    offsetX = new int[] { 0, 0, 0, 0 };
                    offsetY = new int[] { 0, 0, 0, 0 };
                    break;
                }
            case 2: // Room2
                {
                    shape = new int[,] {    {   0,  0,  0,  0,  0,  0,  0   },
                                            {   0,  id, id, id, id, id, 0   },
                                            {   -1, id, id, id, id, id, 0   },
                                            {   0,  id, id, id, id, id, 0   },
                                            {   0,  0,  0,  0,  -1, 0,  0   }
                    };
                    offsetX = new int[] { 0, 0, 0, 0 };
                    offsetY = new int[] { 0, 0, 0, 0 };
                    break;
                }
            case 3: // Room3
                {
                    shape = new int[,] {    {   0,  0,  0,  0,  0,  0   },
                                            {   0,  id, id, 0,  0,  0   },
                                            {   0,  id, id, -1, 0,  0   },
                                            {   0,  id, id, 0,  0,  0   },
                                            {   0,  id, id, id, id, 0   },
                                            {   0,  id, id, id, id, -1  },
                                            {   0,  id, id, id, id, 0   },
                                            {   0,  0,  0,  0,  0,  0   }
                    };
                    offsetX = new int[] { 0, 0, 0, 0 };
                    offsetY = new int[] { 0, 0, 0, 0 };
                    break;
                }
            case 4: // Room4 
                {
                    shape = new int[,] {    {   0,  -1, 0   },
                                            {   0,  id, 0   },
                                            {   0,  id, 0   },
                                            {   0,  id, 0   },
                                            {   0,  id, 0   },
                                            {   0,  -1, 0   }
                    };
                    offsetX = new int[] { 0, 0, 0, 0 };
                    offsetY = new int[] { 0, 0, 0, 0 };
                    break;
                }
            case 6: // BossRoom
                {
                    shape = new int[,] {    {   0,  0,  0,  0,  0   },
                                            {   0,  id, id, id, 0   },
                                            {   0,  id, id, id, 0   },
                                            {   0,  0,  id, 0,  0   },
                                            {   0,  0,  id, 0,  0   },
                                            {   0,  0,  -1, 0,  0   }

                    };
                    offsetX = new int[] { 0, 0, 0, 0 };
                    offsetY = new int[] { 0, 0, 0, 0 };
                    break;
                }
        }
        // getRoomDoors(); // This will track the doors between rooms
    }

    /// <summary>
    /// This will build a mesh to track where the doors are 
    /// so other parts can keep track and place things accordingly
    /// </summary>
    /// 
    /* private void getRoomDoors()
    {

        int[,] aux = new int[shape.GetLength(0), shape.GetLength(1)];
        for (int i = 1; i <= shape.GetLength(0); i++)
        {
            for (int j = 1; j <= shape.GetLength(1); j++)
            {
                aux[i, j] = shape[i - 1, j - 1];
            }
        }
        shape = aux;

        for (int i = 0; i < shape.GetLength(0); i++)
        {
            for (int j = 0; j < shape.GetLength(1); j++)
            {
                if (shape[i, j] == id)
                {
                    if (i - 1 >= 0 && shape[i - 1, j] != id)
                        shape[i - 1, j] += -1;
                    if (i + 1 < shape.GetLength(0) && shape[i + 1, j] != id)
                        shape[i + 1, j] += -1;
                    if (j - 1 >= 0 && shape[i, j - 1] != id)
                        shape[i, j - 1] += -1;
                    if (j + 1 < shape.GetLength(1) && shape[i, j + 1] != id)
                        shape[i, j + 1] += -1;
                }

            }
        }
    }*/

    /// <summary>
    /// Transpose a Room (Flip it, but with matrix)
    /// </summary>
    public void flipRoom()
    {
        int[,] ret = new int[shape.GetLength(0), shape.GetLength(1)];

        for (int i = 0; i < shape.GetLength(0); i++)
        {
            for (int j = 0; j < shape.GetLength(1); j++)
            {
                ret[i, j] = shape[i, shape.GetLength(1) - 1 - j];
            }
        }
        shape = ret.Clone() as int[,];
    }


    /// <summary>
    /// Method that keeps track of the pivot of a room to check for a tile in the matrix
    /// </summary>
    /// <param name="first"> Flag for first room </param>
    /// <param name="pos">pivot position </param>
    /// <returns> position from first room </returns>
    public int[] buildRoomMatrix(bool first, int[] pos = null)
    {
        int[] posTotal = new int[5];
        int posX = Random.Range(0, Shape.GetLength(0));
        int posY = Random.Range(0, Shape.GetLength(1));
        if (first)
        {
            while (shape[posX, posY] != -1)
            {
                posX = Random.Range(0, Shape.GetLength(0));
                posY = Random.Range(0, Shape.GetLength(1));
            }

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i + posX < shape.GetLength(0) && i + posX > 0 && j + posY < shape.GetLength(1) && j + posY > 0)
                        if (shape[posX + i, posY + j] == id && (i == 0 || j == 0) && first)
                        {
                            posX += i;
                            posY += j;
                            shape[posX, posY] = 0;
                            posTotal[0] = 0;
                            posTotal[1] = 0;
                            posTotal[2] = posX;
                            posTotal[3] = posY;
                            posTotal[4] = 1; // Boleana que controla si es la primera de todas o no.
                            first = false;


                        }
                }
            }

            //CON ESTO COMPROBAMOS QUE ESTÁ INSERTANDO LA SUPERIOR O NO, PARA INSERTAR EL OBJETO O NO.
            for (int i = 0; i < shape.GetLength(0); i++)
            {
                for (int j = 0; j < shape.GetLength(1); j++)
                {
                    if (shape[i, j] == id && (i != posX || j != posY) && ((i < posX) || ((j < posY) && (i == posX))))
                    {
                        posTotal[4] = 0;
                    }
                }
            }

        }
        else
        {
            posX = pos[0];
            posY = pos[1];

            for (int i = 1; i < shape.GetLength(0); i++)
            {
                for (int j = 1; j < shape.GetLength(1); j++)
                {
                    if (shape[i, j] == id && !first)
                    {
                        posTotal[0] = i - posX;
                        posTotal[1] = j - posY;
                        posTotal[2] = posX;
                        posTotal[3] = posY;
                        posTotal[4] = 1;
                        shape[i, j] = 0;
                        first = true;
                    }
                }
            }
            if (posTotal[0] == 0 && posTotal[1] == 0)
                posTotal[0] = 99;

        }
        return posTotal;
    }
}