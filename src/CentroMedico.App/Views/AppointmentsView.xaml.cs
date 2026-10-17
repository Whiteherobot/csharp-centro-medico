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
    /// Interaction logic for AppointmentsView.xaml
    /// </summary>
    public partial class AppointmentsView : Page
    {
       
        public AppointmentsView()
        {
            InitializeComponent();
            
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Confirmar la cita
            string cedulaPaciente = txtCedPAC.Text;
            string cedulaMedico = txtCedMED.Text;
            

            

           
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Cancelar la operación
            txtCedPAC.Clear();
            txtCedMED.Clear();
            
        }

        
    }
}
