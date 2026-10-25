// Assets/Editor/PlayerAnimationFixer.cs
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[InitializeOnLoad]
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

    static PlayerAnimationFixer()
    {
        EditorApplication.delayCall += () =>
        {
            FixPlayerAnimations();
        };
    }

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

        // Asegurar que Walk_Right tenga los 4 frames
        EnsureWalkRight4Frames(walkRight);
        EnsureWalkLeft4Frames(walkLeft);

        // 2. Cargar Controller
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            sb.AppendLine("ERROR: No se encontro AnimatorController en " + ControllerPath);
            return sb.ToString();
        }

        var layer = controller.layers[0];
        var sm = layer.stateMachine;

        // Eliminar estados huérfanos que confunden al usuario en el editor visual de Animator
        var statesToRemove = sm.states
            .Where(s => s.state.name != "Idle" && s.state.name != "Walk")
            .Select(s => s.state)
            .ToList();

        foreach (var st in statesToRemove)
        {
            sb.AppendLine($"Removiendo estado huérfano suelto: {st.name}");
            sm.RemoveState(st);
        }

        // Configurar BlendTrees y transiciones
        AnimatorState idleState = null;
        AnimatorState walkState = null;

        foreach (var stateChild in sm.states)
        {
            var state = stateChild.state;
            if (state.name == "Idle") idleState = state;
            if (state.name == "Walk") walkState = state;

            if (state.motion is BlendTree bt)
            {
                if (state.name == "Walk")
                {
                    bt.blendType = BlendTreeType.SimpleDirectional2D;
                    bt.blendParameter = "MoveX";
                    bt.blendParameterY = "MoveY";

                    var children = new ChildMotion[4];
                    children[0] = new ChildMotion { motion = walkDown,  position = new Vector2(0f, -1f), timeScale = 1f };
                    children[1] = new ChildMotion { motion = walkUp,    position = new Vector2(0f,  1f), timeScale = 1f };
                    children[2] = new ChildMotion { motion = walkLeft,  position = new Vector2(-1f, 0f), timeScale = 1f };
                    children[3] = new ChildMotion { motion = walkRight, position = new Vector2( 1f, 0f), timeScale = 1f };
                    bt.children = children;
                    sb.AppendLine("Walk BlendTree configurado: Down(0,-1), Up(0,1), Left(-1,0), Right(1,0)");
                }
                else if (state.name == "Idle")
                {
                    bt.blendType = BlendTreeType.SimpleDirectional2D;
                    bt.blendParameter = "MoveX";
                    bt.blendParameterY = "MoveY";

                    var children = new ChildMotion[4];
                    children[0] = new ChildMotion { motion = idleDown,  position = new Vector2(0f, -1f), timeScale = 1f };
                    children[1] = new ChildMotion { motion = idleUp,    position = new Vector2(0f,  1f), timeScale = 1f };
                    children[2] = new ChildMotion { motion = idleLeft,  position = new Vector2(-1f, 0f), timeScale = 1f };
                    children[3] = new ChildMotion { motion = idleRight, position = new Vector2( 1f, 0f), timeScale = 1f };
                    bt.children = children;
                    sb.AppendLine("Idle BlendTree configurado: Down(0,-1), Up(0,1), Left(-1,0), Right(1,0)");
                }
            }
        }

        // Asegurar transiciones inmediatas sin retraso (0s duración para pixel art)
        if (idleState != null && walkState != null)
        {
            sm.defaultState = idleState;

            foreach (var tr in idleState.transitions)
            {
                tr.duration = 0f;
                tr.hasExitTime = false;
                tr.exitTime = 0f;
            }
            foreach (var tr in walkState.transitions)
            {
                tr.duration = 0f;
                tr.hasExitTime = false;
                tr.exitTime = 0f;
            }
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(ControllerPath, ImportAssetOptions.ForceUpdate);

        sb.AppendLine("¡PlayerController y BlendTrees actualizados con éxito!");
        return sb.ToString();
    }

    static void EnsureWalkLeft4Frames(AnimationClip walkLeft)
    {
        if (walkLeft == null) return;
        var bindings = AnimationUtility.GetObjectReferenceCurveBindings(walkLeft);
        if (bindings.Length == 0) return;

        var keyframes = AnimationUtility.GetObjectReferenceCurve(walkLeft, bindings[0]);
        if (keyframes.Length < 4)
        {
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Player/Carlos_Caminando.png").OfType<Sprite>().ToArray();
            var s8  = sprites.FirstOrDefault(s => s.name == "Carlos_Caminando_8");
            var s9  = sprites.FirstOrDefault(s => s.name == "Carlos_Caminando_9");
            var s10 = sprites.FirstOrDefault(s => s.name == "Carlos_Caminando_10");
            var s11 = sprites.FirstOrDefault(s => s.name == "Carlos_Caminando_11");

            if (s8 != null && s9 != null && s10 != null && s11 != null)
            {
                var newKeys = new ObjectReferenceKeyframe[4];
                newKeys[0] = new ObjectReferenceKeyframe { time = 0.000f, value = s8 };
                newKeys[1] = new ObjectReferenceKeyframe { time = 0.125f, value = s9 };
                newKeys[2] = new ObjectReferenceKeyframe { time = 0.250f, value = s10 };
                newKeys[3] = new ObjectReferenceKeyframe { time = 0.375f, value = s11 };

                AnimationUtility.SetObjectReferenceCurve(walkLeft, bindings[0], newKeys);
                EditorUtility.SetDirty(walkLeft);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(WalkLeftPath, ImportAssetOptions.ForceUpdate);
            }
        }
    }

    static void EnsureWalkRight4Frames(AnimationClip walkRight)
    {
        if (walkRight == null) return;
        var bindings = AnimationUtility.GetObjectReferenceCurveBindings(walkRight);
        if (bindings.Length == 0) return;

        var keyframes = AnimationUtility.GetObjectReferenceCurve(walkRight, bindings[0]);
        if (keyframes.Length < 4)
        {
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
