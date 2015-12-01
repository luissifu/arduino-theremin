using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Configuration;

namespace InterfacesFisicasGUI
{
    public partial class Form1 : Form
    {
        private OpenFileDialog ofd = new OpenFileDialog();
        private String filePath = "";
        private SoundPlayer sp = new SoundPlayer();
        private SerialPort arduinoBoard = new SerialPort("COM5", 9600);
        private Boolean stop = false;
        private int[] sizes = new int[6];
        private double maxHeight;
        private Object syncLock = new Object();

        public Form1()
        {
            
            InitializeComponent();
            ofd.Filter = "Wav Files (.wav)|*.wav";
            ofd.Title = "Selecciona un archivo sampler";
            
            arduinoBoard.Open();

            Thread t3 = new Thread(delegate()
            {
                while (!stop)
                {
                    visualUpdate();
                    //Console.WriteLine(sizes[0]);
                    Thread.Sleep(100);
                }
            });

            Thread t2 = new Thread(delegate()
            {
                char[] buffer = new char[1];

                while (!stop)
                {
                    arduinoBoard.Read(buffer, 0, 1);
                    makeSound(buffer[0]);
                    
                }
            });
            t2.Start();
            t3.Start();
            
            if (!System.IO.Directory.Exists("gensamples/"))
                System.IO.Directory.CreateDirectory("gensamples/");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                
                lblSamplerName.Text = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName);
                filePath = ofd.FileName;
                filePath = filePath.Substring(0, filePath.Length - 4);
                sp.buildSamples(filePath);
                sp.addSound(10);

                Thread t = new Thread(delegate()
                {
                    sp.run();
                });
                t.Start();

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            maxHeight = gbLimit.Height;
            for(int i = 0; i < 6; i++){
                sizes[i]=0;
            }
        }

        public void makeSound(int tone)
        {
            if (sp.isReady())
            {
                sp.addSound(tone);
                int aux = (tone - 1) / 6 + 1;

                resizeBox((tone-1)%6, (int)(aux * 16.666666666666666));
                Console.WriteLine(tone);
            }
            
            Console.WriteLine("^");
            //visualUpdate();
        }

        private void visualUpdate()
        {
            updateSizes();
            updateBox(boxB, (int)(sizes[0] * maxHeight / 100.0));
            updateBox(boxC, (int)(sizes[1] * maxHeight / 100.0));
            updateBox(boxD, (int)(sizes[2] * maxHeight / 100.0));
            updateBox(boxE, (int)(sizes[3] * maxHeight / 100.0));
            updateBox(boxF, (int)(sizes[4] * maxHeight / 100.0));
            updateBox(boxG, (int)(sizes[5] * maxHeight / 100.0));

        }

        private void updateBox(PictureBox box, int size){
            if (ControlInvokeRequired(box, () => updateBox(box, size))) return;
            box.Height = size;
            //box.Location = new Point(box.Location.X, -size);
        }

        private void resizeBox(int index, int percent){
            if (sizes[index] < percent)
            {
                sizes[index] = percent;
            }
        }

        private void updateSizes()
        {
            for (int i = 0; i < 6; i++)
            {
                if (sizes[i] > 70)
                {
                    sizes[i] = (int)(sizes[i] * 0.85);
                }
                else if (sizes[i] > 60)
                {
                    sizes[i] = (int)(sizes[i] * 0.90);
                }
                if (sizes[i] > 40)
                {
                    sizes[i] = (int)(sizes[i] * 0.95);
                }
                else if (sizes[i] > 0)
                {
                    sizes[i]--;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            sp.kill();
            stop = true;
        }

        public bool ControlInvokeRequired(Control c, Action a)
        {
            try
            {
                if (c.InvokeRequired) c.Invoke(new MethodInvoker(delegate { a(); }));
                else return false;

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            resizeBox(0, 100);
            resizeBox(1, 90);
            resizeBox(2, 80);
            resizeBox(3, 70);
            resizeBox(4, 50);
            resizeBox(5, 60);
        }
    }
}
