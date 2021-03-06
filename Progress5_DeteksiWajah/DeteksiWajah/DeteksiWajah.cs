﻿/*=================================================
 *Kelompok 4
 *[10109316] Riza Hardian
 *[10109321] Mirah Anugraheny
 *[10109326] Andri Nugraha Ramdhon
 *[10109345] Fitrianingsih
 *IF-8
 *Sistem Multimedia
 * ================================================== */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;



namespace deteksiwajah
{
    public partial class DeteksiWajah : Form
    {
        //variabel global
        private Capture tangkap; //mengambil gambar dari kamera dan menyimpan ke dalam frame-frame gambar
        private HaarCascade haar; //deklarasi penggunaan kelas viola-jones untuk mendeteksi gambar 
        Image<Bgr, Byte> FrameGambar;
        Bitmap[] ExtWajah;
        int NoWajah = 0;
        public DeteksiWajah() 
        {
            InitializeComponent(); //inialisasi komponen
        }

        private void ProsesMenangkapFrame(object sender, EventArgs arg)
        {
            //menyimpan frame gambar yang ditangkap kamera ke imageframe
            Image<Bgr, Byte> FrameGambar = tangkap.QueryFrame();
            //mendeteksi wajah dari framegambar
            if (FrameGambar != null) //mengkonfirmasi valid tidaknya framegambar
            {
                //konversi gambar ke gray-scale
                Image<Gray, byte> framegrayscale = FrameGambar.Convert<Gray, byte>();

                //mendeteksi wajah dari gray-scale image dan menyimpan ke dalam array wajah
                var wajah = framegrayscale.DetectHaarCascade(haar, 1.1, 2,
                                        HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                                        new Size(25, 25))[0];
                //menandai kotak biru pada wajah yang dideteksi dari gambar
                foreach (var wajah2 in wajah)
                {
                    FrameGambar.Draw(wajah2.rect, new Bgr(Color.Blue),3);
                }
            }
            //menampilkan gambar pada EmguCV ImageBox
            KameraImageBox.Image = FrameGambar.Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR, true);
        }

        private void btnKamera_Click(object sender, EventArgs e)
        {
            if (tangkap == null)
            {
                try
                {
                    tangkap = new Capture();
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }


            if (tangkap != null)
            {
                if (btnKamera.Text=="Berhenti")
                {
                    btnKamera.Text = "Dari Live Kamera";
                    Application.Idle -= ProsesMenangkapFrame;
                }
                else
                {
                    btnKamera.Text = "Berhenti";
                    Application.Idle += ProsesMenangkapFrame;
                }
            }
        }

        private void MendeteksiWajah()
        {
            //menyimpan frame gambar yang ditangkap kamera ke imageframe
            Image<Gray, byte> framegrayscale = FrameGambar.Convert<Gray, byte>();
            //mendeteksi wajah dari gray-scale image dan menyimpan ke dalam array wajah
            var wajah = framegrayscale.DetectHaarCascade(haar, 1.1, 1,
                                    HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                                    new Size(25, 25))[0];
            if (wajah.Length > 0)
            {
                //menampilkan jumlah wajah yang terdeteksi
                MessageBox.Show("Total wajah yang terdeteksi: " + wajah.Length.ToString());
                //deklarasi awal pembuatan ekstraksi wajah yang sudah ditangkap
                Bitmap BmpInput = framegrayscale.ToBitmap();
                Bitmap EkstraksiWajah;
                Graphics KanvasWajah;
                ExtWajah = new Bitmap[wajah.Length];
                NoWajah = 0;
                //menandai kotak biru pada wajah yang dideteksi dari gambar
                foreach (var wajah2 in wajah)
                {
                    FrameGambar.Draw(wajah2.rect, new Bgr(Color.Blue), 3);
                    //setting ukuran extraksiwajah yang akan menyimpan hasil data pendeteksian wajah
                    EkstraksiWajah = new Bitmap(wajah2.rect.Width, wajah2.rect.Height);
                    //set set kanvas yang nantinya akan digambarkan hasil dari ekstraksiwajah
                    KanvasWajah = Graphics.FromImage(EkstraksiWajah);
                    KanvasWajah.DrawImage(BmpInput, 0, 0, wajah2.rect, GraphicsUnit.Pixel);
                    ExtWajah[NoWajah] = EkstraksiWajah;
                    NoWajah++;
                }
                //menampilkan ekstraksiwajah ke dalam array extwajah[] 
                pbGambar.Image = ExtWajah[0];
                //menampilkan gambar ke kameraimagebox
                KameraImageBox.Image = FrameGambar;
                btnNext.Enabled = true;
                btnPrev.Enabled = true;
            }
        }

        private void DeteksiWajah_Load(object sender, EventArgs e)
        {
            //mencari xml yang akan digunakan
            haar = new HaarCascade("haarcascade_frontalface_alt_tree.xml");
        }

        private void btnGambar_Click(object sender, EventArgs e)
        {
            //mengambil data gambar dari direktori untuk di deteksi wajah
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Image InputGambar = Image.FromFile(openFileDialog.FileName);
                FrameGambar = new Image<Bgr, byte>(new Bitmap(InputGambar));
                KameraImageBox.Image = FrameGambar;
                MendeteksiWajah();
            }
            //menyimpan gambar ke direktori c:\
            FrameGambar.Save(@"C:\ScDariGambar.jpg");
            //menampilkan notifikasi penyimpanan gambar
            MessageBox.Show("Gambar telah disimpan di 'C:\' dengan nama 'ScDariGambar.jpg'");
        }

        //diskonek kamera
        private void ReleaseData()
        {
            if (tangkap != null)
                tangkap.Dispose();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (NoWajah > 0)
            {
                NoWajah--;
                pbGambar.Image = ExtWajah[NoWajah];
            }
            else
                MessageBox.Show("Ini gambar wajah pertama");
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (NoWajah < ExtWajah.Length - 1)
            {
                NoWajah++;
                pbGambar.Image = ExtWajah[NoWajah];
            }
            else
                MessageBox.Show("Ini gambar wajah terakhir");
        }

        private void btnFotoKamera_Click(object sender, EventArgs e)
        {
            //inialisasi jika tangkap=null
            if (tangkap == null)
            {
                try
                {
                    tangkap = new Capture();
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }
            if (tangkap != null)
            {
                if (btnFotoKamera.Text == "Foto Sekarang")
                { 
                    btnFotoKamera.Text = "Dari Foto kamera";

                    //menghentikan streaming video dari kamera
                    Application.Idle -= ProsesPenangkapanFrameTanpaDeteksi;

                    //Call face detection
                    MendeteksiWajah();
                    //menyimpan gambar ke direktori c:\
                    FrameGambar.Save(@"C:\ScDariFotoKamera.jpg");
                    //menampilkan notifikasi penyimpanan gambar
                    MessageBox.Show("Gambar telah disimpan di 'C:\' dengan nama 'ScDariFotoKamera.jpg'");
                }
                else
                {
                    //if camera is NOT getting frames then start the capture and set button
                    // Text to "Pause" for pausing capture
                    btnFotoKamera.Text = "Foto Sekarang";
                    Application.Idle += ProsesPenangkapanFrameTanpaDeteksi;
                }
            }
        }
        private void ProsesPenangkapanFrameTanpaDeteksi(object sender, EventArgs arg)
        {
            //fetch the frame captured by web camera
            FrameGambar = tangkap.QueryFrame();

            //show the image in the EmguCV ImageBox
            KameraImageBox.Image = FrameGambar;
        }
    }
}    
        

