using UnityEngine;

[CreateAssetMenu]
public class EnemyWave : ScriptableObject {

    [SerializeField]
    private EnemySpawnSequence[] _spawnSequences = {
        new EnemySpawnSequence() };

    public State Begin() => new State(this);

    [System.Serializable]
    public struct State {
        private EnemyWave _wave;
        private int _index;

        private EnemySpawnSequence.State sequence;

        public State(EnemyWave wave) {
            _wave = wave;
            _index = 0;
            Debug.Assert(wave._spawnSequences.Length > 0, "Empty wave!");
            sequence = wave._spawnSequences[0].Begin();
        }

        public float Progress(float deltaTime) {
            deltaTime = sequence.Progress(deltaTime);
            while (deltaTime >= 0f) {
                if (++_index >= _wave._spawnSequences.Length) {
                    return deltaTime;
                }
                sequence = _wave._spawnSequences[_index].Begin();
                deltaTime = sequence.Progress(deltaTime);
            }
            return -1f;
        }
    }
}