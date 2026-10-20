using UnityEngine;

/// <summary>
/// Creates missing gameplay systems so Play Mode works on the greybox scene without Inspector wiring.
/// Called from PlayerController.Awake.
/// </summary>
public static class SystemsBootstrap
{
    const string RootName = "GameSystems";

    public static void Ensure(PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(player.tag) || player.CompareTag("Untagged"))
        {
            try
            {
                player.tag = "Player";
            }
            catch (UnityException)
            {
                // Tag may be missing in TagManager; interaction still works without it.
            }
        }

        GameObject root = GameObject.Find(RootName);
        if (root == null)
        {
            root = new GameObject(RootName);
        }

        AddIfMissing<GameFlow>(root);
        PuzzleManager puzzles = AddIfMissing<PuzzleManager>(root);
        HintSystem hints = AddIfMissing<HintSystem>(root);
        AssignDefaultHints(hints);
        AddIfMissing<NoteReader>(root);
        AddIfMissing<SubtitleDirector>(root);
        AddIfMissing<PickupVfxPool>(root);

        AudioManager.Ensure();
        SceneFader.Ensure();

        InteractionRaycaster raycaster = player.GetComponent<InteractionRaycaster>();
        InteractableHighlight highlight = player.GetComponent<InteractableHighlight>();
        if (highlight == null)
        {
            highlight = player.gameObject.AddComponent<InteractableHighlight>();
        }

        highlight.Raycaster = raycaster;

        Transform cam = player.CameraTransform;
        if (cam != null)
        {
            if (cam.GetComponent<CameraImpulse>() == null)
            {
                cam.gameObject.AddComponent<CameraImpulse>();
            }

            if (cam.GetComponent<VolumeDirector>() == null)
            {
                cam.gameObject.AddComponent<VolumeDirector>();
            }
        }

        FootstepEmitter steps = player.GetComponent<FootstepEmitter>();
        if (steps != null)
        {
            steps.Footstep -= OnFootstep;
            steps.Footstep += OnFootstep;
        }

        _ = puzzles;
    }

    static void OnFootstep()
    {
        AudioManager.Play(AudioCue.Footstep);
    }

    static T AddIfMissing<T>(GameObject host) where T : Component
    {
        T found = Object.FindFirstObjectByType<T>();
        if (found != null)
        {
            return found;
        }

        return host.AddComponent<T>();
    }

    static void AssignDefaultHints(HintSystem hints)
    {
        if (hints == null)
        {
            return;
        }

        // Hints are Inspector-driven; greybox fills them in Stage3Greybox via SerializedObject-free public API if added later.
    }
}
