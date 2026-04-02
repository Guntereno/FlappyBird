using Microsoft.Xna.Framework.Audio;
using Momo.MathUtil;

namespace Momo.Audio;

public class SoundEffectPool
{
    public bool ShuffleOrder { get; set; } = true;

    private readonly List<SoundEffect> _sounds = new();
    private int[] _indexPool = [];
    private int _currentIndex = 0;
    private int _lastPlayedIndex = -1;
    private readonly Random _random;


    public SoundEffectPool(Random? random = null)
    {
        _random = (random == null) ? Random.Shared : random;
    }

    public void Add(SoundEffect soundEffect)
    {
        if (soundEffect == null)
            throw new ArgumentNullException(nameof(soundEffect));

        _sounds.Add(soundEffect);

        InitialiseIndexPool(_sounds.Count);
        _currentIndex = 0;
    }

    public void Play()
    {
        if (_sounds.Count == 0)
            return;

        if (_sounds.Count == 1)
        {
            _sounds[0].Play();
            return;
        }

        if (++_currentIndex >= _indexPool.Length)
        {
            _indexPool.Shuffle(_random);

            // If we accidentally picked the last played and there are others, swap with the last
            int next = _indexPool[0];
            if (next == _lastPlayedIndex)
            {
                int alt = _indexPool[^1];
                _indexPool[^1] = next;
                _indexPool[0] = alt;
            }

            _currentIndex = 0;
        }

        int index = _indexPool[_currentIndex];

        _lastPlayedIndex = index;
        _sounds[index].Play();
    }

    private void InitialiseIndexPool(int required)
    {
        if (_indexPool != null && _indexPool.Length >= required)
            return;

        _indexPool = new int[required];
        
        for (int i=0; i<required; ++i)
        {
            _indexPool[i] = i;
        }

        _indexPool.Shuffle(_random);
    }
}
