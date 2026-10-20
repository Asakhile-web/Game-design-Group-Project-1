using UnityEngine;

/// <summary>
/// Keypad, combination lock, lamp sequence, UV writing, exit, and a thought trigger for the fallback room.
/// </summary>
public static class Stage3Greybox
{
    public static void Spawn(Transform root)
    {
        if (root == null)
        {
            return;
        }

        ItemDefinition uv = ItemDefinition.CreateRuntime("uv_lamp", "UV Lamp", consume: false,
            "A cheap inspection lamp. The tube hums violet when selected.");

        CreateUvLamp(root, uv);
        CreateKeypad(root);
        CreateCombination(root);
        CreateSequence(root);
        CreateUvWriting(root, uv);
        CreateExit(root);
        CreateThought(root);
        ConfigureHints();
        ConfigureLamp(root);

        ObjectiveTracker tracker = Object.FindFirstObjectByType<ObjectiveTracker>();
        if (tracker != null)
        {
            tracker.SetObjective("Find three ways out: keypad, dials, and the lamps");
        }

        Debug.Log("[Stage3Greybox] Spawned puzzles, UV writing, and exit.");
    }

    static void CreateUvLamp(Transform root, ItemDefinition uv)
    {
        GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lamp.name = "UvLamp";
        lamp.transform.SetParent(root);
        lamp.transform.position = new Vector3(-2.2f, 0.35f, 1.4f);
        lamp.transform.localScale = new Vector3(0.08f, 0.22f, 0.08f);
        ApplyColor(lamp, new Color(0.45f, 0.2f, 0.75f));
        ItemPickup pickup = lamp.AddComponent<ItemPickup>();
        pickup.Definition = uv;
    }

    static void CreateKeypad(Transform root)
    {
        GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.name = "Keypad";
        pad.transform.SetParent(root);
        pad.transform.position = new Vector3(-3.2f, 1.2f, 2.4f);
        pad.transform.localScale = new Vector3(0.25f, 0.35f, 0.08f);
        ApplyColor(pad, new Color(0.12f, 0.12f, 0.14f));
        KeypadLock lockComp = pad.AddComponent<KeypadLock>();
        _ = lockComp;
    }

    static void CreateCombination(Transform root)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = "CombinationSafe";
        box.transform.SetParent(root);
        box.transform.position = new Vector3(3.8f, 0.45f, -0.4f);
        box.transform.localScale = new Vector3(0.5f, 0.45f, 0.4f);
        ApplyColor(box, new Color(0.18f, 0.16f, 0.14f));
        box.AddComponent<CombinationLock>();

        Vector3[] bookPos =
        {
            new Vector3(3.55f, 0.95f, -0.4f),
            new Vector3(3.8f, 0.95f, -0.4f),
            new Vector3(4.05f, 0.95f, -0.4f)
        };
        Color[] colors =
        {
            new Color(0.55f, 0.12f, 0.12f),
            new Color(0.15f, 0.28f, 0.55f),
            new Color(0.15f, 0.45f, 0.22f)
        };
        for (int i = 0; i < 3; i++)
        {
            GameObject book = GameObject.CreatePrimitive(PrimitiveType.Cube);
            book.name = "Book" + (i + 1);
            book.transform.SetParent(root);
            book.transform.position = bookPos[i];
            book.transform.localScale = new Vector3(0.18f, 0.32f, 0.06f);
            ApplyColor(book, colors[i]);
        }
    }

    static void CreateSequence(Transform root)
    {
        GameObject holder = new GameObject("LampSequence");
        holder.transform.SetParent(root);
        holder.transform.position = new Vector3(0f, 0f, -3.2f);
        SequencePuzzle puzzle = holder.AddComponent<SequencePuzzle>();
        _ = puzzle;

        Color[] colors =
        {
            new Color(0.7f, 0.2f, 0.2f),
            new Color(0.7f, 0.65f, 0.2f),
            new Color(0.2f, 0.45f, 0.7f),
            new Color(0.3f, 0.7f, 0.3f)
        };

        SequenceSwitch[] list = new SequenceSwitch[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject sw = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sw.name = "LampSwitch" + (i + 1);
            sw.transform.SetParent(holder.transform);
            sw.transform.position = new Vector3(-1.2f + i * 0.8f, 1.3f, -3.2f);
            sw.transform.localScale = new Vector3(0.25f, 0.35f, 0.08f);
            ApplyColor(sw, colors[i] * 0.25f);
            list[i] = sw.AddComponent<SequenceSwitch>();
        }

        puzzle.SetSwitches(list);
    }

    static void CreateUvWriting(Transform root, ItemDefinition uv)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Quad);
        wall.name = "UvWriting";
        wall.transform.SetParent(root);
        wall.transform.position = new Vector3(0f, 1.4f, 4.6f);
        wall.transform.localScale = new Vector3(1.6f, 0.6f, 1f);
        ApplyColor(wall, new Color(0.2f, 0.16f, 0.12f, 1f));

        GameObject textObject = new GameObject("HiddenText");
        textObject.transform.SetParent(wall.transform, false);
        textObject.transform.localPosition = new Vector3(0f, 0f, -0.02f);
        TextMesh text = textObject.AddComponent<TextMesh>();
        text.text = "CODE  3141";
        text.characterSize = 0.08f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = new Color(0.55f, 0.2f, 0.95f, 0f);
        text.fontSize = 48;

        UvReveal reveal = wall.AddComponent<UvReveal>();
        reveal.UvItem = uv;
        _ = reveal;
    }

    static void CreateExit(Transform root)
    {
        GameObject hinge = new GameObject("ExitDoor");
        hinge.transform.SetParent(root);
        hinge.transform.position = new Vector3(4.5f, 0f, -2.5f);

        GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = "Panel";
        panel.transform.SetParent(hinge.transform);
        Vector3 size = new Vector3(1.1f, 2.2f, 0.08f);
        panel.transform.localPosition = new Vector3(size.x * 0.5f, size.y * 0.5f, 0f);
        panel.transform.localScale = size;
        ApplyColor(panel, new Color(0.18f, 0.1f, 0.07f));

        LockedOpenable door = hinge.AddComponent<LockedOpenable>();
        door.Locked = true;
        door.RequiredItem = null;
        door.Motion = LockedOpenable.OpenMotion.RotateYaw;
        door.OpenAngle = -95f;
        door.IsExit = true;

        PuzzleManager puzzles = Object.FindFirstObjectByType<PuzzleManager>();
        if (puzzles != null)
        {
            puzzles.ExitDoor = door;
        }

        GameObject volume = new GameObject("ExitVolume");
        volume.transform.SetParent(root);
        volume.transform.position = new Vector3(5.2f, 1f, -2.5f);
        BoxCollider box = volume.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(1.2f, 2f, 1.2f);
        volume.AddComponent<ExitTrigger>();
    }

    static void CreateThought(Transform root)
    {
        GameObject volume = new GameObject("OpeningThought");
        volume.transform.SetParent(root);
        volume.transform.position = new Vector3(0f, 1f, 0.8f);
        BoxCollider box = volume.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(2f, 2f, 2f);
        NarrativeTrigger trigger = volume.AddComponent<NarrativeTrigger>();
        trigger.FallbackText = "The door clicked behind me. Of course it did. There will be a way out. There always is, in a room like this.";
    }

    static void ConfigureHints()
    {
        HintSystem hints = Object.FindFirstObjectByType<HintSystem>();
        if (hints == null)
        {
            return;
        }

        hints.Configure(new[]
        {
            new HintSystem.HintEntry { puzzleId = "keypad", text = "Select the UV lamp and look at the far wall." },
            new HintSystem.HintEntry { puzzleId = "combination", text = "Three books sit on the safe: red, blue, green — 3, 1, 4 if you count their place in a rainbow of sorts. Try 3-1-4." },
            new HintSystem.HintEntry { puzzleId = "sequence", text = "The lamps on the back wall want a simple order: left to right." }
        });
    }

    static void ConfigureLamp(Transform root)
    {
        Light[] lights = root.GetComponentsInChildren<Light>();
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i].GetComponent<LightFlicker>() == null)
            {
                lights[i].gameObject.AddComponent<LightFlicker>();
            }

            DustMotes.Spawn(lights[i].transform, Vector3.zero);
        }
    }

    static void ApplyColor(GameObject go, Color color)
    {
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        renderer.material.color = color;
        if (renderer.material.HasProperty("_BaseColor"))
        {
            renderer.material.SetColor("_BaseColor", color);
        }
    }
}
