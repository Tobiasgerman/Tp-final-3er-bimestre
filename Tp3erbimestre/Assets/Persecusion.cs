using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Persecusion : MonoBehaviour
{
    [SerializeField] Transform[] lugares;
    int destinoActual = 0;
    [SerializeField] Transform jugador;
    [SerializeField] float rangoVision = 10f;
    [SerializeField] float anguloVision = 60f;
    [SerializeField] float distanciaPersecucion = 1.5f;
    [SerializeField] Transform ojosNPC;
    [SerializeField] float tiempoPerdida = 2f;
    float tiempoSinVer = 0f;
    bool persiguiendo = false;
    NavMeshAgent agente;
    Animator anim;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (lugares.Length > 0)
        {
            agente.SetDestination(lugares[destinoActual].position);
        }
    }

    void Update()
    {
        if (persiguiendo)
        {
            PerseguirJugador();
        }
        else
        {
            Patrullar();
            DetectarJugador();
        }

        float velocidad = agente.velocity.magnitude;
        anim.SetFloat("Speed", velocidad);
    }

    void Patrullar()
    {
        if (lugares.Length == 0) return;

        if (!agente.pathPending && agente.remainingDistance < 0.5f)
        {
            destinoActual = (destinoActual + 1) % lugares.Length;
            agente.SetDestination(lugares[destinoActual].position);
        }
    }

    void DetectarJugador()
    {
        Vector3 direccion = (jugador.position - ojosNPC.position).normalized;
        float distancia = Vector3.Distance(jugador.position, ojosNPC.position);

        if (distancia <= rangoVision)
        {
            float angulo = Vector3.Angle(ojosNPC.forward, direccion);
            if (angulo < anguloVision)
            {
                if (Physics.Raycast(ojosNPC.position, direccion, out RaycastHit hit, rangoVision))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        persiguiendo = true;
                        // tiempoSinVer = 0f;
                    }
                }
            }
        }
    }

    void PerseguirJugador()
    {
        agente.SetDestination(jugador.position);

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= distanciaPersecucion)
        {
            SceneManager.LoadScene("Escenaposterior");
        }

        Vector3 direccion = (jugador.position - ojosNPC.position).normalized;
        if (Physics.Raycast(ojosNPC.position, direccion, out RaycastHit hit, rangoVision))
        {
            if (hit.collider.CompareTag("Player"))
            {
                tiempoSinVer = 0f;
                return;
            }
        }

        tiempoSinVer += Time.deltaTime;
        if (tiempoSinVer >= tiempoPerdida)
        {
            persiguiendo = false;
            tiempoSinVer = 0f;

            if (lugares.Length > 0)
            {
                Transform puntoCercano = lugares[0];
                float minDistancia = Vector3.Distance(transform.position, lugares[0].position);

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
