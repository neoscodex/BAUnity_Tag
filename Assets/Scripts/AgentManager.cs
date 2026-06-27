using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AgentManager : Agent
{
    [SerializeField] private GameObject _target;
    [SerializeField] private GameObject _start;
    [SerializeField] private float _rotationAmount = 5.0f;
    [SerializeField] private float _isTagger = 0.0f;

    public bool targetHit = false;

    private int _currentEpisode = 0;
    private float _cumulativeReward = 0.0f;
    private float _maxDistance;
    private float _moveSpeed;
    private Rigidbody _rb;

    public override void Initialize()
    {
        targetHit = false;

        _currentEpisode = 0;
        _cumulativeReward = 0.0f;
        _maxDistance = gameObject.GetComponent<RayManager>().GetMaxDistance();
        _moveSpeed = gameObject.GetComponent<AgentController>().moveSpeed;
        _rb = gameObject.GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        targetHit = false;

        if (_isTagger >= 1.0f)
        {
            _isTagger = 0.0f;
            gameObject.GetComponent<AgentController>().moveSpeed = _moveSpeed;
        }
        else
        {
            _isTagger = 1.0f;
            gameObject.GetComponent<AgentController>().moveSpeed = _moveSpeed * 1.05f;
        }

        _currentEpisode++;
        _cumulativeReward = 0.0f;

        ResetEnviorment();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float agentRole = _isTagger;

        Vector3 targetDirection = (_target.transform.position - transform.position).normalized;
        float targetDistance = Mathf.Clamp(Vector3.Distance(transform.position, _target.transform.position) / _maxDistance, -1.0f, 1.0f);

        float angle = transform.eulerAngles.y * Mathf.Deg2Rad;
        float agentSin = Mathf.Sin(angle);
        float agentCos = Mathf.Cos(angle);
        Vector3 agentVelocity = _rb.linearVelocity.normalized;
        float agentIsGrounded = gameObject.GetComponent<AgentController>().GetIsGrounded() ? 1.0f : 0.0f;
        float agentHitTarget = targetHit? 1.0f : 0.0f;

        sensor.AddObservation(agentRole);
        sensor.AddObservation(targetDirection.x);
        sensor.AddObservation(targetDirection.y);
        sensor.AddObservation(targetDirection.z);
        sensor.AddObservation(targetDistance);
        sensor.AddObservation(agentSin);
        sensor.AddObservation(agentCos);
        sensor.AddObservation(agentVelocity.x);
        sensor.AddObservation(agentVelocity.y);
        sensor.AddObservation(agentVelocity.z);
        sensor.AddObservation(agentIsGrounded);
        sensor.AddObservation(agentHitTarget);

        RaycastHit[] hits = gameObject.GetComponent<RayManager>().GetRaycastHits();

        foreach (RaycastHit hit in hits)
        {
            Vector3 hitDirection = new Vector3(0.0f, 0.0f, 0.0f);
            float hitDistance = 1.0f;
            float targetHit = 0.0f;
            float anyHit = 0.0f;

            if (hit.collider != null)
            {
                hitDirection = (hit.point - transform.position).normalized;
                hitDistance = Vector3.Distance(transform.position, hit.point) / _maxDistance;
                targetHit = hit.collider.gameObject == _target ? 1.0f : 0.0f;
                anyHit = 1.0f;
            }

            sensor.AddObservation(hitDirection.x);
            sensor.AddObservation(hitDirection.y);
            sensor.AddObservation(hitDirection.z);
            sensor.AddObservation(hitDistance);
            sensor.AddObservation(targetHit);
            sensor.AddObservation(anyHit);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (targetHit ) EndEpisode(); //&& _target.GetComponent<AgentManager>().targetHit

        MoveAgent(actions.DiscreteActions);

        if (_isTagger >= 1.0f) AddReward(-0.0002f);
        else AddReward(0.0002f);

        _cumulativeReward = GetCumulativeReward();
    }

    private void ResetEnviorment()
    {
        float randomOffsetX = Random.Range(-2.5f, 2.5f);
        float randomOffsetY = Random.Range(-2.5f, 2.5f);

        transform.localPosition = _start.transform.localPosition + new Vector3(randomOffsetX, 0 , randomOffsetY);
        transform.localRotation = Quaternion.identity;
    }

    private void MoveAgent(ActionSegment<int> actions)
    {
        Vector2 vertical = actions[0] != 0 ? actions[0] == 1 ? new Vector2(0.0f, 1.0f) : new Vector2(0.0f, -1.0f) : Vector2.zero;
        Vector2 horizontal = actions[1] != 0 ? actions[1] == 1 ? new Vector2(1.0f, 0.0f) : new Vector2(-1.0f, 0.0f) : Vector2.zero;

        Vector2 movement = vertical + horizontal;
        Vector2 rotation = actions[2] != 0 ? actions[2] == 1 ? new Vector2(_rotationAmount, 0) : new Vector2(-_rotationAmount, 0) : Vector2.zero;
        bool jump = actions[3] != 0 ? false : true;

        gameObject.GetComponent<AgentController>().SetInput(movement, rotation, jump);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == _target)
        {
            if (_isTagger >= 1.0f) AddReward(1.0f);
            else AddReward(-1.0f);
            targetHit = true;
        }
    }
}
