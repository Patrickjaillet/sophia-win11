using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

public sealed class AnimationService : IAnimationService
{
    private readonly ILogger<AnimationService> _logger;

    public AnimationService(ILogger<AnimationService> logger)
    {
        _logger = logger;
        AnimationsEnabled = true;
    }

    public bool AnimationsEnabled { get; private set; }

    public void PlayIntro()
    {
        throw new NotImplementedException();
    }
}
