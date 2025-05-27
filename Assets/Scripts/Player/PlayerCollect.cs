using System;
using UnityEngine;

public class PlayerCollect 
{
    [Header("Configuración de Colleccionables")]
    public int ActualCacahuetes; //Cacahuetes Actuales
    public int ActualLLaves; //LLaves Actuales


    public event Action<int> MeCambiaCacahuete; // Evento para cambios en coleciionable
    public event Action<int> MeCambiaLLave; // Evento para cambios en coleciionable

    public PlayerCollect(int actualCacahuetes=0, int actualLLaves=0)
    {
        ActualCacahuetes = actualCacahuetes;
        ActualLLaves = actualLLaves;
        MeCambiaCacahuete?.Invoke(ActualCacahuetes);
        MeCambiaLLave?.Invoke(ActualLLaves);
    }

    public void Recoger(int CantidadRecogida, string NombreCollecionable)
    {
        if (CantidadRecogida<=0)
        {
            return;
        }
        if (NombreCollecionable == "Cacahuete")
        {
            ActualCacahuetes += CantidadRecogida;
        }
        if (NombreCollecionable == "LLaves")
        {
            ActualLLaves += CantidadRecogida;
        }
        
        Debug.Log($"Has recogido {CantidadRecogida} cacahuetes. Total: {ActualCacahuetes}");
        Debug.Log($"Has recogido {CantidadRecogida}. Total: {ActualLLaves}");

        // Notificamos el cambio
        MeCambiaCacahuete?.Invoke(ActualCacahuetes);
        MeCambiaLLave?.Invoke(ActualLLaves);
    }

   

    
}
