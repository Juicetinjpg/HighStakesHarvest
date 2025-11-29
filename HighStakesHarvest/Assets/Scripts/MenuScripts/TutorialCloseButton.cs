using UnityEngine;

public class TutorialCloseButton : MonoBehaviour
{
    public void CloseTutorial()
    {
        Transform t = transform;

        // climb up until we leave the prefab or hit the scene canvas
        while (t.parent != null && t.parent.GetComponent<Canvas>() == null)
        {
            t = t.parent;
        }

        // destroy the topmost child BEFORE Canvas
        Destroy(t.gameObject);
    }
}
