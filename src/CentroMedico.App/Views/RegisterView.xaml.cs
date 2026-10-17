using CentroMedico.App.Views;
using CentroMedico.App.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace CentroMedico.App.Views
{
    /// <summary>
    /// Lógica de interacción para RegisterView.xaml
    /// </summary>
    public partial class RegisterView : Window
    {
        public RegisterViewModel RegisterViewModel { get; private set; }
        public RegisterView()
        {
            InitializeComponent();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            // Application.Current.Shutdown();
            this.Close();
        }

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
            string.IsNullOrWhiteSpace(txtSecondName.Text) ||
            string.IsNullOrWhiteSpace(txtIDnumber.Text) ||
            string.IsNullOrWhiteSpace(txtAddres.Text) ||
            string.IsNullOrWhiteSpace(txtPhone.Text) ||
            string.IsNullOrWhiteSpace(txtMail.Text))
            {
                MessageBox.Show("Todos los campos son obligatorios.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            RegisterViewModel = new RegisterViewModel
            {
                nombre = txtName.Text,
                apellido = txtSecondName.Text,
                cedula = txtIDnumber.Text,
                direccion = txtAddres.Text,
                telefono = txtPhone.Text,
                Email = txtMail.Text
            };

            UserView userView = new UserView(RegisterViewModel);

            userView.Show();

            this.Close();
        }


        private void TextBlock_MouseDown(object sender, RoutedEventArgs e)
        {
            LoginView loginView = new LoginView();

            loginView.Show();

            this.Close();
        }
    }
}
