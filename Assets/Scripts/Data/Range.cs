using UnityEngine;

[System.Serializable]
public class Range
{
    [SerializeField] private float _min;
    [SerializeField] private float _max;
    public float Min => _min;
    public float Max => _max;

    public float Normalized(float value) => Mathf.InverseLerp(_min, _max, value);
    public bool Contains(float value) => value >= _min && value <= _max;
}