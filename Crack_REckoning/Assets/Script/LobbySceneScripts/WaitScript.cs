using UnityEngine;

public class WaitScript : MonoBehaviour
{
    float timer = 0f;
    private void Update()
    {
        timer += Time.deltaTime;
        if(timer > 0.5f)
        {
            gameObject.SetActive(false);
        }

    }
}