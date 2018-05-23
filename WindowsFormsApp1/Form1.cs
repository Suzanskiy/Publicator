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
namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Form2 form2;
        VkApi Api = new VkApi();
        ulong Photo_count = 4;
        int groupId = -159059779;
        long[] targetArray = new long[1000];
        List<MediaAttachment> attachments = new List<MediaAttachment>();
        DateTime time = new DateTime();

        public Form1()
        {
            InitializeComponent();
            form2 = new Form2();
            form2.ShowDialog();
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, pictureBox1.Width - 3, pictureBox1.Height - 3);
            Region rg = new Region(gp);
            pictureBox1.Region = rg;

            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
        }

        void TargetDraw(int reciever)
        {
            Settings1.Default.user_id_post = reciever;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
            pictureBox4.Image = null;
            pictureBox5.Image = null;
            attachments.Clear();
            try
            {
                var target = Api.Photo.Get(new PhotoGetParams
                {
                    Reversed = true,
                    AlbumId = VkNet.Enums.SafetyEnums.PhotoAlbumType.Profile,
                    OwnerId = reciever,
                    Count = Photo_count,
                    Extended = true,

                });
                int id = 0;
                foreach (var Tparam in target)
                {
                    switch (id)
                    {
                        case 0:
                            pictureBox2.LoadAsync(Tparam.Photo604.ToString());
                            break;
                        case 1:
                            pictureBox3.LoadAsync(Tparam.Photo604.ToString());
                            break;
                        case 2:
                            pictureBox4.LoadAsync(Tparam.Photo604.ToString());
                            break;
                        case 3:
                            pictureBox5.LoadAsync(Tparam.Photo604.ToString());
                            break;
                    }
                    attachments.Add(new Photo
                    {
                        Id = Tparam.Id,
                        OwnerId = Tparam.OwnerId,
                    });
                    id++;
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
        private void Form1_Load(object sender, EventArgs e)
        {
            time = new DateTime(dateTimePicker1.Value.Ticks);

            Api.AuthorizeAsync(new ApiAuthParams
            {
                AccessToken = Settings1.Default.Token
            });


            var adm_id = new long[] { long.Parse(Settings1.Default.id) };
            var adm = Api.Users.Get(adm_id, ProfileFields.Photo200);

            foreach (var param in adm)
            {

                this.Text = "VkGroup is active, adm - " + param.LastName + " " + param.FirstName;
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox1.LoadAsync(param.Photo200.ToString());
            }

            UserSearch((bool)chBoxOnline.Checked, GetChkBoxPopularState(chBoxPopular), (ushort)BoxAgeFrom.Value, (ushort)BoxAgeTo.Value);
           
                TargetDraw((int)targetArray[rand.Next(1, 990)]);
            
        }

        void UserSearch(bool online, VkNet.Enums.UserSort sort, ushort ageFrom, ushort ageTo)
        {          
               var users = Api.Users.Search(new UserSearchParams
               {
                   AgeFrom = ageFrom,
                   AgeTo = ageTo,
                   Sort = sort,
                   Online = online,
                   Sex = VkNet.Enums.Sex.Female,
                   Count = 1000,
                   HasPhoto = true,

               });
            
            int i = 0;
               foreach (var item in users)
               {
                   targetArray[i] = item.Id;
                   i++;
               }
          
        }

        DateTime GetPublishDate(DateTime newValue)
        {
            return newValue.AddMinutes(30);
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
                            Message = "Model: @id" + param.Id + "(" + param.FirstName + " " + param.LastName + ") <3 \n ___________ \n with best wishes, ну вау team. ",
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
                            Message = "Model: @id" + param.Id + "(" + param.FirstName + " " + param.LastName + ") <3 \n ___________ \n with best wishes, ну вау team. ",
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

        void SearchUpdate()
        {
            UserSearch((bool)chBoxOnline.Checked, GetChkBoxPopularState(chBoxPopular), (ushort)BoxAgeFrom.Value, (ushort)BoxAgeTo.Value);

            TargetDraw((int)targetArray[rand.Next(1, 990)]);
            

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
                
        private void Panel2_MouseLeave(object sender, EventArgs e)
        {
            SearchUpdate();
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
            TargetDraw((int)targetArray[rand.Next(1, 990)]);
        }


    }
}
