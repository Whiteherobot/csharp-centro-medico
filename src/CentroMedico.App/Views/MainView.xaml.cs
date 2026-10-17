using CentroMedico.App.Models;
using CentroMedico.App.Views;
using FontAwesome.Sharp;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace CentroMedico.App.Views
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private UserModel user;
        public MainView(UserModel user)
        {
            InitializeComponent();
            this.user = user;
            txtNameUser.Text = $"{user.Name} {user.LastName}"; // Asigna el nombre del usuario al TextBlock
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);

        }
        private void pnlControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else this.WindowState = WindowState.Normal;
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void RbShowHomeView_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new EmployeView());
            ShowText_Click(sender, e);
        }
        
        private void RbShowHomeView_Click1(object sender, RoutedEventArgs e)
        {
            CustomerView customerView = new CustomerView();

            mainFrame.Navigate(new CustomerView());
            ShowText_Click(sender, e);

        }

        private void RbShowHomeView_Click2(object sender, RoutedEventArgs e)
        {

            mainFrame.Navigate(new PACA());
            ShowText_Click(sender, e);

        }

        private void RbShowHomeView_Click3(object sender, RoutedEventArgs e)
        {
            

            mainFrame.Navigate(new DateView());
            ShowText_Click(sender, e);

        }

        private void RbShowHomeView_Click4(object sender, RoutedEventArgs e)
        {

            mainFrame.Navigate(new InvoiceView());
            ShowText_Click(sender, e);
        }

        private void RbShowHomeView_Click5(object sender, RoutedEventArgs e)
        {

            mainFrame.Navigate(new ServiceView());
            ShowText_Click(sender, e);

        }

        private void ShowText_Click(object sender, RoutedEventArgs e)
        {
            // Limpia el StackPanel
            mainStackPanel.Children.Clear();

            // Obtén el RadioButton que lanzó el evento
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null)
            {
                // Accede directamente al contenido del RadioButton
                object content = radioButton.Content;

                // Verifica que el contenido no sea nulo y sea de tipo TextBlock
                if (content is TextBlock textBlock)
                {
                    // Crea un nuevo TextBlock con el texto del RadioButton
                    TextBlock newTextBlock = new TextBlock
                    {
                        Text = textBlock.Text,
                        FontSize = 18,
                        FontWeight = FontWeights.Medium,
                        Margin = new Thickness(10)
                    };

                    // Agrega el nuevo TextBlock al StackPanel
                    mainStackPanel.Children.Add(newTextBlock);
                }
                else if (content is StackPanel stackPanel)
                {
                    // En caso de tener más elementos dentro del StackPanel
                    foreach (var child in stackPanel.Children)
                    {
                        if (child is IconImage iconImage)
                        {
                            // Crea un nuevo IconImage con el icono del RadioButton
                            IconImage newIconImage = new IconImage
                            {
                                Icon = iconImage.Icon,
                                Height = 20,
                                Width = 20,
                                Foreground = Brushes.White, // Cambiar según necesites
                                Margin = new Thickness(10)
                            };

                            // Agrega el nuevo IconImage al StackPanel
                            mainStackPanel.Children.Add(newIconImage);
                        }
                        else if (child is TextBlock textBlockInside)
                        {
                            // Crea un nuevo TextBlock con el texto del RadioButton
                            TextBlock newTextBlock = new TextBlock
                            {
                                Text = textBlockInside.Text,
                                FontSize = 16,
                                FontWeight = FontWeights.Medium,
                                Foreground = Brushes.White,
                                Margin = new Thickness(9)
                            };

                            // Agrega el nuevo TextBlock al StackPanel
                            mainStackPanel.Children.Add(newTextBlock);
                        }
                    }
                }
            }
        }
    }
}
