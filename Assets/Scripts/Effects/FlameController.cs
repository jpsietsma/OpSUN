using UnityEngine;

public class FlameController : MonoBehaviour
{
    public GameObject flameObject;
    public bool isLit = false;
    public int burnDuration = 5;

    public void Start()
    {

    }

    public void update()
    {

    }

    public void ToggleFlame()
    {
        flameObject.SetActive(!isLit);

        isLit = !isLit;
    }

    public void ExtinguishFlame()
    {
        flameObject.SetActive(false);
    }
}
