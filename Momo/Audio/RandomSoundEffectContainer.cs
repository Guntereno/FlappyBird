using Microsoft.Xna.Framework.Audio;

namespace Momo.Audio;

public class SoundEffectPool
{
    public bool ShuffleOrder { get; set; } = true;

    private readonly List<SoundEffect> _sounds = new();
    private readonly Queue<int> _availableIndices = new();
    private int? _lastPlayedIndex;

    /// <summary>
    /// Adds a SoundEffect to the container. Can be called multiple times to add more sounds.
    /// </summary>
    public void Add(SoundEffect soundEffect)
    {
        if (soundEffect == null) throw new ArgumentNullException(nameof(soundEffect));

        _sounds.Add(soundEffect);
        _availableIndices.Enqueue(_sounds.Count - 1);
    }

    /// <summary>
    /// Plays a random sound effect, avoiding immediate repetition.
    /// </summary>
    public void Play()
    {
        if (_sounds.Count == 0) return;
        if (_sounds.Count == 1)
        {
            _sounds[0].Play();
            return;
        }

        // If no available indices, refill (all sounds have been played this cycle)
        if (_availableIndices.Count == 0)
        {
            ResetAvailableIndices(ShuffleOrder);
        }

        int index;

        // Pick a random sound that isn't the last one played
        do
        {
            index = _availableIndices.Dequeue();
        } while (index == _lastPlayedIndex && _availableIndices.Count > 0);

        _lastPlayedIndex = index;
        _sounds[index].Play();
    }

    /// <summary>
    /// Refills the available indices pool, optionally shuffled for random ordering.
    /// </summary>
    /// <param name="shuffle">If true, randomly shuffles the order of available sounds.</param>
    public void ResetAvailableIndices(bool shuffle)
    {
        _availableIndices.Clear();

        var indices = Enumerable.Range(0, _sounds.Count).ToList();

        if (shuffle)
        {
            // Fisher-Yates shuffle
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }
        }

        foreach (var index in indices)
        {
            _availableIndices.Enqueue(index);
        }
    }

    /// <summary>
    /// Clears all sounds from the container.
    /// </summary>
    public void Clear()
    {
        _sounds.Clear();
        _availableIndices.Clear();
        _lastPlayedIndex = null;
    }

    /// <summary>
    /// Gets the number of sounds currently in the container.
    /// </summary>
    public int Count => _sounds.Count;
}
