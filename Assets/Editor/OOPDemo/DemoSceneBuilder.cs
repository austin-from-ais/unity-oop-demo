using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// One-click setup for the OOP demo scenes.
/// Menu: OOP Demo > Build Demo Arena                    (bare arena, no scripts)
///       OOP Demo > Build Scene 1 - Cold Open           (Lecture01: hero pokes the skeleton's fields)
///       OOP Demo > Build Scene 2 - Classes and Objects (Lecture02: Enemy class, spawner loop)
///       OOP Demo > Build Scene 3 - Encapsulation       (Lecture03: opens broken, fix by making health private)
///
/// Builds (idempotently, overwriting previous output):
///   Assets/KayKit/Animators/Adventurer.controller
///   Assets/KayKit/Animators/Skeleton.controller
///   Assets/Prefabs/Characters/Knight.prefab          (sword + shield in hand sockets)
///   Assets/Prefabs/Characters/SkeletonWarrior.prefab (blade + shield)
///   Assets/Prefabs/Characters/SkeletonMinion.prefab  (unarmed)
///   Assets/Materials/Ground.mat
///   Assets/Scenes/DemoArena.unity
///   Assets/Scenes/Scene1_ColdOpen.unity
///   Assets/Scenes/Scene2_ClassesAndObjects.unity
///   Assets/Scenes/Scene3_Encapsulation.unity
///
/// Animator parameters (identical on both controllers so demo scripts can treat them alike):
///   bool    "Walking"
///   trigger "Attack"
///   trigger "Hit"
///   trigger "Die"
/// </summary>
public static class DemoSceneBuilder
{
    const string KayKit      = "Assets/KayKit";
    const string AnimDir     = KayKit + "/Animations/Rig_Medium";
    const string AnimatorDir = KayKit + "/Animators";
    const string PrefabDir   = "Assets/Prefabs/Characters";
    const string MaterialDir = "Assets/Materials";
    const string ScenePath   = "Assets/Scenes/DemoArena.unity";

    // Clips that must loop.
    static readonly string[] LoopingClips =
    {
        "Idle_A", "Idle_B", "Walking_A", "Walking_B", "Running_A",
        "Skeletons_Idle", "Skeletons_Walking", "Melee_Unarmed_Idle",
    };

    const string ColdOpenScenePath = "Assets/Scenes/Scene1_ColdOpen.unity";
    const string ClassesScenePath  = "Assets/Scenes/Scene2_ClassesAndObjects.unity";
    const string EncapScenePath    = "Assets/Scenes/Scene3_Encapsulation.unity";
    const string EnemyPrefabDir    = "Assets/Prefabs/Lecture02";
    const string EncapPrefabDir    = "Assets/Prefabs/Lecture03";

    enum Mode { Arena, ColdOpen, Classes, Encapsulation }

    /// Empty arena: ground, foliage, a knight and a skeleton standing there. No scripts.
    [MenuItem("OOP Demo/Build Demo Arena")]
    public static void BuildArena() => Build(Mode.Arena, ScenePath);

    /// Scene 1: click-to-move hero, skeleton with SkeletonEnemy + health bar, follow camera.
    [MenuItem("OOP Demo/Build Scene 1 - Cold Open")]
    public static void BuildColdOpen() => Build(Mode.ColdOpen, ColdOpenScenePath);

    /// Scene 2: Enemy class, two hand-placed instances with different values, spawner with a loop.
    [MenuItem("OOP Demo/Build Scene 2 - Classes and Objects")]
    public static void BuildClasses() => Build(Mode.Classes, ClassesScenePath);

    /// Scene 3: opens BROKEN. Three scripts write enemy.health directly. Fix live by making it private.
    [MenuItem("OOP Demo/Build Scene 3 - Encapsulation")]
    public static void BuildEncapsulation() => Build(Mode.Encapsulation, EncapScenePath);

    static void Build(Mode mode, string scenePath)
    {
        var (knight, skeleton) = BuildAssets();
        BuildScene(knight, skeleton, scenePath, mode);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[OOP Demo] Built " + mode + " -> " + scenePath);
    }

    /// Animator controllers, prefabs, ground material. Safe to re-run.
    static (GameObject knight, GameObject skeleton) BuildAssets()
    {
        EnsureFolders();
        ConfigureAnimationImports();

        var adventurerCtrl = BuildController(
            AnimatorDir + "/Adventurer.controller",
            idle: "Idle_A", walk: "Walking_A",
            attack: "Melee_1H_Attack_Slice_Horizontal", hit: "Hit_A", die: "Death_A");

        var skeletonCtrl = BuildController(
            AnimatorDir + "/Skeleton.controller",
            idle: "Skeletons_Idle", walk: "Skeletons_Walking",
            attack: "Melee_1H_Attack_Chop", hit: "Hit_B", die: "Skeletons_Death");

        var knight = BuildCharacterPrefab(
            KayKit + "/Adventurers/Characters/Knight.fbx", "Knight", adventurerCtrl,
            rightHand: KayKit + "/Adventurers/Props/sword_1handed.fbx",
            leftHand:  KayKit + "/Adventurers/Props/shield_round.fbx");

        var skeletonWarrior = BuildCharacterPrefab(
            KayKit + "/Skeletons/Characters/Skeleton_Warrior.fbx", "SkeletonWarrior", skeletonCtrl,
            rightHand: KayKit + "/Skeletons/Props/Skeleton_Blade.fbx",
            leftHand:  KayKit + "/Skeletons/Props/Skeleton_Shield_Small_A.fbx");

        var skeletonMinion = BuildCharacterPrefab(
            KayKit + "/Skeletons/Characters/Skeleton_Minion.fbx", "SkeletonMinion", skeletonCtrl,
            rightHand: null, leftHand: null);

        // The unarmed minion is the default enemy: "he has no items to defend himself".
        return (knight, skeletonMinion);
    }

    // ---------------------------------------------------------------- folders

    static void EnsureFolders()
    {
        foreach (var dir in new[] { AnimatorDir, PrefabDir, EnemyPrefabDir, EncapPrefabDir, MaterialDir, "Assets/Scenes" })
        {
            if (!AssetDatabase.IsValidFolder(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.ImportAsset(dir);
            }
        }
    }

    // ------------------------------------------------------- import settings

    /// Mark idle/walk clips as looping, keep everything Generic, lock root so characters stay planted.
    static void ConfigureAnimationImports()
    {
        foreach (var fbx in Directory.GetFiles(AnimDir, "*.fbx"))
        {
            var path = fbx.Replace('\\', '/');
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) continue;

            var clips = importer.clipAnimations;
            bool firstTime = clips == null || clips.Length == 0;
            if (firstTime) clips = importer.defaultClipAnimations;

            bool changed = firstTime;
            foreach (var c in clips)
            {
                bool shouldLoop = LoopingClips.Contains(c.name);
                if (c.loopTime != shouldLoop) { c.loopTime = shouldLoop; changed = true; }
                if (!c.lockRootPositionXZ) { c.lockRootPositionXZ = true; changed = true; }
                if (!c.lockRootHeightY)    { c.lockRootHeightY = true;    changed = true; }
                if (!c.lockRootRotation)   { c.lockRootRotation = true;   changed = true; }
            }

            if (importer.animationType != ModelImporterAnimationType.Generic)
            {
                importer.animationType = ModelImporterAnimationType.Generic;
                changed = true;
            }
            if (importer.materialImportMode != ModelImporterMaterialImportMode.None)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.None; // mannequin needs no materials
                changed = true;
            }

            if (changed)
            {
                importer.clipAnimations = clips;
                importer.SaveAndReimport();
            }
        }
    }

    // ------------------------------------------------------------ animators

    static AnimationClip FindClip(string clipName)
    {
        foreach (var fbx in Directory.GetFiles(AnimDir, "*.fbx"))
        {
            var path = fbx.Replace('\\', '/');
            var clip = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .FirstOrDefault(c => c.name == clipName);
            if (clip != null) return clip;
        }
        Debug.LogError("[OOP Demo] Clip not found: " + clipName);
        return null;
    }

    static AnimatorController BuildController(string path, string idle, string walk, string attack, string hit, string die)
    {
        AssetDatabase.DeleteAsset(path);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);

        ctrl.AddParameter("Walking", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Attack",  AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Hit",     AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Die",     AnimatorControllerParameterType.Trigger);

        var sm = ctrl.layers[0].stateMachine;

        var idleState   = AddState(sm, "Idle",   idle,   new Vector3(300, 0));
        var walkState   = AddState(sm, "Walk",   walk,   new Vector3(300, 100));
        var attackState = AddState(sm, "Attack", attack, new Vector3(600, 0));
        var hitState    = AddState(sm, "Hit",    hit,    new Vector3(600, 100));
        var deadState   = AddState(sm, "Dead",   die,    new Vector3(600, 200));
        sm.defaultState = idleState;

        // Idle <-> Walk on the bool
        var toWalk = idleState.AddTransition(walkState);
        toWalk.AddCondition(AnimatorConditionMode.If, 0, "Walking");
        toWalk.hasExitTime = false;
        toWalk.duration = 0.15f;

        var toIdle = walkState.AddTransition(idleState);
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "Walking");
        toIdle.hasExitTime = false;
        toIdle.duration = 0.15f;

        // Any state -> Attack / Hit / Dead on triggers
        AnyTo(sm, attackState, "Attack");
        AnyTo(sm, hitState,    "Hit");
        AnyTo(sm, deadState,   "Die");

        // Attack / Hit return to Idle when the clip finishes
        ExitTo(attackState, idleState);
        ExitTo(hitState,    idleState);

        EditorUtility.SetDirty(ctrl);
        return ctrl;
    }

    static AnimatorState AddState(AnimatorStateMachine sm, string stateName, string clipName, Vector3 pos)
    {
        var state = sm.AddState(stateName, pos);
        state.motion = FindClip(clipName);
        return state;
    }

    static void AnyTo(AnimatorStateMachine sm, AnimatorState target, string trigger)
    {
        var t = sm.AddAnyStateTransition(target);
        t.AddCondition(AnimatorConditionMode.If, 0, trigger);
        t.hasExitTime = false;
        t.duration = 0.05f;
        t.canTransitionToSelf = false;
    }

    static void ExitTo(AnimatorState from, AnimatorState to)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = true;
        t.exitTime = 0.9f;
        t.duration = 0.1f;
    }

    // -------------------------------------------------------------- prefabs

    static GameObject BuildCharacterPrefab(string modelPath, string prefabName, AnimatorController ctrl, string rightHand, string leftHand)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (model == null)
        {
            Debug.LogError("[OOP Demo] Model missing: " + modelPath);
            return null;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
        instance.name = prefabName;

        var animator = instance.GetComponent<Animator>();
        if (animator == null) animator = instance.AddComponent<Animator>();
        animator.runtimeAnimatorController = ctrl;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        AttachToSocket(instance, "handslot.r", rightHand);
        AttachToSocket(instance, "handslot.l", leftHand);

        // Simple capsule so demo scripts can raycast / overlap against characters.
        var col = instance.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, 0.9f, 0f);
        col.height = 1.8f;
        col.radius = 0.35f;

        var prefabPath = PrefabDir + "/" + prefabName + ".prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        Object.DestroyImmediate(instance);
        return prefab;
    }

    static void AttachToSocket(GameObject root, string socketName, string propPath)
    {
        if (string.IsNullOrEmpty(propPath)) return;

        var socket = FindDeep(root.transform, socketName);
        if (socket == null)
        {
            Debug.LogWarning("[OOP Demo] Socket not found: " + socketName + " on " + root.name);
            return;
        }

        var prop = AssetDatabase.LoadAssetAtPath<GameObject>(propPath);
        if (prop == null)
        {
            Debug.LogWarning("[OOP Demo] Prop missing: " + propPath);
            return;
        }

        var propInstance = (GameObject)PrefabUtility.InstantiatePrefab(prop);
        propInstance.transform.SetParent(socket, false);
        propInstance.transform.localPosition = Vector3.zero;
        propInstance.transform.localRotation = Quaternion.identity;
        propInstance.transform.localScale = Vector3.one;
    }

    static Transform FindDeep(Transform t, string name)
    {
        if (t.name == name) return t;
        for (int i = 0; i < t.childCount; i++)
        {
            var r = FindDeep(t.GetChild(i), name);
            if (r != null) return r;
        }
        return null;
    }

    /// Prefab variant of a character prefab with an enemy component + HealthBar attached and stats filled in.
    /// Fields are written through SerializedObject so this keeps working whether they are public
    /// or [SerializeField] private (scene 3 has you change that).
    static T MakeEnemyPrefab<T>(GameObject basePrefab, string dir, string fileName,
                                string enemyName, int maxHealth, int defence) where T : Component
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
        instance.name = fileName;

        var bar   = instance.AddComponent<HealthBar>();
        var enemy = instance.AddComponent<T>();

        var so = new SerializedObject(enemy);
        SetString(so, "enemyName", enemyName);
        SetInt(so, "maxHealth", maxHealth);
        SetInt(so, "defence", defence);
        SetObj(so, "animator", instance.GetComponent<Animator>());
        SetObj(so, "healthBar", bar);
        so.ApplyModifiedPropertiesWithoutUndo();

        var path  = dir + "/" + fileName + ".prefab";
        var saved = PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        return saved.GetComponent<T>();
    }

    static void SetInt(SerializedObject so, string name, int v)       { var p = so.FindProperty(name); if (p != null) p.intValue = v; }
    static void SetFloat(SerializedObject so, string name, float v)   { var p = so.FindProperty(name); if (p != null) p.floatValue = v; }
    static void SetString(SerializedObject so, string name, string v) { var p = so.FindProperty(name); if (p != null) p.stringValue = v; }
    static void SetObj(SerializedObject so, string name, Object v)    { var p = so.FindProperty(name); if (p != null) p.objectReferenceValue = v; }

    /// Flat coloured disc on the ground marking an area effect, with the given component on it.
    static GameObject MakeAreaMarker<T>(string name, Vector3 pos, float radius, Material mat) where T : Component
    {
        var go = new GameObject(name);
        go.transform.position = pos;

        var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.name = "Disc";
        Object.DestroyImmediate(disc.GetComponent<Collider>());     // visual only; must not block clicks
        disc.transform.SetParent(go.transform, false);
        disc.transform.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);
        disc.transform.localPosition = new Vector3(0f, 0.02f, 0f);
        disc.GetComponent<Renderer>().sharedMaterial = mat;

        var comp = go.AddComponent<T>();
        var so = new SerializedObject(comp);
        SetFloat(so, "radius", radius);
        so.ApplyModifiedPropertiesWithoutUndo();
        return go;
    }

    // ---------------------------------------------------------------- scene

    static void BuildScene(GameObject knightPrefab, GameObject skeletonPrefab, string scenePath, Mode mode)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Camera: elevated three-quarter view
        var cam = Camera.main;
        cam.transform.position = new Vector3(0f, 7f, -7f);
        cam.transform.LookAt(new Vector3(0f, 0.8f, 0f));
        cam.fieldOfView = 45f;

        // Light: warm sun
        var light = Object.FindAnyObjectByType<Light>();
        if (light != null)
        {
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.color = new Color(1f, 0.96f, 0.88f);
            light.intensity = 1.6f;
            light.shadows = LightShadows.Soft;
        }

        // Ground: 40 x 40 m plane
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(4f, 1f, 4f);
        ground.isStatic = true;
        ground.GetComponent<Renderer>().sharedMaterial = GroundMaterial();

        // Characters
        var chars = new GameObject("Characters");

        var knight = (GameObject)PrefabUtility.InstantiatePrefab(knightPrefab);
        knight.name = "Adventurer";
        knight.transform.SetParent(chars.transform);

        if (mode == Mode.Classes)
        {
            // Scene 2 places Enemy prefab instances instead of the bare skeleton.
            knight.transform.position = new Vector3(-3f, 0f, -1f);
            knight.transform.rotation = Quaternion.Euler(0f, 60f, 0f);

            var hero = knight.AddComponent<Lecture02.Hero>();
            hero.animator = knight.GetComponent<Animator>();
            hero.cam = cam;

            var follow = cam.gameObject.AddComponent<FollowCamera>();
            follow.target = knight.transform;

            var minionPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/SkeletonMinion.prefab");
            var warriorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/SkeletonWarrior.prefab");

            var minionEnemy  = MakeEnemyPrefab<Lecture02.Enemy>(minionPrefab,  EnemyPrefabDir, "Enemy_Skeleton",        "Skeleton",         100, 0);
            var warriorEnemy = MakeEnemyPrefab<Lecture02.Enemy>(warriorPrefab, EnemyPrefabDir, "Enemy_SkeletonWarrior", "Skeleton Warrior", 300, 5);

            // Two hand-placed objects from the same blueprint, different values in the Inspector.
            var a = (GameObject)PrefabUtility.InstantiatePrefab(minionEnemy.gameObject);
            a.name = "Skeleton A";
            a.transform.SetParent(chars.transform);
            a.transform.position = new Vector3(3f, 0f, 2f);
            a.transform.rotation = Quaternion.Euler(0f, -110f, 0f);

            var b = (GameObject)PrefabUtility.InstantiatePrefab(warriorEnemy.gameObject);
            b.name = "Skeleton B";
            b.transform.SetParent(chars.transform);
            b.transform.position = new Vector3(2f, 0f, -3f);
            b.transform.rotation = Quaternion.Euler(0f, -70f, 0f);

            // Spawner, present but switched off. Tick its checkbox to spawn ten enemies.
            var spawnerGo = new GameObject("Enemy Spawner");
            spawnerGo.transform.position = new Vector3(9f, 0f, 0f);
            var spawner = spawnerGo.AddComponent<Lecture02.EnemySpawner>();
            spawner.enemyPrefab = minionEnemy;
            spawner.count = 10;
            spawner.enabled = false;

            ScatterFoliage();
            EditorSceneManager.SaveScene(scene, scenePath);
            return;
        }

        if (mode == Mode.Encapsulation)
        {
            knight.transform.position = new Vector3(-3f, 0f, 0f);
            knight.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            var hero = knight.AddComponent<Lecture03.Hero>();
            hero.animator = knight.GetComponent<Animator>();
            hero.cam = cam;

            var follow = cam.gameObject.AddComponent<FollowCamera>();
            follow.target = knight.transform;

            var minionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/SkeletonMinion.prefab");
            var enemyPrefab  = MakeEnemyPrefab<Lecture03.Enemy>(minionPrefab, EncapPrefabDir, "Enemy_Skeleton", "Skeleton", 100, 0);

            var poisonPos = new Vector3(3f, 0f, 2.5f);
            var shrinePos = new Vector3(3f, 0f, -2.5f);

            PlaceEnemy(enemyPrefab.gameObject, chars.transform, "Skeleton A", poisonPos, -110f);          // standing in poison
            PlaceEnemy(enemyPrefab.gameObject, chars.transform, "Skeleton B", shrinePos, -70f);           // standing at the shrine
            PlaceEnemy(enemyPrefab.gameObject, chars.transform, "Skeleton C", new Vector3(0f, 0f, 4.5f), 180f);

            var poisonMat = ColoredMaterial("Poison", new Color(0.35f, 0.9f, 0.3f));
            var shrineMat = ColoredMaterial("Shrine", new Color(1f, 0.8f, 0.25f));
            MakeAreaMarker<Lecture03.PoisonCloud>("Poison Cloud", poisonPos, 1.5f, poisonMat);
            MakeAreaMarker<Lecture03.HealingShrine>("Healing Shrine", shrinePos, 1.5f, shrineMat);

            new GameObject("Debug Cheats").AddComponent<Lecture03.DebugCheats>();

            ScatterFoliage();
            EditorSceneManager.SaveScene(scene, scenePath);
            return;
        }

        var skel = (GameObject)PrefabUtility.InstantiatePrefab(skeletonPrefab);
        skel.name = "Skeleton";
        skel.transform.SetParent(chars.transform);

        if (mode == Mode.ColdOpen)
        {
            // Hero starts a short walk away so the click-to-approach is visible.
            knight.transform.position = new Vector3(-3f, 0f, -1f);
            knight.transform.rotation = Quaternion.Euler(0f, 60f, 0f);

            skel.transform.position = new Vector3(4f, 0f, 2f);
            skel.transform.rotation = Quaternion.Euler(0f, -110f, 0f);

            var hero = knight.AddComponent<Lecture01.HeroController>();
            hero.animator = knight.GetComponent<Animator>();
            hero.cam = cam;

            var bar = skel.AddComponent<HealthBar>();
            var enemy = skel.AddComponent<Lecture01.SkeletonEnemy>();
            enemy.animator = skel.GetComponent<Animator>();
            enemy.healthBar = bar;

            var follow = cam.gameObject.AddComponent<FollowCamera>();
            follow.target = knight.transform;
        }
        else
        {
            knight.transform.position = new Vector3(-1.2f, 0f, 0f);
            knight.transform.rotation = Quaternion.Euler(0f, 90f, 0f);   // face +X toward the skeleton

            skel.transform.position = new Vector3(1.2f, 0f, 0f);
            skel.transform.rotation = Quaternion.Euler(0f, -90f, 0f);   // face -X toward the knight
        }

        ScatterFoliage();

        EditorSceneManager.SaveScene(scene, scenePath);
    }

    static Material GroundMaterial() => ColoredMaterial("Ground", new Color(0.36f, 0.55f, 0.27f));

    static Material ColoredMaterial(string name, Color color)
    {
        var path = MaterialDir + "/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.05f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static GameObject PlaceEnemy(GameObject prefab, Transform parent, string name, Vector3 pos, float yaw)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        return go;
    }

    static void ScatterFoliage()
    {
        var forest = KayKit + "/Forest";
        var trees  = Load(forest, "Tree_1_A", "Tree_1_B", "Tree_2_A", "Tree_2_C", "Tree_3_A", "Tree_4_B");
        var bushes = Load(forest, "Bush_1_A", "Bush_1_C", "Bush_2_B", "Bush_3_A", "Bush_4_A");
        var rocks  = Load(forest, "Rock_1_A", "Rock_1_D", "Rock_2_B", "Rock_3_C", "Rock_3_G");
        var grass  = Load(forest, "Grass_1_A", "Grass_1_C", "Grass_2_A", "Grass_2_D");

        var parent = new GameObject("Foliage");
        var rng = new System.Random(42);

        // Keep the inner few metres clear so the fight is readable.
        Place(parent, rng, trees,  22, 10f, 18f, 0.9f, 1.3f, "Trees");
        Place(parent, rng, bushes, 18,  5f, 16f, 0.8f, 1.2f, "Bushes");
        Place(parent, rng, rocks,  10,  6f, 17f, 0.7f, 1.4f, "Rocks");
        Place(parent, rng, grass,  40,  3f, 15f, 0.8f, 1.2f, "Grass");
    }

    static void Place(GameObject parent, System.Random rng, GameObject[] set, int count,
                      float minR, float maxR, float minScale, float maxScale, string group)
    {
        if (set.Length == 0) return;

        var g = new GameObject(group);
        g.transform.SetParent(parent.transform);

        for (int i = 0; i < count; i++)
        {
            var prefab = set[rng.Next(set.Length)];
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.SetParent(g.transform);

            float angle = Rand(rng, 0f, Mathf.PI * 2f);
            float r = Rand(rng, minR, maxR);
            go.transform.position = new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);
            go.transform.rotation = Quaternion.Euler(0f, Rand(rng, 0f, 360f), 0f);
            go.transform.localScale = Vector3.one * Rand(rng, minScale, maxScale);
            go.isStatic = true;
        }
    }

    static float Rand(System.Random rng, float a, float b)
    {
        return (float)(a + rng.NextDouble() * (b - a));
    }

    static GameObject[] Load(string dir, params string[] stems)
    {
        var list = new List<GameObject>();
        foreach (var stem in stems)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(dir + "/" + stem + "_Color1.fbx");
            if (go != null) list.Add(go);
            else Debug.LogWarning("[OOP Demo] Foliage missing: " + stem);
        }
        return list.ToArray();
    }
}
