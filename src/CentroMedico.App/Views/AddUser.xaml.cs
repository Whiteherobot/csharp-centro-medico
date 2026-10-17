using CentroMedico.App.Repositories;
using CentroMedico.App.Views;
using CentroMedico.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CentroMedico.App.Views
{
    /// <summary>
    /// Interaction logic for AddUser.xaml
    /// </summary>
    public partial class AddUser : Page
    {
        private RegisterViewModel registerViewModel;
        public AddUser(RegisterViewModel data)
        {
            registerViewModel = data;
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text) ||
            string.IsNullOrWhiteSpace(txtPass.Password) ||
            string.IsNullOrWhiteSpace(txtPassConfirm.Password))
            {
                MessageBox.Show("Todos los campos son obligatorios.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (txtPass.Password != txtPassConfirm.Password)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            UserRepository userRepository = new UserRepository();
            userRepository.InsertData(registerViewModel, txtUser.Text, txtPass.Password);

            // Mostrar mensaje de éxito y cerrar las ventanas
            MessageBox.Show("Registro exitoso", "Éxito");

            MainView mainView = Application.Current.Windows.OfType<MainView>().FirstOrDefault();

            if (mainView != null)
            {
                // Navegar a la nueva página dentro del frame de la ventana principal
                mainView.mainFrame.Navigate(new CustomerView());
            }
        }

    }
}
