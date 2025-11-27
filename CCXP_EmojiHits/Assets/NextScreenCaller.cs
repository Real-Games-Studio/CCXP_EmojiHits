using UnityEngine;
using UnityEngine.UI;

public class NextScreenCaller : MonoBehaviour
{
    [SerializeField] private CanvasScreen myScreen;
    [SerializeField] private Image fillImage; //feedback visual do tempo
    [SerializeField] private float timeToCallNextScreen = 5f; //tempo para chamar a pr�xima tela

    void Update()
    {
        if (myScreen.IsOn())
        {
            fillImage.fillAmount += 1 / timeToCallNextScreen * Time.deltaTime;

            if (fillImage.fillAmount >= 1)
            {
                myScreen.CallNextScreen();
                fillImage.fillAmount = 0;
            }
        }
        else
        {
            fillImage.fillAmount = 0;
        }        
    }
}
