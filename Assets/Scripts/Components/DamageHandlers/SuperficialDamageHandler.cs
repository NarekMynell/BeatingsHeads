using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class SuperficialDamageHandler : DamageHandler<SuperficialDamageDealer>
{
    [SerializeField] private bool _applyDamageFromEverything = false;
    [SerializeField] private Texture2D[] _damageTextures;
    [SerializeField] private Range _damageTextureSize;
    [SerializeField] private Material _layeredMaterial;
    [SerializeField] private string _blendTexname = "_BlendTex";
    [SerializeField] private MeshCollider _raycastCollider;
    [SerializeField] private Vector2Int _renderTextureSize = new (512, 512);
    [SerializeField] private Vector2 _decalTextureSize = new (0.1f, 0.1f);
    [SerializeField] private Rigidbody _rigidbody;
    
    // Runtime fields
    private RenderTexture _renderTexture;
    private RenderTexture _tempRenderTexture;
    private Material _blendMaterial;
    private CommandBuffer _commandBuffer;


    private void Awake()
    {
        _renderTexture = new(_renderTextureSize.x, _renderTextureSize.y, 0, RenderTextureFormat.R8);
        _renderTexture.Create();
        _layeredMaterial.SetTexture(_blendTexname, _renderTexture);

        _tempRenderTexture = RenderTexture.GetTemporary(_renderTexture.descriptor);
        _blendMaterial = new Material(Shader.Find("Hidden/DecalBlend"));
        _commandBuffer = new CommandBuffer { name = "Decal Painting" };
    }

    private void OnDestroy()
    {
        _renderTexture.Release();
        RenderTexture.ReleaseTemporary(_tempRenderTexture);
        Destroy(_blendMaterial);
        _commandBuffer.Dispose();
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        OnDamaged(collision);
        // if (_applyDamageFromEverything || collision.gameObject.TryGetComponent(out SuperficialDamageDealer damageDealer))
        // {
        //     base.OnCollisionEnter(collision);
        // }
        // else
        // {
        //     OnDamaged(collision);
        // }

    }

    protected override void OnDamaged(Collision collision, SuperficialDamageDealer damageDealer)
    {
        
    }

    private void OnDamaged(Collision collision)
    {
        float magnitude = collision.impulse.magnitude;
        if (magnitude < _damageRange.Min) return;
        Debug.Log($"Collision magnitude: {magnitude}");

        Vector3 velocity = _rigidbody.velocity;
        Vector3 angularVelocity = _rigidbody.angularVelocity;
        bool isKinematic = _rigidbody.isKinematic;
        _rigidbody.isKinematic = true;
        _raycastCollider.enabled = true;

        Vector3 collisionPoint = collision.contacts[0].point;
        Vector3 collisionNormal = collision.contacts[0].normal;
        Ray ray = new (collisionPoint - collisionNormal * 1f, collisionNormal);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);

        if (_raycastCollider.Raycast(ray, out RaycastHit hit, 2f))
        {
            Vector2 uvPos = hit.textureCoord;
            Debug.Log($"Painting at UV: {uvPos} with force: {magnitude}");
            DrawDecal(uvPos, magnitude);
        }

        _raycastCollider.enabled = false;
        _rigidbody.isKinematic = isKinematic;
        if (!isKinematic)
        {
            _rigidbody.velocity = velocity;
            _rigidbody.angularVelocity = angularVelocity;
        }
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = ray.origin;
                sphere.transform.localScale = Vector3.one * 0.05f;
                Rigidbody rigidbody = sphere.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                // rigidbody.AddForceAtPosition(ray.direction * 10f, ray.origin, ForceMode.Impulse);
                rigidbody.velocity = ray.direction * 3;

                // Vector3 contactPoint = hit.point;
                // Vector3 contactNormal = hit.normal;
                // Vector2 uvPos = hit.textureCoord;
                // DrawDecal(uvPos, hit.distance);
            }
        }
    }


    private void DrawDecal(Vector2 uvPos, float magnitude)
    {
        Texture2D decalTexture = _damageTextures[Random.Range(0, _damageTextures.Length)];

        
        _blendMaterial.SetTexture("_MainTex", _renderTexture);
        _blendMaterial.SetTexture("_DecalTex", decalTexture);
        _blendMaterial.SetVector("_DecalCenter", new Vector4(uvPos.x, uvPos.y, 0, 0));
        _blendMaterial.SetVector("_DecalSize", new Vector4(_decalTextureSize.x, _decalTextureSize.y, 0, 0));


        _commandBuffer.Blit(_renderTexture, _tempRenderTexture, _blendMaterial);
        _commandBuffer.Blit(_tempRenderTexture, _renderTexture);
        Graphics.ExecuteCommandBuffer(_commandBuffer);
    }
}