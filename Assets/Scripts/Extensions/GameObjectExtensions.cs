using UnityEngine;

public static class GameObjectExtensions
{
    /// <summary>
    /// Attempts to calculate the total world-space bounds of this GameObject, including all child Renderers.
    /// </summary>
    /// <param name="go">The GameObject to calculate bounds for.</param>
    /// <param name="bounds">The output bounds if any Renderers are found.</param>
    /// <returns>True if at least one Renderer was found and bounds were calculated, false otherwise.</returns>
    public static bool TryGetBounds(this GameObject go, out Bounds bounds)
    {
        bounds = default;
        var renderers = go.GetComponentsInChildren<Renderer>(includeInactive: true);

        if (renderers.Length == 0)
            return false;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            if (renderers[i].gameObject.activeInHierarchy)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            
        }

        return true;
    }
}