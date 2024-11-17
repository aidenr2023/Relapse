using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  UnityEngine.Animations.Rigging;


public class EnvironmentInteractionContext : MonoBehaviour
{
    public enum Ebodyside
    {
        RIGHT,
        LEFT
    }
    //read-only fields
    //references to the rigging constraints
    [SerializeField] private TwoBoneIKConstraint _leftIKConstraint;
    [SerializeField] private TwoBoneIKConstraint _rightIKConstraint;
    [SerializeField] private MultiAimConstraint _headIKConstraint;
    [SerializeField] private MultiRotationConstraint _leftMultiRotationConstraint;
    [SerializeField] private MultiRotationConstraint _rightMultiRotationConstraint;
    [SerializeField] private Rigidbody _rigidbody;
    //root collider & transform
    [SerializeField] private CapsuleCollider _rootCollider;
    [SerializeField] private Transform _rootTransform;
    
    //constructor
    public EnvironmentInteractionContext(TwoBoneIKConstraint leftIKConstraint, TwoBoneIKConstraint rightIKConstraint, 
    MultiAimConstraint headIKConstraint, MultiRotationConstraint leftMultiRotationConstraint, 
    MultiRotationConstraint rightMultiRotationConstraint, Rigidbody rigidbody, CapsuleCollider capsuleCollider, 
    Transform rootTransform)
    {
        _leftIKConstraint = leftIKConstraint;
        _rightIKConstraint = rightIKConstraint;
        _headIKConstraint = headIKConstraint;
        _leftMultiRotationConstraint = leftMultiRotationConstraint;
        _rightMultiRotationConstraint = rightMultiRotationConstraint;
        _rigidbody = rigidbody;
        _rootCollider = capsuleCollider;
        _rootTransform = rootTransform;
        CharacterShoulderHeight = _leftIKConstraint.data.root.transform.position.y;
    }
    
    //read-only getters
    public TwoBoneIKConstraint LeftIKConstraint => _leftIKConstraint;
    public TwoBoneIKConstraint RightIKConstraint => _rightIKConstraint;
    public MultiAimConstraint HeadIKConstraint => _headIKConstraint;
    public MultiRotationConstraint LeftMultiRotationConstraint => _leftMultiRotationConstraint;
    public MultiRotationConstraint RightMultiRotationConstraint => _rightMultiRotationConstraint;
    public Rigidbody Rb => _rigidbody;
    public CapsuleCollider RootCollider => _rootCollider;
    public Transform RootTransform => _rootTransform;
    
    
    #region getters 
    public float CharacterShoulderHeight { get;  set; }
    public Collider _currentIntersectingCollider {get;  set;}
    public TwoBoneIKConstraint _currentIKConstraint {get;  set;}
    public MultiRotationConstraint _currentMultiRotationConstraint {get; private set;}
    //current ik target transform & shoulder transform
    public Transform _currentIKTarget {get; private set;}
    public Transform _currentShoulderTransform {get; private set;}
    public Ebodyside _currentBodySide {get; private set;}
    public Vector3 ClosestPointOnColliderFromShoulder { get; set; } = Vector3.positiveInfinity;
    #endregion getters
    
    
    public void SetCurrentSide(Vector3 positionToCheck)
    {
        Vector3 rightShoulder = _rightIKConstraint.data.root.transform.position;
        Vector3 leftShoulder = _leftIKConstraint.data.root.transform.position;
        
        bool isLeftCloser = Vector3.Distance(positionToCheck, leftShoulder) < Vector3.Distance(positionToCheck, rightShoulder);

        if (isLeftCloser)
        {
            Debug.Log("Left side is closer");
            _currentBodySide = Ebodyside.LEFT;
            _currentIKConstraint = _leftIKConstraint;
            _currentMultiRotationConstraint = _leftMultiRotationConstraint;
        }
        else
        {
            Debug.Log("Right side is closer");
            _currentBodySide = Ebodyside.RIGHT;
            _currentIKConstraint = _rightIKConstraint;
            _currentMultiRotationConstraint = _rightMultiRotationConstraint;
        }
        _currentIKTarget = _currentIKConstraint.data.target.transform;
        _currentShoulderTransform = _currentIKConstraint.data.root.transform;
        
    }
    
}
