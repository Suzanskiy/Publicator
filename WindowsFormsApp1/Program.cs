using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using VkNet;
using VkNet.Enums.Filters;

namespace WindowsFormsApp1
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new Form1());
            }
            catch (System.Reflection.TargetInvocationException)
            {
                MessageBox.Show("Токен выдан другому ip-адрессу");

                Form2 form2 = new Form2();
                form2.ShowDialog();
                Form1 form1 = new Form1();
                form1.ShowDialog();
            }
            
        }
    }
}
