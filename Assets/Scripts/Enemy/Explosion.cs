using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Explosion : WarEntity {

    static int colorPropertyId = Shader.PropertyToID("_Color");

    static MaterialPropertyBlock _propertyBlock;

    [SerializeField,Range(0f,1f)]
    private float _duration = 0.5f;
    [SerializeField]
    AnimationCurve _opacityCurve = default,
                   _scaleCurve   = default;


    float   _age,
            _scale;
    MeshRenderer _meshRenderer;

    private void Awake() {
        _meshRenderer = GetComponent<MeshRenderer>();
        Debug.Assert(_meshRenderer != null, "Explosion whithout renderer");
    }

    public void Initialize(
        Vector3 position,
        float blastRadius,
        float damage = 0f) {
        if (damage > 0f) {
            TargetPoint.FillBuffer(position, blastRadius);
            for (int i = 0; i < TargetPoint.BufferedCount; i++) {
                TargetPoint.GetBuffered(i).Enemy.ApplyDamage(damage);
            }
        }
        transform.localPosition = position;
        _scale = blastRadius * 2f;
    }

    public override bool GameUpdate() {
        _age += Time.deltaTime;
        if(_age >= _duration) {
            OriginFactory.Reclaim(this);
            return false;
        }
        _propertyBlock ??= new MaterialPropertyBlock();
        var t = _age / _duration;
        var c = Color.clear;
        c.a = _opacityCurve.Evaluate(t);
        _propertyBlock.SetColor(colorPropertyId, c);
        _meshRenderer.SetPropertyBlock(_propertyBlock);
        transform.localScale = Vector3.one * (_scale * _scaleCurve.Evaluate(t));
        return true;
    }
}
