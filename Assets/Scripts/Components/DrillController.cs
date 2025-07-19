using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class DrillController : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable _grabInteractable;
    [SerializeField] private AudioSource _audioSource;
    [Header("Rotation Settings")]
    [SerializeField] private Transform _rotatingPart;
    [SerializeField] private float _rotationSpeed = 100f;
    [Header("Button Settings")]
    [SerializeField] private Transform _buttonTransform;
    [SerializeField] private Vector3 _buttonPressedPosition;
    [SerializeField] private Vector3 _buttonReleasedPosition;
    private Tween _rotationTween;


    private void Awake()
    {
        _rotationTween = _rotatingPart
            .DOLocalRotate(new Vector3(0, 0, _rotationSpeed), 1f, RotateMode.LocalAxisAdd)
            .SetRelative(true)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental)
            .SetLink(_rotatingPart.gameObject);
    }

    private void OnEnable()
    {
        _grabInteractable.activated.AddListener(OnActivated);
        _grabInteractable.deactivated.AddListener(OnDeactivated);
        _grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        _grabInteractable.activated.RemoveListener(OnActivated);
        _grabInteractable.deactivated.RemoveListener(OnDeactivated);
        _grabInteractable.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnActivated(ActivateEventArgs args)
    {
        TurnOn();
    }

    private void OnDeactivated(DeactivateEventArgs args)
    {
        TurnOff();
    }
    
    private void OnSelectExited(SelectExitEventArgs args)
    {
        TurnOff();
    }

    private void TurnOn()
    {
        _rotationTween.Play();
        _audioSource.Play();

        if (DOTween.IsTweening(_buttonTransform.gameObject)) DOTween.Kill(_buttonTransform.gameObject);
        _buttonTransform.DOLocalMove(_buttonPressedPosition, 0.2f)
            .SetEase(Ease.OutBack)
            .SetRecyclable(true)
            .SetLink(_buttonTransform.gameObject)
            .Play();
    }

    private void TurnOff()
    {
        _rotationTween.Pause();
        _audioSource.Stop();

        if (DOTween.IsTweening(_buttonTransform.gameObject)) DOTween.Kill(_buttonTransform.gameObject);
        _buttonTransform.DOLocalMove(_buttonReleasedPosition, 0.2f)
            .SetEase(Ease.OutBack)
            .SetRecyclable(true)
            .SetLink(_buttonTransform.gameObject)
            .Play();
    }
}