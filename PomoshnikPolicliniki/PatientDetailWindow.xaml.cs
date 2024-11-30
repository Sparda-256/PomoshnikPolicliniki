
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace PomoshnikPolicliniki
{
    public partial class PatientDetailWindow : Window
    {
        private int patientID;
        private string connectionString = "data source=192.168.147.54;initial catalog=PomoshnikPolicliniki;user id=is;password=1;encrypt=False;";

        public Patient SelectedPatient { get; set; }
        public ObservableCollection<PatientSymptom> PatientSymptoms { get; set; }
        public ObservableCollection<PatientAnswer> PatientAnswers { get; set; }
        public ObservableCollection<PatientDiagnosis> PatientDiagnoses { get; set; }

        public PatientDetailWindow(int patientId)
        {
            InitializeComponent();
            patientID = patientId;
            PatientSymptoms = new ObservableCollection<PatientSymptom>();
            PatientAnswers = new ObservableCollection<PatientAnswer>();
            PatientDiagnoses = new ObservableCollection<PatientDiagnosis>();
            DataContext = this;
            LoadPatientDetailsAsync();
        }

        private async void LoadPatientDetailsAsync()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    string patientQuery = "SELECT PatientID, FullName, Age, Gender FROM Patients WHERE PatientID = @PatientID";
                    using (SqlCommand cmd = new SqlCommand(patientQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PatientID", patientID);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                SelectedPatient = new Patient
                                {
                                    PatientID = reader.GetInt32(0),
                                    FullName = reader.GetString(1),
                                    Age = reader.GetInt32(2),
                                    Gender = reader.GetString(3)
                                };
                            }
                        }
                    }

                    string symptomsQuery = @"
                        SELECT S.SymptomName
                        FROM PatientSymptoms PS
                        JOIN Symptoms S ON PS.SymptomID = S.SymptomID
                        WHERE PS.PatientID = @PatientID";
                    using (SqlCommand cmd = new SqlCommand(symptomsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PatientID", patientID);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                PatientSymptoms.Add(new PatientSymptom
                                {
                                    SymptomName = reader.GetString(0)
                                });
                            }
                        }
                    }

                    string answersQuery = @"
                        SELECT FQ.Question, A.Answer
                        FROM PatientAnswers PA
                        JOIN Answers A ON PA.AnswerID = A.AnswerID
                        JOIN FollowUpQuestions FQ ON A.QuestionID = FQ.QuestionID
                        WHERE PA.PatientID = @PatientID";
                    using (SqlCommand cmd = new SqlCommand(answersQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PatientID", patientID);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                PatientAnswers.Add(new PatientAnswer
                                {
                                    Question = reader.GetString(0),
                                    Answer = reader.GetString(1)
                                });
                            }
                        }
                    }

                    string diagnosesQuery = @"
                        SELECT D.DiagnosisName, PD.PercentageOfDiagnosis
                        FROM PatientDiagnoses PD
                        JOIN Diagnoses D ON PD.DiagnosisID = D.DiagnosisID
                        WHERE PD.PatientID = @PatientID";
                    using (SqlCommand cmd = new SqlCommand(diagnosesQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PatientID", patientID);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                PatientDiagnoses.Add(new PatientDiagnosis
                                {
                                    DiagnosisName = reader.GetString(0),
                                    PercentageOfDiagnosis = reader.GetInt32(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке деталей пациента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
