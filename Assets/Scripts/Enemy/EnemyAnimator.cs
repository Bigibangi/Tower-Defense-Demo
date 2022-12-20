using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[System.Serializable]
public struct EnemyAnimator {

    public enum Clip {
        Move,
        Intro,
        Outro,
        Dying
    }

    private const float transitionSped = 5f;

    public Clip CurrentClip { get; private set; }
    public bool IsDone => GetPlayable(CurrentClip).IsDone();
#if UNITY_EDITOR
    public bool IsValid => _graph.IsValid();

    private double _clipTime;
#endif

    private PlayableGraph _graph;
    private AnimationMixerPlayable _mixer;
    private Clip _previousClip;
    private float _transitionProgress;

    public void Configure(Animator animator, EnemyAnimationConfig config) {
        _graph = PlayableGraph.Create();
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        _mixer = AnimationMixerPlayable.Create(_graph, 4);
        var clip = AnimationClipPlayable.Create(_graph, config.Move);
        clip.Pause();
        _mixer.ConnectInput((int) Clip.Move, clip, 0);
        clip = AnimationClipPlayable.Create(_graph, config.Intro);
        clip.SetDuration(config.Intro.length);
        _mixer.ConnectInput((int) Clip.Intro, clip, 0);
        clip = AnimationClipPlayable.Create(_graph, config.Outro);
        clip.SetDuration(config.Outro.length);
        clip.Pause();
        _mixer.ConnectInput((int) Clip.Outro, clip, 0);
        clip = AnimationClipPlayable.Create(_graph, config.Dying);
        clip.SetDuration(config.Dying.length);
        clip.Pause();
        _mixer.ConnectInput((int) Clip.Dying, clip, 0);
        var output = AnimationPlayableOutput.Create(_graph, "Enemy", animator);
        output.SetSourcePlayable(_mixer);
    }

    public void GameUpdate() {
        if (_transitionProgress >= 0f) {
            _transitionProgress += Time.deltaTime * transitionSped;
            if (_transitionProgress >= 1f) {
                _transitionProgress = -1f;
                SetWeight(CurrentClip, 1f);
                SetWeight(_previousClip, 0f);
                GetPlayable(_previousClip).Pause();
            }
            else {
                SetWeight(CurrentClip, _transitionProgress);
                SetWeight(_previousClip, 1f - _transitionProgress);
            }
        }
#if UNITY_EDITOR
        _clipTime = GetPlayable(CurrentClip).GetTime();
#endif
    }

#if UNITY_EDITOR

    public void RestoreHotReload(
        Animator animator,
        EnemyAnimationConfig config,
        float speed) {
        Configure(animator, config);
        GetPlayable(Clip.Move).SetSpeed(speed);
        var clip = GetPlayable(CurrentClip);
        clip.SetTime(_clipTime);
        clip.Play();
        SetWeight(CurrentClip, 1f);
        _graph.Play();
    }

#endif

    public void PlayIntro() {
        SetWeight(Clip.Intro, 1f);
        CurrentClip = Clip.Intro;
        _graph.Play();
        _transitionProgress = -1f;
    }

    public void PlayMove(float speed) {
        GetPlayable(Clip.Move).SetSpeed(speed);
        BeginTransition(Clip.Move);
    }

    public void PlayOutro() {
        BeginTransition(Clip.Outro);
    }

    public void PlayDying() {
        BeginTransition(Clip.Dying);
    }

    private Playable GetPlayable(Clip clip) {
        return _mixer.GetInput((int) clip);
    }

    private void SetWeight(Clip clip, float weight) {
        _mixer.SetInputWeight((int) clip, weight);
    }

    private void BeginTransition(Clip nextclip) {
        _previousClip = CurrentClip;
        CurrentClip = nextclip;
        _transitionProgress = 0f;
        GetPlayable(nextclip).Play();
    }

    public void Stop() {
        _graph.Stop();
    }

    public void Destroy() {
        _graph.Destroy();
    }
}