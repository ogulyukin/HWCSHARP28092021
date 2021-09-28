using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Xml;
using System.Data;
using System.Text;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using System.IO;

namespace XmlToJson
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string _connection = "Data Source=base.sqlite;Mode=ReadWrite;";
        private ObservableCollection<RowElement> _InfoRows;
        List<User> users;

        public MainWindow()
        {
            InitializeComponent();
            _InfoRows = new();
            LoadedData.ItemsSource = _InfoRows;
            SaveJson.IsEnabled = false;
            users = new();
        }

        private void OpenXml_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML files (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == true)
            {
                var xml = new XmlDocument();
                try
                {
                    xml.Load(openFileDialog.FileName);
                }catch
                {
                    MessageBox.Show($"Ошибка: не могу открыть файл {openFileDialog.FileName}");
                }
                var root = xml.DocumentElement;
                int count = 0;
                foreach(XmlNode i in root.ChildNodes)
                {
                    var user = new User();
                    user.Name = i.Attributes[0].Value;
                    foreach(XmlNode j in i.ChildNodes)
                    {
                        if (j.Name == "company")
                        {
                            user.Company = j.InnerText;
                        }
                        else
                        {
                            user.Age = j.InnerText;
                        }

                    }
                    user.Id = SaveToDataBase(user);
                    var row = new RowElement(0, $"ID : {user.Id}");
                    _InfoRows.Add(row);
                    row = new RowElement(count, $"Name : {user.Name}");
                    _InfoRows.Add(row);
                    count++;
                    row = new RowElement(count, $"Company : {user.Company}");
                    _InfoRows.Add(row);
                    count++;
                    row = new RowElement(count, $"Age : {user.Age}");
                    _InfoRows.Add(row);
                    users.Add(user);
                }
            }
            SaveJson.IsEnabled = true;
        }

        private void SaveJson_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Json files (*.json)|*.json|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == true)
            {
                if (saveFileDialog.FileName == string.Empty)
                    return;
                using (var file = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    JsonSerializer.SerializeAsync(file, users);
                }
            }
        }

        private int SaveToDataBase(User user)
        {
            int result = 0;
            using var db = new SqliteConnection(_connection);
            db.Open();
            var sql = $"INSERT INTO 'Person_Tab'('name', 'company', 'age') VALUES ('{user.Name}', '{user.Company}', '{user.Age}');";
            using var query01 = new SqliteCommand(sql, db);
            query01.ExecuteNonQuery();
            sql = $"SELECT id FROM Person_Tab WHERE name = '{user.Name}'";
            using var query02 = new SqliteCommand(sql, db);
            using var res01 = query02.ExecuteReader();
            if (res01.HasRows)
            {
                res01.Read();
                result = res01.GetInt32("id");
            }
            return result;
        }
    }
}
