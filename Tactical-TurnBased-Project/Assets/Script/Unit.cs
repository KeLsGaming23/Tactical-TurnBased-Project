using kelsgaming.site;
using UnityEngine;


public class Unit : MonoBehaviour
{
    private MoveAction moveAction;
    private SpinAction spinAction;
    private GridPosition gridPosition;
    private void Awake()
    {
        moveAction = GetComponent<MoveAction>();
        spinAction = GetComponent<SpinAction>();
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
    public MoveAction GetMoveAction()
    {
        return moveAction;
    }
    public SpinAction GetSpinAction()
    {
        return spinAction;
    }
    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

}
