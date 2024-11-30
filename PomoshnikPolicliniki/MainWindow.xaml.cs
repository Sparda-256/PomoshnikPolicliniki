using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;

namespace PomoshnikPolicliniki
{
    public class Patient : INotifyPropertyChanged
    {
        private int patientID;
        private string fullName;
        private int age;
        private string gender;

        public int PatientID
        {
            get => patientID;
            set
            {
                patientID = value;
                OnPropertyChanged(nameof(PatientID));
            }
        }
        public string FullName
        {
            get => fullName;
            set
            {
                fullName = value;
                OnPropertyChanged(nameof(FullName));
            }
        }
        public int Age
        {
            get => age;
            set
            {
                age = value;
                OnPropertyChanged(nameof(Age));
            }
        }
        public string Gender
        {
            get => gender;
            set
            {
                gender = value;
                OnPropertyChanged(nameof(Gender));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public class PatientSymptom
    {
        public string SymptomName { get; set; }
    }

    public class PatientDiagnosis
    {
        public string DiagnosisName { get; set; }
        public int PercentageOfDiagnosis { get; set; }
    }
    public class PatientAnswer
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }
    public partial class MainWindow : Window
    {
        private string connectionString = "data source=192.168.147.54;initial catalog=PomoshnikPolicliniki;user id=is;password=1;encrypt=False;";

        public ObservableCollection<Patient> Patients { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Patients = new ObservableCollection<Patient>();
            PatientsDataGrid.ItemsSource = Patients;
            LoadPatients();
        }

        private void LoadPatients(string searchQuery = "")
        {
            Patients.Clear();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT PatientID, FullName, Age, Gender FROM Patients";

                    if (!string.IsNullOrWhiteSpace(searchQuery))
                    {
                        query += " WHERE FullName LIKE @SearchQuery";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (!string.IsNullOrWhiteSpace(searchQuery))
                        {
                            cmd.Parameters.AddWithValue("@SearchQuery", "%" + searchQuery + "%");
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Patients.Add(new Patient
                                {
                                    PatientID = reader.GetInt32(0),
                                    FullName = reader.GetString(1),
                                    Age = reader.GetInt32(2),
                                    Gender = reader.GetString(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пациентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim();
            LoadPatients(searchText);
        }
        private void PatientsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PatientsDataGrid.SelectedItem is Patient selectedPatient)
            {
                PatientDetailWindow detailWindow = new PatientDetailWindow(selectedPatient.PatientID);
                detailWindow.Owner = this;
                detailWindow.ShowDialog();
            }
        }
    }
}
