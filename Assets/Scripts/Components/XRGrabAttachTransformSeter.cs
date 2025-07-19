using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class XRGrabAttachTransformSeter : MonoBehaviour
{
    [SerializeField] private Transform _leftControllerAttachTransform;
    [SerializeField] private Transform _rightControllerAttachTransform;
    private XRGrabInteractable _xRGrabInteractable;

    private void Awake()
    {
        _xRGrabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        _xRGrabInteractable.hoverEntered.AddListener(OnHoverEntered);
    }

    private void OnDisable()
    {
        _xRGrabInteractable.hoverEntered.RemoveListener(OnHoverEntered);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        SetAttachTransform(args.interactorObject);
    }

    private void SetAttachTransform(IXRInteractor interactor)
    {
        if (interactor is XRBaseControllerInteractor controllerInteractor)
        {
            if (controllerInteractor.gameObject.TryGetComponent(out SideSelectorHorizontal sideSelector))
            {
                if (sideSelector.Side == SideHorizontal.Left)
                {
                    _xRGrabInteractable.attachTransform = _leftControllerAttachTransform;
                }
                else
                {
                    _xRGrabInteractable.attachTransform = _rightControllerAttachTransform;
                }
            }
            else
            {
                Debug.LogWarning($"No SideSelectorHorizontal component attached to {controllerInteractor.gameObject.name}.");
            }
        }
    }
}
