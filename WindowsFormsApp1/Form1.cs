using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Abstractions;
using VkNet.Model.RequestParams;
using xNet;
using Newtonsoft.Json.Linq;
using VkNet.Model.Attachments;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Form2 form2 = new Form2();
        
        VkApi Api = new VkApi();
        ulong Photo_count = 4;
        int groupId = -159059779;
  
        List<MediaAttachment> attachments = new List<MediaAttachment>();
        DateTime time = new DateTime();
        VkNet.Utils.VkCollection<VkNet.Model.User> Userlist;
        public Form1()
        {            
            InitializeComponent();            
            form2.ShowDialog();
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, pictureBox1.Width - 3, pictureBox1.Height - 3);
            Region rg = new Region(gp);
            pictureBox1.Region = rg;
            pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
        }
       

            async void TargetDraw(int reciever)
        {

            Settings1.Default.user_id_post = reciever;
            pictureBox4.Image = null;
            pictureBox3.Image = null;
            pictureBox5.Image = null;
            pictureBox2.Image = null;

            attachments.Clear();
            try
            {
                VkNet.Utils.VkCollection<Photo> x = await Api.Photo.GetAsync(new PhotoGetParams
                {
                    Reversed = true,
                    AlbumId = VkNet.Enums.SafetyEnums.PhotoAlbumType.Profile,
                    OwnerId = reciever,
                    Count = Photo_count,
                    Extended = true,
                });
                for (int i = 0; i < x.Count; i++)
                {
                    if (i == 0) pictureBox4.LoadAsync(x[i].Photo604.ToString());
                    if (i == 1) pictureBox3.LoadAsync(x[i].Photo604.ToString());
                    if (i == 2) pictureBox5.LoadAsync(x[i].Photo604.ToString());
                    if (i == 3) pictureBox2.LoadAsync(x[i].Photo604.ToString());
                    attachments.Add(new Photo
                    {
                        Id = x[i].Id,
                        OwnerId = x[i].OwnerId,
                    });
                }
            }
            catch (VkNet.Exception.CannotBlacklistYourselfException)
            {
                TargetDraw(reciever + 1);
                Notify.ShowBalloonTip(100, "Нет доступа", "я отобразила следующую девушку..", ToolTipIcon.Warning);
            }
            catch (System.FormatException)
            {
                const string message = "Не верный id!";
                const string caption = "Ошибка";
                MessageBox.Show(message, caption,
                                             MessageBoxButtons.OK,
                                             MessageBoxIcon.Question);
            }

        }
        Random rand = new Random();
        private async void Form1_Load(object sender, EventArgs e)
        {
            
            await Api.AuthorizeAsync(new ApiAuthParams
            {
            AccessToken = Settings1.Default.Token
            });

          await UserSearch((bool)chBoxOnline.Checked, GetChkBoxPopularState(chBoxPopular), (ushort)BoxAgeFrom.Value, (ushort)BoxAgeTo.Value);
            
            time = new DateTime(dateTimePicker1.Value.Ticks);
            var adm_id = new long[] { long.Parse(Settings1.Default.id) };
            var adm = Api.Users.Get(adm_id, ProfileFields.Photo200);

            foreach (var param in adm)
            {
                this.Text = "VkGroup is active, adm - " + param.LastName + " " + param.FirstName;
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox1.LoadAsync(param.Photo200.ToString());
            }

            TargetDraw((int)Userlist[rand.Next(1,1000)].Id);
           
        }
        
       async Task UserSearch(bool online, VkNet.Enums.UserSort sort, ushort ageFrom, ushort ageTo)
        {

            Userlist = await Api.Users.SearchAsync(new UserSearchParams
            {
                AgeFrom = ageFrom,
                AgeTo = ageTo,
                Sort = sort,
                Online = online,
                Sex = VkNet.Enums.Sex.Female,
                Count = 1000,
                HasPhoto = true,
                BirthDay = (ushort)rand.Next(1,28),
                
            });
        

        }

        DateTime GetPublishDate(DateTime newValue)
        {
            return newValue.AddMinutes(60);
        }

        string RandomText()
        {
            string str;
            using (StreamReader strr = new StreamReader(HttpWebRequest.Create(@"https://fish-text.ru/get?format=html&number=1").GetResponse().GetResponseStream()))
                str = strr.ReadToEnd();
            str = str.Remove(0, 3);
           str= str.Remove(str.Length-4, 4);
           
            return str;

        }
        Exception PostProcess(long user_post_id, VkApi api)
        {
           
            time = GetPublishDate(time);
            var user_id = api.Users.Get(new long[] { user_post_id });
            try
            {
                foreach (var param in user_id)
                {
                    if (DelayPost.Checked)
                    {
#pragma warning disable S1481 // Unused local variables should be removed
                        var post = api.Wall.PostAsync(new WallPostParams
#pragma warning restore S1481 // Unused local variables should be removed
                        {

                            PublishDate = GetPublishDate(time),
                            OwnerId = groupId,
                            FromGroup = true,
                            Message = "Model: @id" + param.Id + "(" + param.FirstName + " " + param.LastName + ") <3 \n ___________ \n"+RandomText()+" \n Ну Вау. ",
                            Attachments = attachments,

                        });
                    }
                    else
                    {
#pragma warning disable S1481 // Unused local variables should be removed
                        var post = api.Wall.PostAsync(new WallPostParams
#pragma warning restore S1481 // Unused local variables should be removed
                        {
                            OwnerId = groupId,
                            FromGroup = true,
                            Message = "Model: @id" + param.Id + "(" + param.FirstName + " " + param.LastName + ") <3 \n ___________ \n" + RandomText() + " \n Ну Вау. ",
                            Attachments = attachments,

                        });
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }



        VkNet.Enums.UserSort GetChkBoxPopularState(CheckBox box)
        {
            return box.Checked ? VkNet.Enums.UserSort.ByPopularity : VkNet.Enums.UserSort.ByRegDate;
        }

        async Task SearchUpdate()
        {
            await UserSearch((bool)chBoxOnline.Checked, GetChkBoxPopularState(chBoxPopular), (ushort)BoxAgeFrom.Value, (ushort)BoxAgeTo.Value);
        }

        private void Notify_MouseClick(object sender, MouseEventArgs e)
        {
            WebBrowser w1 = new WebBrowser();
#pragma warning disable S1075 // URIs should not be hardcoded
            string url = "http://vk.com/ny_vay";
#pragma warning restore S1075 // URIs should not be hardcoded
            w1.Navigate(url, true);
            w1.Dispose();
        }

        private void DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            btnPost.Enabled = true;
            time = new DateTime(dateTimePicker1.Value.Ticks);
        }

        private void DelayPost_CheckedChanged(object sender, EventArgs e)
        {
            if (DelayPost.Checked)
                dateTimePicker1.Enabled = true;
            else
                dateTimePicker1.Enabled = false;
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Specify that the link was visited.
            this.linkLabel1.LinkVisited = true;
            string url = "http://vk.com/ny_vay";
            // Navigate to a URL.
            System.Diagnostics.Process.Start(url);
        }

        private async void Panel2_MouseLeave(object sender, EventArgs e)
        {
           await SearchUpdate();
        }

        private void BtnPost_Click(object sender, EventArgs e)
        {
            btnPost.Enabled = false;

            var user_post_id = (long)Settings1.Default.user_id_post;

            var result = PostProcess(user_post_id, Api);

            if (result != null)
                Notify.ShowBalloonTip(10, "Не-а, на сегодня хватит", "Выбери отложенную публикацию и другую дату", ToolTipIcon.Warning);
            else
            {
                Notify.Visible = true;

                if (DelayPost.Checked)
                    Notify.ShowBalloonTip(10, "Готово", "я отложила публикацию на " + time.Hour + ":" + time.Minute + " выбранной даты", ToolTipIcon.Info);
                else
                    Notify.ShowBalloonTip(10, "Готово", "Я опубликовала эту красавицу ", ToolTipIcon.Info);

            }

        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            btnPost.Enabled = true;
            try
            {
                TargetDraw((int)Userlist[rand.Next(1, 1000)].Id);
            }
            catch (Exception ex)
            {
                ex = null;             
            }
            
        }


    }
}
