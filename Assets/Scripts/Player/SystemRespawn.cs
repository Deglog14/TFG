using UnityEngine;

public class SystemRespawn : MonoBehaviour
{
    public Transform posRespawn;// Posición inicial de reaparecimiento (por defecto al iniciar la partida).
    public Transform posPlayer; //Transform del objeto del jugador.
    public Vector3 posPuntoControl;// Punto de control actualizado durante el juego

    public void Start()
    {
        posPlayer.position = posRespawn.position;
    }

    public void Respawn()
    {
        
        if (posPuntoControl != Vector3.zero)
        {
            posPlayer.position = posPuntoControl;
        }
        else
        {
            posPlayer.position = posRespawn.position;
        }
    }
}
