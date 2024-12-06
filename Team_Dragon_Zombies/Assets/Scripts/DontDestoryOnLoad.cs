using UnityEngine;

public class DontDestoryOnLoad : MonoBehaviour
{
    private static DontDestoryOnLoad instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }
}
