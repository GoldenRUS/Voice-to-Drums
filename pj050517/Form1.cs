using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Media;
using System.Threading;
using System.Diagnostics;
using NAudio.Wave;

namespace pj050517
{
    public partial class Form1 : Form
    {
        Wav wav;

        bool isRec = false;

        Output output;

        Neuronet nt;
        int parts = 8;

        //WaveIn - поток для записи
        WaveIn waveIn;
        //Класс для записи в файл
        WaveFileWriter writer;
        //Имя файла для записи
        string outputFilename = "tmp.wav";

        //Получение данных из входного буфера 
        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<WaveInEventArgs>(waveIn_DataAvailable), sender, e);
            }
            else
            {
                //Записываем данные из буфера в файл
                writer.Write(e.Buffer, 0, e.BytesRecorded);
            }
        }
        //Завершаем запись
        void StopRecording()
        {
            //MessageBox.Show("StopRecording");
            waveIn.StopRecording();
        }
        //Окончание записи
        private void waveIn_RecordingStopped(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler(waveIn_RecordingStopped), sender, e);
            }
            else
            {
                waveIn.Dispose();
                waveIn = null;
                writer.Close();
                writer = null;
            }
        }

        public Form1()
        {
            InitializeComponent();
        }
        void load()
        {
            var header = new WavHeader();
            // Размер заголовка
            var headerSize = Marshal.SizeOf(header);

            var fileStream = new FileStream(outputFilename, FileMode.Open, FileAccess.Read);
            var buffer = new byte[headerSize];
            fileStream.Read(buffer, 0, headerSize);

            // Чтобы не считывать каждое значение заголовка по отдельности,
            // воспользуемся выделением unmanaged блока памяти
            var headerPtr = Marshal.AllocHGlobal(headerSize);
            // Копируем считанные байты из файла в выделенный блок памяти
            Marshal.Copy(buffer, 0, headerPtr, headerSize);
            // Преобразовываем указатель на блок памяти к нашей структуре
            Marshal.PtrToStructure(headerPtr, header);

            // Выводим полученные данные
            label1.Text = "Sample rate: " + header.SampleRate;
            label2.Text = "Channels: " + header.NumChannels;
            label3.Text = "Bits per sample: " + header.BitsPerSample;

            // Посчитаем длительность воспроизведения в секундах
            var durationSeconds = 1.0 * header.Subchunk2Size / (header.BitsPerSample / 8.0) / header.NumChannels / header.SampleRate;
            var durationMinutes = (int)Math.Floor(durationSeconds / 60);
            durationSeconds = durationSeconds - (durationMinutes * 60);
            label4.Text = "Duration:" + durationMinutes + ":" + durationSeconds;

            // Освобождаем выделенный блок памяти
            Marshal.FreeHGlobal(headerPtr);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.ScrollBars = ScrollBars.Vertical;
            // Allow the RETURN key to be entered in the TextBox control.
            textBox1.AcceptsReturn = true;
            // Allow the TAB key to be entered in the TextBox control.
            textBox1.AcceptsTab = true;
            // Set WordWrap to true to allow text to wrap to the next line.
            textBox1.WordWrap = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            output.Make();


            //play
            play("out");

        }

        private void play(string id)
        {
            SoundPlayer simpleSound = new SoundPlayer(@".\" + id + ".wav");
            simpleSound.Play();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (isRec)
            {
                button2.Text = "Rec";
                if (waveIn != null)
                {
                    StopRecording();
                }
            }
            else
            {
                try
                {
                    waveIn = new WaveIn();
                    //Дефолтное устройство для записи (если оно имеется)
                    waveIn.DeviceNumber = 0;
                    //Прикрепляем к событию DataAvailable обработчик, возникающий при наличии записываемых данных
                    waveIn.DataAvailable += waveIn_DataAvailable;
                    //Прикрепляем обработчик завершения записи
                    waveIn.RecordingStopped += new EventHandler<NAudio.Wave.StoppedEventArgs>(waveIn_RecordingStopped);
                    //Формат wav-файла - принимает параметры - частоту дискретизации и количество каналов(здесь mono)
                    waveIn.WaveFormat = new WaveFormat(44100, 1);
                    //Инициализируем объект WaveFileWriter
                    writer = new WaveFileWriter(outputFilename, waveIn.WaveFormat);
                    //Начало записи
                    waveIn.StartRecording();
                    button2.Text = "Stop";

                }
                catch (Exception ex)
                { MessageBox.Show(ex.Message); }
            }
            isRec = !isRec;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Neurostud ns = new Neurostud();
            ns.teach(0, wav.range[0]);
            ns.teach(1, wav.range[1]);
            ns.teach(2, wav.range[2]);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            output = new Output();
            nt = new Neuronet(3);
            //parce
            load();
            wav = new Wav();
            wav.parce(outputFilename);

            //recogn
            for (int j = 0; j < wav.fft.Count(); j++)
            {
                int frame = wav.fft.delimed[j].Length / (2 * parts);
                Double[] tmp = new Double[parts];
                for (int i = 0; i < parts; i++)
                {
                    tmp[i] = wav.maxOfFrame(wav.fft.delimed[j].ToArray(), i * frame, frame);
                }
                tmp = wav.normalize(tmp);
                wav.range.Add(tmp);
                textBox1.Text += nt.compute(tmp).ToString() + "  -  " + wav.fft.sampletime[j] + "  ;  " + wav.fft.time[j] + "  |        ";
                output.Add(nt.compute(tmp), wav.fft.sampletime[j], wav.fft.time[j]);
                tmp = null;
            }
            GC.Collect(GC.MaxGeneration);

            //prepare to play
            output.LoadSamples(3);
            
            textBox2.Text = output.getBpm().ToString();

            checkBox1.Enabled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.ReadOnly = !checkBox1.Checked;
            output.chgBpm = checkBox1.Checked;
            output.newBpm = Convert.ToInt32(textBox2.Text);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            output.newBpm = Convert.ToInt32(textBox2.Text);
        }
    }
}
