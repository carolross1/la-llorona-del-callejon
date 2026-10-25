// Assets/Editor/PlayerAnimationFixer.cs
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class PlayerAnimationFixer
{
    const string ControllerPath = "Assets/Animations/Player/PlayerController.controller";
    const string WalkUpPath     = "Assets/Animations/Player/Walk_Up.anim";
    const string WalkDownPath   = "Assets/Animations/Player/Walk_Down.anim";
    const string WalkLeftPath   = "Assets/Animations/Player/Walk_Left.anim";
    const string WalkRightPath  = "Assets/Animations/Player/Walk_Right.anim";

    const string IdleUpPath     = "Assets/Animations/Player/Idle_Up.anim";
    const string IdleDownPath   = "Assets/Animations/Player/Idle_Down.anim";
    const string IdleLeftPath   = "Assets/Animations/Player/Idle_Left.anim";
    const string IdleRightPath  = "Assets/Animations/Player/Idle_Right.anim";

    [MenuItem("La Llorona/Corregir Animaciones Player")]
    public static string FixPlayerAnimations()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== DIAGNOSTICO Y CORRECCION DE ANIMACIONES DE CARLOS ===");

        // 1. Cargar Clips
        AnimationClip walkUp    = AssetDatabase.LoadAssetAtPath<AnimationClip>(WalkUpPath);
        AnimationClip walkDown  = AssetDatabase.LoadAssetAtPath<AnimationClip>(WalkDownPath);
        AnimationClip walkLeft  = AssetDatabase.LoadAssetAtPath<AnimationClip>(WalkLeftPath);
        AnimationClip walkRight = AssetDatabase.LoadAssetAtPath<AnimationClip>(WalkRightPath);

        AnimationClip idleUp    = AssetDatabase.LoadAssetAtPath<AnimationClip>(IdleUpPath);
        AnimationClip idleDown  = AssetDatabase.LoadAssetAtPath<AnimationClip>(IdleDownPath);
        AnimationClip idleLeft  = AssetDatabase.LoadAssetAtPath<AnimationClip>(IdleLeftPath);
        AnimationClip idleRight = AssetDatabase.LoadAssetAtPath<AnimationClip>(IdleRightPath);

        sb.AppendLine($"Walk_Up clip: {(walkUp != null ? walkUp.name + " (" + walkUp.length + "s, loop=" + walkUp.isLooping + ")" : "NULL")}");
        sb.AppendLine($"Walk_Down clip: {(walkDown != null ? walkDown.name : "NULL")}");
        sb.AppendLine($"Walk_Left clip: {(walkLeft != null ? walkLeft.name : "NULL")}");
        sb.AppendLine($"Walk_Right clip: {(walkRight != null ? walkRight.name : "NULL")}");

        // Asegurar que Walk_Right tenga los 4 frames
        EnsureWalkRight4Frames(walkRight);

        // 2. Cargar Controller
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            sb.AppendLine("ERROR: No se encontro AnimatorController en " + ControllerPath);
            return sb.ToString();
        }

        // Inspeccionar capas y estados
        var layer = controller.layers[0];
        var sm = layer.stateMachine;
        sb.AppendLine($"Capa 0: {layer.name}, DefaultState: {sm.defaultState?.name}");

        foreach (var stateChild in sm.states)
        {
            var state = stateChild.state;
            sb.AppendLine($"\nEstado: {state.name}");
            if (state.motion is BlendTree bt)
            {
                sb.AppendLine($"  -> BlendTree: {bt.name}, BlendType: {bt.blendType}, Parameters: {bt.blendParameter}, {bt.blendParameterY}");
                for (int i = 0; i < bt.children.Length; i++)
                {
                    var cm = bt.children[i];
                    sb.AppendLine($"     Child {i}: Motion={cm.motion?.name}, Pos=({cm.position.x}, {cm.position.y})");
                }

                // Si es el BlendTree de Caminata (Walk)
                if (state.name.ToLower().Contains("walk") || state.name.ToLower().Contains("camin"))
                {
                    sb.AppendLine("  -> Corrigiendo BlendTree de Caminata...");
                    bt.blendType = BlendTreeType.SimpleDirectional2D;
                    bt.blendParameter = "MoveX";
                    bt.blendParameterY = "MoveY";

                    // Reemplazar o asignar hijos correctamente
                    var children = new ChildMotion[4];
                    children[0] = new ChildMotion { motion = walkDown,  position = new Vector2(0f, -1f), timeScale = 1f };
                    children[1] = new ChildMotion { motion = walkUp,    position = new Vector2(0f,  1f), timeScale = 1f };
                    children[2] = new ChildMotion { motion = walkLeft,  position = new Vector2(-1f, 0f), timeScale = 1f };
                    children[3] = new ChildMotion { motion = walkRight, position = new Vector2( 1f, 0f), timeScale = 1f };
                    bt.children = children;
                    sb.AppendLine("  -> BlendTree de Caminata reconfigurado: Down(0,-1), Up(0,1), Left(-1,0), Right(1,0)");
                }

                // Si es el BlendTree de Idle
                if (state.name.ToLower().Contains("idle") || state.name.ToLower().Contains("quieto"))
                {
                    sb.AppendLine("  -> Corrigiendo BlendTree de Idle...");
                    bt.blendType = BlendTreeType.SimpleDirectional2D;
                    bt.blendParameter = "MoveX";
                    bt.blendParameterY = "MoveY";

                    var children = new ChildMotion[4];
                    children[0] = new ChildMotion { motion = idleDown,  position = new Vector2(0f, -1f), timeScale = 1f };
                    children[1] = new ChildMotion { motion = idleUp,    position = new Vector2(0f,  1f), timeScale = 1f };
                    children[2] = new ChildMotion { motion = idleLeft,  position = new Vector2(-1f, 0f), timeScale = 1f };
                    children[3] = new ChildMotion { motion = idleRight, position = new Vector2( 1f, 0f), timeScale = 1f };
                    bt.children = children;
                    sb.AppendLine("  -> BlendTree de Idle reconfigurado: Down(0,-1), Up(0,1), Left(-1,0), Right(1,0)");
                }
            }
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(ControllerPath, ImportAssetOptions.ForceUpdate);

        sb.AppendLine("\n¡Controlador guardado y reimportado exitosamente!");
        return sb.ToString();
    }

    static void EnsureWalkRight4Frames(AnimationClip walkRight)
    {
        if (walkRight == null) return;
        var bindings = AnimationUtility.GetObjectReferenceCurveBindings(walkRight);
        if (bindings.Length == 0) return;

        var keyframes = AnimationUtility.GetObjectReferenceCurve(walkRight, bindings[0]);
        if (keyframes.Length < 4)
        {
            // Cargar los 4 sprites de la fila 3 (Carlos_Caminando_12..15)
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Player/Carlos_Caminando.png").OfType<Sprite>().ToArray();
            var s12 = sprites.FirstOrDefault(s => s.name == "Carlos_Caminando_12");
            var s13 = sprites.FirstOrDefault(s => s.name == "Carlos_Caminando_13");
            var s14 = sprites.FirstOrDefault(s => s.name == "Carlos_Caminando_14");
            var s15 = sprites.FirstOrDefault(s => s.name == "Carlos_Caminando_15");

            if (s12 != null && s13 != null && s14 != null && s15 != null)
            {
                var newKeys = new ObjectReferenceKeyframe[4];
                newKeys[0] = new ObjectReferenceKeyframe { time = 0.000f, value = s12 };
                newKeys[1] = new ObjectReferenceKeyframe { time = 0.125f, value = s13 };
                newKeys[2] = new ObjectReferenceKeyframe { time = 0.250f, value = s14 };
                newKeys[3] = new ObjectReferenceKeyframe { time = 0.375f, value = s15 };

                AnimationUtility.SetObjectReferenceCurve(walkRight, bindings[0], newKeys);
                EditorUtility.SetDirty(walkRight);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(WalkRightPath, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}
