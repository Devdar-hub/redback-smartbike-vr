using UnityEngine;

public class Demo : MonoBehaviour
{
    [SerializeField] private Timer2 timer1;

    [SerializeField] private int timerDuration = 15;
    [SerializeField] private float startDelay = 3f;

    private void Start()
    {
        timer1 = FindObjectOfType<Timer2>();

        if (timer1 != null)
        {
            timer1.SetDuration(timerDuration)
                  .BeginWithDelay(startDelay);
        }
        else
        {
            Debug.LogError("Timer2 component not found!");
        }
    }
}