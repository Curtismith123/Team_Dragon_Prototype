using UnityEngine;
using TMPro;

public class PopUpDmgTxt : MonoBehaviour
{
    public TMP_Text popUpTxt;

    public void Intialize(float amount)
    {
        popUpTxt.text = amount.ToString("F1");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
    }
}
