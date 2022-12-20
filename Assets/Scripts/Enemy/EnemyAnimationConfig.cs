using UnityEngine;

[CreateAssetMenu]
public class EnemyAnimationConfig : ScriptableObject {

    [SerializeField]
    private AnimationClip   _move   = default,
                            _intro  = default,
                            _outro  = default,
                            _dying  = default;

    [SerializeField]
    private float _moveAnimationSpeed;

    public float MoveAnimationSpeed => _moveAnimationSpeed;
    public AnimationClip Move => _move;
    public AnimationClip Intro => _intro;
    public AnimationClip Outro => _outro;
    public AnimationClip Dying => _dying;
}