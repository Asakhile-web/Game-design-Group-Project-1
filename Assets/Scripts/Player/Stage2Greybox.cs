using UnityEngine;

/// <summary>
/// Extra Stage 2 props parented under the fallback greybox: keys, a sliding drawer, a hinged door, a locker.
/// </summary>
public static class Stage2Greybox
{
    public static void Spawn(Transform root)
    {
        if (root == null)
        {
            return;
        }

        ItemDefinition brass = ItemDefinition.CreateRuntime("brass_key", "Brass Key", consume: true);
        ItemDefinition iron = ItemDefinition.CreateRuntime("iron_key", "Iron Key", consume: true);

        CreateKeyPickup(root, "BrassKey", new Vector3(0.35f, 0.08f, 1.8f), new Color(0.82f, 0.65f, 0.22f), brass);
        CreateKeyPickup(root, "IronKey", new Vector3(0.7f, 0.08f, 1.8f), new Color(0.55f, 0.58f, 0.62f), iron);

        CreateDeskAndDrawer(root, brass);
        CreateHingedDoor(root, new Vector3(-4f, 0f, 2f), locked: false, required: null, "StudyDoor");
        CreateLocker(root, new Vector3(3.2f, 0f, 1.4f), iron);

        Debug.Log("[Stage2Greybox] Spawned keys, locked drawer, door, and locker.");
    }

    static void CreateKeyPickup(Transform root, string objectName, Vector3 position, Color color, ItemDefinition definition)
    {
        GameObject key = GameObject.CreatePrimitive(PrimitiveType.Cube);
        key.name = objectName;
        key.transform.SetParent(root);
        key.transform.position = position;
        key.transform.localScale = new Vector3(0.22f, 0.07f, 0.1f);
        ApplyColor(key, color);

        ItemPickup pickup = key.AddComponent<ItemPickup>();
        pickup.Definition = definition;
    }

    static void CreateDeskAndDrawer(Transform root, ItemDefinition brass)
    {
        GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        desk.name = "Desk";
        desk.transform.SetParent(root);
        desk.transform.position = new Vector3(2.4f, 0.4f, 2.2f);
        desk.transform.localScale = new Vector3(1.2f, 0.8f, 0.7f);
        ApplyColor(desk, new Color(0.35f, 0.22f, 0.12f));

        GameObject drawer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        drawer.name = "LockedDrawer";
        drawer.transform.SetParent(root);
        drawer.transform.position = new Vector3(2.4f, 0.28f, 2.58f);
        drawer.transform.localScale = new Vector3(0.75f, 0.2f, 0.28f);
        ApplyColor(drawer, new Color(0.42f, 0.28f, 0.14f));

        LockedOpenable openable = drawer.AddComponent<LockedOpenable>();
        openable.RequiredItem = brass;
        openable.Locked = true;
        openable.Motion = LockedOpenable.OpenMotion.SlideLocal;
        openable.SlideOffset = new Vector3(0f, 0f, 0.9f);
    }

    static void CreateHingedDoor(Transform root, Vector3 hingePosition, bool locked, ItemDefinition required, string objectName)
    {
        GameObject hinge = new GameObject(objectName);
        hinge.transform.SetParent(root);
        hinge.transform.position = hingePosition;

        GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = "Panel";
        panel.transform.SetParent(hinge.transform);
        Vector3 size = new Vector3(1.1f, 2.2f, 0.08f);
        panel.transform.localPosition = new Vector3(size.x * 0.5f, size.y * 0.5f, 0f);
        panel.transform.localScale = size;
        ApplyColor(panel, new Color(0.28f, 0.18f, 0.1f));

        LockedOpenable openable = hinge.AddComponent<LockedOpenable>();
        openable.RequiredItem = required;
        openable.Locked = locked;
        openable.Motion = LockedOpenable.OpenMotion.RotateYaw;
        openable.OpenAngle = -95f;
    }

    static void CreateLocker(Transform root, Vector3 position, ItemDefinition iron)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "LockerBody";
        body.transform.SetParent(root);
        body.transform.position = position + new Vector3(0f, 0.9f, 0f);
        body.transform.localScale = new Vector3(0.7f, 1.8f, 0.45f);
        ApplyColor(body, new Color(0.25f, 0.28f, 0.32f));

        GameObject hinge = new GameObject("LockerDoor");
        hinge.transform.SetParent(root);
        hinge.transform.position = position + new Vector3(-0.35f, 0f, 0.23f);

        GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = "Panel";
        panel.transform.SetParent(hinge.transform);
        Vector3 size = new Vector3(0.68f, 1.7f, 0.05f);
        panel.transform.localPosition = new Vector3(size.x * 0.5f, size.y * 0.5f, 0f);
        panel.transform.localScale = size;
        ApplyColor(panel, new Color(0.32f, 0.36f, 0.4f));

        LockedOpenable openable = hinge.AddComponent<LockedOpenable>();
        openable.RequiredItem = iron;
        openable.Locked = true;
        openable.Motion = LockedOpenable.OpenMotion.RotateYaw;
        openable.OpenAngle = 100f;
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
