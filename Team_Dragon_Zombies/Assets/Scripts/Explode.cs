using UnityEngine;

public class Explode : MonoBehaviour
{
    private PlayerController player;
    public float cubeSize = 0.2f;
    public float explosionForce = 50f;
    public float explosionRadius = 4f;
    public float explosionUpward = 0.4f;
    public GameObject DropItem;

    public Rigidbody rb;
    public bool isBroken;
    public LayerMask ignoreLayers;
    public Material pieceMaterial;

    public float pieceLifetime = 5f;

    private Vector3 objectSize;
    private Vector3 objectMinBounds;
    private AudioSource audioSource;
    public AudioClip clip;

    [Range(0.1f, 1.0f)] public float resolutionFactor = 1.0f;

    public Color emissionColor = new Color(1f, 0.647f, 0f);
    public float emissionIntensity = 0.5f;

    public Light glowLight;

    void Start()
    {
        player = gameManager.instance.playerScript;
        rb.isKinematic = true;
        audioSource = player.GetComponent<AudioSource>();
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            objectSize = renderer.bounds.size;
            objectMinBounds = renderer.bounds.min;
        }
        else
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                objectSize = Vector3.Scale(meshFilter.sharedMesh.bounds.size, transform.lossyScale);
                objectMinBounds = renderer.bounds.min;
            }
            else
            {
                Collider collider = GetComponent<Collider>();
                if (collider != null)
                {
                    objectSize = collider.bounds.size;
                    objectMinBounds = collider.bounds.min;
                }
            }
        }

        SetEmissionColor();
        AddGlowLight();
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
            InitObject();
            DropItem = null;
            ExplodeObjects();
            CheckChildrenForExplosion();

        }
    }

    public void ExplodeObjects()
    {
        PlaySound(clip);
        DestroyActivate destroyActivate = FindAnyObjectByType<DestroyActivate>();
        if (destroyActivate != null && destroyActivate.objectToDestroy == gameObject)
        {
            destroyActivate.OnObjectDestroyed();
        }

        int cubesInRowX = Mathf.Max(1, Mathf.RoundToInt(objectSize.x / cubeSize));
        int cubesInRowY = Mathf.Max(1, Mathf.RoundToInt(objectSize.y / cubeSize));
        int cubesInRowZ = Mathf.Max(1, Mathf.RoundToInt(objectSize.z / cubeSize));

        if (resolutionFactor < 1.0f)
        {
            cubesInRowX = Mathf.Max(1, Mathf.RoundToInt(cubesInRowX * resolutionFactor));
            cubesInRowY = Mathf.Max(1, Mathf.RoundToInt(cubesInRowY * resolutionFactor));
            cubesInRowZ = Mathf.Max(1, Mathf.RoundToInt(cubesInRowZ * resolutionFactor));
        }

        for (int x = 0; x < cubesInRowX; x++)
        {
            for (int y = 0; y < cubesInRowY; y++)
            {
                for (int z = 0; z < cubesInRowZ; z++)
                {
                    createPiece(x, y, z, cubesInRowX, cubesInRowY, cubesInRowZ);
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

    void createPiece(int x, int y, int z, int cubesInRowX, int cubesInRowY, int cubesInRowZ)
    {
        float offsetX = (x + 0.5f) / cubesInRowX;
        float offsetY = (y + 0.5f) / cubesInRowY;
        float offsetZ = (z + 0.5f) / cubesInRowZ;

        Vector3 worldPos = objectMinBounds + new Vector3(
            offsetX * objectSize.x,
            offsetY * objectSize.y,
            offsetZ * objectSize.z
        );

        GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);

        piece.transform.position = worldPos;
        piece.transform.rotation = Quaternion.identity;
        piece.transform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);

        if (pieceMaterial != null)
        {
            piece.GetComponent<Renderer>().material = pieceMaterial;
        }

        Rigidbody rb = piece.AddComponent<Rigidbody>();
        rb.mass = cubeSize;

        Destroy(piece, pieceLifetime);
    }

    private void SetEmissionColor()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material.HasProperty("_EmissionColor"))
            {
                renderer.material.SetColor("_EmissionColor", emissionColor * emissionIntensity);
                renderer.material.EnableKeyword("_EMISSION");
            }
        }
    }

    private void AddGlowLight()
    {
        glowLight = gameObject.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.color = emissionColor;
        glowLight.intensity = 2f;
        glowLight.range = 3f;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    private void InitObject()
    {
        if (DropItem != null)
        {
            Instantiate(DropItem, this.transform.position + Vector3.up, Quaternion.identity);
        }
    }
}
