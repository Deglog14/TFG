using UnityEngine;

public class SystemRespawn : MonoBehaviour
{
    public Transform posRespawn;
    public Transform posPlayer;
    public Vector3 posPuntoControl;

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
