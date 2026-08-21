namespace SophiaWin11.Core.Abstractions;

public interface IAnimationService
{
    bool AnimationsEnabled { get; }

    void PlayIntro();
}
