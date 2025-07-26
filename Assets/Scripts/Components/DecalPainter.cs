using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Renderer), typeof(Rigidbody))]
public class DecalPainter : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private Collider _collisionCollider;
    [SerializeField] private MeshCollider _uvCollider;
    [SerializeField] private LayerMask _paintableLayers = ~0;
    [SerializeField] private float _minCollisionForce = 0.5f;
    [SerializeField] private float _maxCollisionForce = 10f;

    [Header("Performance")]
    [SerializeField] private bool _useMipmaps = true;
    [SerializeField] private DecalSettings _decalSettings;


    // Runtime fields
    private Rigidbody _rigidbody;
    private Renderer _renderer;
    private RenderTexture _renderTexture;
    private Texture2D _originalTexture;
    private bool _textureModified;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        InitializeTexture();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsPaintableCollision(collision)) return;

        float magnitude = collision.impulse.magnitude;

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.thisCollider != _collisionCollider) continue;
            
            Vector3 contactPoint = contact.point;
            Vector3 contactNormal = contact.normal;
            
            Vector3 velocity = _rigidbody.velocity;
            Vector3 angularVelocity = _rigidbody.angularVelocity;
            _rigidbody.isKinematic = true;
            _uvCollider.enabled = true;

            PaintAtContact(_collisionCollider.ClosestPoint(contactPoint), contactNormal, magnitude);

            _uvCollider.enabled = false;
            _rigidbody.isKinematic = false;
            _rigidbody.velocity = velocity;
            _rigidbody.angularVelocity = angularVelocity;

            break;
        }
    }

    private void InitializeTexture()
    {
        _originalTexture = _renderer.material.mainTexture as Texture2D;

        _renderTexture = new RenderTexture(
            _originalTexture.width,
            _originalTexture.height,
            0
        );

        _renderTexture.Create();

        if (_originalTexture != null)
        {
            Graphics.Blit(_originalTexture, _renderTexture);
        }
        else
        {
            ClearTexture();
        }

        _renderer.material.mainTexture = _renderTexture;
        _textureModified = false;
    }

    bool IsPaintableCollision(Collision collision)
    {
        return
            (_paintableLayers.value & (1 << collision.gameObject.layer)) != 0 &&
            collision.impulse.magnitude >= _minCollisionForce;
    }

    void PaintAtContact(Vector3 contactPoint, Vector3 contactNormal, float collisionForce)
    {
        Transform tf = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        Destroy(tf.GetComponent<SphereCollider>());
        tf.localScale = Vector3.one * 0.01f;
        tf.SetParent(transform);
        tf.position = contactPoint;
        tf.GetComponent<Renderer>().material.color = Color.green;

        Ray ray = new(contactPoint + contactNormal * 0.08f, -contactNormal);

        if (!_uvCollider.Raycast(ray, out RaycastHit hit, 0.09f))
        {
            return;
        }

        Debug.Log($"Painting at UV: {hit.textureCoord} with force: {collisionForce}");


        float intensity = Mathf.Clamp01(
            (collisionForce - _minCollisionForce) /
            (_maxCollisionForce - _minCollisionForce)
        );


        // Create a temporary RenderTexture
        RenderTexture tempRT = RenderTexture.GetTemporary(
            _renderTexture.width, 
            _renderTexture.height, 
            0, 
            _renderTexture.format
        );

        // Copy the original content to the temporary RT
        Graphics.Blit(_renderTexture, tempRT);



        Material _decalMaterial = new Material(Shader.Find("Hidden/DecalDrawer"));
        // Set shader properties
        _decalMaterial.SetTexture("_MainTex", _renderTexture);
        _decalMaterial.SetTexture("_DecalTex", _decalSettings.decalTexture);
        _decalMaterial.SetVector("_UVPos", hit.textureCoord);
        _decalMaterial.SetFloat("_DecalSize", 0.1f); // 10% of render texture size

        Graphics.Blit(tempRT, _renderTexture, _decalMaterial);

        // Clean up
        RenderTexture.ReleaseTemporary(tempRT);
    }



    void ClearTexture()
    {
        // Color32[] clearPixels = new Color32[_runtimeTexture.width * _runtimeTexture.height];
        // for (int i = 0; i < clearPixels.Length; i++)
        // {
        //     clearPixels[i] = Color.clear;
        // }
        // _runtimeTexture.SetPixels32(clearPixels, 0);
        // _runtimeTexture.Apply();
    }

    void OnDestroy()
    {
        if (_renderer != null && _originalTexture != null)
        {
            _renderer.material.mainTexture = _originalTexture;
        }

        if (_renderTexture != null)
        {
            _renderTexture.Release();
        }
    }
}


[System.Serializable]
public class DecalSettings
{
    [Header("Decal Visuals")]
    public Texture2D decalTexture;
    public Color decalColor = new Color(0.5f, 0f, 0.8f, 1f);
    
    [Header("Size Settings")]
    public Vector2 scaleRange = new Vector2(0.1f, 0.3f);
    public bool scaleWithIntensity = true;
    
    [Header("Opacity Settings")]
    [Range(0,1)] public float maxOpacity = 0.8f;
    public float opacityMultiplier = 1f;
}

