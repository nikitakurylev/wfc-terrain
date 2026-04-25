using UnityEngine;

public static class Ease
{
    public static float InOutQuad(float x ) => x < 0.5 ? 2 * x * x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2;
    public static float InOutCubic(float x ) => x < 0.5 ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;
}