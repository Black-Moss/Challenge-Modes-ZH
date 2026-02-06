using System;
using System.IO;
using UnityEngine;
using BepInEx.Logging;
using System.Reflection;
using System.Linq;
using NAudio.Wave;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ChallengeModes.Helpers
{
    public static class FileLoader
    {
        // Loads files that have been set to Embedded Resource in the Build Action file properties
        public static (string, Stream) LoadFileStream(string fileName, string folderName = null)
        {
            // Gets the file name. If it isn't exact, it returns null
            Assembly asm = Assembly.GetExecutingAssembly();
            fileName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(fileName));
            if (fileName == null)
            {
                Debug.LogError($"名为 {fileName} 的文件不存在，请检查大小写和文件扩展名");
                return (null, null);
            }

            // Gets the file stream
            Stream stream = asm.GetManifestResourceStream(fileName);
            if (!fileName.StartsWith(Assembly.GetExecutingAssembly().GetName().Name + "." + (folderName != null ? folderName + "." : "")))
            {
                Debug.LogError($"文件在嵌入资源中不存在");
                return (null, null);
            }
            int lastDot = fileName.LastIndexOf(".");
            int secondLastDot = fileName.LastIndexOf(".", lastDot - 1);
            return (fileName.Substring(secondLastDot + 1), stream);
        }

        public static (string, byte[]) LoadFileBytes(string fileName)
        {
            (string, Stream) fileInfo = LoadFileStream(fileName);
            Stream stream = fileInfo.Item2;
            byte[] fileData = new byte[stream.Length];
            stream.Read(fileData, 0, fileData.Length);
            return (fileInfo.Item1, fileData);
        }

        public static AudioClip LoadEmbeddedAudio(string fileName)
        {
            (string, Stream) streamData = LoadFileStream(fileName);
            string newFileName = streamData.Item1;
            Stream stream = streamData.Item2;
            string clipName = newFileName.Substring(10);
            string fileExt = clipName.Substring(clipName.IndexOf(".") + 1);
            ISampleProvider provider;
            switch (fileExt)
            {
                case "wav":
                    provider = new WaveFileReader(stream).ToSampleProvider();
                    break;

                case "mp1":
                case "mp2":
                case "mp3":
                    provider = new Mp3FileReader(stream).ToSampleProvider();
                    break;

                case "cue":
                    provider = new CueWaveFileReader(stream).ToSampleProvider();
                    break;

                case "aif":
                case "aiff":
                    provider = new AiffFileReader(stream).ToSampleProvider();
                    break;

                default:
                    Debug.LogError($"无法加载名为 {fileName} 的音频文件: 未知扩展名 {fileExt}");
                    return null;
            }

            // Reads file data sample rate and channels to make a samples data array
            List<float> samples = new List<float>();
            float[] buffer = new float[provider.WaveFormat.SampleRate * provider.WaveFormat.Channels];
            int read;
            while ((read = provider.Read(buffer, 0, buffer.Length)) > 0)
            {
                samples.AddRange(buffer.Take(read));
            }

            // Sets mandatory variables for sample rate, channels, and samples/channel
            WaveFormat waveFormat = provider.WaveFormat;
            int sampleRate = waveFormat.SampleRate;
            int channels = waveFormat.Channels;
            int samplesPerChannel = samples.Count / channels;

            // Creates and returns the audio clip
            AudioClip clip = AudioClip.Create(fileName, samplesPerChannel, channels, sampleRate, false);
            clip.SetData(samples.ToArray(), 0);
            return clip;
        }

        public static Sprite LoadEmbeddedSprite(string fileName, float ppu = 100, FilterMode filterMode = FilterMode.Point, int widthMultiplier = 1, int heightMultiplier = 1)
        {
            (string, Stream) streamData = LoadFileStream(fileName);
            string newFilename = streamData.Item1;
            Stream stream = streamData.Item2;
            byte[] fileData = new byte[stream.Length];
            stream.Read(fileData, 0, fileData.Length);
            // Creates texture and sprite by the stream data
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBAHalf, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = filterMode,
                name = newFilename
            };
            texture.LoadImage(fileData);
            texture.Apply();

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                ppu
            );
            sprite.name = newFilename;

            return sprite;
        }
    }
}