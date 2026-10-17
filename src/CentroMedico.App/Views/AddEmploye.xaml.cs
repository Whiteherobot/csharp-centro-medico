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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CentroMedico.App.Views
{
    /// <summary>
    /// Interaction logic for AddEmploye.xaml
    /// </summary>
    public partial class AddEmploye : Page
    {
        public RegisterViewModel RegisterViewModel { get; private set; }
        public AddEmploye()
        {
            InitializeComponent();
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

            MainView mainView = Application.Current.Windows.OfType<MainView>().FirstOrDefault();

            if (mainView != null)
            {
                // Navegar a la nueva página dentro del frame de la ventana principal
                mainView.mainFrame.Navigate(new AddUser(RegisterViewModel));
            }
            else
            {
                MessageBox.Show("La ventana principal no está abierta.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
