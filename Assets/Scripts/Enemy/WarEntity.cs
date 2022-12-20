using UnityEngine;

public abstract class WarEntity : GameBehaviour {
    private WarFactory _originFactory;

    public WarFactory OriginFactory {
        get => _originFactory;
        set {
            Debug.Assert(_originFactory == null, "Redifened origin factory");
            _originFactory = value;
        }
    }

    public override void Recycle() {
        _originFactory.Reclaim(this);
    }
}