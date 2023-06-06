using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace Encoder
{
    public partial class Form1 : Form
    {
       public Bitmap b;
       public bool ok = false;


        public Form1()
        {
            InitializeComponent();
            Bitmap bmp = new Bitmap("C:\\Users\\gbsgr\\OneDrive\\Savannah's Photos\\questionmark.jpg");
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.Image = bmp;
            pictureBox2.Image = bmp;

        }

        public void button1_Click(object sender, EventArgs e)
        {


            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //create bitmap from ppm file and set it to picturebox
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

                    this.b = CreateBitmap();

                    pictureBox1.Image = b;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public void button2_Click(object sender, EventArgs e)
        {

            //user validation
            if (richTextBox1.Text == null || richTextBox2.Text == null)
            {
                MessageBox.Show("Please enter a message and password.");
            }
            else if (richTextBox1.Text.Length > b.Width)
            {
                MessageBox.Show("Message is too long.");
            }
            else if (richTextBox2.Text.Length > 10 || richTextBox2.Text.Length < 4)
            {
                MessageBox.Show("PIN number must be between 4 and 10 digits long.");
            }
            else if (b == null)
            {
                MessageBox.Show("Please select a photo first.");
            }
            else
            {
                string message = richTextBox1.Text;
                bool check = true;

                //checks message for invalid chars
                for (int i = 0; i < message.Length; i++)
                {
                    if (Char.IsLetter(message[i]) || message[i] == '.' || message[i] == '!' || message[i] == ' ' || message[i] == '?')
                    {
                        check = true;
                    }
                    else
                    {
                        check = false;
                        break;
                    }

                }

                if (!check)
                {
                    MessageBox.Show("Message must only contain letters and basic punctuation.");
                }
                else
                {

                    //message is checked, now check password
                    string password = richTextBox2.Text;
                    check = true;

                    //makes sure password is numbers only
                    for (int i = 0; i < password.Length; i++)
                    {
                        if (Char.IsDigit(password[i]))
                        {
                            check = true;
                        }
                        else
                        {
                            check = false;
                            break;
                        }

                    }

                    if (!check)
                    {
                        MessageBox.Show("Password can only contain numbers.");
                    }
                    //after everything is checked, filter and encode message
                    else
                    {
                        try
                        {

                            b = FilterBitmap(b);
                            b = EncodeBitmap(b, message);
                            b = EncodePassword(b, richTextBox2.Text);
                            pictureBox2.Image = b;

                            MessageBox.Show("Message successfully encoded.");

                            ok = true; //user can save file now
                           
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                    }


                }
            }
         
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (ok) //if message and password have been added
            {
                saveFileDialog1.Filter = "PPM File | *.ppm";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    SaveFile(b);
                }
            }
            else
            {
                MessageBox.Show("Message and password must be added first.");
            }
        }

        public Bitmap CreateBitmap()
        {
            string path = openFileDialog1.FileName;
            string ppm = "";

            //if P6 file
            if(path.Contains("raw"))
            {
                FileStream f = new FileStream(path, FileMode.OpenOrCreate);

                int count = 0;
                bool flag = true;
                int countN = 0;
                char ch = 'c';

                //reads and stores bytes until end of stream
                while (flag)
                {
                    count = f.ReadByte();

                    //if not end of stream
                    if(count != -1)
                    {
                       //if we're reading ppm header
                       if (countN < 4)
                       {
                           ch = (char)count;
                           ppm += ch;
                       }
                       //read RGB values
                       else
                       {
                           ppm += count.ToString();
                           ppm += "\n";
                       }


                       if (ch == '\n' && countN < 4)
                       {
                           countN++;
                       }
                    }
                    else //end of stream
                    {
                        flag = false;
                    }
                   
                }
            }
            else //if P3 file
            {
               //read and store entire file in string
               StreamReader reader = new StreamReader(openFileDialog1.FileName);
               ppm = reader.ReadToEnd();
            }

            
            string str = "";
            StringBuilder sb = new StringBuilder();
            int i = 0;
            bool endOfLine = true;
            int counter = 0;


            //determining what length my array holding the file info should be
            while (i < ppm.Length)
            {
                if (ppm[i] == '\n')
                {
                    counter++;
                }

                i++;
            }

            string[] ppmList = new string[counter];  //create array and variables
            i = 0;
            counter = 0;

            //storing ppm data into an array
            while (i < ppm.Length)
            {
                if (ppm[i] != '\n')
                {
                    str += ppm[i];
                }
                else
                {
                    ppmList[counter] = str + "\n";
                    str = "";
                    counter++;
                }

                i++;

            }

            str = ppmList[2].Trim();

            int width = 0;
            int height = 0;
            counter = 0;
            string s = "";
            Color c = new Color();
            int r = 0;
            int bl = 0;
            int g = 0;

            //getting width for bitmap
            for (i = 0; i < str.Length; i++)
            {
                if (Char.IsDigit(str[i]))
                {
                    s += str[i];
                }
                else if (str[i] == ' ')
                {
                    break;
                }

            }

            width = int.Parse(s);
            int num = s.Length;
            s = "";

            //getting height for bitmap
            for (i = num; i < str.Length; i++)
            {
                if (Char.IsDigit(str[i]))
                {
                    s += str[i];
                }
                else if (str[i] == ' ' && s != "")
                {
                    break;
                }
            }

            height = int.Parse(s);
            counter = 4;
            int wTracker = 0;
            int hTracker = 0;
            int x = 0;
            int y = 0;
            Bitmap b = new Bitmap(width, height);

             
            //create bitmap using array values
            while(endOfLine)
            {
                //if we haven't reached the end of row or list
                if(wTracker < (width * 3) && counter < ppmList.Length)
                {
                    //get RGB values from file
                    r = int.Parse(ppmList[counter]);
                    g = int.Parse(ppmList[counter + 1]);
                    bl = int.Parse(ppmList[counter + 2]);

                    //create color 
                    c = Color.FromArgb(r, g, bl);

                    //set pixel
                    b.SetPixel(x, y, c);

                    counter += 3;
                    wTracker += 3;
                    x++;
                    
                }
                else //we reached the end of the row
                {
                    if(hTracker < height)
                    {
                       wTracker = 0; //reset width to beginning of row
                       hTracker++;
                       x = 0;
                       y++;
                    }
                    else //we reached the end of pic
                    {
                        endOfLine = false;
                    }
                   
                }
            }

            return b;
        }

        public Bitmap FilterBitmap(Bitmap b)
        {
            Color c = new Color();

            //filter 2nd row
            int y = 2;

         
             for(int x = 0; x < b.Width; x++)
             {
                 if(b.GetPixel(x, y).G <= 122 && b.GetPixel(x, y).G >= 97) //if the green value falls in our ascii table set
                 {
                     //if it's closer to 122
                     if(b.GetPixel(x, y).G >= 110)
                     {
                         c = Color.FromArgb(b.GetPixel(x, y).R, 123, b.GetPixel(x, y).B);

                         b.SetPixel(x, y, c);
                     }
                     //if it's closer to 97
                     else
                     {
                         c = Color.FromArgb(b.GetPixel(x, y).R, 96, b.GetPixel(x, y).B);

                         b.SetPixel(x, y, c);
                     }
                 }
             }
           

            //filter next to last row for password
            y = b.Height - 1; 

            for(int x = 0; x < b.Width; x++)
            {
                if(b.GetPixel(x, y).G < 57 && b.GetPixel(x, y).G >= 48)  //if it falls within range
                {
                    //round Green value up
                    if(b.GetPixel(x, y).G >= 52)
                    {
                        c = Color.FromArgb(b.GetPixel(x, y).R, 58, b.GetPixel(x, y).B);

                        b.SetPixel(x, y, c);
                    }
                    //round down
                    else
                    {
                        c = Color.FromArgb(b.GetPixel(x, y).R, 47, b.GetPixel(x, y).B);

                        b.SetPixel(x, y, c);
                    }
                }
            }

            return b;
        }

        public Bitmap EncodeBitmap(Bitmap b, string message)
        {

            message = message.ToLower();
            int current = 0;
            int i = 0;
            int x = 0;
            int y = 2; //start at the beginning of 2nd row
            Color c = new Color();

            //encode each char
            while(i < message.Length)
            {
                //get next char
                current = message[i];

                //create color using new Green value
                c = Color.FromArgb(b.GetPixel(x, y).R, current, b.GetPixel(x, y).B);

                //set new pixel
                b.SetPixel(x, y, c);

                x++;
                i++;

            }

            return b; //return encoded bitmap
        }

        public Bitmap EncodePassword(Bitmap b, string password)
        {
   
            int i = 0;
            int x = 0;
            int y = b.Height - 1; //next to last row
            int current = 0;
            Color c = new Color();

            while(i < password.Length)
            {
                current = password[i]; //get next char

                //replace Green value with new color
                c = Color.FromArgb(b.GetPixel(x, y).R, current, b.GetPixel(x, y).B);

                b.SetPixel(x, y, c);

                x++; //move down row
                i++;

            }

            return b;
        }


        public void SaveFile(Bitmap b)
        {
            DateTime dt = DateTime.Now;
            string path = saveFileDialog1.FileName;
           
            //open streamwriter
            using(StreamWriter sw = new StreamWriter(path))
            {
               //write header info
               sw.WriteLine("P3");
               sw.WriteLine("# Encoded ppm file created by Savannah Luker on " + dt.ToString("d"));
               sw.WriteLine(b.Width.ToString() + " " + b.Height.ToString());
               sw.WriteLine("255");

               //write RGB values
               for (int y = 0; y < b.Height; y++)
               {
                   for (int x = 0; x < b.Width; x++)
                   {
                       sw.WriteLine(b.GetPixel(x, y).R.ToString());
                       sw.WriteLine(b.GetPixel(x, y).G.ToString());
                       sw.WriteLine(b.GetPixel(x, y).B.ToString());
                   }
               }
            }

            
        }
    }


}