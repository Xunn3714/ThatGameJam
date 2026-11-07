using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeedGame
{
    // One persistent sound bed keeps the music continuous across level changes.
    public sealed class GameAudio : MonoBehaviour
    {
        static GameAudio instance;

        AudioSource music;
        AudioSource effects;
        AudioSource resonance;
        AudioSource plantWindow;
        AudioSource openingWind;
        AudioClip[] grass;
        AudioClip[] concrete;
        AudioClip[] plantCuts;
        AudioClip plantLong;
        AudioClip wind;
        AudioClip energyRecharge;
        AudioClip empty;
        AudioClip gear;
        AudioClip shine;
        float nextWindAt;
        float resonanceEndsAt;
        float plantWindowEndsAt;
        float lastPlantAt = -10f;
        int lastGrass = -1;
        int lastConcrete = -1;
        int lastPlant = -1;
        bool rebornWind;
        bool openingWindActive;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (instance) return;
            new GameObject("Game audio").AddComponent<GameAudio>();
        }

        void Awake()
        {
            if (instance && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
            music = NewSource("Music", .3f);
            effects = NewSource("Effects", .65f);
            resonance = NewSource("Resonance", .24f);
            plantWindow = NewSource("Plant growth window", .55f);
            openingWind = NewSource("Opening comic wind", 0f);
            grass = LoadNumbered("Grass", 4);
            concrete = LoadNumbered("Concrete", 3);
            plantLong = Load("PlantsMovement");
            if (plantLong) plantLong.LoadAudioData();
            plantCuts = MakePlantCuts(plantLong);
            wind = Load("SoftWind");
            openingWind.clip = wind;
            openingWind.loop = false;
            energyRecharge = Load("EnergyRecharge");
            empty = Load("NoEnergy");
            gear = Load("GearPickup");
            shine = Load("Resonance");
            music.clip = Load("GameMusic");
            music.loop = true;
            if (music.clip) music.Play();
            ScheduleWind(true);
        }

        void Update()
        {
            if (resonance.isPlaying && Time.time >= resonanceEndsAt) resonance.Stop();
            if (plantWindow.isPlaying && Time.time >= plantWindowEndsAt) plantWindow.Stop();
            if (openingWind && openingWind.clip)
            {
                float target = openingWindActive ? .14f : 0f;
                openingWind.volume = Mathf.MoveTowards(openingWind.volume, target, Time.unscaledDeltaTime * .22f);
                if (!openingWindActive && openingWind.volume <= .001f && openingWind.isPlaying)
                    openingWind.Stop();
            }
            if (wind && Time.time >= nextWindAt)
            {
                if (SceneManager.GetActiveScene().name == "Level03_Alpha")
                    effects.PlayOneShot(wind, rebornWind ? .055f : .035f);
                ScheduleWind(false);
            }
        }

        public static void PlayFootstep()
        {
            if (!instance) return;
            bool onStone = SceneManager.GetActiveScene().name == "Level02_Alpha";
            var choices = onStone ? instance.concrete : instance.grass;
            int last = onStone ? instance.lastConcrete : instance.lastGrass;
            var clip = PickDifferent(choices, ref last);
            if (onStone) instance.lastConcrete = last;
            else instance.lastGrass = last;
            if (clip) instance.effects.PlayOneShot(clip, onStone ? .43f : .37f);
        }

        public static void PlayResonance(int stage)
        {
            if (!instance || !instance.shine) return;
            instance.resonance.Stop();
            instance.resonance.clip = instance.shine;
            // Each new chain starts on Do; only expansion raises the note.
            float[] pitches = { 1f, 1.122462f, 1.259921f, 1.33484f };
            instance.resonance.pitch = pitches[Mathf.Clamp(stage - 1, 0, pitches.Length - 1)];
            instance.resonance.time = 0f;
            instance.resonance.Play();
            instance.resonanceEndsAt = Time.time + 2.1f;
        }

        public static void PlayNoEnergy()
        {
            if (instance && instance.empty) instance.effects.PlayOneShot(instance.empty, .8f);
        }

        public static void PlayGearPickup()
        {
            if (instance && instance.gear) instance.effects.PlayOneShot(instance.gear, .55f);
        }

        public static void PlayEnergyRecharge()
        {
            if (instance && instance.energyRecharge)
                instance.effects.PlayOneShot(instance.energyRecharge, .38f);
        }

        public static void SetOpeningComicWind(bool enabled)
        {
            if (!instance || !instance.openingWind || !instance.wind) return;
            instance.openingWindActive = enabled;
            if (enabled && !instance.openingWind.isPlaying)
            {
                instance.openingWind.volume = 0f;
                instance.openingWind.Play();
            }
        }

        public static void PlayPlantGrowth()
        {
            if (!instance || Time.time - instance.lastPlantAt < .18f) return;
            instance.lastPlantAt = Time.time;
            if ((instance.plantCuts == null || instance.plantCuts.Length == 0) &&
                instance.plantLong && instance.plantLong.loadState == AudioDataLoadState.Loaded)
                instance.plantCuts = MakePlantCuts(instance.plantLong);
            int index = instance.lastPlant;
            var cut = PickDifferent(instance.plantCuts, ref index);
            instance.lastPlant = index;
            if (cut)
            {
                instance.effects.PlayOneShot(cut, .60f);
                return;
            }
            // If an MP3 was not decompressed, still play a short timed section.
            if (!instance.plantLong) return;
            instance.plantLong.LoadAudioData();
            instance.plantWindow.Stop();
            instance.plantWindow.clip = instance.plantLong;
            float[] starts = { 1.0f, 6.0f, 11.0f, 17.0f };
            instance.plantWindow.time = Mathf.Min(starts[UnityEngine.Random.Range(0, starts.Length)],
                Mathf.Max(0f, instance.plantLong.length - .8f));
            instance.plantWindow.Play();
            instance.plantWindowEndsAt = Time.time + .9f;
        }

        public static void SetRebornWind(bool enabled)
        {
            if (!instance) return;
            instance.rebornWind = enabled;
            if (enabled)
                instance.nextWindAt = Time.time + UnityEngine.Random.Range(3f, 7f);
            else
                instance.ScheduleWind(true);
        }

        AudioSource NewSource(string label, float volume)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = volume;
            return source;
        }

        static AudioClip Load(string name) => Resources.Load<AudioClip>("Audio/" + name);

        static AudioClip[] LoadNumbered(string prefix, int count)
        {
            var clips = new AudioClip[count];
            for (int i = 0; i < count; i++) clips[i] = Load(prefix + (i + 1));
            return clips;
        }

        static AudioClip PickDifferent(AudioClip[] clips, ref int previous)
        {
            if (clips == null || clips.Length == 0) return null;
            int choice;
            if (previous < 0 || previous >= clips.Length || clips.Length == 1)
                choice = UnityEngine.Random.Range(0, clips.Length);
            else
            {
                choice = UnityEngine.Random.Range(0, clips.Length - 1);
                if (choice >= previous) choice++;
            }
            previous = choice;
            return clips[choice];
        }

        static AudioClip[] MakePlantCuts(AudioClip source)
        {
            if (!source || source.samples <= 0 || source.channels <= 0) return Array.Empty<AudioClip>();
            var audio = new float[source.samples * source.channels];
            if (!source.GetData(audio, 0)) return Array.Empty<AudioClip>();
            int cutFrames = Mathf.Min(source.samples, Mathf.RoundToInt(.7f * source.frequency));
            int scanFrames = Mathf.Max(1, Mathf.RoundToInt(.3f * source.frequency));
            var candidates = new List<(int start, float power)>();
            for (int start = 0; start + cutFrames < source.samples; start += scanFrames)
            {
                float power = 0f;
                int reads = 0;
                for (int frame = start; frame < start + cutFrames; frame += 64)
                {
                    float sample = audio[frame * source.channels];
                    power += sample * sample;
                    reads++;
                }
                candidates.Add((start, power / Mathf.Max(1, reads)));
            }
            candidates.Sort((a, b) => b.power.CompareTo(a.power));
            var selected = new List<int>();
            int spacing = Mathf.RoundToInt(1.5f * source.frequency);
            foreach (var candidate in candidates)
            {
                if (candidate.power < .00001f) break;
                bool tooClose = false;
                foreach (int start in selected)
                    if (Mathf.Abs(start - candidate.start) < spacing) tooClose = true;
                if (tooClose) continue;
                selected.Add(candidate.start);
                if (selected.Count == 4) break;
            }
            var cuts = new AudioClip[selected.Count];
            int fadeFrames = Mathf.RoundToInt(.04f * source.frequency);
            for (int i = 0; i < selected.Count; i++)
            {
                var samples = new float[cutFrames * source.channels];
                Array.Copy(audio, selected[i] * source.channels,
                    samples, 0, samples.Length);
                for (int frame = 0; frame < cutFrames; frame++)
                {
                    float fade = Mathf.Min(1f, frame / (float)fadeFrames,
                        (cutFrames - frame - 1) / (float)fadeFrames);
                    for (int channel = 0; channel < source.channels; channel++)
                        samples[frame * source.channels + channel] *= fade;
                }
                cuts[i] = AudioClip.Create("Plant growth short " + (i + 1),
                    cutFrames, source.channels, source.frequency, false);
                cuts[i].SetData(samples, 0);
            }
            return cuts;
        }

        void ScheduleWind(bool first)
        {
            bool lush = rebornWind && SceneManager.GetActiveScene().name == "Level03_Alpha";
            nextWindAt = Time.time + UnityEngine.Random.Range(
                first ? 16f : lush ? 11f : 45f,
                first ? 28f : lush ? 22f : 80f);
        }

        void OnDestroy()
        {
            if (instance != this) return;
            instance = null;
            if (plantCuts != null)
                foreach (var cut in plantCuts) if (cut) Destroy(cut);
        }
    }
}
