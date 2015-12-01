using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SoundTouchNet;
using NAudio.Wave;
using NAudio;

namespace InterfacesFisicasGUI
{
    class SoundPlayer
    {
        Boolean doneLoading = false;
        String fileName;
        LinkedList<int> sounds;
        bool running;
        readonly object locker = new object();

        public bool isReady()
        {
            return doneLoading;
        }

        public SoundPlayer()
        {
            sounds = new LinkedList<int>();
            running = true;
        }

        public void kill()
        {
            running = false;
        }

        public void run()
        {
            while (running)
            {
                if (sounds.Count() > 0)
                {
                    String sound = "";
                    lock (locker)
                    {
                        sound = "gensamples/" + System.IO.Path.GetFileNameWithoutExtension(fileName) + sounds.First() + ".wav";
                        sounds.RemoveFirst();
                    }
                    playSound(sound);
                }
            }
        }

        public void addSound(int which)
        {
            lock (locker)
            {
                if (doneLoading)
                    sounds.AddFirst(which);
            }
        }

        void playSound(String fileName)
        {
            var output = new WaveOutEvent();
            try
            {
                var player = new WaveFileReader(fileName);
                output.Init(player);
                output.Play();
                while (output.PlaybackState == PlaybackState.Playing && sounds.Count == 0)
                {
                    Thread.Sleep(100);
                }
                output.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void buildSamples(String fileName)
        {
            doneLoading = false;
            this.fileName = fileName;
            for (float pitch = 0.1f; pitch <= 5.0; pitch += 0.1f)
            {
                int id = (int)(pitch * 10);
                changePitchNTempo(fileName + ".wav", "gensamples/" + System.IO.Path.GetFileNameWithoutExtension(fileName) + id + ".wav", 1.0f, pitch);
            }
            doneLoading = true;
        }

        void changePitchNTempo(String fileName, String fileOut, float newTempo, float newPitch)
        {
            WaveFileReader reader = new WaveFileReader(fileName);
            int numChannels = reader.WaveFormat.Channels;
            if (numChannels > 2)
                throw new Exception("SoundTouch supports only mono or stereo.");

            int sampleRate = reader.WaveFormat.SampleRate;
            int bitPerSample = reader.WaveFormat.BitsPerSample;
            const int BUFFER_SIZE = 1024 * 16;

            SoundStretcher stretcher = new SoundStretcher(sampleRate, numChannels);
            WaveFileWriter writer = new WaveFileWriter(fileOut, new WaveFormat(sampleRate, 16, numChannels));

            stretcher.Tempo = newTempo;
            stretcher.Pitch = newPitch;

            byte[] buffer = new byte[BUFFER_SIZE];
            short[] buffer2 = null;

            if (bitPerSample != 16 && bitPerSample != 8)
            {
                throw new Exception("Not implemented yet.");
            }

            if (bitPerSample == 8)
            {
                buffer2 = new short[BUFFER_SIZE];
            }

            bool finished = false;

            while (true)
            {
                int bytesRead = 0;
                if (!finished)
                {
                    bytesRead = reader.Read(buffer, 0, BUFFER_SIZE);

                    if (bytesRead == 0)
                    {
                        finished = true;
                        stretcher.Flush();
                    }
                    else
                    {
                        if (bitPerSample == 16)
                        {
                            stretcher.PutSamplesFromBuffer(buffer, 0, bytesRead);
                        }
                        else if (bitPerSample == 8)
                        {
                            for (int i = 0; i < BUFFER_SIZE; i++)
                                buffer2[i] = (short)((buffer[i] - 128) * 256);

                            stretcher.PutSamples(buffer2);
                        }
                    }
                }

                bytesRead = stretcher.ReceiveSamplesToBuffer(buffer, 0, BUFFER_SIZE);
                writer.Write(buffer, 0, bytesRead);

                if (finished && bytesRead == 0)
                    break;
            }

            reader.Close();
            writer.Close();
        }

    }
}
