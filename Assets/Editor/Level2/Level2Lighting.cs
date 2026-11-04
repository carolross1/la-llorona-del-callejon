#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Módulo de Iluminación para el Nivel 2 (Callejón Vertical de Guanajuato).
/// Gestiona la luz ambiental nocturna URP 2D y el emplazamiento de faroles coloniales
/// parpadeantes a lo largo de las múltiples rutas y niveles de altura.
/// </summary>
public static class Level2Lighting
{
    public static void ConfigureGlobalLight()
    {
        GameObject globalLightObj = GameObject.Find("Global Light 2D");
        if (globalLightObj == null)
        {
            globalLightObj = new GameObject("Global Light 2D");
        }

        Light2D globalLight = globalLightObj.GetComponent<Light2D>();
        if (globalLight == null) globalLight = globalLightObj.AddComponent<Light2D>();

        globalLight.lightType = Light2D.LightType.Global;
        // Tono azul oscuro nocturno colonial (overlay general según especificación)
        globalLight.color = new Color(0.20f, 0.22f, 0.36f, 1.0f);
        globalLight.intensity = 0.45f;
    }

    public static GameObject SpawnWallLantern(GameObject parent, string name, Vector3 pos, Sprite lanternSprite)
    {
        GameObject lantern = new GameObject(name);
        lantern.transform.SetParent(parent.transform, false);
        lantern.transform.position = pos;
        lantern.transform.localScale = new Vector3(0.14f, 0.14f, 1f);

        if (lanternSprite != null)
        {
            SpriteRenderer sr = lantern.AddComponent<SpriteRenderer>();
            sr.sprite = lanternSprite;
            sr.sortingLayerName = "Props";
            sr.sortingOrder = 12;
        }

        // Luz de punto 2D (halo naranja cálido)
        GameObject lightChild = new GameObject("Luz_Point2D");
        lightChild.transform.SetParent(lantern.transform, false);
        lightChild.transform.localPosition = new Vector3(0f, 0.5f, 0f);

        Light2D l2d = lightChild.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.65f, 0.22f, 1.0f); // Cálido naranja ámbar
        l2d.intensity = 1.8f;
        l2d.pointLightOuterRadius = 3.2f;
        l2d.pointLightInnerRadius = 0.4f;

        // Leve parpadeo de llama de gas/vela
        lightChild.AddComponent<LightFlicker>();

        return lantern;
    }

    public static GameObject SpawnExitBeaconLight(GameObject parent, Vector3 localPos)
    {
        GameObject beacon = new GameObject("Luz_Dorada_Salida");
        beacon.transform.SetParent(parent.transform, false);
        beacon.transform.localPosition = localPos;

        Light2D l2d = beacon.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.88f, 0.40f, 1.0f); // Resplandor dorado de meta
        l2d.intensity = 2.4f;
        l2d.pointLightOuterRadius = 5.0f;
        l2d.pointLightInnerRadius = 1.0f;

        return beacon;
    }
}
#endif
