using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.IO;

public class AgentManager : Agent
{
    [SerializeField] private GameObject _target;
    [SerializeField] private GameObject _start;
    [SerializeField] private float _rotationAmount = 2.0f;
    [SerializeField] private float _isTagger = 0.0f;
    [SerializeField] private string filename;

    public bool targetHit = false;

    private float _cumulativeReward = 0.0f;
    private float _maxDistance;
    private int _stepCount = 0;
    private string _filePath;
    private Rigidbody _rb;

    public override void Initialize()
    {
        targetHit = false;

        _cumulativeReward = 0.0f;
        _maxDistance = gameObject.GetComponent<RayManager>().GetMaxDistance();
        _stepCount = 0;
        _rb = gameObject.GetComponent<Rigidbody>();

        gameObject.GetComponent<AgentController>().moveSpeed *= 1.05f;

        CreateCSVFile();
    }

    public override void OnEpisodeBegin()
    {
        LogData();
        Debug.Log(_stepCount + "," + _cumulativeReward + "," + targetHit);
        targetHit = false;
        _cumulativeReward = 0.0f;
        _stepCount = 0;

        ResetEnviorment();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float agentRole = _isTagger;

        Vector3 targetDirection = transform.InverseTransformDirection((_target.transform.position - transform.position).normalized);
        float targetDistance = Mathf.Clamp(Vector3.Distance(transform.position, _target.transform.position) / _maxDistance, -1.0f, 1.0f);

        float angle = transform.eulerAngles.y * Mathf.Deg2Rad;
        float agentSin = Mathf.Sin(angle);
        float agentCos = Mathf.Cos(angle);
        Vector3 agentVelocity = transform.InverseTransformDirection(_rb.linearVelocity.normalized);
        float agentIsGrounded = gameObject.GetComponent<AgentController>().GetIsGrounded() ? 1.0f : 0.0f;
        float agentHitTarget = targetHit? 1.0f : 0.0f;

        sensor.AddObservation(agentRole);
        sensor.AddObservation(targetDirection.x);
        //sensor.AddObservation(targetDirection.y);
        sensor.AddObservation(targetDirection.z);
        sensor.AddObservation(targetDistance);
        sensor.AddObservation(agentSin);
        sensor.AddObservation(agentCos);
        sensor.AddObservation(agentVelocity.x);
        //sensor.AddObservation(agentVelocity.y);
        sensor.AddObservation(agentVelocity.z);
        //sensor.AddObservation(agentIsGrounded);
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
                hitDirection = transform.InverseTransformDirection((hit.point - transform.position).normalized);
                hitDistance = Vector3.Distance(transform.position, hit.point) / _maxDistance;
                targetHit = hit.collider.gameObject == _target ? 1.0f : 0.0f;
                anyHit = 1.0f;
            }

            sensor.AddObservation(hitDirection.x);
            //sensor.AddObservation(hitDirection.y);
            sensor.AddObservation(hitDirection.z);
            sensor.AddObservation(hitDistance);
            sensor.AddObservation(targetHit);
            sensor.AddObservation(anyHit);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);
        _stepCount = StepCount;

        if (_isTagger >= 1.0f) AddReward(-1.0f / (float)MaxStep);
        else AddReward(1.0f / (float)MaxStep);

        _cumulativeReward = GetCumulativeReward();

        //if (targetHit) EndEpisode(); //&& _target.GetComponent<AgentManager>().targetHit
    }

    private void ResetEnviorment()
    {
        float randomOffsetX = Random.Range(-2.5f, 2.5f);
        float randomOffsetY = Random.Range(-2.5f, 2.5f);

        transform.localPosition = _start.transform.localPosition + new Vector3(randomOffsetX, 0 , randomOffsetY);
        _rb.linearVelocity = new Vector3(0, 0, 0);
        transform.localRotation = Quaternion.identity;
    }

    private void MoveAgent(ActionSegment<int> actions)
    {
        Vector2 vertical = actions[0] != 0 ? actions[0] == 1 ? new Vector2(0.0f, 1.0f) : new Vector2(0.0f, -1.0f) : Vector2.zero;
        Vector2 horizontal = actions[1] != 0 ? actions[1] == 1 ? new Vector2(1.0f, 0.0f) : new Vector2(-1.0f, 0.0f) : Vector2.zero;

        Vector2 movement = vertical + horizontal;
        Vector2 rotation = actions[2] != 0 ? actions[2] == 1 ? new Vector2(_rotationAmount, 0) : new Vector2(-_rotationAmount, 0) : Vector2.zero;
        //bool jump = actions[3] != 0 ? false : true;

        //gameObject.GetComponent<AgentController>().SetInput(movement, rotation, jump);
        gameObject.GetComponent<AgentController>().SetInput(movement, rotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == _target)
        {
            if (_isTagger >= 1.0f) AddReward(1.0f);
            else AddReward(-1.0f);
            _cumulativeReward = GetCumulativeReward();
            targetHit = true;
            EndEpisode();
        }
    }

    private void CreateCSVFile()
    {
        _filePath = Path.Combine(Application.persistentDataPath, filename + "_tag_data.csv");

        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "StepCount,CumulativeReward,isTagger,targetHit\n");
        }
    }

    private void LogData()
    {
        string line = $"{_stepCount},{_cumulativeReward},{_isTagger},{targetHit}\n";
        File.AppendAllText(_filePath, line);
    }
}
