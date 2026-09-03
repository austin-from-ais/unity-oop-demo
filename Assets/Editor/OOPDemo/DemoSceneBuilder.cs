using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// The script behind the "OOP Demo" menu. Each item rebuilds one lecture scene from scratch.
///
///   OOP Demo > Build Demo Arena                    bare arena, no scripts
///   OOP Demo > Build Scene 1 - Cold Open           Lecture01: the hero pokes the skeleton's fields
///   OOP Demo > Build Scene 2 - Classes and Objects Lecture02: Enemy class, spawner loop
///   OOP Demo > Build Scene 3 - Encapsulation       Lecture03: opens broken, fix by making health private
///   OOP Demo > Build Scene 4 - Inheritance         Lecture04: FlyingEnemy : Enemy
///   OOP Demo > Build Scene 5 - Polymorphism        Lecture05: List&lt;Enemy&gt;, one loop
///   OOP Demo > Build Scene 6 - Composition         Lecture06: IDamageable, GameObjects as bags of parts
///
/// Also generates (safe to re-run): animator controllers, character prefabs, per-lecture enemy
/// prefab variants, materials. Prefab fields are written through SerializedObject so the builder
/// keeps compiling when a lecture changes a field from public to [SerializeField] private.
///
/// Animator parameters (identical on both controllers): bool "Walking", triggers "Attack" "Hit" "Die".
/// </summary>
public static class DemoSceneBuilder
{
    const string KayKit        = "Assets/KayKit";
    const string AnimDir       = KayKit + "/Animations/Rig_Medium";
    const string AnimatorDir   = KayKit + "/Animators";
    const string CharPrefabDir = "Assets/Prefabs/Characters";
    const string MaterialDir   = "Assets/Materials";
    const string SceneDir      = "Assets/Scenes";

    static readonly string[] LoopingClips =
    {
        "Idle_A", "Idle_B", "Walking_A", "Walking_B", "Running_A",
        "Skeletons_Idle", "Skeletons_Walking", "Melee_Unarmed_Idle",
    };

    enum Mode { Arena, ColdOpen, Classes, Encapsulation, Inheritance, Polymorphism, Composition }

    struct Chars
    {
        public GameObject knight, minion, warrior, mage, mageShielded;
    }

    // ================================================================== menu

    [MenuItem("OOP Demo/Build Demo Arena")]
    public static void BuildArena() => Build(Mode.Arena, "DemoArena");

    [MenuItem("OOP Demo/Build Scene 1 - Cold Open")]
    public static void BuildColdOpen() => Build(Mode.ColdOpen, "Scene1_ColdOpen");

    [MenuItem("OOP Demo/Build Scene 2 - Classes and Objects")]
    public static void BuildClasses() => Build(Mode.Classes, "Scene2_ClassesAndObjects");

    [MenuItem("OOP Demo/Build Scene 3 - Encapsulation")]
    public static void BuildEncapsulation() => Build(Mode.Encapsulation, "Scene3_Encapsulation");

    [MenuItem("OOP Demo/Build Scene 4 - Inheritance")]
    public static void BuildInheritance() => Build(Mode.Inheritance, "Scene4_Inheritance");

    [MenuItem("OOP Demo/Build Scene 5 - Polymorphism")]
    public static void BuildPolymorphism() => Build(Mode.Polymorphism, "Scene5_Polymorphism");

    [MenuItem("OOP Demo/Build Scene 6 - Composition")]
    public static void BuildComposition() => Build(Mode.Composition, "Scene6_Composition");

    static void Build(Mode mode, string sceneName)
    {
        var chars = BuildAssets();
        var path  = SceneDir + "/" + sceneName + ".unity";
        BuildScene(mode, chars, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[OOP Demo] Built " + mode + " -> " + path);
    }

    // ================================================================ assets

    static Chars BuildAssets()
    {
        foreach (var dir in new[] { AnimatorDir, CharPrefabDir, MaterialDir, SceneDir })
            EnsureFolder(dir);

        ConfigureAnimationImports();

        var adventurerCtrl = BuildController(
            AnimatorDir + "/Adventurer.controller",
            idle: "Idle_A", walk: "Walking_A",
            attack: "Melee_1H_Attack_Slice_Horizontal", hit: "Hit_A", die: "Death_A");

        var skeletonCtrl = BuildController(
            AnimatorDir + "/Skeleton.controller",
            idle: "Skeletons_Idle", walk: "Skeletons_Walking",
            attack: "Melee_1H_Attack_Chop", hit: "Hit_B", die: "Skeletons_Death");

        var c = new Chars();

        c.knight = BuildCharacterPrefab(
            KayKit + "/Adventurers/Characters/Knight.fbx", "Knight", adventurerCtrl,
            rightHand: KayKit + "/Adventurers/Props/sword_1handed.fbx",
            leftHand:  KayKit + "/Adventurers/Props/shield_round.fbx");

        c.warrior = BuildCharacterPrefab(
            KayKit + "/Skeletons/Characters/Skeleton_Warrior.fbx", "SkeletonWarrior", skeletonCtrl,
            rightHand: KayKit + "/Skeletons/Props/Skeleton_Blade.fbx",
            leftHand:  KayKit + "/Skeletons/Props/Skeleton_Shield_Small_A.fbx");

        c.minion = BuildCharacterPrefab(
            KayKit + "/Skeletons/Characters/Skeleton_Minion.fbx", "SkeletonMinion", skeletonCtrl,
            rightHand: null, leftHand: null);

        c.mage = BuildCharacterPrefab(
            KayKit + "/Skeletons/Characters/Skeleton_Mage.fbx", "SkeletonMage", skeletonCtrl,
            rightHand: KayKit + "/Skeletons/Props/Skeleton_Staff.fbx",
            leftHand:  null);

        c.mageShielded = BuildCharacterPrefab(
            KayKit + "/Skeletons/Characters/Skeleton_Mage.fbx", "SkeletonMageShielded", skeletonCtrl,
            rightHand: KayKit + "/Skeletons/Props/Skeleton_Staff.fbx",
            leftHand:  KayKit + "/Skeletons/Props/Skeleton_Shield_Small_A.fbx");

        return c;
    }

    static void EnsureFolder(string dir)
    {
        if (AssetDatabase.IsValidFolder(dir)) return;
        Directory.CreateDirectory(dir);
        AssetDatabase.ImportAsset(dir);
    }

    /// Loop idle/walk clips, keep everything Generic, lock root so characters stay planted.
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
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
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

        var toWalk = idleState.AddTransition(walkState);
        toWalk.AddCondition(AnimatorConditionMode.If, 0, "Walking");
        toWalk.hasExitTime = false;
        toWalk.duration = 0.15f;

        var toIdle = walkState.AddTransition(idleState);
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "Walking");
        toIdle.hasExitTime = false;
        toIdle.duration = 0.15f;

        AnyTo(sm, attackState, "Attack");
        AnyTo(sm, hitState,    "Hit");
        AnyTo(sm, deadState,   "Die");

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

    // ------------------------------------------------------- character prefabs

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

        var col = instance.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, 0.9f, 0f);
        col.height = 1.8f;
        col.radius = 0.35f;

        var prefab = PrefabUtility.SaveAsPrefabAsset(instance, CharPrefabDir + "/" + prefabName + ".prefab");
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

    // ------------------------------------------------ serialized field helpers

    static void SetInt(Object target, string field, int v)       { var so = new SerializedObject(target); var p = so.FindProperty(field); if (p != null) { p.intValue = v;             so.ApplyModifiedPropertiesWithoutUndo(); } }
    static void SetFloat(Object target, string field, float v)   { var so = new SerializedObject(target); var p = so.FindProperty(field); if (p != null) { p.floatValue = v;           so.ApplyModifiedPropertiesWithoutUndo(); } }
    static void SetString(Object target, string field, string v) { var so = new SerializedObject(target); var p = so.FindProperty(field); if (p != null) { p.stringValue = v;          so.ApplyModifiedPropertiesWithoutUndo(); } }
    static void SetObj(Object target, string field, Object v)    { var so = new SerializedObject(target); var p = so.FindProperty(field); if (p != null) { p.objectReferenceValue = v; so.ApplyModifiedPropertiesWithoutUndo(); } }

    // ------------------------------------------------------- scene helpers

    /// Save a configured copy of a character prefab as a prefab variant.
    static GameObject MakeVariant(GameObject basePrefab, string dir, string fileName, System.Action<GameObject> configure)
    {
        EnsureFolder(dir);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
        instance.name = fileName;
        configure(instance);
        var saved = PrefabUtility.SaveAsPrefabAsset(instance, dir + "/" + fileName + ".prefab");
        Object.DestroyImmediate(instance);
        return saved;
    }

    /// HealthBar + one enemy-style component with enemyName/maxHealth/defence/animator/healthBar fields.
    static T AddEnemy<T>(GameObject go, string enemyName, int maxHealth, int defence, float barHeight = 2.1f) where T : Component
    {
        var bar = go.AddComponent<HealthBar>();
        bar.heightAboveFeet = barHeight;

        var enemy = go.AddComponent<T>();
        SetString(enemy, "enemyName", enemyName);
        SetInt(enemy, "maxHealth", maxHealth);
        SetInt(enemy, "defence", defence);
        SetObj(enemy, "animator", go.GetComponent<Animator>());
        SetObj(enemy, "healthBar", bar);
        return enemy;
    }

    static GameObject Place(GameObject prefab, Transform parent, string name, Vector3 pos, float yaw)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        return go;
    }

    static T AddHero<T>(GameObject knight, Camera cam) where T : Component
    {
        var hero = knight.AddComponent<T>();
        SetObj(hero, "animator", knight.GetComponent<Animator>());
        SetObj(hero, "cam", cam);

        var follow = cam.gameObject.AddComponent<FollowCamera>();
        follow.target = knight.transform;
        return hero;
    }

    /// Flat coloured disc on the ground marking an area effect, with the given component on it.
    static GameObject MakeAreaMarker<T>(string name, Vector3 pos, float radius, Material mat) where T : Component
    {
        var go = new GameObject(name);
        go.transform.position = pos;

        var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.name = "Disc";
        Object.DestroyImmediate(disc.GetComponent<Collider>());
        disc.transform.SetParent(go.transform, false);
        disc.transform.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);
        disc.transform.localPosition = new Vector3(0f, 0.02f, 0f);
        disc.GetComponent<Renderer>().sharedMaterial = mat;

        var comp = go.AddComponent<T>();
        SetFloat(comp, "radius", radius);
        return go;
    }

    static GameObject MakeCrate(Vector3 pos, float yaw, Material mat)
    {
        var crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate.name = "Crate";
        crate.transform.position = pos + Vector3.up * 0.45f;
        crate.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        crate.transform.localScale = Vector3.one * 0.9f;
        crate.GetComponent<Renderer>().sharedMaterial = mat;
        crate.AddComponent<Lecture06.BreakableCrate>();
        return crate;
    }

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

    // ================================================================= scenes

    static void BuildScene(Mode mode, Chars chars, string scenePath)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var cam = Camera.main;
        cam.transform.position = new Vector3(0f, 7f, -7f);
        cam.transform.LookAt(new Vector3(0f, 0.8f, 0f));
        cam.fieldOfView = 45f;

        var light = Object.FindAnyObjectByType<Light>();
        if (light != null)
        {
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.color = new Color(1f, 0.96f, 0.88f);
            light.intensity = 1.6f;
            light.shadows = LightShadows.Soft;
        }

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(4f, 1f, 4f);   // 40 x 40 m
        ground.isStatic = true;
        ground.GetComponent<Renderer>().sharedMaterial = ColoredMaterial("Ground", new Color(0.36f, 0.55f, 0.27f));

        var charsRoot = new GameObject("Characters").transform;
        var knight = Place(chars.knight, charsRoot, "Adventurer", new Vector3(-3f, 0f, 0f), 90f);

        switch (mode)
        {
            case Mode.Arena:         ArenaScene(chars, knight, charsRoot);              break;
            case Mode.ColdOpen:      ColdOpenScene(chars, knight, charsRoot, cam);      break;
            case Mode.Classes:       ClassesScene(chars, knight, charsRoot, cam);       break;
            case Mode.Encapsulation: EncapsulationScene(chars, knight, charsRoot, cam); break;
            case Mode.Inheritance:   InheritanceScene(chars, knight, charsRoot, cam);   break;
            case Mode.Polymorphism:  PolymorphismScene(chars, knight, charsRoot, cam);  break;
            case Mode.Composition:   CompositionScene(chars, knight, charsRoot, cam);   break;
        }

        ScatterFoliage();
        EditorSceneManager.SaveScene(scene, scenePath);
    }

    // ---- Arena: nothing scripted

    static void ArenaScene(Chars chars, GameObject knight, Transform root)
    {
        knight.transform.position = new Vector3(-1.2f, 0f, 0f);
        Place(chars.minion, root, "Skeleton", new Vector3(1.2f, 0f, 0f), -90f);
    }

    // ---- Scene 1: hero reaches into a bag of fields

    static void ColdOpenScene(Chars chars, GameObject knight, Transform root, Camera cam)
    {
        knight.transform.position = new Vector3(-3f, 0f, -1f);
        knight.transform.rotation = Quaternion.Euler(0f, 60f, 0f);
        AddHero<Lecture01.HeroController>(knight, cam);

        var skel = Place(chars.minion, root, "Skeleton", new Vector3(4f, 0f, 2f), -110f);
        AddEnemy<Lecture01.SkeletonEnemy>(skel, "Skeleton", 100, 0);
    }

    // ---- Scene 2: Enemy class, two instances, spawner

    static void ClassesScene(Chars chars, GameObject knight, Transform root, Camera cam)
    {
        const string dir = "Assets/Prefabs/Lecture02";

        knight.transform.position = new Vector3(-3f, 0f, -1f);
        knight.transform.rotation = Quaternion.Euler(0f, 60f, 0f);
        AddHero<Lecture02.Hero>(knight, cam);

        var minionEnemy  = MakeVariant(chars.minion,  dir, "Enemy_Skeleton",        go => AddEnemy<Lecture02.Enemy>(go, "Skeleton",         100, 0));
        var warriorEnemy = MakeVariant(chars.warrior, dir, "Enemy_SkeletonWarrior", go => AddEnemy<Lecture02.Enemy>(go, "Skeleton Warrior", 300, 5));

        Place(minionEnemy,  root, "Skeleton A", new Vector3(3f, 0f, 2f),  -110f);
        Place(warriorEnemy, root, "Skeleton B", new Vector3(2f, 0f, -3f), -70f);

        var spawnerGo = new GameObject("Enemy Spawner");
        spawnerGo.transform.position = new Vector3(9f, 0f, 0f);
        var spawner = spawnerGo.AddComponent<Lecture02.EnemySpawner>();
        SetObj(spawner, "enemyPrefab", minionEnemy.GetComponent<Lecture02.Enemy>());
        SetInt(spawner, "count", 10);
        spawner.enabled = false;   // switched on during the scene
    }

    // ---- Scene 3: opens broken

    static void EncapsulationScene(Chars chars, GameObject knight, Transform root, Camera cam)
    {
        const string dir = "Assets/Prefabs/Lecture03";

        AddHero<Lecture03.Hero>(knight, cam);

        var enemyPrefab = MakeVariant(chars.minion, dir, "Enemy_Skeleton", go => AddEnemy<Lecture03.Enemy>(go, "Skeleton", 100, 0));

        var poisonPos = new Vector3(3f, 0f, 2.5f);
        var shrinePos = new Vector3(3f, 0f, -2.5f);

        Place(enemyPrefab, root, "Skeleton A", poisonPos, -110f);
        Place(enemyPrefab, root, "Skeleton B", shrinePos, -70f);
        Place(enemyPrefab, root, "Skeleton C", new Vector3(0f, 0f, 4.5f), 180f);

        MakeAreaMarker<Lecture03.PoisonCloud>("Poison Cloud", poisonPos, 1.5f, ColoredMaterial("Poison", new Color(0.35f, 0.9f, 0.3f)));
        MakeAreaMarker<Lecture03.HealingShrine>("Healing Shrine", shrinePos, 1.5f, ColoredMaterial("Shrine", new Color(1f, 0.8f, 0.25f)));

        new GameObject("Debug Cheats").AddComponent<Lecture03.DebugCheats>();
    }

    // ---- Scene 4: FlyingEnemy : Enemy

    static void InheritanceScene(Chars chars, GameObject knight, Transform root, Camera cam)
    {
        const string dir = "Assets/Prefabs/Lecture04";

        AddHero<Lecture04.Hero>(knight, cam);

        var ground = MakeVariant(chars.minion, dir, "Enemy_Ground", go => AddEnemy<Lecture04.Enemy>(go, "Skeleton", 100, 0));
        var flying = MakeVariant(chars.mage,   dir, "Enemy_Flying", go => AddEnemy<Lecture04.FlyingEnemy>(go, "Skeleton Mage", 60, 0, barHeight: 1.9f));

        Place(ground, root, "Ground Skeleton 1", new Vector3(4f, 0f, 3f),    -120f);
        Place(ground, root, "Ground Skeleton 2", new Vector3(6f, 0f, -2f),   -100f);
        Place(flying, root, "Flying Mage 1",     new Vector3(2f, 1.8f, 7f),  -160f);
        Place(flying, root, "Flying Mage 2",     new Vector3(8f, 1.8f, 1f),  -90f);
    }

    // ---- Scene 5: one list, one loop

    static void PolymorphismScene(Chars chars, GameObject knight, Transform root, Camera cam)
    {
        const string dir = "Assets/Prefabs/Lecture05";

        knight.transform.position = Vector3.zero;
        knight.transform.rotation = Quaternion.identity;
        AddHero<Lecture05.Hero>(knight, cam);

        var ground = MakeVariant(chars.minion,  dir, "Enemy_Ground", go => AddEnemy<Lecture05.GroundEnemy>(go, "Skeleton", 100, 0));
        var flying = MakeVariant(chars.mage,    dir, "Enemy_Flying", go => AddEnemy<Lecture05.FlyingEnemy>(go, "Skeleton Mage", 60, 0, barHeight: 1.9f));
        var boss   = MakeVariant(chars.warrior, dir, "Enemy_Boss",   go =>
        {
            go.transform.localScale = Vector3.one * 1.6f;
            AddEnemy<Lecture05.BossEnemy>(go, "Skeleton Boss", 400, 4, barHeight: 3.4f);
        });

        var hordeGo = new GameObject("Horde");
        hordeGo.transform.position = Vector3.zero;
        var horde = hordeGo.AddComponent<Lecture05.Horde>();
        SetObj(horde, "groundPrefab", ground.GetComponent<Lecture05.GroundEnemy>());
        SetObj(horde, "flyingPrefab", flying.GetComponent<Lecture05.FlyingEnemy>());
        SetObj(horde, "bossPrefab",   boss.GetComponent<Lecture05.BossEnemy>());
    }

    // ---- Scene 6: IDamageable + components

    static void CompositionScene(Chars chars, GameObject knight, Transform root, Camera cam)
    {
        const string dir = "Assets/Prefabs/Lecture06";

        knight.transform.position = new Vector3(-4f, 0f, 0f);
        AddHero<Lecture06.Hero>(knight, cam);
        knight.AddComponent<HealthBar>();
        SetInt(knight.AddComponent<Lecture06.Health>(), "maxHealth", 200);

        // Every enemy is just a different pile of parts on a model.
        var groundSkel = MakeVariant(chars.minion, dir, "Ground Skeleton", go =>
        {
            go.AddComponent<HealthBar>();
            SetInt(go.AddComponent<Lecture06.Health>(), "maxHealth", 100);
            go.AddComponent<Lecture06.GroundMover>();
            go.AddComponent<Lecture06.MeleeAttacker>();
        });

        var flyingSkel = MakeVariant(chars.mage, dir, "Flying Skeleton", go =>
        {
            go.AddComponent<HealthBar>().heightAboveFeet = 1.9f;
            SetInt(go.AddComponent<Lecture06.Health>(), "maxHealth", 60);
            go.AddComponent<Lecture06.FlyingMover>();
            go.AddComponent<Lecture06.Shooter>();
        });

        var shieldedSkel = MakeVariant(chars.warrior, dir, "Shielded Skeleton", go =>
        {
            go.AddComponent<HealthBar>();
            SetInt(go.AddComponent<Lecture06.Health>(), "maxHealth", 120);
            var shield = go.AddComponent<Lecture06.Shield>();
            SetInt(shield, "strength", 40);
            var shieldMesh = FindDeep(go.transform, "Skeleton_Shield_Small_A");
            if (shieldMesh != null) SetObj(shield, "visual", shieldMesh.gameObject);
            go.AddComponent<Lecture06.Armour>();
            go.AddComponent<Lecture06.GroundMover>();
            go.AddComponent<Lecture06.MeleeAttacker>();
        });

        var fssSkel = MakeVariant(chars.mageShielded, dir, "Flying Shooting Shielded Skeleton", go =>
        {
            go.AddComponent<HealthBar>().heightAboveFeet = 1.9f;
            SetInt(go.AddComponent<Lecture06.Health>(), "maxHealth", 80);
            var shield = go.AddComponent<Lecture06.Shield>();
            SetInt(shield, "strength", 30);
            var shieldMesh = FindDeep(go.transform, "Skeleton_Shield_Small_A");
            if (shieldMesh != null) SetObj(shield, "visual", shieldMesh.gameObject);
            go.AddComponent<Lecture06.FlyingMover>();
            go.AddComponent<Lecture06.Shooter>();
        });

        Place(groundSkel,   root, "Ground Skeleton",                   new Vector3(5f, 0f, 2f),    -110f);
        Place(shieldedSkel, root, "Shielded Skeleton",                 new Vector3(6f, 0f, -3f),   -100f);
        Place(flyingSkel,   root, "Flying Skeleton",                   new Vector3(2f, 1.8f, 7f),  -170f);
        Place(fssSkel,      root, "Flying Shooting Shielded Skeleton", new Vector3(9f, 1.8f, 0f),  -90f);

        var crateMat = ColoredMaterial("Crate", new Color(0.55f, 0.36f, 0.18f));
        var crates = new GameObject("Crates").transform;
        MakeCrate(new Vector3(-1f, 0f, 4f),  15f, crateMat).transform.SetParent(crates);
        MakeCrate(new Vector3(1f, 0f, -5f),  40f, crateMat).transform.SetParent(crates);
        MakeCrate(new Vector3(-3f, 0f, -3f), 70f, crateMat).transform.SetParent(crates);

        MakeAreaMarker<Lecture06.DamageZone>("Poison Cloud", new Vector3(1f, 0f, 3f), 1.5f, ColoredMaterial("Poison", new Color(0.35f, 0.9f, 0.3f)));
    }

    // =============================================================== foliage

    static void ScatterFoliage()
    {
        var forest = KayKit + "/Forest";
        var trees  = Load(forest, "Tree_1_A", "Tree_1_B", "Tree_2_A", "Tree_2_C", "Tree_3_A", "Tree_4_B");
        var bushes = Load(forest, "Bush_1_A", "Bush_1_C", "Bush_2_B", "Bush_3_A", "Bush_4_A");
        var rocks  = Load(forest, "Rock_1_A", "Rock_1_D", "Rock_2_B", "Rock_3_C", "Rock_3_G");
        var grass  = Load(forest, "Grass_1_A", "Grass_1_C", "Grass_2_A", "Grass_2_D");

        var parent = new GameObject("Foliage");
        var rng = new System.Random(42);

        // Keep the inner ring clear so the fight is readable.
        Scatter(parent, rng, trees,  22, 11f, 18f, 0.9f, 1.3f, "Trees");
        Scatter(parent, rng, bushes, 18,  9f, 16f, 0.8f, 1.2f, "Bushes");
        Scatter(parent, rng, rocks,  10,  9f, 17f, 0.7f, 1.4f, "Rocks");
        Scatter(parent, rng, grass,  40,  3f, 15f, 0.8f, 1.2f, "Grass");
    }

    static void Scatter(GameObject parent, System.Random rng, GameObject[] set, int count,
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

    static float Rand(System.Random rng, float a, float b) => (float)(a + rng.NextDouble() * (b - a));

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
