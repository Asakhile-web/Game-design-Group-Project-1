using System;
using UnityEngine;

/// <summary>
/// Current objective text plus the in-level play timer shown in the HUD's top-left.
/// Call SetObjective / CompleteObjective from scripts or UnityEvents (e.g. SimpleInteractable.onInteract).
/// The timer uses scaled time, so it stops while the game is paused.
/// </summary>
public class ObjectiveTracker : MonoBehaviour
{
    [TextArea] [SerializeField] string startingObjective;

    string current = string.Empty;
    float elapsed;

    public string Current => current;
    public bool HasObjective => !string.IsNullOrEmpty(current);
    public float ElapsedSeconds => elapsed;

    public event Action<string> ObjectiveChanged;
    public event Action<string> ObjectiveCompleted;

    void Start()
    {
        if (!HasObjective && !string.IsNullOrEmpty(startingObjective))
        {
            SetObjective(startingObjective);
        }
    }

    void Update()
    {
        elapsed += Time.deltaTime;
    }

    public void SetObjective(string objective)
    {
        objective = objective ?? string.Empty;
        if (objective == current)
        {
            return;
        }

        current = objective;
        ObjectiveChanged?.Invoke(current);
    }

    public void CompleteObjective()
    {
        if (!HasObjective)
        {
            return;
        }

        string done = current;
        current = string.Empty;
        ObjectiveCompleted?.Invoke(done);
        ObjectiveChanged?.Invoke(current);
    }

    /// <summary>Marks the current objective complete, then sets the next one.</summary>
    public void CompleteAndSetObjective(string next)
    {
        CompleteObjective();
        SetObjective(next);
    }
}
