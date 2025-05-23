using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class PlayerVida 
{
    [Header("Configuraci�n de Vida")]
    public int MaxCorazones;     // Corazones m�ximos.
    public int ActualCorazones;     // Corazones actuales.
    public int MaxVidas ;      // Vidas m�ximas
    public int ActualVidas;      // Vidas restantes.




    public event Action MeMuero; // Evento muerto
    public event Action MeHacenDa�o;  // Nuevo evento para cuando recibe da�o
    public event Action MeCuro;        // Nuevo evento para cuando cura
    public event Action <int> MeCambiaVida; // Evento para cambios en vidas/corazones

    public PlayerVida(int corazonesIniciales = 3, int vidasIniciales = 3, int maxCorazonesPermitidos = 3, int maxVidasPermitidas = 99)
    {
        MaxCorazones = maxCorazonesPermitidos;
        MaxVidas = maxVidasPermitidas;
        ActualCorazones = corazonesIniciales;
        ActualVidas = vidasIniciales;
        MeCambiaVida?.Invoke(ActualVidas);
    }

   
   

    public void RecibirDa�o(int CantidadDeDa�o)
    {
        ActualCorazones = Mathf.Max(0, ActualCorazones - CantidadDeDa�o); //Resta los corazones
        MeHacenDa�o?.Invoke();
        MeCambiaVida?.Invoke(ActualVidas);
        if (ActualCorazones <= 0) {
            PerderVida();
            MeMuero?.Invoke();
            Curar(MaxCorazones);
        }
    }

    

    public void PerderVida()
    {
        ActualVidas--;
        ActualVidas = Mathf.Clamp(ActualVidas, 0, MaxVidas);
        MeCambiaVida?.Invoke(ActualVidas);
        MeMuero?.Invoke();
        if (ActualVidas <= 0)
        {
            //FUTURO DE CUANDO MUERA SE REINICIA NIVEL
        }
        
    }


    public void Curar(int value)
    {
        ActualCorazones = Mathf.Clamp(ActualCorazones + value, 1, MaxCorazones) ;
        MeCuro?.Invoke();
        MeCambiaVida?.Invoke(ActualVidas);
    }




    

}
