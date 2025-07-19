using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private AssetReference _sampleSceneReference;

    private void Awake()
    {
        DOTween.Init().SetCapacity(500, 50);
    }

    private async void Start()
    {
        await InitializeGameAsync();
    }

    private async UniTask InitializeGameAsync()
    {
        // Wait for testing
        // TODO: Remove this in production
        await UniTask.WaitForSeconds(2);
        
        var loadSceneOperation = _sampleSceneReference.LoadSceneAsync(LoadSceneMode.Single);

        try
        {
            await loadSceneOperation.ToUniTask();

            if (loadSceneOperation.Status == AsyncOperationStatus.Failed)
                {
                    Debug.LogError("Scene loading failed: " + loadSceneOperation.OperationException);
                }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Unexpected error: " + e.Message);
        }
    }
}