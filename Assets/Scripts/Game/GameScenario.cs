using UnityEngine;

[CreateAssetMenu]
public class GameScenario : ScriptableObject {

    [SerializeField]
    private EnemyWave[] _enemyWaves = {};

    public State Begin() => new State(this);

    [System.Serializable]
    public struct State {
        private GameScenario _scenario;

        private int _index;

        private EnemyWave.State _wave;

        public State(GameScenario scenario) {
            this._scenario = scenario;
            _index = 0;
            Debug.Assert(scenario._enemyWaves.Length > 0, "Empty scenario!");
            _wave = scenario._enemyWaves[0].Begin();
        }

        public bool Progress() {
            var deltaTime = _wave.Progress(Time.deltaTime);
            while (deltaTime >= 0f) {
                if (++_index >= _scenario._enemyWaves.Length) {
                    return false;
                }
                _wave = _scenario._enemyWaves[_index].Begin();
                deltaTime = _wave.Progress(deltaTime);
            }
            return true;
        }
    }
}