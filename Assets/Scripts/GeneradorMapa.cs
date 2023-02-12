using UnityEngine;
using System.Collections;
#if UNITY_EDITOR

using UnityEditor.AI;
#endif


public class GeneradorMapa : MonoBehaviour
{
    public int SalasMaximas;//Numero de salas maximas posibles
    public int MapSize;//Tamaño de la matriz
    public GameObject[] habitaciones;//Referencia a las salas
    int[,] forma = new int[10, 10];//forma es el mapa donde se colocan las salas es una matriz bidimensional
    int numSalas = 0;//Es el numero de salas
    int fallos = 0;//Intentos de colocacion
   
    int nivel = 1; //Nivel del jugador
    int[] mejoras = new int[3]; //Array que contabiliza las mejoras obtenidas por el jugador


    ///Getter and Setters
    public int NumSalas { get => numSalas; set => numSalas = value; }
    public int[,] Forma { get => forma; set => forma = value; }
    public int Nivel { get => nivel; set => nivel = value; }
    public int[] Mejoras { get => mejoras; set => mejoras = value; }

    /// <summary>
    /// Vacia el tablero
    /// </summary>
    public void vaciarTablero()
    {
        for (int i = 0; i < forma.GetLength(0); i++)
        {
            for (int j = 0; j < forma.GetLength(1); j++)
            {
                forma[i, j] = 0;
            }

        }
    }

    /// <summary>
    /// Intenta insertar en el tablero una sala mediante aleatoriedad
    /// </summary>
    /// <param name="sala">objeto que se va a intentar insertar</param>
    public void InsertarEnTableroFB(Sala sala)
    {
        Sala salaAux = new Sala(sala.Id); //Sala auxiliar que se guarda por si necesitase rotarse
        int posicionX = 0; //posX de la matriz
        int posicionY = 0; //posY de la matriz
        bool valido = false; //Booleana que calcula si es valida la colocación o no
        int imposible = 0; //Intentos de insertar
        while (!valido && imposible < 10) //Mientras que no sea valido y se haya intentado menos de 10 veces se seguirá intentando
        {
            //Encontramos una casilla en la matriz que contenga un -1 para empezar
            posicionX = Random.Range(0, MapSize);
            posicionY = Random.Range(0, MapSize);
            valido = true;
            while (forma[posicionY, posicionX] != -1)
            {
                posicionX = Random.Range(0, MapSize);
                posicionY = Random.Range(0, MapSize);
            }
            //Calculamos si la sala colinda con otra del mismo index para descartarla
            if (posicionY + 1 <= MapSize - 1)
                if (forma[posicionY + 1, posicionX] == sala.Id)
                    valido = false;
            if (posicionY - 1 >= MapSize - 1)
                if (forma[posicionY - 1, posicionX] == sala.Id)
                    valido = false;
            if (posicionX + 1 <= MapSize - 1)
                if (forma[posicionY, posicionX + 1] == sala.Id)
                    valido = false;
            if (posicionX - 1 >= MapSize - 1)
                if (forma[posicionY, posicionX - 1] == sala.Id)
                    valido = false;
            imposible++;
        }

        if (imposible < 10)
        {

            int[,] aux = forma.Clone() as int[,]; //Clon de la matriz actual para revertir en caso de mala colocación
            bool error = false;
            bool colocado = false;
            int rotaciones = 0;
            int[] FuerzaBruta;
            int intentos = 0;
            //Mientras que no haya conseguido colocar la sala rotandola al menos 4 veces
            while (!colocado && rotaciones < 4)
            {
                GameObject habitacion = null;
                FuerzaBruta = sala.insertarFuerzaBruta(true); //Primera casilla de la sala a insertar
                colocado = true;
                while ((intentos <= 5 && !error) && FuerzaBruta[0] != 99)
                {
                    //AQUI COLOCA LA SALA EN LAS CASILLAS COMPROBANDO QUE NO COLISIONA CON NADA, USANDO LA PRIMERA CASILLA COMO PIVOTE PARA COLOCAR EL RESTO
                    int i = posicionY + FuerzaBruta[0];
                    int j = posicionX + FuerzaBruta[1];
                    if (i < forma.GetLength(0) && i > 0 && j < forma.GetLength(1) && j > 0)
                    {
                        if (forma[i, j] <= 0)
                        {
                            error = false;
                            if (forma[i, j] < 0)
                            {
                                if (i + 1 <= MapSize - 1)
                                    if (forma[i + 1, j] == sala.Id)
                                        error = true;
                                if (i - 1 >= 0)
                                    if (forma[i - 1, j] == sala.Id)
                                        error = true;
                                if (j + 1 <= MapSize - 1)
                                    if (forma[i, j + 1] == sala.Id)
                                        error = true;
                                if (j - 1 >= 0)
                                    if (forma[i, j - 1] == sala.Id)
                                        error = true;
                            }
                            if (habitacion == null && FuerzaBruta[4] == 1) //si ha conseguido insertar todas las casillas en la matriz, crea la sala en el juego en dicha posición
                            {
                                habitacion = Instantiate(habitaciones[sala.Id - 1], new Vector3(sala.OffsetX[rotaciones] + (22 * j), sala.OffsetZ[rotaciones] + (22 * -i), 0), Quaternion.Euler(0, 90 * rotaciones, 0));
                                habitacion.name = "Sala " + sala.Id;
                                habitacion.transform.parent = gameObject.transform;
                            }
                            forma[i, j] = sala.Id;
                            FuerzaBruta = sala.insertarFuerzaBruta(false, new int[] { FuerzaBruta[2], FuerzaBruta[3] });

                            colocado = true;
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
                    if (error) //Si hay error, destruye el el objeto, devuelve la matriz a su estado anterior, aumenta los intentos y la sala auxiliar
                    {
                        Destroy(habitacion);
                        forma = aux.Clone() as int[,];
                        colocado = false;
                        intentos++;
                        sala.Forma = salaAux.Forma.Clone() as int[,];
                        // FuerzaBruta = sala.insertarFuerzaBruta(true);
                    }
                }


                sala.Forma = salaAux.Forma.Clone() as int[,];
                intentos = 0;
                error = false;
                sala.rotarSala(); //Rota la sala
                sala.rotarSala(); //Rota la sala
                salaAux.rotarSala(); //Rota la sala auxiliar
                salaAux.rotarSala(); //Rota la sala auxiliar
                rotaciones+=2;
            }
            if (colocado) //Si consigue colocar la sala, aumenta el contador total y resetea los fallos
            {
                numSalas++;
                fallos = 0;
            }
            else
                fallos++;
        }
    }
    /// <summary>
    /// Inserta la primera sala dentro de la matriz
    /// El funcionamiento es identico a InsertarEnTableroFB()
    /// </summary>
    /// <param name="sala">Objeto que se va a insertar</param>
    public void InsertarEnTableroInicio(Sala sala)
    {
        numSalas++;
        int[,] fSala = sala.Forma.Clone() as int[,];
        int posicionX = Random.Range(0, MapSize);
        int posicionY = Random.Range(0, MapSize);
        int[,] aux = forma.Clone() as int[,];
        int i = posicionY - 1;
        int j = posicionX - 1;
        bool error = false;
        bool colocado = false;
        int rotaciones = 0;
        while (!colocado && rotaciones < 3)
        {
            colocado = true;
            while (i < (posicionY + fSala.GetLength(0) - 1) && !error)
            {
                while (j < (posicionX + fSala.GetLength(1) - 1) && !error)
                {
                    if (i >= 0 && i < forma.GetLength(0) && j >= 0 && j < forma.GetLength(1))
                    {
                        if (forma[i, j] <= 0)
                        {

                            forma[i, j] = fSala[i - posicionY + 1, j - posicionX + 1];

                            if (fSala[i - posicionY + 1, j - posicionX + 1] > 0)
                            {
                                GameObject habitacion = Instantiate(habitaciones[0], new Vector3((22 * j), 0, (22 * -i)), Quaternion.identity);
                                habitacion.name = "Inicio";
                                habitacion.transform.parent = gameObject.transform;
                            }
                        }
                        else if (fSala[i - posicionY + 1, j - posicionX + 1] > 0)
                        {
                            error = true;
                            forma = aux.Clone() as int[,];
                            colocado = false;
                        }
                    }
                    else
                    {
                        if (fSala[i - posicionY + 1, j - posicionX + 1] > 0)
                        {
                            error = true;
                            forma = aux.Clone() as int[,];
                            colocado = false;
                        }
                    }
                    j++;
                }
                j = posicionX - 1;
                i++;
            }

            sala.rotarSala();
            fSala = sala.Forma.Clone() as int[,];
            rotaciones++;
        }
    }

    /// <summary>
    /// Elimina los negativos, un negativo es los puntos donde puede conectarse a otra sala
    /// </summary>
    public void EliminarNegativos()
    {
        for (int i = 0; i < forma.GetLength(0); i++)
        {
            for (int j = 0; j < forma.GetLength(1); j++)
            {
                if (forma[i, j] < 0)
                    forma[i, j] = 0;
            }
        }
    }

    /// <summary>
    /// Genera los negativos que sirven como conexiones donde colocar otras salas
    /// </summary>
    public void GenerarMallaPuertas()
    {
        for (int i = 0; i < forma.GetLength(0); i++)
        {
            for (int j = 0; j < forma.GetLength(1); j++)
            {
                if (forma[i, j] > 0)
                {
                    if (i - 1 >= 0 && forma[i - 1, j] <= 0)
                        forma[i - 1, j] += -1;
                    if (i + 1 < forma.GetLength(0) && forma[i + 1, j] <= 0)
                        forma[i + 1, j] += -1;
                    if (j - 1 >= 0 && forma[i, j - 1] <= 0)
                        forma[i, j - 1] += -1;
                    if (j + 1 < forma.GetLength(1) && forma[i, j + 1] <= 0)
                        forma[i, j + 1] += -1;
                }

            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        // GameObject.Find("Nivel").GetComponent<UnityEngine.UI.Text>().text = "Nivel "+nivel;//Setea el texto del nivel actual
        // DatosMuerte.Nivel = nivel;//Guarda la variable en registros de estadisticas
        numSalas = 0;//numero de salas
        fallos = 0;//Intento de rellenar la matriz actual
        forma = new int[MapSize, MapSize];//Matriz del mapa de x area
        while (NumSalas < SalasMaximas)
        {
            numSalas = 0;
            fallos = 0;
            vaciarTablero();//Vacia el tablero por si hay datos residuales
            Sala inicio = new Sala(1);//obtiene la sala de inicio a colocar y la coloca
            InsertarEnTableroInicio(inicio);
            Sala resto;//Resto de salas a insertar
            while (NumSalas < SalasMaximas && fallos < 50)//Intenta colocar las salas 50 veces en la matriz, si falla inicia de nuevo
            {
                if (numSalas < 2)
                {
                    resto = new Sala(Random.Range(2, 7));
                }
                else if (numSalas == SalasMaximas - 1)
                {
                    resto = new Sala(7);
                }
                else
                {
                    resto = new Sala(Random.Range(2, 7));
                }
                InsertarEnTableroFB(resto);
                EliminarNegativos();
                GenerarMallaPuertas();
            }
            if (fallos == 50)//Si falla 50 veces borro todas las salas 
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
    public void Lyoko()
    {
     
        nivel++;
        // DatosMuerte.Nivel = nivel;
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        Start();
    }


}


/// <summary>
/// Clase Sala
/// </summary>
public class Sala
{
    private int id;//Id de la sala
    private int[,] forma;//Es la forma de la sala se guarda en una matriz
    private int[] offsetX;//Desviacion que tiene que tomar la sala para colocarse correctamente en el eje X
    private int[] offsetZ;//Desviacion que tiene que tomar la sala para colocarse correctamente en el eje Z

    /// <summary>
    /// Getter and Setters
    /// </summary>
    public int[,] Forma { get => forma; set => forma = value; }
    public int Id { get => id; set => id = value; }
    public int[] OffsetX { get => offsetX; set => offsetX = value; }
    public int[] OffsetZ { get => offsetZ; set => offsetZ = value; }

    /// <summary>
    /// Constructor de la clase sala, dependiendo de su id realizara una sala u otra
    /// </summary>
    /// <param name="id">Id de la sala</param>
    public Sala(int id)
    {
        this.id = id;

        //Dependiendo de la id de la sala se generara una forma u otra.
        switch (id)
        {
            case 1: // Room0Start & Room1 & Room5
            case 5:
            case 6:
                {
                    forma = new int[,] { { id } };
                    offsetX = new int[] { 0, 0, 0, 0 };
                    offsetZ = new int[] { 0, 0, 0, 0 };
                    break;
                }
            case 2: // Room2
                {
                    forma = new int[,] { { id, id } };
                    offsetX = new int[] { 0, 0, 0, 0 };
                    offsetZ = new int[] { 0, 0, 0, 0 };
                    break;
                }
            case 3: // Room3
                {
                    forma = new int[,] { { id, 0 }, { id, id } };
                    offsetX = new int[] { 0, 0, 0, 0 };
                    offsetZ = new int[] { 0, 0, 0, 0 };
                    break;
                }
            case 4:
            case 7: // Room4 & BossRoom
                {
                    forma = new int[,] { { id }, { id } };
                    offsetX = new int[] { 0, 0, 0, 0 };
                    offsetZ = new int[] { 0, 0, 0, 0 };
                    break;
                }
            /*
            case 5: 
                {
                    forma = new int[,] { { id, id, id }, { 0, id, 0 } };
                    offsetX = new int[] { 22, -11, 0, 11 };
                    offsetZ = new int[] { -11, -22, -11, -22 };
                    break;
                }
            
            case 6: //sala O
                {
                    forma = new int[,] { { id, id, id }, { id, 0, id }, { id, id, id } };
                    offsetX = new int[] { 22, 22, 22, 22 };
                    offsetZ = new int[] { -22, -22, -22, -22 };
                    break;
                }
            case 7: //sala U
                {
                    forma = new int[,] { { id, 0, id }, { id, 0, id }, { id, id, id } };
                    offsetX = new int[] { 22, 22, 22, 22 };
                    offsetZ = new int[] { -22, -22, -22, -22 };
                    break;
                }
            case 8: // Sala X
                {
                    forma = new int[,] { { 0, id, 0 }, { id, id, id }, { 0, id, 0 } };
                    offsetX = new int[] { 0, 0, 0, 0 };
                    offsetZ = new int[] { -22, -22, -22, -22 };
                    break;
                } */

        }
        generarMallaPuertas(); //Genera la malla de puerta de esa sala
    }

    /// <summary>
    /// Busca la sala que se ha generado en el constructor y genera alrededor de la sala una malla (-1) que informara
    /// a otras funciones que ahi se puede generar una puerta o no.
    /// </summary>
    private void generarMallaPuertas()
    {

        int[,] aux = new int[forma.GetLength(0) + 2, forma.GetLength(1) + 2];
        for (int i = 1; i <= forma.GetLength(0); i++)
        {
            for (int j = 1; j <= forma.GetLength(1); j++)
            {
                aux[i, j] = forma[i - 1, j - 1];
            }
        }
        forma = aux;

        for (int i = 0; i < forma.GetLength(0); i++)
        {
            for (int j = 0; j < forma.GetLength(1); j++)
            {
                if (forma[i, j] == id)
                {
                    if (i - 1 >= 0 && forma[i - 1, j] != id)
                        forma[i - 1, j] += -1;
                    if (i + 1 < forma.GetLength(0) && forma[i + 1, j] != id)
                        forma[i + 1, j] += -1;
                    if (j - 1 >= 0 && forma[i, j - 1] != id)
                        forma[i, j - 1] += -1;
                    if (j + 1 < forma.GetLength(1) && forma[i, j + 1] != id)
                        forma[i, j + 1] += -1;
                }

            }
        }
    }

    /// <summary>
    /// Rota las salas, transponiendo filas por columnas
    /// </summary>
    public void rotarSala()
    {
        int[,] ret = new int[forma.GetLength(1), forma.GetLength(0)];

        for (int j = ret.GetLength(1) - 1; j >= 0; j--)
        {
            for (int i = 0; i < ret.GetLength(0); ++i)
            {
                ret[i, j] = forma[ret.GetLength(1) - (j + 1), i];
            }
        }
        forma = ret.Clone() as int[,];
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
        int posX = Random.Range(0, Forma.GetLength(1));
        int posY = Random.Range(0, Forma.GetLength(0));
        if (primero)
        {
            while (forma[posY, posX] != -1)
            {
                posX = Random.Range(0, Forma.GetLength(1));
                posY = Random.Range(0, Forma.GetLength(0));
            }

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i + posY < forma.GetLength(0) && i + posY > 0 && j + posX < forma.GetLength(1) && j + posX > 0)
                        if (forma[posY + i, posX + j] == id && (i == 0 || j == 0) && primero)
                        {
                            posY += i;
                            posX += j;
                            forma[posY, posX] = 0;
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
            for (int i = 0; i < forma.GetLength(0); i++)
            {
                for (int j = 0; j < forma.GetLength(1); j++)
                {
                    if (forma[i, j] == id && (i != posY || j != posX) && ((i < posY) || ((j < posX) && (i == posY))))
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

            for (int i = 1; i < forma.GetLength(0); i++)
            {
                for (int j = 1; j < forma.GetLength(1); j++)
                {
                    if (forma[i, j] == id && !primero)
                    {
                        posTotal[0] = i - posY;
                        posTotal[1] = j - posX;
                        posTotal[2] = posY;
                        posTotal[3] = posX;
                        posTotal[4] = 1;
                        forma[i, j] = 0;
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