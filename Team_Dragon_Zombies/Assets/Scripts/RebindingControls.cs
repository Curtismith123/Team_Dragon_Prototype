using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindingContols : MonoBehaviour
{
    public InputActionReference m_Action; // Reference to an action to rebind.
    public int m_BindingIndex; // Index into m_Action.bindings for binding to rebind.
    public TMP_Text m_DisplayText; // Text in UI that receives the binding display string.

    //[SerializeField] private InputActionReference inputAction;
    //[SerializeField] private PlayerController playerController;
    //[SerializeField] private TMP_Text bindDisplayTxt;
    [SerializeField] private GameObject rebindStart;
    [SerializeField] private GameObject waitingInput;

    public void startRebinding()
    {
        rebindStart.SetActive(false);
        waitingInput.SetActive(true);

        string keyRebind = Input.GetButtonDown("Vertical").ToString();
        
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
