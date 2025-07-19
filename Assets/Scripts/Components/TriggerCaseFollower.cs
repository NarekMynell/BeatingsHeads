using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TriggerCaseFollower : MonoBehaviour
{
    [SerializeField] private GameObject _follower;
    [SerializeField] private AnchorType _anchorType;
    [SerializeField] private bool _deactivateOnTriggerExit = true;
    [SerializeField] private bool _acceptTargetSize = true;
    private Vector3 _defaultFollowerGoScale;
    private CancellationTokenSource _followCts;
    

    private void Awake()
    {
        _defaultFollowerGoScale = _follower.transform.localScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        CancelFollow();
        SetupFolloving(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        EndFollowing();
    }

    private void OnDisable()
    {
        EndFollowing();
    }

    private void SetupFolloving(GameObject targetGo)
    {
        _follower.SetActive(true);
        Vector3 centreOffset = Vector3.zero;
        Vector3 scale = _defaultFollowerGoScale;

        if (_acceptTargetSize || _anchorType == AnchorType.CentreOfBounds)
        {
            if (targetGo.TryGetBounds(out Bounds targetBounds))
            {
                // Get the scale
                if (_acceptTargetSize)
                {
                    if (_follower.TryGetBounds(out Bounds folowerBounds))
                    {
                        scale = new(
                            targetBounds.size.x * _follower.transform.localScale.x / folowerBounds.size.x,
                            targetBounds.size.y * _follower.transform.localScale.y / folowerBounds.size.y,
                            targetBounds.size.z * _follower.transform.localScale.z / folowerBounds.size.z
                        );
                    }
                    else
                    {
                        scale = targetBounds.size;
                    }
                }

                // Get the offset of the anchor
                if (_anchorType == AnchorType.CentreOfBounds)
                {
                    centreOffset = targetBounds.center - targetGo.transform.position;
                }
            }
        }

        _follower.transform.localScale = scale;

        _followCts = new CancellationTokenSource();
        FollowAsync(targetGo, centreOffset, _followCts.Token).Forget();
    }

    private void EndFollowing()
    {
        if (_deactivateOnTriggerExit)
            _follower.SetActive(false);
        CancelFollow();
    }

    private void CancelFollow()
    {
        if (_followCts != null)
        {
            _followCts.Cancel();
            _followCts.Dispose();
            _followCts = null;
        }
    }

    private async UniTaskVoid FollowAsync(GameObject targetGo, Vector3 centreOffset, CancellationToken externalToken)
    {
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            externalToken,
            targetGo.GetCancellationTokenOnDestroy()
        );

        var token = linkedCts.Token;

        while (!token.IsCancellationRequested)
        {
            _follower.transform.position = targetGo.transform.position + centreOffset;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private enum AnchorType
    {
        CentreOfBounds,
        Possition
    }
}