using UnityEngine;

public class Explode : MonoBehaviour
{
    public float cubeSize = 0.2f;
    public int cubesInRow = 5;

    float cubesPivotDistance;
    Vector3 cubesPivot;

    public float explosionForce = 50f;
    public float explosionRadius = 4f;
    public float explosionUpward = 0.4f;

    public Rigidbody rb;
    public bool isBroken;
    public LayerMask ignoreLayers;
    public Material pieceMaterial;

    void Start()
    {
        cubesPivotDistance = cubeSize * cubesInRow / 2;
        cubesPivot = new Vector3(cubesPivotDistance, cubesPivotDistance, cubesPivotDistance);
        rb.isKinematic = true;
    }

    void Update()
    {
        if (isBroken)
        {
            rb.isKinematic = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((ignoreLayers.value & (1 << other.gameObject.layer)) != 0)
        {
            return;
        }

        if (other.CompareTag("Breaker"))
        {
            isBroken = true;
            ExplodeObjects();

            CheckChildrenForExplosion();
        }
    }

    public void ExplodeObjects()
    {
        DestroyActivate destroyActivate = FindObjectOfType<DestroyActivate>();
        if (destroyActivate != null && destroyActivate.objectToDestroy == gameObject)
        {
            destroyActivate.OnObjectDestroyed();
        }

        for (int x = 0; x < cubesInRow; x++)
        {
            for (int y = 0; y < cubesInRow; y++)
            {
                for (int z = 0; z < cubesInRow; z++)
                {
                    createPiece(x, y, z);
                }
            }
        }

        Vector3 explosionPos = transform.position;

        Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, explosionUpward);
            }
        }
        Destroy(gameObject, 0.1f);
    }

    private void CheckChildrenForExplosion()
    {
        foreach (Transform child in transform)
        {
            Explode childExplode = child.GetComponent<Explode>();
            if (childExplode != null && child.CompareTag("Breaker"))
            {
                childExplode.isBroken = true;
                childExplode.ExplodeObjects();
            }
        }
    }

    void createPiece(int x, int y, int z)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning("No Renderer found on exploding object!");
            return;
        }

        Vector3 boundsCenter = renderer.bounds.center;

        Vector3 adjustedPivot = new Vector3(
            cubesPivotDistance - (cubesInRow * cubeSize) / 2f,
            cubesPivotDistance - (cubesInRow * cubeSize) / 2f,
            cubesPivotDistance - (cubesInRow * cubeSize) / 2f
        );

        GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);

        piece.transform.position = boundsCenter + new Vector3(cubeSize * x, cubeSize * y, cubeSize * z) - adjustedPivot;
        piece.transform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);

        if (pieceMaterial != null)
        {
            piece.GetComponent<Renderer>().material = pieceMaterial;
        }

        Rigidbody rb = piece.AddComponent<Rigidbody>();
        rb.mass = cubeSize;
    }
}
