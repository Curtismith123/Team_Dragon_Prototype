using UnityEngine;

public class ActTrap : MonoBehaviour
{
    public float climb = 2f;
   public void Commence (float amount )
    {
        climb -= amount;
        if (climb <= 0f)
        {
            Go();
        }
    }
    public void Go() 
    {
        Destroy(gameObject);
    }
}
