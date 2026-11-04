#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Adaptador del Nivel 2: redirige hacia el nuevo controlador modular de 5 niveles.
/// </summary>
public static class Level2Adapter
{
    public static void AdaptLevel2ToImage()
    {
        Level2MasterController.BuildFullVerticalLevel2();
    }
}
#endif
