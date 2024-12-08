using UnityEngine;
using TMPro;

public class PopUpDmgTxt : MonoBehaviour
{
    public TMP_Text popUpTxt;

    public void Intialize(float amount)
    {
        Destroy(this.gameObject, 2.5f);
        popUpTxt.text = amount.ToString("F0");
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
