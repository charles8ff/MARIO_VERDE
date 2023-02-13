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
    int[,] layout = new int[10, 10]; // Layout attempted to fit into the matrix of size = mapSize
    int roomNum = 0; // Room number per floor
    int failsCount = 0; // Fail counter
   
    int playerLvl = 1; // Player  level (unused)



    /// Getters and Setters
    public int RoomNum { get => roomNum; set => roomNum = value; }
    public int[,] Layout { get => layout; set => layout = value; }
    public int PlayerLvl { get => playerLvl; set => playerLvl = value; }


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
    public void InsertByForce(Room room)
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
            while (layout[coordY, coordX] != -1)
            {
                coordX = Random.Range(0, mapSize);
                coordY = Random.Range(0, mapSize);
            }
            // Checks if room has same index than next room
            if (coordY + 1 <= mapSize - 1)
                if (layout[coordY + 1, coordX] == room.Id)
                    validPosFlag = false;
            if (coordY - 1 >= mapSize - 1)
                if (layout[coordY - 1, coordX] == room.Id)
                    validPosFlag = false;
            if (coordX + 1 <= mapSize - 1)
                if (layout[coordY, coordX + 1] == room.Id)
                    validPosFlag = false;
            if (coordX - 1 >= mapSize - 1)
                if (layout[coordY, coordX - 1] == room.Id)
                    validPosFlag = false;
            tries++;
        }

        if (tries < 10)
        {

            int[,] aux = layout.Clone() as int[,]; // Copy of matrix in case of missposition
            bool error = false;
            bool validPos = false;
            int flipsCount = 0;
            int[] BruteForce;
            int triesCount = 0;
            // While it could not place the room and it has flipped less than 2 times
            while (!validPos && flipsCount < 2)
            {
                GameObject rooms = null;
                BruteForce = room.insertarFuerzaBruta(true); //Primera casilla de la sala a insertar
                validPos = true;
                while ((triesCount <= 5 && !error) && BruteForce[0] != 99)
                {
                    //AQUI COLOCA LA SALA EN LAS CASILLAS COMPROBANDO QUE NO COLISIONA CON NADA, USANDO LA PRIMERA CASILLA COMO PIVOTE PARA COLOCAR EL RESTO
                    int i = coordY + BruteForce[0];
                    int j = coordX + BruteForce[1];
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
                            if (rooms == null && BruteForce[4] == 1) //si ha conseguido insertar todas las casillas en la matriz, crea la sala en el juego en dicha posición
                            {
                                rooms = Instantiate(this.rooms[room.Id - 1], new Vector3(room.OffsetX[flipsCount] + (22 * j), room.OffsetY[flipsCount] + (22 * -i), 0), Quaternion.Euler(0, 90 * flipsCount, 0));
                                rooms.name = "Room " + room.Id;
                                rooms.transform.parent = gameObject.transform;
                            }
                            layout[i, j] = room.Id;
                            BruteForce = room.insertarFuerzaBruta(false, new int[] { BruteForce[2], BruteForce[3] });

                            validPos = true;
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
                        validPos = false;
                        triesCount++;
                        room.Shape = auxRoom.Shape.Clone() as int[,];
                        // FuerzaBruta = sala.insertarFuerzaBruta(true);
                    }
                }


                room.Shape = auxRoom.Shape.Clone() as int[,];
                triesCount = 0;
                error = false;
                room.flipRoom(); // Flips Room
                auxRoom.flipRoom(); // Flips Aux Room
                flipsCount+=2;
            }
            if (validPos) // Si consigue colocar la sala, aumenta el contador total y resetea los fallos
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
    /// Works like InsertByForce()
    /// </summary>
    /// <param name="room"> Room Prefab to insert </param>
    public void InsertFirstRoom(Room room)
    {
        roomNum++;
        int[,] firstRoom = room.Shape.Clone() as int[,];
        int posicionX = Random.Range(0, mapSize);
        int posicionY = Random.Range(0, mapSize);
        int[,] aux = layout.Clone() as int[,];
        int i = posicionY - 1;
        int j = posicionX - 1;
        bool error = false;
        bool placedSuccess = false;
        int flipTries = 0;
        while (!placedSuccess && flipTries < 1)
        {
            placedSuccess = true;
            while (i < (posicionY + firstRoom.GetLength(0) - 1) && !error)
            {
                while (j < (posicionX + firstRoom.GetLength(1) - 1) && !error)
                {
                    if (i >= 0 && i < layout.GetLength(0) && j >= 0 && j < layout.GetLength(1))
                    {
                        if (layout[i, j] <= 0)
                        {

                            layout[i, j] = firstRoom[i - posicionY + 1, j - posicionX + 1];

                            if (firstRoom[i - posicionY + 1, j - posicionX + 1] > 0)
                            {
                                GameObject newRoom = Instantiate(rooms[0], new Vector3((22 * j), (22 * -i), 0), Quaternion.identity);
                                newRoom.name = "Start";
                                newRoom.transform.parent = gameObject.transform;
                            }
                        }
                        else if (firstRoom[i - posicionY + 1, j - posicionX + 1] > 0)
                        {
                            error = true;
                            layout = aux.Clone() as int[,];
                            placedSuccess = false;
                        }
                    }
                    else
                    {
                        if (firstRoom[i - posicionY + 1, j - posicionX + 1] > 0)
                        {
                            error = true;
                            layout = aux.Clone() as int[,];
                            placedSuccess = false;
                        }
                    }
                    j++;
                }
                j = posicionX - 1;
                i++;
            }

            room.flipRoom();
            firstRoom = room.Shape.Clone() as int[,];
            flipTries++;
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
        roomNum = 
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
                    elseRooms = new Room(Random.Range(2, 7));
                }
                else if (roomNum == maxRooms - 1)
                {
                    elseRooms = new Room(7);
                }
                else
                {
                    elseRooms = new Room(Random.Range(2, 7));
                }
                InsertByForce(elseRooms);
                ClearNegatives();
                BuildDoorMesh();
            }
            if (failsCount == 50)//Si falla 50 veces borro todas las salas 
            {
                foreach (Transform child in transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }
        // StartCoroutine(cargarCompleto());//Tapa la camara un par de segundos mientras que cargan los resources
    }
    /// <summary>
    /// Desabilita la camara durante dos segundos
    /// </summary>
    /// <returns></returns>
    IEnumerator cargarCompleto()
    {
        GameObject.Find("Jugador").transform.Find("MainCamera").GetComponent<Camera>().enabled = false;
        GameObject.Find("HUD").GetComponent<Canvas>().enabled = false;
        yield return new WaitForSeconds(2);
        GameObject.Find("Jugador").transform.Find("MainCamera").GetComponent<Camera>().enabled = true;
        GameObject.Find("HUD").GetComponent<Canvas>().enabled = true;
    }

 /// <summary>
 /// Cuando terminas un nivel suma uno al contador, borra todas las salas y vuelve a comenzar
 /// </summary>
 /// 
 /*
    public void Lyoko()
    {
     
        playerLvl++;
        // DatosMuerte.Nivel = nivel;
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        Start();
    }
 */


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
            case 0: // Room0Start & Room1 & Room5
            case 1:
            case 5:
                {
                    shape = new int[,] { { id } };
                    offsetX = new int[] { 0, 0, 0, 0 };
                    offsetY = new int[] { 0, 0, 0, 0 };
                    break;
                }
            case 2: // Room2
                {
                    shape = new int[,] { { id, id } };
                    offsetX = new int[] { 0, 0, 0, 0 };
                    offsetY = new int[] { 0, 0, 0, 0 };
                    break;
                }
            case 3: // Room3
                {
                    shape = new int[,] { { id, 0 }, { id, id } };
                    offsetX = new int[] { 0, 0, 0, 0 };
                    offsetY = new int[] { 0, 0, 0, 0 };
                    break;
                }
            case 4: // Room4 
                {
                    shape = new int[,] { { id }, { id } };
                    offsetX = new int[] { 0, 0, 0, 0 };
                    offsetY = new int[] { 0, 0, 0, 0 };
                    break;
                }
            case 7: // BossRoom
                {
                    shape = new int[,] { { id }, { id } };
                    offsetX = new int[] { 0, 0, 0, 0 };
                    offsetY = new int[] { 0, 0, 0, 0 };
                    break;
                }
        }
        getRoomDoors(); // This will track the doors between rooms
    }

    /// <summary>
    /// This will build a mesh to track where the doors are 
    /// so other parts can keep track and place things accordingly
    /// </summary>
    private void getRoomDoors()
    {

        int[,] aux = new int[shape.GetLength(0) + 2, shape.GetLength(1) + 2];
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
    }

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
    /// Metodo que controla el pivote de la sala para controlar la casilla que se debe insertar de cada sala en la matriz
    /// </summary>
    /// <param name="primero">Controla si es la primera casilla</param>
    /// <param name="pos">posición de pivote</param>
    /// <returns>retorna la posición respecto a la primera casilla que ha de colocarse</returns>
    public int[] insertarFuerzaBruta(bool primero, int[] pos = null)
    {
        int[] posTotal = new int[5];
        int posX = Random.Range(0, Shape.GetLength(1));
        int posY = Random.Range(0, Shape.GetLength(0));
        if (primero)
        {
            while (shape[posY, posX] != -1)
            {
                posX = Random.Range(0, Shape.GetLength(1));
                posY = Random.Range(0, Shape.GetLength(0));
            }

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i + posY < shape.GetLength(0) && i + posY > 0 && j + posX < shape.GetLength(1) && j + posX > 0)
                        if (shape[posY + i, posX + j] == id && (i == 0 || j == 0) && primero)
                        {
                            posY += i;
                            posX += j;
                            shape[posY, posX] = 0;
                            posTotal[0] = 0;
                            posTotal[1] = 0;
                            posTotal[2] = posY;
                            posTotal[3] = posX;
                            posTotal[4] = 1; //Boleana que controla si es la primera de todas o no.
                            primero = false;


                        }
                }
            }

            //CON ESTO COMPROBAMOS QUE ESTÁ INSERTANDO LA SUPERIOR O NO, PARA INSERTAR EL OBJETO O NO.
            for (int i = 0; i < shape.GetLength(0); i++)
            {
                for (int j = 0; j < shape.GetLength(1); j++)
                {
                    if (shape[i, j] == id && (i != posY || j != posX) && ((i < posY) || ((j < posX) && (i == posY))))
                    {
                        posTotal[4] = 0;
                    }
                }
            }

        }
        else
        {
            posX = pos[1];
            posY = pos[0];

            for (int i = 1; i < shape.GetLength(0); i++)
            {
                for (int j = 1; j < shape.GetLength(1); j++)
                {
                    if (shape[i, j] == id && !primero)
                    {
                        posTotal[0] = i - posY;
                        posTotal[1] = j - posX;
                        posTotal[2] = posY;
                        posTotal[3] = posX;
                        posTotal[4] = 1;
                        shape[i, j] = 0;
                        primero = true;
                    }
                }
            }
            if (posTotal[0] == 0 && posTotal[1] == 0)
                posTotal[0] = 99;

        }
        return posTotal;
    }
}