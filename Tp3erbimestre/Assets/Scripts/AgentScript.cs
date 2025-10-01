using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class AgentScript : MonoBehaviour
{

    [SerializeField]  Transform[] lugares;
    [SerializeField]  int destinoActual = 0;


    [SerializeField]  Transform jugador;
    [SerializeField]  float capacidadVision = 10f;
    [SerializeField]  float anguloVision = 60f;
    [SerializeField]  float distanciaPerseguir = 1.5f;
    [SerializeField]  Transform vistaNPC;
    [SerializeField]  float tiempoPerdida = 2f;
    [SerializeField]  float tiempoSinVer = 0f;
    [SerializeField]  bool persiguiendo = false;
    [SerializeField]  Transform puntoCercano;
    [SerializeField] float minDistancia;
    [SerializeField]  NavMeshAgent agente;
    [SerializeField]  Animator anim;

    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        if (lugares.Length > 0)
        {
            // El NPC comienza en el primer punto 
            agente.SetDestination(lugares[destinoActual].position);
        }
    }

    private void Update()
    {
        
        anim.SetFloat("Speed", agente.velocity.magnitude);

        if (persiguiendo)
        {
            PerseguirJugador();
        }
        else
        {
            Patrullar();
            DetectarJugador();
        }
    }

    void Patrullar()
    {
        if (lugares.Length == 0) return;

        // Cambia al siguiente punto de una vez que llega al actual
        if (!agente.pathPending && agente.remainingDistance < 0.5f)
        {
            destinoActual = (destinoActual + 1) % lugares.Length;
            agente.SetDestination(lugares[destinoActual].position);
        }
    }

    void DetectarJugador()
    {
        Vector3 direccion = (jugador.position - vistaNPC.position).normalized;
        float distancia = Vector3.Distance(jugador.position, vistaNPC.position);

        if (distancia <= capacidadVision)
        {
            float angulo = Vector3.Angle(vistaNPC.forward, direccion);
            if (angulo < anguloVision)
            {
                // Usa  Raycast 
                if (Physics.Raycast(vistaNPC.position, direccion, out RaycastHit hit, capacidadVision))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        // Si el Raycast detecta al jugador, inicia la persecución
                        persiguiendo = true;
                        tiempoSinVer = 0f;
                    }
                }
            }
        }
    }

    void PerseguirJugador()
    {
        // El NPC persigue  al jugador
        agente.SetDestination(jugador.position);

        float distancia = Vector3.Distance(transform.position, jugador.position);
        if (distancia <= distanciaPerseguir)
        {
            // Carga una nueva escena si el NPC alcanza al jugador
            SceneManager.LoadScene("Escenaposterior");
        }

        Vector3 direccion = (jugador.position - vistaNPC.position).normalized;
        if (Physics.Raycast(vistaNPC.position, direccion, out RaycastHit hit, capacidadVision) && hit.collider.CompareTag("Player"))
        {
            // Si el jugador sigue a la vista, el temporizador de  se reinicia
            tiempoSinVer = 0f;
        }
        else
        {
            // Si el jugador no está a la vista, el temporizador de  se activa
            tiempoSinVer += Time.deltaTime;
            if (tiempoSinVer >= tiempoPerdida)
            {
                // Si el temporizador llega al límite, el NPC deja de perseguir y vuelve a patrullar
                persiguiendo = false;
                tiempoSinVer = 0f;

                if (lugares.Length > 0)
                {
                    puntoCercano = lugares[0];
                    minDistancia = Vector3.Distance(transform.position, lugares[0].position);

                    // Busca el punto de patrulla más cercano para volver
                    foreach (Transform p in lugares)
                    {
                        float d = Vector3.Distance(transform.position, p.position);
                        if (d < minDistancia)
                        {
                            minDistancia = d;
                            puntoCercano = p;
                        }
                    }
                    agente.SetDestination(puntoCercano.position);
                }
            }
        }
    }
}