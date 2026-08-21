using System;
using kelsgaming.site;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private const int DEFAULT_ACTION_POINTS = 2;

    public static event EventHandler OnAnyActionPointsChanged;

    [SerializeField] private bool isEnemy = false;
    [SerializeField] private int speed = 0;
    [SerializeField] private int maxActionPoints = DEFAULT_ACTION_POINTS;

    private int actionPoints;
    private bool hasActedThisRound;
    private MoveAction moveAction;
    private SpinAction spinAction;
    private BaseAction[] baseActionArray;
    private GridPosition gridPosition;

    private void Awake()
    {
        moveAction = GetComponent<MoveAction>();
        spinAction = GetComponent<SpinAction>();
        baseActionArray = GetComponents<BaseAction>();

        // If speed is not set in inspector, randomize speed between 5 and 30
        if (speed <= 0)
        {
            speed = UnityEngine.Random.Range(5, 30);
        }

        actionPoints = maxActionPoints;
        hasActedThisRound = false;
    }

    private void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.AddUnitAtGridPosition(gridPosition, this);
    }

    private void Update()
    {
        GridPosition newgridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        if (newgridPosition != gridPosition)
        {
            LevelGrid.Instance.UnitMovedGridPosition(this, gridPosition, newgridPosition);
            gridPosition = newgridPosition;
        }
    }

    public bool TrySpendActionPointsToTakeAction(BaseAction baseAction)
    {
        if (CanSpendActionPointsToTakeAction(baseAction))
        {
            SpendActionPoints(baseAction.GetActionPointsCost());
            return true;
        }
        return false;
    }

    public bool CanSpendActionPointsToTakeAction(BaseAction baseAction)
    {
        if (baseAction == null) return false;
        return actionPoints >= baseAction.GetActionPointsCost();
    }

    private void SpendActionPoints(int amount)
    {
        actionPoints -= amount;
        if (actionPoints < 0) actionPoints = 0;
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ResetActionPoints()
    {
        actionPoints = maxActionPoints;
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetMaxActionPoints(int maxAP)
    {
        maxActionPoints = maxAP;
        actionPoints = maxAP;
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsEnemy() => isEnemy;
    public void SetIsEnemy(bool value) => isEnemy = value;
    public int GetSpeed() => speed;
    public void SetSpeed(int newSpeed) => speed = newSpeed;
    public int GetActionPoints() => actionPoints;
    public int GetMaxActionPoints() => maxActionPoints;
    public bool HasActedThisRound() => hasActedThisRound;
    public void SetHasActedThisRound(bool value) => hasActedThisRound = value;

    public MoveAction GetMoveAction() => moveAction;
    public SpinAction GetSpinAction() => spinAction;
    public BaseAction[] GetBaseActionArray() => baseActionArray;
    public GridPosition GetGridPosition() => gridPosition;
}
