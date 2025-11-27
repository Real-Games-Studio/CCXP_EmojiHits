using UnityEngine;

[DisallowMultipleComponent]
public class Floating2DEffect : MonoBehaviour
{
    [Tooltip("Maximum distance from the starting position.")]
    public float maxRange = 0.5f;

    [Tooltip("Frequency of the floating motion.")]
    public float speed = 1f;

    [Tooltip("Multiplier applied to the sinusoidal offset.")]
    public float intensidade = 1f;

    private Vector3 initialPosition;
    private Vector2 direction;
    private Vector2 targetDirection;
    private float directionTimer;
    private float phase;
    [Tooltip("How often the floating direction changes.")]
    public float directionChangeInterval = 3f;

    private void Awake()
    {
        initialPosition = transform.localPosition;
        direction = Random.insideUnitCircle;
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = Vector2.up;
        }
        direction.Normalize();
        targetDirection = direction;
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        directionTimer += Time.deltaTime;
        float blend = Mathf.Clamp01(directionTimer / Mathf.Max(directionChangeInterval, 0.01f));
        Vector2 interpolated = Vector2.Lerp(direction, targetDirection, blend);
        interpolated.Normalize();
        direction = interpolated;

        if (directionTimer >= directionChangeInterval)
        {
            directionTimer = 0f;
            targetDirection = Random.insideUnitCircle;
            if (targetDirection.sqrMagnitude < 0.01f)
            {
                targetDirection = Vector2.up;
            }
            targetDirection.Normalize();
        }

        float oscillation = Mathf.Sin(Time.time * speed + phase);
        float offsetMagnitude = oscillation * maxRange * intensidade;
        Vector3 offset = (Vector3)(direction * offsetMagnitude);
        transform.localPosition = initialPosition + offset;
    }
}
