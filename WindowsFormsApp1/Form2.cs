using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using xNet;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {

        public Form2()
        {
            InitializeComponent();

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.Text = "Auth form";
#pragma warning disable S1075 // URIs should not be hardcoded
            webBrowser1.Navigate("https://oauth.vk.com/authorize?client_id=5384783&display=page&redirect_uri=http://vk.com/&scope=groups,friends,photos,messages,wall&response_type=token&v=5.74");
#pragma warning restore S1075 // URIs should not be hardcoded
           
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

            string url = webBrowser1.Url.ToString();
            string str = url.Split('#')[1];
            if (str[0] == 'a')
            {
                Settings1.Default.Token = url.Split('&')[0].Split('=')[1];
                Settings1.Default.id = url.Split('=')[3];
                this.Close();
            }


        }

    }
    }
