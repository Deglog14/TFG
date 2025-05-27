using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float velocidadMovimiento = 3f;
    [SerializeField] private float distanciaParada = 0.5f;

    [Header("Puntos de Patrulla")]
    [SerializeField] private Transform puntoPatrullaA;
    [SerializeField] private Transform puntoPatrullaB;

    [Header("Detección del Jugador")]
    [SerializeField] private float radioDeteccion = 5f;
    [SerializeField] private LayerMask capaJugador;

    private Transform[] puntosPatrulla;
    private int indicePuntoActual = 0;
    private Transform jugador;
    private bool estaPersiguiendo = false;
    private NavMeshAgent navMeshAgent;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = velocidadMovimiento;

        InicializarPuntosPatrulla();
        IrAlSiguientePuntoDePatrulla();
    }

    void Update()
    {
        if (!estaPersiguiendo)
        {
            Patrullar();
            DetectarJugador();
        }
        else
        {
            PerseguirJugador();
        }
    }

    private void InicializarPuntosPatrulla()
    {
        // Verificar que tenemos al menos 2 puntos asignados
        if (puntoPatrullaA != null && puntoPatrullaB != null)
        {
            puntosPatrulla = new Transform[2];
            puntosPatrulla[0] = puntoPatrullaA;
            puntosPatrulla[1] = puntoPatrullaB;
        }
        else
        {
            Debug.LogError("Asigna ambos puntos de patrulla en el Inspector");
            CrearPuntosRespaldo();
        }
    }

    private void CrearPuntosRespaldo()
    {
        puntosPatrulla = new Transform[2];

        // Crear punto A a la izquierda del enemigo
        GameObject puntoA = new GameObject("PuntoPatrulla_A");
        puntoA.transform.position = transform.position + Vector3.left * 3f;
        puntosPatrulla[0] = puntoA.transform;

        // Crear punto B a la derecha del enemigo
        GameObject puntoB = new GameObject("PuntoPatrulla_B");
        puntoB.transform.position = transform.position + Vector3.right * 3f;
        puntosPatrulla[1] = puntoB.transform;
    }

    private void Patrullar()
    {
        if (navMeshAgent.remainingDistance <= distanciaParada && !navMeshAgent.pathPending)
        {
            indicePuntoActual = (indicePuntoActual + 1) % puntosPatrulla.Length;
            IrAlSiguientePuntoDePatrulla();
        }
    }

    private void IrAlSiguientePuntoDePatrulla()
    {
        if (puntosPatrulla.Length == 0 || puntosPatrulla[indicePuntoActual] == null) return;
        navMeshAgent.destination = puntosPatrulla[indicePuntoActual].position;
    }

    private void DetectarJugador()
    {
        Collider[] colisiones = Physics.OverlapSphere(transform.position, radioDeteccion, capaJugador);
        if (colisiones.Length > 0)
        {
            jugador = colisiones[0].transform;
            estaPersiguiendo = true;
            navMeshAgent.destination = jugador.position;
        }
    }

    private void PerseguirJugador()
    {
        if (jugador == null)
        {
            estaPersiguiendo = false;
            IrAlSiguientePuntoDePatrulla();
            return;
        }

        navMeshAgent.destination = jugador.position;

        if (Vector3.Distance(transform.position, jugador.position) > radioDeteccion)
        {
            estaPersiguiendo = false;
            IrAlSiguientePuntoDePatrulla();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);

        // Dibujar líneas a los puntos de patrulla si están asignados
        if (puntoPatrullaA != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, puntoPatrullaA.position);
        }

        if (puntoPatrullaB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, puntoPatrullaB.position);
        }
    }
}