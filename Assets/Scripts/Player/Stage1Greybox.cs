using UnityEngine;

/// <summary>
/// Spawns a tiny test room if the gameplay scene has no floor. Disable spawnFallbackGreybox on PlayerController
/// once the real study greybox is in the scene.
/// </summary>
public static class Stage1Greybox
{
    const string RootName = "Stage1Greybox";

    public static void Ensure(PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        if (GameObject.Find(RootName) != null)
        {
            return;
        }

        if (HasGroundUnderPlayer(player.transform.position))
        {
            return;
        }

        GameObject root = new GameObject(RootName);

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(root.transform);
        floor.transform.position = new Vector3(0f, -0.05f, 0f);
        floor.transform.localScale = new Vector3(16f, 0.1f, 16f);

        GameObject lightObject = new GameObject("FallbackLamp");
        lightObject.transform.SetParent(root.transform);
        lightObject.transform.position = new Vector3(0f, 3.2f, 0f);
        Light lamp = lightObject.AddComponent<Light>();
        lamp.type = LightType.Point;
        lamp.intensity = 4f;
        lamp.range = 18f;
        lamp.color = new Color(1f, 0.9f, 0.7f);

        GameObject inspect = GameObject.CreatePrimitive(PrimitiveType.Cube);
        inspect.name = "TestInspectCube";
        inspect.transform.SetParent(root.transform);
        inspect.transform.position = new Vector3(1.5f, 0.5f, 2.5f);
        inspect.transform.localScale = Vector3.one * 0.6f;
        SimpleInteractable simple = inspect.AddComponent<SimpleInteractable>();
        simple.SetCanInteract(true);

        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.name = "TestPickup";
        pickup.tag = "Pickup";
        pickup.transform.SetParent(root.transform);
        pickup.transform.position = new Vector3(-1.2f, 0.35f, 2.2f);
        pickup.transform.localScale = Vector3.one * 0.25f;
        pickup.AddComponent<PickupHoldInteractable>();
        Rigidbody body = pickup.AddComponent<Rigidbody>();
        body.mass = 0.5f;

        Stage2Greybox.Spawn(root.transform);

        Debug.Log("[Stage1Greybox] Spawned fallback floor + test objects. Turn this off when the study room is in the scene.");
    }

    static bool HasGroundUnderPlayer(Vector3 origin)
    {
        return Physics.Raycast(origin + Vector3.up * 0.2f, Vector3.down, 3f);
    }
}
