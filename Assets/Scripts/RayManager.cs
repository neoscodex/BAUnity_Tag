using UnityEngine;

public class RayManager : MonoBehaviour
{
    [SerializeField] private Vector3[] directions;
    [SerializeField] private float maxDistance = 50.0f;

    private RaycastHit[] hits;
    private RaycastHit downHit;

    void Start()
    {
        hits = new RaycastHit[directions.Length];
        downHit = new RaycastHit();
    }

    void Update()
    {
        MakeHitArray();

        //downHit = CastRay(new Vector3(0, -1, 0));
    }

    RaycastHit CastRay(Vector3 direction)
    {
        Ray ray = new Ray(gameObject.transform.position, direction);
        //Debug.DrawRay(gameObject.transform.position, direction * maxDistance, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            return hit;
        }

        return new RaycastHit();
    }

    RaycastHit CastSphere(Vector3 direction)
    {
        //Debug.DrawRay(gameObject.transform.position, direction * maxDistance, Color.red);

        if (Physics.SphereCast(gameObject.transform.position, 1.0f, direction, out RaycastHit hit, maxDistance))
        {
            return hit;
        }

        return new RaycastHit();
    }

    void MakeHitArray()
    {
        RaycastHit[] newHits = new RaycastHit[hits.Length];

        for (int i = 0; i < hits.Length; i++)
        {
            Vector3 direction = (gameObject.transform.rotation * directions[i]).normalized;
            newHits[i] = CastSphere(direction);
        }

        hits = newHits;
    }

    public RaycastHit[] GetRaycastHits()
    {
        return hits;
    }

    public RaycastHit GetDownHit()
    {
        return downHit;
    }

    public float GetMaxDistance()
    {
        return maxDistance;
    }

    public Vector3[] GetDirections()
    {
        return directions;
    }
}

