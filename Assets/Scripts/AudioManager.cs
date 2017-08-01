using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains the information for a sound clip such as volume and clip itself
/// </summary>
[System.Serializable]
public class SoundClip
{
    /// <summary>
    /// The name of the sound clip
    /// </summary>
    public AudioManager.SoundName soundName;

    /// <summary>
    /// volume level to play this clip at
    /// </summary>
    [Range(0, 1)]
    public float volume = 1f;

    /// <summary>
    /// The audio clip component
    /// </summary>
    public AudioClip clip;

    /// <summary>
    /// References the audio source component associated with this clip
    /// </summary>
    AudioSource source;

    /// <summary>
    /// Create the AudioSource and assign it the clio
    /// </summary>
    public void SetSource(AudioSource source)
    {
        this.source = source;
        this.source.playOnAwake = false;
        this.source.clip = this.clip;
    }

    /// <summary>
    /// Plays this sound clip if not already playing
    /// </summary>
    public void Play()
    {
        this.source.volume = this.volume;
        this.source.Play();
    }
}

/// <summary>
/// Controls playing the sounds for the game
/// There's only ever one of the audio manager (singleton)
/// Creates AudioSources for each sound so that all sounds can be independent from each other
/// </summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// A list of sound name enums to reference the sound clip to play
    /// </summary>
    public enum SoundName
    {
        Bomb,
        BridgeActivated,
        BridgeDeactivated,
        CompanionCharge,
        CompanionDrain, 
        CompanionDropped,
        CompanionPickedUp,
        CompanionPowerDown,
        PlayerFalls,
        TeleportalActivated,
        TeleportalDeactivated,
        TeleportalUsed,
        SpikesActivated,
        SpikesDeactivated,
		CompanionRecall,
        NoSound,
    }

    /// <summary>
    /// A reference to the AudioManager singleton
    /// </summary>
    public static AudioManager instance;

    /// <summary>
    /// Contains a list of all the sound clips to use
    /// </summary>
    [SerializeField]
    SoundClip[] clips;

    /// <summary>
    /// A references to the 
    /// </summary>
    Dictionary<SoundName, SoundClip> soundClips = new Dictionary<SoundName, SoundClip>();
	
    /// <summary>
    /// Singleton setup
    /// </summary>
	void Update ()
    {
	    if(instance == null) {
            instance = this;
        } else {
            Destroy(this);
        }
        DontDestroyOnLoad(this);
	}

    /// <summary>
    /// Creates game objects for all of the sound clips
    /// </summary>
    void Start()
    {
        foreach(SoundClip clip in this.clips) {
            GameObject soundGO = new GameObject(clip.soundName.ToString());
            soundGO.transform.SetParent(this.transform);
            soundGO.AddComponent(typeof(SoundClip));
            clip.SetSource(soundGO.AddComponent<AudioSource>());

            // store a reference so that we can invoke it later
            this.soundClips[clip.soundName] = clip;
        }
    }

    /// <summary>
    /// Plays the given sound if it exists
    /// </summary>
    /// <param name="soundName"></param>
    public void PlaySound(SoundName soundName)
    {
        if(this.soundClips.ContainsKey(soundName)) {
            this.soundClips[soundName].Play();
        }
    }
}
